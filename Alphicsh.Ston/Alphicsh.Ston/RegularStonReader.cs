using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using Alphicsh.Ston.Building;
using Alphicsh.Ston.Tokenization;

namespace Alphicsh.Ston
{
    /// <summary>
    /// Provides the functionality of reading a STON abstract structure from a regular STON text.
    /// </summary>
    public sealed class RegularStonReader : IStonReader<IStonEntity, IStonDocument>
    {
        /// <summary>
        /// Provides a STON reader functionality using default elements and documents factories.
        /// </summary>
        public static IStonReader<IStonEntity, IStonDocument> Default { get; } = new RegularStonReader();

        // the factory used to create STON elements
        private IStonElementFactory ElementFactory { get; }

        // the factory used to create STON documents
        private IStonDocumentFactory DocumentFactory { get; }

        /// <summary>
        /// Creates a new STON reader using a provided STON elements factory and STON documents factory.
        /// </summary>
        /// <param name="elementFactory">The factory used to create STON elements.</param>
        /// <param name="documentFactory">The factory used to build STON documents.</param>
        public RegularStonReader(IStonElementFactory elementFactory, IStonDocumentFactory documentFactory)
        {
            if (elementFactory == null) throw new ArgumentNullException("elementFactory");
            if (documentFactory == null) throw new ArgumentNullException("documentFactory");
            ElementFactory = elementFactory;
            DocumentFactory = documentFactory;
        }

        /// <summary>
        /// Creates a new STON reader using a factory of core STON elements.
        /// </summary>
        public RegularStonReader()
            : this(CoreStonElementFactory.Instance, CoreStonDocumentFactory.Instance) { }

        #region IStonReader implementation

        /// <summary>
        /// Gets the StonFormat.RegularSton value, to indicate that regular STON strings are supported by the reader.
        /// </summary>
        public StonFormat SupportedFormat => StonFormat.RegularSton;

        /// <summary>
        /// Reads a STON entity from a given text reader.
        /// </summary>
        /// <param name="reader">The text reader to read the entity from.</param>
        /// <returns>The read STON entity.</returns>
        public IStonEntity ReadEntity(TextReader reader)
        {
            if (reader == null) throw new ArgumentNullException("reader");
            var tokenReader = new StonTokenReader(reader);
            tokenReader.PeekSignificant();
            var entity = _ReadEntity(tokenReader, false);
            if (!tokenReader.Peek().HasChartype(StonChartype.EOS)) throw tokenReader.MakeUnexpectedCharacterException(StonChartype.EOS, $"Unexpected character after the valid entity: '{(char)tokenReader.Peek()}'.");

            return entity;
        }

        /// <summary>
        /// Reads a STON document from a given text reader, with whitelists of known application extensions and rules determining valid application extension names.
        /// </summary>
        /// <param name="reader">The text reader to read the document from.</param>
        /// <param name="knownApplicationExtensionTypes">The whitelist of known application extension types.</param>
        /// <param name="knownApplicationExtensionMembers">The whitelist of known application extension members.</param>
        /// <param name="extensionTypesRule">The rule determining if a given name is a valid application extension type name.</param>
        /// <param name="extensionMembersRule">The rule determining if a given name is a valid application extension member name.</param>
        /// <returns>The read STON document.</returns>
        public IStonDocument ReadDocument(
            TextReader reader,
            IEnumerable<string> knownApplicationExtensionTypes, IEnumerable<string> knownApplicationExtensionMembers,
            Func<string, bool> extensionTypesRule, Func<string, bool> extensionMembersRule
            )
        {
            if (reader == null) throw new ArgumentNullException("reader");
            var tokenReader = new StonTokenReader(reader);
            tokenReader.PeekSignificant();
            var entity = _ReadEntity(tokenReader, false);
            if (!tokenReader.Peek().HasChartype(StonChartype.EOS)) throw tokenReader.MakeUnexpectedCharacterException(StonChartype.EOS, $"Unexpected character after the valid entity: '{(char)tokenReader.Peek()}'.");
            if (!(entity is IStonValuedEntity)) throw new StonException("The parsed entity is not a valid document core.");

            return DocumentFactory.CreateDocument(entity as IStonValuedEntity, knownApplicationExtensionTypes, knownApplicationExtensionMembers, extensionTypesRule, extensionMembersRule);
        }

        #endregion

        #region Reading entities

        // reads a complete entity
        private IStonEntity ReadEntity(StonTokenReader reader) => _ReadEntity(reader, false);

        // reads an address index parameter entity; i.e. simple or reference, without the global identifier
        private IStonEntity ReadAddressEntity(StonTokenReader reader) => _ReadEntity(reader, true);

        // reads a STON entity, complete or address
        private IStonEntity _ReadEntity(StonTokenReader reader, bool isAddress)
        {
            // stores a CANUN identifier or path for later use until its purpose is known
            string storedWord = null;

            // determining the global identifier, if any
            string globalIdentifier = null;

            // the identifier is explicitly prepended with a global identifier sigil
            if (reader.Peek().HasChartype(StonChartype.IdentifierSigil))
            {
                if (isAddress) throw reader.MakeUnexpectedCharacterException(StonChartype.None);        // identifiers are not allowed for index parameter entities
                reader.ReadAndSkip();
                globalIdentifier = reader.ReadCanun();
                reader.ExpectAndSkip(StonChartype.IdentifierAssign);
            }
            // the entity begins with a CANUN name that might or might not be a global identifier
            else if (reader.Peek().HasChartype(StonChartype.CanunBegin))
            {
                storedWord = reader.ReadCanun();
                // if and only if the CANUN name is followed by a global identifier assignment token
                // the name is to be used as a global identifier
                if (reader.Peek().HasChartype(StonChartype.IdentifierAssign))
                {
                    if (isAddress) throw reader.MakeUnexpectedCharacterException(StonChartype.None);    // once again, identifiers are not allowed for index parameter entities
                    reader.ReadAndSkip();
                    globalIdentifier = storedWord;
                    storedWord = null;
                }
            }

            // finding the address, if any
            if (reader.Peek().HasChartype(StonChartype.AddressBegin))
            {
                // the reference entity cannot have a type declaration
                if (storedWord != null) throw reader.MakeUnexpectedCharacterException(StonChartype.None);

                return ElementFactory.CreateReferenceEntity(ReadAddress(reader), globalIdentifier);
            }

            // determining the type, if any
            IStonType type = null;
            bool isTypeFromWord = false;
            bool collectionStarted = false;     // a variable indicating whether during reading type a collection initialization has been started or not
                                                // when bare type definitions are involved, a collection initialization might appear like a collection type suffix
                                                // so it's impossible to tell the two apart before reading the initial sign

            // no word is stored; either there was no word in the first place, or there was a global identifier before
            if (storedWord == null)
            {
                // storing a CANUN path, which might be a type or a named value
                if (reader.Peek().HasChartype(StonChartype.CanunBegin)) storedWord = ReadCanunPath(reader);
                // reading a type if the following token is characteristic for types
                else if (reader.Peek().HasChartype(StonChartype.Extension | StonChartype.TypeOpen)) type = ReadBareUnion(reader, null, ref collectionStarted);
            }
            else
            {
                // if a CANUN name is stored, it might be a part of a longer CANUN path
                // which itself might be a type or a named value
                if (reader.Peek().HasChartype(StonChartype.NameSeparator)) storedWord = storedWord + (char)reader.ReadAndSkip() + ReadCanunPath(reader);
            }

            // if a CANUN path is followed by a type opening, collection suffix or a union type separator
            // it means that path must be a type name
            if (storedWord != null && reader.Peek().HasChartype(StonChartype.TypeOpen | StonChartype.CollectionSuffixBegin | StonChartype.UnionTypeSeparator))
            {
                type = ReadBareUnion(reader, storedWord, ref collectionStarted);
                storedWord = null;
            }
            if (storedWord != null)
            {
                type = ElementFactory.CreateNamedType(storedWord);
                isTypeFromWord = true;
            }
            

            // finding the complex value, if any

            // when the collection initialization has been entered when reading a type definition
            // the rest of the collection initialization is read
            if (collectionStarted)
            {
                // a complex value is not allowed in an address index parameter
                if (isAddress) throw reader.MakeUnexpectedCharacterException(StonChartype.CollectionSuffixContinue);

                // reading the rest of the collection initialization
                // and subsequent member initialization, if available
                IStonCollectionInit collectionInit = ReadCollectionInit(reader, true);
                IStonMemberInit memberInit = null;
                if (reader.Peek().HasChartype(StonChartype.MemberInitOpen)) memberInit = ReadMemberInit(reader);

                return ElementFactory.CreateComplexEntity(null, memberInit, collectionInit, type, globalIdentifier);
            }

            // reading any valid combination of complex value components (construction, member initialization, collection initialization)
            if (reader.Peek().HasChartype(StonChartype.ConstructionOpen | StonChartype.MemberInitOpen | StonChartype.CollectionInitOpen))
            {
                // a complex value is not allowed in an address index parameter
                if (isAddress) throw reader.MakeUnexpectedCharacterException(StonChartype.None);

                IStonConstruction constructor = null;
                IStonMemberInit memberInit = null;
                IStonCollectionInit collectionInit = null;

                if (reader.Peek().HasChartype(StonChartype.ConstructionOpen)) constructor = ReadConstruction(reader);
                if (reader.Peek().HasChartype(StonChartype.MemberInitOpen)) memberInit = ReadMemberInit(reader);
                if (reader.Peek().HasChartype(StonChartype.CollectionInitOpen))
                {
                    collectionInit = ReadCollectionInit(reader);
                    if (memberInit == null && reader.Peek().HasChartype(StonChartype.MemberInitOpen)) memberInit = ReadMemberInit(reader);
                }

                return ElementFactory.CreateComplexEntity(constructor, memberInit, collectionInit, type, globalIdentifier);
            }

            // finding the string, binary or number simple value, if complex value is not present

            // reading a string literal or string literals chain
            if (reader.TryAndSkip(StonChartype.StringChainOpen))
            {
                if (!reader.Peek().HasChartype(StonChartype.TextDelimiter | StonChartype.CodeDelimiter)) throw reader.MakeUnexpectedCharacterException(StonChartype.TextDelimiter | StonChartype.CodeDelimiter);
            }
            if (reader.Peek().HasChartype(StonChartype.TextDelimiter | StonChartype.CodeDelimiter))
            {
                bool isCode = reader.Peek().HasChartype(StonChartype.CodeDelimiter);        // whether the value is a text or a code

                var builder = new StringBuilder();
                reader.ReadString(builder, isCode);
                while (reader.Peek().HasChartype(StonChartype.StringChainContinue))
                {
                    if (reader.ReadAndSkip().HasChartype(StonChartype.StringChainOpen)) builder.Append('\n');
                    reader.ReadString(builder, isCode);
                }

                return ElementFactory.CreateSimpleEntity(ElementFactory.CreateSimpleValue(isCode ? StonDataType.Code : StonDataType.Text, builder.ToString()), type, globalIdentifier);
            }

            // reading a number of binary value
            bool minus = false;
            // reading the initial sign, if any
            if (reader.Peek().HasChartype(StonChartype.Sign))
            {
                if (reader.ReadAndSkip() == '-') minus = true;
                // a digit must follow the sign
                if (!reader.Peek().HasChartype(StonChartype.Digit)) throw reader.MakeUnexpectedCharacterException(StonChartype.Digit);
            }
            // both number and binary literals might begin with a zero (excluding initial sign)
            // whichever it is, is decided by whether the zero is followed by a base identifier or not
            if (reader.TryAndSkip(StonChartype.ZeroDigit))
            {
                if (reader.Peek().HasChartype(StonChartype.BaseIdentifier)) return ElementFactory.CreateSimpleEntity(ElementFactory.CreateSimpleValue(StonDataType.Binary, reader.ReadBinaryContent(minus)), type, globalIdentifier);
                else return ElementFactory.CreateSimpleEntity(ElementFactory.CreateSimpleValue(StonDataType.Number, reader.ReadNumberContent(minus)), type, globalIdentifier);
            }
            // only number literals might begin with a non-zero digit (aside from initial sign)
            else if (reader.Peek().HasChartype(StonChartype.NonZeroDigit))
            {
                return ElementFactory.CreateSimpleEntity(ElementFactory.CreateSimpleValue(StonDataType.Number, reader.ReadNumberContent(minus)), type, globalIdentifier);
            }

            // finding the named or null value, if everything else fails

            string value;
            if (reader.Peek().HasChartype(StonChartype.CanunBegin))
            {
                value = ReadCanunPath(reader);
            }
            else if (isTypeFromWord)
            {
                value = (type as IStonNamedType).Name;
                type = null;
            }
            else throw reader.MakeUnexpectedCharacterException(StonChartype.None);      // there is no named value, and thus there's all the reason to panic

            if (value == "null") return ElementFactory.CreateSimpleEntity(ElementFactory.CreateSimpleValue(StonDataType.Null, null), type, globalIdentifier);
            else return ElementFactory.CreateSimpleEntity(ElementFactory.CreateSimpleValue(StonDataType.Named, value), type, globalIdentifier);
        }

        #endregion

        #region General functions

        // reads a CANUN path
        private string ReadCanunPath(StonTokenReader reader)
        {
            StringBuilder builder = new StringBuilder();
            reader.ReadCanun(builder);
            while (reader.Peek().HasChartype(StonChartype.NameSeparator))
            {
                builder.Append((char)reader.ReadAndSkip());
                reader.ReadCanun(builder);
            }
            return builder.ToString();
        }

        // reads a sequence of elements, using a given element reading function and sequence terminator chartype
        private IList<TElement> ReadSequence<TElement>(StonTokenReader reader, Func<StonTokenReader, TElement> elementReading, StonChartype sequenceTerminator)
        {
            var result = new List<TElement>();

            while (!reader.TryAndSkip(sequenceTerminator))
            {
                result.Add(elementReading(reader));
                if (reader.TryAndSkip(StonChartype.SequenceSeparator)) continue;
                else if (!reader.Peek().HasChartype(sequenceTerminator)) throw reader.MakeUnexpectedCharacterException(StonChartype.SequenceSeparator | sequenceTerminator);
            }

            return result;
        }

        #endregion

        #region Reading type definition

        // reads a type wrapping expression, in which a wrapped union type expression is contained
        private IStonType ReadWrapping(StonTokenReader reader)
        {
            reader.ExpectAndSkip(StonChartype.TypeOpen);
            var result = ReadWrappedUnion(reader);
            reader.ExpectAndSkip(StonChartype.TypeClose);

            return result;
        }

        // reads a wrapped union type expression
        // which might represent a single wrapped collection type expression
        // or a union of multiple wrapped collection type expression
        private IStonType ReadWrappedUnion(StonTokenReader reader)
        {
            var permittedTypes = new List<IStonType>();
            var result = ReadWrappedCollection(reader);

            while (reader.TryAndSkip(StonChartype.UnionTypeSeparator))
            {
                permittedTypes.Add(result);
                result = ReadWrappedCollection(reader);
            }

            if (!permittedTypes.Any()) return result;
            else
            {
                permittedTypes.Add(result);
                return ElementFactory.CreateUnionType(permittedTypes);
            }
        }

        // reads a bare union type expression
        // which might represent a single collection type expression
        // or a union of multiple collection type expression
        private IStonType ReadBareUnion(StonTokenReader reader, string preread, ref bool collectionStarted)
        {
            var permittedTypes = new List<IStonType>();
            var result = ReadBareCollection(reader, preread, ref collectionStarted);
            if (result == null) return null;

            while (reader.Peek().HasChartype(StonChartype.UnionTypeSeparator))
            {
                if (collectionStarted) throw reader.MakeUnexpectedCharacterException(StonChartype.None);
                reader.ReadAndSkip();
                permittedTypes.Add(result);
                result = ReadBareCollection(reader, null, ref collectionStarted);
                if (result == null) throw new StonParsingException(reader.Position, reader.Line, reader.Column, "Implicit type declaration has been read as a part of union type.");
            }

            if (!permittedTypes.Any()) return result;
            else
            {
                permittedTypes.Add(result);
                return ElementFactory.CreateUnionType(permittedTypes);
            }
        }

        // reads a wrapped collection type expression
        // which might represent a wrapped named type expression
        // or an indefinitely nested collection of element type, represented as wrapped named type expression
        private IStonType ReadWrappedCollection(StonTokenReader reader)
        {
            IStonType result = ReadWrappedNamedType(reader);

            // each collection suffix adds one more nesting of collection
            while (reader.Peek().HasChartype(StonChartype.CollectionSuffixBegin))
            {
                reader.ReadCollectionTypeSuffix(false);
                result = ElementFactory.CreateCollectionType(result);
            }

            return result;
        }

        // reads a bare collection type expression
        // which might represent a bare named type expression
        // or an indefinitely nested collection of element type, represented as bare named type expression
        private IStonType ReadBareCollection(StonTokenReader reader, string preread, ref bool collectionStarted)
        {
            IStonType result = ReadBareNamedType(reader, preread);
            if (result == null) return null;

            // each collection suffix adds one more nesting of collection
            while (reader.Peek().HasChartype(StonChartype.CollectionSuffixBegin))
            {
                if (!reader.ReadCollectionTypeSuffix(true))
                {
                    collectionStarted = true;
                    break;
                }
                else result = ElementFactory.CreateCollectionType(result);
            }

            return result;
        }

        // reads a wrapped named type
        // which might represent an actual named type
        // or a type wrapping expression
        private IStonType ReadWrappedNamedType(StonTokenReader reader)
        {
            if (reader.Peek().HasChartype(StonChartype.TypeOpen)) return ReadWrapping(reader);      // the type is a wrapping

            string name = null;
            IList<IStonType> typeParameters = null;
            bool isExtension = false;

            if (reader.TryAndSkip(StonChartype.Extension)) isExtension = true;

            if (reader.Peek().HasChartype(StonChartype.TextDelimiter)) name = reader.ReadString();
            else if (reader.Peek().HasChartype(StonChartype.CanunBegin)) name = ReadCanunPath(reader);
            else throw new StonUnexpectedCharacterParsingException(reader.Position, reader.Line, reader.Column, reader.Peek(), StonChartype.TextDelimiter | StonChartype.CanunBegin);

            if (reader.TryAndSkip(StonChartype.TypeOpen)) typeParameters = ReadSequence(reader, ReadWrappedUnion, StonChartype.TypeClose);

            return ElementFactory.CreateNamedType(name, typeParameters, isExtension);
        }

        // reads a bare named type
        // which might represent an actual named type
        // a type wrapping expression
        // or a declaration of implicit type
        private IStonType ReadBareNamedType(StonTokenReader reader, string preread)
        {
            string name = null;
            IList<IStonType> typeParameters = new List<IStonType>();
            bool isExtension = false;

            if (preread != null) name = preread;
            else
            {
                // reading a type wrapping or an implicit type declaration 
                if (reader.TryAndSkip(StonChartype.TypeOpen))
                {
                    if (reader.TryAndSkip(StonChartype.TypeClose)) return null;     // implicit type declaration

                    // type wrapping
                    var result = ReadWrappedUnion(reader);
                    reader.ExpectAndSkip(StonChartype.TypeClose);
                    return result;
                }

                if (reader.TryAndSkip(StonChartype.Extension)) isExtension = true;

                if (reader.Peek().HasChartype(StonChartype.CanunBegin)) name = ReadCanunPath(reader);
                else throw new StonUnexpectedCharacterParsingException(reader.Position, reader.Line, reader.Column, reader.Peek(), StonChartype.CanunBegin);
            }

            if (reader.TryAndSkip(StonChartype.TypeOpen)) typeParameters = ReadSequence(reader, ReadWrappedUnion, StonChartype.TypeClose);

            return ElementFactory.CreateNamedType(name, typeParameters, isExtension);
        }

        #endregion

        #region Reading complex values

        // reads a construction component of a complex value
        private IStonConstruction ReadConstruction(StonTokenReader reader)
        {
            reader.ExpectAndSkip(StonChartype.ConstructionOpen);

            var positionalParameters = new List<IStonEntity>();
            var namedParameters = new List<KeyValuePair<string, IStonEntity>>();

            while (!reader.TryAndSkip(StonChartype.ConstructionClose))
            {
                // trying to read a positional parameter, or the first named parameter
                if (namedParameters.Count == 0)
                {
                    // value prompt right at the beginning clearly indicates a positional parameter
                    if (reader.TryAndSkip(StonChartype.ValuePrompt)) positionalParameters.Add(ReadEntity(reader));
                    // string literal might be an entity or a named parameter name
                    else if (reader.Peek().HasChartype(StonChartype.TextDelimiter))
                    {
                        var builder = new StringBuilder();

                        reader.ReadString(builder, false);

                        // if it's a single string literal followed by a value prompt, it means it is the first named parameter
                        if (reader.TryAndSkip(StonChartype.ValuePrompt)) namedParameters.Add(new KeyValuePair<string, IStonEntity>(builder.ToString(), ReadEntity(reader)));
                        // if no value prompt follows, it is assumed to be a string-valued entity
                        else
                        {
                            while (reader.Peek().HasChartype(StonChartype.StringChainContinue))
                            {
                                if (reader.ReadAndSkip().HasChartype(StonChartype.StringChainOpen)) builder.Append('\n');
                                reader.ReadString(builder, false);
                            }
                            positionalParameters.Add(ElementFactory.CreateSimpleEntity(ElementFactory.CreateSimpleValue(StonDataType.Text, builder.ToString())));
                        }
                    }
                    else
                    {
                        var entity = ReadEntity(reader);
                        if (reader.Peek().HasChartype(StonChartype.ValuePrompt))
                        {
                            // if value prompt follows after the read entity, it means the read entity must be a typeless, non-identified CANUN named value
                            // serving as the parameter name
                            if (!(entity is IStonSimpleEntity)) throw reader.MakeUnexpectedCharacterException(StonChartype.SequenceSeparator | StonChartype.ConstructionClose);
                            var simpleEntity = entity as IStonSimpleEntity;
                            if (simpleEntity.GlobalIdentifier != null || simpleEntity.Type != null || simpleEntity.Value.DataType != StonDataType.Named || simpleEntity.Value.Content.Contains('.'))
                            {
                                throw reader.MakeUnexpectedCharacterException(StonChartype.SequenceSeparator | StonChartype.ConstructionClose);
                            }
                            reader.ReadAndSkip();
                            namedParameters.Add(new KeyValuePair<string, IStonEntity>(simpleEntity.Value.Content, ReadEntity(reader)));
                        }
                        else positionalParameters.Add(entity);
                    }
                }
                // reading remaining named parameters
                else
                {
                    string name;
                    if (reader.Peek().HasChartype(StonChartype.TextDelimiter)) name = reader.ReadString();
                    else if (reader.Peek().HasChartype(StonChartype.CanunBegin)) name = reader.ReadCanun();
                    else throw reader.MakeUnexpectedCharacterException(StonChartype.TextDelimiter | StonChartype.CanunBegin);

                    reader.ExpectAndSkip(StonChartype.ValuePrompt);
                    IStonEntity value = ReadEntity(reader);

                    namedParameters.Add(new KeyValuePair<string, IStonEntity>(name, value));
                }

                if (reader.TryAndSkip(StonChartype.SequenceSeparator)) continue;
                else if (!reader.Peek().HasChartype(StonChartype.ConstructionClose)) throw reader.MakeUnexpectedCharacterException(StonChartype.SequenceSeparator | StonChartype.ConstructionClose);
            }

            return ElementFactory.CreateConstruction(positionalParameters, namedParameters);
        }

        // reads a member initialization component of a complex value
        private IStonMemberInit ReadMemberInit(StonTokenReader reader)
        {
            // just a sequence of member bindings
            reader.ExpectAndSkip(StonChartype.MemberInitOpen);
            IEnumerable<KeyValuePair<IStonBindingKey, IStonEntity>> memberBindings = ReadSequence(reader, ReadMemberBinding, StonChartype.MemberInitClose);
            return ElementFactory.CreateMemberInit(memberBindings);
        }

        // reads a member binding in a member initialization
        private KeyValuePair<IStonBindingKey, IStonEntity> ReadMemberBinding(StonTokenReader reader)
        {
            IStonBindingKey key;
            if (reader.Peek().HasChartype(StonChartype.CanunBegin | StonChartype.TextDelimiter | StonChartype.Extension)) key = ReadBindingName(reader);
            else if (reader.Peek().HasChartype(StonChartype.IndexOpen)) key = ReadBindingIndex(reader);
            else throw reader.MakeUnexpectedCharacterException(StonChartype.IndexOpen | StonChartype.CanunBegin | StonChartype.TextDelimiter | StonChartype.Extension);

            reader.ExpectAndSkip(StonChartype.ValuePrompt);
            IStonEntity value = ReadEntity(reader);

            return new KeyValuePair<IStonBindingKey, IStonEntity>(key, value);
        }

        // reads a binding key
        private IStonBindingName ReadBindingName(StonTokenReader reader)
        {
            string name = null;
            bool isExtension = false;

            if (reader.TryAndSkip(StonChartype.Extension)) isExtension = true;

            if (reader.Peek().HasChartype(StonChartype.TextDelimiter)) name = reader.ReadString();
            else if (reader.Peek().HasChartype(StonChartype.CanunBegin)) name = reader.ReadCanun();
            else throw reader.MakeUnexpectedCharacterException(StonChartype.TextDelimiter | StonChartype.CanunBegin);

            return ElementFactory.CreateBindingName(name, isExtension);
        }

        // reads a binding index
        private IStonBindingIndex ReadBindingIndex(StonTokenReader reader)
        {
            // just a sequence of entities
            reader.ExpectAndSkip(StonChartype.IndexOpen);
            IEnumerable<IStonEntity> indexParameters = ReadSequence(reader, ReadEntity, StonChartype.IndexClose);
            return ElementFactory.CreateBindingIndex(indexParameters);
        }

        // reads a collection initialization component of a complex value
        private IStonCollectionInit ReadCollectionInit(StonTokenReader reader, bool preStarted = false)
        {
            // just a sequence of entities
            if (!preStarted) reader.ExpectAndSkip(StonChartype.CollectionInitOpen);     // the opening token might have been read earlier when reading a bare type definition
            IEnumerable<IStonEntity> entities = ReadSequence(reader, ReadEntity, StonChartype.CollectionInitClose);
            return ElementFactory.CreateCollectionInit(entities);
        }

        #endregion

        #region Reading reference address

        // reads a STON reference entity address
        private IStonAddress ReadAddress(StonTokenReader reader)
        {
            IStonInitialContext initialContext = ReadInitialContext(reader);

            var path = new List<IStonPathSegment>();
            while (reader.Peek().HasChartype(StonChartype.PathSegmentBegin))
            {
                path.Add(ReadPathSegment(reader));
            }

            return ElementFactory.CreateAddress(initialContext, path);
        }

        // reads a STON address initial context
        private IStonInitialContext ReadInitialContext(StonTokenReader reader)
        {
            int addressOpen = reader.ExpectChartype(StonChartype.AddressBegin);

            // reference defining context
            if (addressOpen == '$')
            {
                reader.PeekSignificant();
                return ElementFactory.CreateAncestorInitialContext(0);
            }
            else if (addressOpen == '^')
            {
                // document core
                if (reader.Peek() == '*')
                {
                    reader.ReadAndSkip();
                    return ElementFactory.CreateGlobalEntityInitialContext("");
                }
                // reference defining context ancestor
                else
                {
                    int ancestorOrder = 1;
                    reader.PeekSignificant();
                    while (reader.TryAndSkip(StonChartype.AncestorAccess)) ++ancestorOrder;
                    return ElementFactory.CreateAncestorInitialContext(ancestorOrder);
                }
            }
            // globally identified entity
            else
            {
                reader.PeekSignificant();
                string globalIdentifier = reader.ReadCanun();
                return ElementFactory.CreateGlobalEntityInitialContext(globalIdentifier);
            }
        }

        // reads a STON address path segment
        private IStonPathSegment ReadPathSegment(StonTokenReader reader)
        {
            int pathOpen = reader.ExpectAndSkip(StonChartype.PathSegmentBegin);

            if (pathOpen == '.')
            {
                // ancestor access segment
                if (reader.TryAndSkip(StonChartype.AncestorAccess))
                {
                    int ancestorOrder = 1;
                    while (reader.TryAndSkip(StonChartype.AncestorAccess)) ++ancestorOrder;
                    return ElementFactory.CreateAncestorPathSegment(ancestorOrder);
                }
                // named member access segment
                else
                {
                    return ElementFactory.CreateMemberPathSegment(ReadBindingName(reader));
                }
            }
            else
            {
                // collection element access segment
                if (reader.TryAndSkip(StonChartype.PathSegmentCollection))
                {
                    int currentPosition = reader.Position;
                    int currentLine = reader.Line;
                    int currentColumn = reader.Column;

                    if (reader.Peek().HasChartype(StonChartype.Sign))
                    {
                        if (reader.Peek() == '-') throw new StonParsingException(currentPosition, currentLine, currentColumn, "Collection element index cannot be negative.");
                        reader.ReadAndSkip();
                        if (!reader.Peek().HasChartype(StonChartype.Digit)) throw reader.MakeUnexpectedCharacterException(StonChartype.Digit);
                    }

                    IStonEntity elementIndex;
                    // number or binary integer index
                    if (reader.Peek().HasChartype(StonChartype.ZeroDigit))
                    {
                        reader.ReadAndSkip();
                        if (reader.Peek().HasChartype(StonChartype.BaseIdentifier)) elementIndex = ElementFactory.CreateSimpleEntity(ElementFactory.CreateSimpleValue(StonDataType.Binary, reader.ReadBinaryContent(false)));
                        else elementIndex = ElementFactory.CreateSimpleEntity(ElementFactory.CreateSimpleValue(StonDataType.Number, reader.ReadNumberContent(false)));
                    }
                    // number integer index
                    else if (reader.Peek().HasChartype(StonChartype.NonZeroDigit))
                    {
                        elementIndex = ElementFactory.CreateSimpleEntity(ElementFactory.CreateSimpleValue(StonDataType.Number, reader.ReadNumberContent(false)));
                    }
                    // resolved reference index
                    else if (reader.Peek().HasChartype(StonChartype.AddressBegin)) elementIndex = ElementFactory.CreateReferenceEntity(ReadAddress(reader));
                    else throw reader.MakeUnexpectedCharacterException(StonChartype.Digit | StonChartype.AddressBegin | StonChartype.Sign);

                    reader.ExpectAndSkip(StonChartype.IndexClose);
                    return ElementFactory.CreateCollectionElementPathSegment(elementIndex);
                }
                // index member access segment
                else
                {
                    IEnumerable<IStonEntity> indexParameters = ReadSequence(reader, ReadAddressEntity, StonChartype.IndexClose);
                    return ElementFactory.CreateMemberPathSegment(ElementFactory.CreateBindingIndex(indexParameters));
                }
            }
        }

        #endregion
    }
}

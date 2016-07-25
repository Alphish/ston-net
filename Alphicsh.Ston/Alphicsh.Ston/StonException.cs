// what could possibly go wrong...?

// well, for example I could look up a page on TvTropes
// and end up coming back to the code nearly a week later
// after storming through archive of ~18 years old webcomic
// delaying the release of a proof-of-concept alpha package

// I'm never asking that question in comments again, and neither should you!

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston
{
    using Tokenization;

    /// <summary>
    /// A generic exception related to invalid handling of STON structures or text.
    /// </summary>
    public class StonException : Exception
    {
        /// <summary>
        /// Creates a new STON exception.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public StonException() : base("An error related to Specifically Typed Object Notation elements occurred.") { }

        /// <summary>
        /// Creates a new STON exception with a given error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        [ExcludeFromCodeCoverage]
        public StonException(string message) : base(message) { }

        /// <summary>
        /// Creates a new STON exception with a given error message and inner exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        [ExcludeFromCodeCoverage]
        public StonException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// The exception that is thrown when a given STON element implements a base STON interface without one of expected derived interfaces.
    /// </summary>
    public class StonImplementationException : StonException
    {
        /// <summary>
        /// The type that implements a base STON interface without any of its derived interfaces.
        /// </summary>
        public Type ElementType { get; private set; }

        /// <summary>
        /// The base STON interface that should be implemented with one of deriving interfaces.
        /// </summary>
        public Type BaseInterface { get; private set; }

        /// <summary>
        /// The deriving interfaces, one of which should be implemented with the base interface.
        /// </summary>
        public IEnumerable<Type> DerivedInterfaces { get; private set; }



        /// <summary>
        /// Creates a new STON implementation exception for a given type, base STON interface and derived interfaces.
        /// </summary>
        /// <param name="elementType">The type implementing the base interfaces.</param>
        /// <param name="baseInterface">The interface implemented by the type.</param>
        /// <param name="derivedInterfaces">The derives interfaces, one of which should be implemented.</param>
        [ExcludeFromCodeCoverage]
        public StonImplementationException(Type elementType, Type baseInterface, IEnumerable<Type> derivedInterfaces)
            : base($"The given STON element of type {elementType?.FullName} implements neither of expected derived interfaces for {baseInterface?.Name}. At least one of these must be implemented: {string.Join(", ", derivedInterfaces?.Select(t => t?.Name ?? "") ?? Enumerable.Empty<string>()) }.")
        {
            Init(elementType, baseInterface, derivedInterfaces);
        }

        /// <summary>
        /// Creates a new STON implementation exception for a given type, base STON interface and derived interfaces.
        /// </summary>
        /// <param name="elementType">The type implementing the base interfaces.</param>
        /// <param name="baseInterface">The interface implemented by the type.</param>
        /// <param name="derivedInterfaces">The derives interfaces, one of which should be implemented.</param>
        [ExcludeFromCodeCoverage]
        public StonImplementationException(Type elementType, Type baseInterface, params Type[] derivedInterfaces)
            : this(elementType, baseInterface, derivedInterfaces as IEnumerable<Type>) { }

        /// <summary>
        /// Creates a new STON implementation exception for a given type, base STON interface and derived interfaces, with a custom error message.
        /// </summary>
        /// <param name="elementType">The type implementing the base interfaces.</param>
        /// <param name="baseInterface">The interface implemented by the type.</param>
        /// <param name="derivedInterfaces">The derives interfaces, one of which should be implemented.</param>
        /// <param name="message">The error message.</param>
        [ExcludeFromCodeCoverage]
        public StonImplementationException(Type elementType, Type baseInterface, IEnumerable<Type> derivedInterfaces, string message)
            : base(message) { Init(elementType, baseInterface, derivedInterfaces); }

        /// <summary>
        /// Creates a new STON implementation exception for a given type, base STON interface and derived interfaces, with a custom error message and inner exception.
        /// </summary>
        /// <param name="elementType">The type implementing the base interfaces.</param>
        /// <param name="baseInterface">The interface implemented by the type.</param>
        /// <param name="derivedInterfaces">The derives interfaces, one of which should be implemented.</param>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        [ExcludeFromCodeCoverage]
        public StonImplementationException(Type elementType, Type baseInterface, IEnumerable<Type> derivedInterfaces, string message, Exception innerException)
            : base(message, innerException) { Init(elementType, baseInterface, derivedInterfaces); }



        // a function to initialize StonImplementationException properties passed in the constructor
        private void Init(Type elementType, Type baseInterface, IEnumerable<Type> derivedInterfaces)
        {
            if (elementType == null) throw new ArgumentNullException("elementType");
            if (baseInterface == null) throw new ArgumentNullException("baseInterface");
            if (derivedInterfaces == null) throw new ArgumentNullException("derivedInterfaces");
            if (derivedInterfaces.Contains(null)) throw new ArgumentException("The derived interfaces list cannot contain any null.");
            ElementType = elementType;
            BaseInterface = baseInterface;
            DerivedInterfaces = derivedInterfaces;
        }
    }

    #region Invalid syntax

    /// <summary>
    /// The exception that is thrown when a STON string is found to be invalid.
    /// </summary>
    public class StonParsingException : StonException
    {
        /// <summary>
        /// Gets the position in the string where the error occurred, or -1 if the position is unknown.
        /// </summary>
        public int Position { get; private set; }

        /// <summary>
        /// Gets the index of line in the string where the error occurred, or -1 if the line is unknown.
        /// </summary>
        public int Line { get; private set; }

        /// <summary>
        /// Gets the index of column in the string where the error occurred, or -1 if the column is unknown.
        /// </summary>
        public int Column { get; private set; }



        /// <summary>
        /// Creates a new STON parsing exception with no position specified.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public StonParsingException() : this(-1, -1, -1) { }

        /// <summary>
        /// Creates a new STON parsing exception with a given error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        [ExcludeFromCodeCoverage]
        public StonParsingException(string message) : this(-1, -1, -1, message) { }

        /// <summary>
        /// Creates a new STON parsing exception with a given error message and inner exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        [ExcludeFromCodeCoverage]
        public StonParsingException(string message, Exception innerException) : this(-1, -1, -1, message, innerException) { }

        /// <summary>
        /// Creates a new STON parsing exception for a given position in string.
        /// </summary>
        /// <param name="position">The position where the error occurred.</param>
        /// <param name="line">The line where the error occurred.</param>
        /// <param name="column">The column where the error occurred.</param>
        [ExcludeFromCodeCoverage]
        public StonParsingException(int position, int line, int column) : base("An error occurred when parsing a STON string.") { Init(position, line, column); }

        /// <summary>
        /// Creates a new STON parsing exception for a given position in string, with a given error message.
        /// </summary>
        /// <param name="position">The position where the error occurred.</param>
        /// <param name="line">The line where the error occurred.</param>
        /// <param name="column">The column where the error occurred.</param>
        /// <param name="message">The error message.</param>
        [ExcludeFromCodeCoverage]
        public StonParsingException(int position, int line, int column, string message) : base(message) { Init(position, line, column); }

        /// <summary>
        /// Creates a new STON parsing exception for a given position in string, with a given error message and inner exception.
        /// </summary>
        /// <param name="position">The position where the error occurred.</param>
        /// <param name="line">The line where the error occurred.</param>
        /// <param name="column">The column where the error occurred.</param>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        [ExcludeFromCodeCoverage]
        public StonParsingException(int position, int line, int column, string message, Exception innerException) : base(message, innerException) { Init(position, line, column); }



        // a function to initialize StonParsingException properties passed in the constructor
        private void Init(int position, int line, int column)
        {
            Position = position;
            Line = line;
            Column = column;
        }
    }

    /// <summary>
    /// The exception that is thrown when an unexpected character is found in a STON string.
    /// </summary>
    public class StonUnexpectedCharacterParsingException : StonParsingException
    {
        /// <summary>
        /// Gets the unexpected character found.
        /// </summary>
        public int Character { get; private set; }

        /// <summary>
        /// Gets the expected STON character type or combination of types.
        /// </summary>
        public StonChartype ExpectedType { get; private set; }



        /// <summary>
        /// Creates a new STON unexpected character exception for a given unexpected character and expected type.
        /// </summary>
        /// <param name="character">The unexpected character.</param>
        /// <param name="expectedType">The expected character type.</param>
        [ExcludeFromCodeCoverage]
        public StonUnexpectedCharacterParsingException(int character, StonChartype expectedType)
            : base(GetDefaultMessage(character, expectedType)) { Init(character, expectedType); }

        /// <summary>
        /// Creates a new STON unexpected character exception for a given unexpected character and expected type, with a given error message.
        /// </summary>
        /// <param name="character">The unexpected character.</param>
        /// <param name="expectedType">The expected character type.</param>
        /// <param name="message">The error message.</param>
        [ExcludeFromCodeCoverage]
        public StonUnexpectedCharacterParsingException(int character, StonChartype expectedType, string message)
            : base(message) { Init(character, expectedType); }

        /// <summary>
        /// Creates a new STON unexpected character exception for a given unexpected character and expected type, with a given error message and inner exception.
        /// </summary>
        /// <param name="character">The unexpected character.</param>
        /// <param name="expectedType">The expected character type.</param>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        [ExcludeFromCodeCoverage]
        public StonUnexpectedCharacterParsingException(int character, StonChartype expectedType, string message, Exception innerException)
            : base(message, innerException) { Init(character, expectedType); }

        /// <summary>
        /// Creates a new STON unexpected character exception for a given unexpected character at a specific position and expected type.
        /// </summary>
        /// <param name="position">The position where the error occurred.</param>
        /// <param name="line">The line where the error occurred.</param>
        /// <param name="column">The column where the error occurred.</param>
        /// <param name="character">The unexpected character.</param>
        /// <param name="expectedType">The expected character type.</param>
        [ExcludeFromCodeCoverage]
        public StonUnexpectedCharacterParsingException(int position, int line, int column, int character, StonChartype expectedType)
            : base(position, line, column, GetDefaultMessage(character, expectedType)) { Init(character, expectedType); }

        /// <summary>
        /// Creates a new STON unexpected character exception for a given unexpected character at a specific position and expected type, with a given error message.
        /// </summary>
        /// <param name="position">The position where the error occurred.</param>
        /// <param name="line">The line where the error occurred.</param>
        /// <param name="column">The column where the error occurred.</param>
        /// <param name="character">The unexpected character.</param>
        /// <param name="expectedType">The expected character type.</param>
        /// <param name="message">The error message.</param>
        [ExcludeFromCodeCoverage]
        public StonUnexpectedCharacterParsingException(int position, int line, int column, int character, StonChartype expectedType, string message)
            : base(position, line, column, message) { Init(character, expectedType); }

        /// <summary>
        /// Creates a new STON unexpected character exception for a given unexpected character at a specific position and expected type, with a given error message and inner exception.
        /// </summary>
        /// <param name="position">The position where the error occurred.</param>
        /// <param name="line">The line where the error occurred.</param>
        /// <param name="column">The column where the error occurred.</param>
        /// <param name="character">The unexpected character.</param>
        /// <param name="expectedType">The expected character type.</param>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        [ExcludeFromCodeCoverage]
        public StonUnexpectedCharacterParsingException(int position, int line, int column, int character, StonChartype expectedType, string message, Exception innerException)
            : base(position, line, column, message, innerException) { Init(character, expectedType); }



        private static readonly int[] SpanishCharacters = new int[]
        {
            193, 201, 205, 211, 218, 209, 220,
            225, 233, 237, 243, 250, 241, 252,
            191, 161
        };

        // builds a default exception message, based on the character and the expected types
        private static string GetDefaultMessage(int character, StonChartype expectedType)
        {
            // I am sorry, I had to
            if (SpanishCharacters.Contains(character)) return "Nobody expected the Spanish Inquisition.";

            var messageBuilder = new StringBuilder();

            if (character <= 0) messageBuilder.Append("Unexpected end of string encountered when parsing a STON string.");
            else messageBuilder.Append($"Unexpected character '{(char)character}' found when parsing a STON string.");

            if (expectedType != StonChartype.None)
            {
                messageBuilder.Append(" Expected character types: ");
                bool first = true;
                long remType = (long)expectedType;
                long matchType = 1;
                while (remType != 0)
                {
                    if ((remType & matchType) != 0)
                    {
                        messageBuilder.Append(first ? "" : ", ");
                        messageBuilder.Append((StonChartype)matchType);
                        remType ^= matchType;
                        first = false;
                    }
                    matchType <<= 1;
                }
                messageBuilder.Append('.');
            }

            return messageBuilder.ToString();
        }

        // a function to initialize StonUnexpectedCharacterParsingException properties passed in the constructor
        private void Init(int character, StonChartype expectedType)
        {
            Character = character;
            ExpectedType = expectedType;
        }
    }

    #endregion

    #region Invalid values

    /// <summary>
    /// The exception that is thrown when a STON entity has invalid value.
    /// </summary>
    public class StonValueException : StonException
    {
        /// <summary>
        /// Gets the entity with the invalid value.
        /// </summary>
        public IStonValuedEntity Entity { get; }

        /// <summary>
        /// Creates a new STON value exception for a given entity.
        /// </summary>
        /// <param name="entity">The entity with the invalid value.</param>
        [ExcludeFromCodeCoverage]
        public StonValueException(IStonValuedEntity entity) : base("The given entity has an invalid value.")
        {
            if (entity == null) throw new ArgumentNullException("entity");
            Entity = entity;
        }

        /// <summary>
        /// Creates a new STON value exception for a given entity, with a given error message.
        /// </summary>
        /// <param name="entity">The entity with the invalid value.</param>
        /// <param name="message">The error message.</param>
        [ExcludeFromCodeCoverage]
        public StonValueException(IStonValuedEntity entity, string message) : base(message)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            Entity = entity;
        }

        /// <summary>
        /// Creates a new STON value exception for a given entity, with a given error message and inner exception.
        /// </summary>
        /// <param name="entity">The entity with the invalid value.</param>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        [ExcludeFromCodeCoverage]
        public StonValueException(IStonValuedEntity entity, string message, Exception innerException) : base(message, innerException)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            Entity = entity;
        }
    }

    /// <summary>
    /// The exception that is thrown when a STON complex entity has duplicate members.
    /// </summary>
    public class StonDuplicateMemberException : StonValueException
    {
        /// <summary>
        /// Gets the entity with the duplicate members.
        /// </summary>
        new public IStonComplexEntity Entity => (this as StonValueException).Entity as IStonComplexEntity;

        /// <summary>
        /// Gets the document where the duplicate keys appear.
        /// </summary>
        public IStonDocument Document { get; private set; }

        /// <summary>
        /// Gets the first duplicate binding key, semantically equivalent to the second key.
        /// </summary>
        public IStonBindingKey FirstKey { get; private set; }

        /// <summary>
        /// Gets the second duplicate binding key, semantically equivalent to the first key.
        /// </summary>
        public IStonBindingKey SecondKey { get; private set; }



        /// <summary>
        /// Creates a new STON duplicate member exception for a given complex entity and duplicate member indices.
        /// </summary>
        /// <param name="entity">The entity with the duplicate members.</param>
        /// <param name="document">The document where the duplicate keys appear.</param>
        /// <param name="firstKey">The first duplicate member key.</param>
        /// <param name="secondKey">The second duplicate member key.</param>
        [ExcludeFromCodeCoverage]
        public StonDuplicateMemberException(IStonComplexEntity entity, IStonDocument document, IStonBindingKey firstKey, IStonBindingKey secondKey)
            : base(entity, "The given entity has multiple members with equivalent keys.") { Init(document, firstKey, secondKey); }

        /// <summary>
        /// Creates a new STON duplicate member exception for a given complex entity and duplicate member indices, with a given error message.
        /// </summary>
        /// <param name="entity">The entity with the duplicate members.</param>
        /// <param name="document">The document where the duplicate keys appear.</param>
        /// <param name="firstKey">The first duplicate member key.</param>
        /// <param name="secondKey">The second duplicate member key.</param>
        /// <param name="message">The error message.</param>
        [ExcludeFromCodeCoverage]
        public StonDuplicateMemberException(IStonComplexEntity entity, IStonDocument document, IStonBindingKey firstKey, IStonBindingKey secondKey, string message)
            : base(entity, message) { Init(document, firstKey, secondKey); }

        /// <summary>
        /// Creates a new STON duplicate member exception for a given complex entity and duplicate member indices, with a given error message and inner exception.
        /// </summary>
        /// <param name="entity">The entity with the duplicate members.</param>
        /// <param name="document">The document where the duplicate keys appear.</param>
        /// <param name="firstKey">The first duplicate member key.</param>
        /// <param name="secondKey">The second duplicate member key.</param>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        [ExcludeFromCodeCoverage]
        public StonDuplicateMemberException(IStonComplexEntity entity, IStonDocument document, IStonBindingKey firstKey, IStonBindingKey secondKey, string message, Exception innerException)
            : base(entity, message, innerException) { Init(document, firstKey, secondKey); }



        // a function to initialize StonDuplicateMemberException properties passed in the constructor
        private void Init(IStonDocument document, IStonBindingKey firstKey, IStonBindingKey secondKey)
        {
            Document = document;
            FirstKey = firstKey;
            SecondKey = secondKey;
        }
    }

    #endregion

    #region Invalid references

    /// <summary>
    /// The exception that is thrown when a STON reference entity is invalid.
    /// </summary>
    public class StonReferenceException : StonException
    {
        /// <summary>
        /// Gets the invalid reference entity.
        /// </summary>
        public IStonReferenceEntity Entity { get; }



        /// <summary>
        /// Creates a new STON reference exception for a given entity.
        /// </summary>
        /// <param name="entity">The invalid reference entity.</param>
        [ExcludeFromCodeCoverage]
        public StonReferenceException(IStonReferenceEntity entity) : base("The reference entity is invalid.")
        {
            if (entity == null) throw new ArgumentNullException("entity");
            Entity = entity;
        }

        /// <summary>
        /// Creates a new STON reference exception for a given entity, with a given error message.
        /// </summary>
        /// <param name="entity">The invalid reference entity.</param>
        /// <param name="message">The error message.</param>
        [ExcludeFromCodeCoverage]
        public StonReferenceException(IStonReferenceEntity entity, string message) : base(message)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            Entity = entity;
        }

        /// <summary>
        /// Creates a new STON reference exception for a given entity, with a given error message and inner exception.
        /// </summary>
        /// <param name="entity">The invalid reference entity.</param>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        [ExcludeFromCodeCoverage]
        public StonReferenceException(IStonReferenceEntity entity, string message, Exception innerException) : base(message, innerException)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            Entity = entity;
        }
    }

    /// <summary>
    /// The exception that is thrown when a STON reference entity points to no valid value.
    /// </summary>
    public class StonUnknownReferenceException : StonReferenceException
    {
        /// <summary>
        /// Gets the context in which the path segment was unresolved, or null if the initial context could not be found.
        /// </summary>
        public IStonValuedEntity Context { get; }

        /// <summary>
        /// Gets the relative path segment that could not be resolved, or null if the initial context could not be found.
        /// </summary>
        public IStonPathSegment UnresolvedSegment { get; }



        /// <summary>
        /// Creates a new STON unknown reference exception for a given entity, context and path segment.
        /// </summary>
        /// <param name="entity">The invalid reference entity.</param>
        /// <param name="context">The context in which the path segment was unresolved, or null if the initial context could not be found.</param>
        /// <param name="unresolvedSegment">The unresolved relative path segment, or null if the initial context is unresolved.</param>
        [ExcludeFromCodeCoverage]
        public StonUnknownReferenceException(IStonReferenceEntity entity, IStonValuedEntity context, IStonPathSegment unresolvedSegment)
            : base(entity, "The reference entity points to no valid value.") { Context = context; UnresolvedSegment = unresolvedSegment; }

        /// <summary>
        /// Creates a new STON unknown reference exception for a given entity and path segment, with a given error message.
        /// </summary>
        /// <param name="entity">The invalid reference entity.</param>
        /// <param name="context">The context in which the path segment was unresolved, or null if the initial context could not be found.</param>
        /// <param name="unresolvedSegment">The unresolved relative path segment, or null if the initial context is unresolved.</param>
        /// <param name="message">The error message.</param>
        [ExcludeFromCodeCoverage]
        public StonUnknownReferenceException(IStonReferenceEntity entity, IStonValuedEntity context, IStonPathSegment unresolvedSegment, string message)
            : base(entity, message) { Context = context; UnresolvedSegment = unresolvedSegment; }

        /// <summary>
        /// Creates a new STON unknown reference exception for a given entity and path segment, with a given error message and inner exception.
        /// </summary>
        /// <param name="entity">The invalid reference entity.</param>
        /// <param name="context">The context in which the path segment was unresolved, or null if the initial context could not be found.</param>
        /// <param name="unresolvedSegment">The unresolved relative path segment, or null if the initial context is unresolved.</param>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        [ExcludeFromCodeCoverage]
        public StonUnknownReferenceException(IStonReferenceEntity entity, IStonValuedEntity context, IStonPathSegment unresolvedSegment, string message, Exception innerException)
            : base(entity, message, innerException) { Context = context; UnresolvedSegment = unresolvedSegment; }
    }

    /// <summary>
    /// The exception that is thrown when a STON reference entity points to itself, directly or indirectly.
    /// </summary>
    public class StonCircularReferenceException : StonReferenceException
    {
        /// <summary>
        /// Gets the sequence of reference entities and their relative path segments that lead back to the invalid entity.
        /// I a given reference entity leads to the next reference through an initial context, its corresponding path segment is null.
        /// </summary>
        public IEnumerable<KeyValuePair<IStonReferenceEntity, IStonPathSegment>> ReferenceCycle { get; }



        /// <summary>
        /// Creates a new STON cyclic reference exception for a given entity and path segment.
        /// </summary>
        /// <param name="referenceCycle">The cycle of references leading back to the invalid entity.</param>
        [ExcludeFromCodeCoverage]
        public StonCircularReferenceException(IEnumerable<KeyValuePair<IStonReferenceEntity, IStonPathSegment>> referenceCycle)
            : base(referenceCycle.First().Key, "The reference entity points to itself, directly or indirectly. Check ReferenceCycle property for more details.") { ReferenceCycle = referenceCycle; }

        /// <summary>
        /// Creates a new STON cyclic reference exception for a given entity and path segment, with a given error message.
        /// </summary>
        /// <param name="referenceCycle">The cycle of references leading back to the invalid entity.</param>
        /// <param name="message">The error message.</param>
        [ExcludeFromCodeCoverage]
        public StonCircularReferenceException(IEnumerable<KeyValuePair<IStonReferenceEntity, IStonPathSegment>> referenceCycle, string message)
            : base(referenceCycle.First().Key, message) { ReferenceCycle = referenceCycle; }

        /// <summary>
        /// Creates a new STON cyclic reference exception for a given entity and path segment, with a given error message and inner exception.
        /// </summary>
        /// <param name="referenceCycle">The cycle of references leading back to the invalid entity.</param>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        [ExcludeFromCodeCoverage]
        public StonCircularReferenceException(IEnumerable<KeyValuePair<IStonReferenceEntity, IStonPathSegment>> referenceCycle, string message, Exception innerException)
            : base(referenceCycle.First().Key, message, innerException) { ReferenceCycle = referenceCycle; }
    }

    #endregion

    #region Invalid document structure

    /// <summary>
    /// The exception that is thrown when a STON document has two entities with the same global identifier.
    /// </summary>
    public class StonDuplicateGlobalEntityException : StonException
    {
        /// <summary>
        /// Gets the index of the first duplicate named parameter among all construction parameters.
        /// </summary>
        public IStonEntity FirstEntity { get; private set; }

        /// <summary>
        /// Gets the index of the second duplicate named parameter among all construction parameters.
        /// </summary>
        public IStonEntity SecondEntity { get; private set; }

        /// <summary>
        /// Gets the duplicate global identifier.
        /// </summary>
        public string GlobalIdentifier { get; private set; }



        /// <summary>
        /// Creates a new STON duplicate global entity exception for a given pair of entities.
        /// </summary>
        /// <param name="firstEntity">The first entity with the duplicate global identifier.</param>
        /// <param name="secondEntity">The second entity with the duplicate global identifier.</param>
        [ExcludeFromCodeCoverage]
        public StonDuplicateGlobalEntityException(IStonEntity firstEntity, IStonEntity secondEntity)
            : base(GetDefaultMessage(firstEntity, secondEntity)) { Init(firstEntity, secondEntity); }

        /// <summary>
        /// Creates a new STON duplicate global entity exception for a given pair of entities, with a given error message.
        /// </summary>
        /// <param name="firstEntity">The first entity with the duplicate global identifier.</param>
        /// <param name="secondEntity">The second entity with the duplicate global identifier.</param>
        /// <param name="message">The error message.</param>
        [ExcludeFromCodeCoverage]
        public StonDuplicateGlobalEntityException(IStonEntity firstEntity, IStonEntity secondEntity, string message)
            : base(message) { Init(firstEntity, secondEntity); }

        /// <summary>
        /// Creates a new STON duplicate global entity exception for a given pair of entities, with a given error message and inner exception.
        /// </summary>
        /// <param name="firstEntity">The first entity with the duplicate global identifier.</param>
        /// <param name="secondEntity">The second entity with the duplicate global identifier.</param>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        [ExcludeFromCodeCoverage]
        public StonDuplicateGlobalEntityException(IStonEntity firstEntity, IStonEntity secondEntity, string message, Exception innerException)
            : base(message, innerException) { Init(firstEntity, secondEntity); }



        // builds a default exception message, based on the entity and construction parameters indices
        private static string GetDefaultMessage(IStonEntity firstEntity, IStonEntity secondEntity)
        {
            if (firstEntity == null) throw new ArgumentNullException("firstEntity");
            if (secondEntity == null) throw new ArgumentNullException("secondEntity");
            var globalIdentifier = firstEntity.GlobalIdentifier;
            if (globalIdentifier != secondEntity.GlobalIdentifier) throw new ArgumentException("The given entities have different global identifiers.");
            if (globalIdentifier == null) throw new ArgumentException("The given entities have no global identifiers.");

            return $"The document has mutliple entities with \"{globalIdentifier}\" global identifier.";
        }

        // a function to initialize StonDuplicateConstructionParameterException properties passed in the constructor
        // as well as retrieve and verify the duplicate name
        private void Init(IStonEntity firstEntity, IStonEntity secondEntity)
        {
            if (firstEntity == null) throw new ArgumentNullException("firstEntity");
            if (secondEntity == null) throw new ArgumentNullException("secondEntity");
            FirstEntity = firstEntity;
            SecondEntity = secondEntity;

            GlobalIdentifier = FirstEntity.GlobalIdentifier;
            if (GlobalIdentifier != SecondEntity.GlobalIdentifier) throw new ArgumentException("The given entities have different global identifiers.");
            if (GlobalIdentifier == null) throw new ArgumentException("The given entities have no global identifiers.");
        }
    }

    /// <summary>
    /// The exception that is thrown when a STON document is found to have no valid construction order.
    /// </summary>
    public class StonCircularConstructionException : StonException
    {
        /// <summary>
        /// Gets the document where the construction cycle appears.
        /// </summary>
        public IStonDocument Document { get; private set; }

        /// <summary>
        /// Gets the value that is a part of the construction loop.
        /// </summary>
        public IStonComplexEntity ConstructedValue { get; private set; }

        /// <summary>
        /// Gets the sequence of values dependencies that leads back to the constructed value.
        /// The number indicates the position of the value in construction.
        /// </summary>
        public IEnumerable<KeyValuePair<IStonComplexEntity, int>> ConstructionCycle { get; private set; }



        /// <summary>
        /// Creates a new STON circular construction exception for a given construction cycle.
        /// </summary>
        /// <param name="document">The document where the construction cycle appears.</param>
        /// <param name="constructionCycle">The construction cycle that leads back to the constructed value.</param>
        [ExcludeFromCodeCoverage]
        public StonCircularConstructionException(IStonDocument document, IEnumerable<KeyValuePair<IStonComplexEntity, int>> constructionCycle)
            : base("The document has no valid construction order due to the construction dependency loop. Check ConstructionCycle property for more details.") { Init(document, constructionCycle); }

        /// <summary>
        /// Creates a new STON circular construction exception for a given construction cycle, with a given error message.
        /// </summary>
        /// <param name="document">The document where the construction cycle appears.</param>
        /// <param name="constructionCycle">The construction cycle that leads back to the constructed value.</param>
        /// <param name="message">The error message.</param>
        [ExcludeFromCodeCoverage]
        public StonCircularConstructionException(IStonDocument document, IEnumerable<KeyValuePair<IStonComplexEntity, int>> constructionCycle, string message)
            : base(message) { Init(document, constructionCycle); }

        /// <summary>
        /// Creates a new STON circular construction exception for a given construction cycle, with a given error message and inner exception.
        /// </summary>
        /// <param name="document">The document where the construction cycle appears.</param>
        /// <param name="constructionCycle">The construction cycle that leads back to the constructed value.</param>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        [ExcludeFromCodeCoverage]
        public StonCircularConstructionException(IStonDocument document, IEnumerable<KeyValuePair<IStonComplexEntity, int>> constructionCycle, string message, Exception innerException)
            : base(message, innerException) { Init(document, constructionCycle); }



        // a function to initialize StonCircularConstructionException cycle passed in the constructor
        // as well as retrieve the constructed value
        private void Init(IStonDocument document, IEnumerable<KeyValuePair<IStonComplexEntity, int>> constructionCycle)
        {
            if (document == null) throw new ArgumentNullException("document");
            Document = document;
            if (constructionCycle == null) throw new ArgumentNullException("constructionCycle");
            ConstructionCycle = constructionCycle;
            ConstructedValue = ConstructionCycle.First().Key;
        }
    }

    #endregion

    #region Invalid extensions

    /// <summary>
    /// The exception that is thrown when there is a problem with a STON extension type.
    /// </summary>
    public class StonExtensionTypeException : StonException
    {
        /// <summary>
        /// Gets the extension type that causes the problem.
        /// </summary>
        public IStonNamedType ExtensionType { get; private set; }

        /// <summary>
        /// Gets the entity related to the problem, if any.
        /// </summary>
        public IStonValuedEntity Entity { get; private set; }



        /// <summary>
        /// Creates a new STON extension type exception for a given extension type.
        /// </summary>
        /// <param name="extensionType">The extension type that causes the problem.</param>
        [ExcludeFromCodeCoverage]
        public StonExtensionTypeException(IStonNamedType extensionType)
            : base($"A problem has occurred with an extension type.") { Init(extensionType, null); }

        /// <summary>
        /// Creates a new STON extension type exception for a given extension type, with a given error message.
        /// </summary>
        /// <param name="extensionType">The extension type that causes the problem.</param>
        /// <param name="message">The error message.</param>
        [ExcludeFromCodeCoverage]
        public StonExtensionTypeException(IStonNamedType extensionType, string message)
            : base(message) { Init(extensionType, null); }

        /// <summary>
        /// Creates a new STON extension type exception for a given extension type, with a given error message and inner exception.
        /// </summary>
        /// <param name="extensionType">The extension type that causes the problem.</param>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        [ExcludeFromCodeCoverage]
        public StonExtensionTypeException(IStonNamedType extensionType, string message, Exception innerException)
            : base(message, innerException) { Init(extensionType, null); }



        /// <summary>
        /// Creates a new STON extension type exception for a given extension-typed entity.
        /// </summary>
        /// <param name="entity">The extension-typed entity related to the problem.</param>
        [ExcludeFromCodeCoverage]
        public StonExtensionTypeException(IStonValuedEntity entity)
            : base($"A problem has occurred with an extension type in a given entity.") { Init(entity); }

        /// <summary>
        /// Creates a new STON extension type exception for a given extension-typed entity, with a given error message.
        /// </summary>
        /// <param name="entity">The extension-typed entity related to the problem.</param>
        /// <param name="message">The error message.</param>
        [ExcludeFromCodeCoverage]
        public StonExtensionTypeException(IStonValuedEntity entity, string message)
            : base(message) { Init(entity); }

        /// <summary>
        /// Creates a new STON extension type exception for a given extension-typed entity, with a given error message and inner exception.
        /// </summary>
        /// <param name="entity">The extension-typed entity related to the problem.</param>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        [ExcludeFromCodeCoverage]
        public StonExtensionTypeException(IStonValuedEntity entity, string message, Exception innerException)
            : base(message, innerException) { Init(entity); }



        /// <summary>
        /// Creates a new STON extension type exception for a given extension type and related entity.
        /// </summary>
        /// <param name="extensionType">The extension type that causes the problem.</param>
        /// <param name="entity">The entity related to the problem.</param>
        [ExcludeFromCodeCoverage]
        public StonExtensionTypeException(IStonNamedType extensionType, IStonValuedEntity entity)
            : base($"A problem has occurred with an extension type and an entity.") { Init(extensionType, entity); }

        /// <summary>
        /// Creates a new STON extension type exception for a given extension type and related entity, with a given error message.
        /// </summary>
        /// <param name="extensionType">The extension type that causes the problem.</param>
        /// <param name="entity">The entity related to the problem.</param>
        /// <param name="message">The error message.</param>
        [ExcludeFromCodeCoverage]
        public StonExtensionTypeException(IStonNamedType extensionType, IStonValuedEntity entity, string message)
            : base(message) { Init(extensionType, entity); }

        /// <summary>
        /// Creates a new STON extension type exception for a given extension type and related entity, with a given error message and inner exception.
        /// </summary>
        /// <param name="extensionType">The extension type that causes the problem.</param>
        /// <param name="entity">The entity related to the problem.</param>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        [ExcludeFromCodeCoverage]
        public StonExtensionTypeException(IStonNamedType extensionType, IStonValuedEntity entity, string message, Exception innerException)
            : base(message, innerException) { Init(extensionType, entity); }



        // a function to initialize StonExtensionTypeException for a given entity
        private void Init(IStonValuedEntity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            if (!(entity.Type is IStonNamedType) || !(entity.Type as IStonNamedType).IsExtension) throw new ArgumentException("The given entity has no extension type.");
            Init(entity.Type as IStonNamedType, entity);
        }

        // a function to initialize StonExtensionTypeException for a given type and entity
        private void Init(IStonNamedType extensionType, IStonValuedEntity entity)
        {
            if (extensionType == null) throw new ArgumentNullException("extensionType");
            if (!extensionType.IsExtension) throw new ArgumentException("The given type is not an extension type.");

            ExtensionType = extensionType;
            Entity = entity;
        }
    }

    /// <summary>
    /// The exception that is thrown when a STON extension type is not a know document-side or application-side extension type.
    /// </summary>
    public class StonUnknownExtensionTypeException : StonExtensionTypeException
    {
        /// <summary>
        /// Creates a new STON unknown extension type exception for a given extension type.
        /// </summary>
        /// <param name="extensionType">The unknown extension type.</param>
        [ExcludeFromCodeCoverage]
        public StonUnknownExtensionTypeException(IStonNamedType extensionType)
            : base(extensionType, $"The extension type \"{extensionType?.Name}\" is not a recognized document-side or application-side extension type.") { }

        /// <summary>
        /// Creates a new STON unknown extension type exception for a given extension type, with a given error message.
        /// </summary>
        /// <param name="extensionType">The unknown extension type.</param>
        /// <param name="message">The error message.</param>
        [ExcludeFromCodeCoverage]
        public StonUnknownExtensionTypeException(IStonNamedType extensionType, string message)
            : base(extensionType, message) { }

        /// <summary>
        /// Creates a new STON unknown extension type exception for a given extension type, with a given error message and inner exception.
        /// </summary>
        /// <param name="extensionType">The unknown extension type.</param>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        [ExcludeFromCodeCoverage]
        public StonUnknownExtensionTypeException(IStonNamedType extensionType, string message, Exception innerException)
            : base(extensionType, message, innerException) { }
    }

    /// <summary>
    /// The exception that is thrown when there is a problem with a STON extension member.
    /// </summary>
    public class StonExtensionMemberException : StonException
    {
        /// <summary>
        /// Gets the extension member name that causes the problem.
        /// </summary>
        public IStonBindingName MemberKey { get; private set; }

        /// <summary>
        /// Gets the value of the extension member, or null if the value is irrelevant.
        /// </summary>
        public IStonEntity MemberValue { get; private set; }



        /// <summary>
        /// Creates a new STON extension member exception for a given extension member name.
        /// </summary>
        /// <param name="memberKey">The extension member name that causes the problem.</param>
        [ExcludeFromCodeCoverage]
        public StonExtensionMemberException(IStonBindingName memberKey)
            : base($"A problem has occurred with an extension type.") { Init(memberKey, null); }

        /// <summary>
        /// Creates a new STON extension member exception for a given extension member name, with a given error message.
        /// </summary>
        /// <param name="memberKey">The extension member name that causes the problem.</param>
        /// <param name="message">The error message.</param>
        [ExcludeFromCodeCoverage]
        public StonExtensionMemberException(IStonBindingName memberKey, string message)
            : base(message) { Init(memberKey, null); }

        /// <summary>
        /// Creates a new STON extension member exception for a given extension member name, with a given error message and inner exception.
        /// </summary>
        /// <param name="memberKey">The extension member name that causes the problem.</param>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        [ExcludeFromCodeCoverage]
        public StonExtensionMemberException(IStonBindingName memberKey, string message, Exception innerException)
            : base(message, innerException) { Init(memberKey, null); }



        /// <summary>
        /// Creates a new STON extension member exception for a given extension member.
        /// </summary>
        /// <param name="memberKey">The extension member that causes the problem.</param>
        /// <param name="memberValue">The value of the extension member.</param>
        [ExcludeFromCodeCoverage]
        public StonExtensionMemberException(IStonBindingName memberKey, IStonEntity memberValue)
            : base($"A problem has occurred with an extension type and an entity.") { Init(memberKey, memberValue); }

        /// <summary>
        /// Creates a new STON extension member exception for a given extension member, with a given error message.
        /// </summary>
        /// <param name="memberKey">The extension member that causes the problem.</param>
        /// <param name="memberValue">The value of the extension member.</param>
        /// <param name="message">The error message.</param>
        [ExcludeFromCodeCoverage]
        public StonExtensionMemberException(IStonBindingName memberKey, IStonEntity memberValue, string message)
            : base(message) { Init(memberKey, memberValue); }

        /// <summary>
        /// Creates a new STON extension member exception for a given extension member, with a given error message and inner exception.
        /// </summary>
        /// <param name="memberKey">The extension member that causes the problem.</param>
        /// <param name="memberValue">The value of the extension member.</param>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        [ExcludeFromCodeCoverage]
        public StonExtensionMemberException(IStonBindingName memberKey, IStonEntity memberValue, string message, Exception innerException)
            : base(message, innerException) { Init(memberKey, memberValue); }



        // a function to initialize StonExtensionTypeException for a given extension member
        private void Init(IStonBindingName memberKey, IStonEntity memberValue)
        {
            if (memberKey == null) throw new ArgumentNullException("memberKey");
            if (!memberKey.IsExtension) throw new ArgumentException("The given member name is not an extension member name.");

            MemberKey = memberKey;
            MemberValue = memberValue;
        }
    }

    /// <summary>
    /// The exception that is thrown when a STON extension member is not a know document-side or application-side extension member.
    /// </summary>
    public class StonUnknownExtensionMemberException : StonExtensionMemberException
    {
        /// <summary>
        /// Creates a new STON unknown extension member exception for a given extension member.
        /// </summary>
        /// <param name="extensionMember">The unknown extension member.</param>
        [ExcludeFromCodeCoverage]
        public StonUnknownExtensionMemberException(IStonBindingName extensionMember)
            : base(extensionMember, $"The extension member \"{extensionMember?.Name}\" is not a recognized document-side or application-side extension member.") { }

        /// <summary>
        /// Creates a new STON unknown extension member exception for a given extension member, with a given error message.
        /// </summary>
        /// <param name="extensionMember">The unknown extension member.</param>
        /// <param name="message">The error message.</param>
        [ExcludeFromCodeCoverage]
        public StonUnknownExtensionMemberException(IStonBindingName extensionMember, string message)
            : base(extensionMember, message) { }

        /// <summary>
        /// Creates a new STON unknown extension member exception for a given extension member, with a given error message and inner exception.
        /// </summary>
        /// <param name="extensionMember">The unknown extension member.</param>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        [ExcludeFromCodeCoverage]
        public StonUnknownExtensionMemberException(IStonBindingName extensionMember, string message, Exception innerException)
            : base(extensionMember, message, innerException) { }
    }

    #endregion
}

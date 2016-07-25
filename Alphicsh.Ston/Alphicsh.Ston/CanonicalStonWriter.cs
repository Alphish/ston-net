using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace Alphicsh.Ston
{
    using Tokenization;

    /// <summary>
    /// Provides the functionality of representing a STON structure in its canonical form.
    /// </summary>
    public sealed class CanonicalStonWriter : IStonWriter
    {
        /// <summary>
        /// Provides a canonical STON writer functionality.
        /// </summary>
        public static IStonWriter Instance { get; } = new CanonicalStonWriter();
        private CanonicalStonWriter() { }

        /// <summary>
        /// Gets the StonFormtat.CanonicalSton value, to indicate the writer writes canonical STON strings.
        /// </summary>
        public StonFormat OutputFormat => StonFormat.CanonicalSton;

        /// <summary>
        /// Writes a STON entity to a given text writer.
        /// </summary>
        /// <param name="writer">The writer to write the entity to.</param>
        /// <param name="entity">The entity to write.</param>
        public void WriteEntity(TextWriter writer, IStonEntity entity)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            if (entity == null) throw new ArgumentNullException("entity");
            WriteEntity(new StonTokenWriter(writer), entity);
        }

        /// <summary>
        /// Writes a STON document to a given text writer.
        /// </summary>
        /// <param name="writer">The writer to write the document to.</param>
        /// <param name="document">The document to write.</param>
        public void WriteDocument(TextWriter writer, IStonDocument document)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            if (document == null) throw new ArgumentNullException("document");
            WriteEntity(writer, document.Core);
        }

        // writes a canonical string literal
        private void WriteStringLiteral(StonTokenWriter writer, string content)
        {
            if (content == null) throw new StonException("A non-existing string has been found in the structure to be written.");
            else writer.WriteStringLiteral(content);
        }

        // writes a sequence of elements, using a given element writing method
        private void WriteSequence<TElement>(StonTokenWriter writer, IEnumerable<TElement> elements, Action<StonTokenWriter, TElement> elementWriting)
        {
            bool first = true;
            foreach (var element in elements)
            {
                if (!first) writer.Write(',');
                elementWriting(writer, element);
                first = false; 
            }
        }

        #region Entity writing

        // writes any entity
        private void WriteEntity(StonTokenWriter writer, IStonEntity entity)
        {
            if (entity == null) throw new StonException("A non-existing entity has been found in the structure to be written.");
            else if (entity is IStonValuedEntity) WriteEntity(writer, entity as IStonValuedEntity);
            else if (entity is IStonReferenceEntity) WriteEntity(writer, entity as IStonReferenceEntity);
            else throw new StonImplementationException(entity.GetType(), typeof(IStonEntity), typeof(IStonValuedEntity), typeof(IStonReferenceEntity));
        }

        // writer a valued entity
        private void WriteEntity(StonTokenWriter writer, IStonValuedEntity entity)
        {
            if (entity is IStonSimpleEntity) WriteEntity(writer, entity as IStonSimpleEntity);
            else if (entity is IStonComplexEntity) WriteEntity(writer, entity as IStonComplexEntity);
            else throw new StonImplementationException(entity.GetType(), typeof(IStonValuedEntity), typeof(IStonSimpleEntity), typeof(IStonComplexEntity));
        }

        // writes a simple valued entity
        private void WriteEntity(StonTokenWriter writer, IStonSimpleEntity entity)
        {
            WriteGlobalIdentifier(writer, entity.GlobalIdentifier);
            WriteTypeDefinition(writer, entity.Type);
            WriteSimpleValue(writer, entity.Value);
        }

        // writes a complex valued entity
        private void WriteEntity(StonTokenWriter writer, IStonComplexEntity entity)
        {
            WriteGlobalIdentifier(writer, entity.GlobalIdentifier);
            WriteTypeDefinition(writer, entity.Type);

            WriteConstruction(writer, entity.Construction);
            WriteMemberInit(writer, entity.MemberInit);
            WriteCollectionInit(writer, entity.CollectionInit);
        }

        // writes a reference entity
        private void WriteEntity(StonTokenWriter writer, IStonReferenceEntity entity)
        {
            WriteGlobalIdentifier(writer, entity.GlobalIdentifier);
            WriteReferenceAddress(writer, entity.Address);
        }

        #endregion

        #region Global identifier writing

        // writes a global identifier, or nothing if the identifier is not present
        private void WriteGlobalIdentifier(StonTokenWriter writer, string globalIdentifier)
        {
            if (globalIdentifier == null) return;
            else writer.Write("&" + globalIdentifier + "=");
        }

        #endregion

        #region Type definition writing

        // writes a wrapped type definition for a given type
        private void WriteTypeDefinition(StonTokenWriter writer, IStonType type)
        {
            if (type == null) return;
            writer.Write('<');
            WriteType(writer, type);
            writer.Write('>');
        }

        // writes any type
        private void WriteType(StonTokenWriter writer, IStonType type)
        {
            if (type == null) throw new StonException("A non-existing type has been found in the structure to be written.");
            else if (type is IStonNamedType) WriteType(writer, type as IStonNamedType);
            else if (type is IStonCollectionType) WriteType(writer, type as IStonCollectionType);
            else if (type is IStonUnionType) WriteType(writer, type as IStonUnionType);
            else throw new StonImplementationException(type.GetType(), typeof(IStonType), typeof(IStonNamedType), typeof(IStonCollectionType), typeof(IStonUnionType));
        }

        // writes a named type
        private void WriteType(StonTokenWriter writer, IStonNamedType type)
        {
            if (type.TypeParameters == null) throw new StonException("A named type cannot have a non-existing type parameters collection.");

            if (type.IsExtension) writer.Write('!');
            WriteStringLiteral(writer, type.Name);
            if (type.TypeParameters.Any())
            {
                writer.Write('<');
                WriteSequence(writer, type.TypeParameters, WriteType);
                writer.Write('>');
            }
        }

        // writes a colleciton type
        private void WriteType(StonTokenWriter writer, IStonCollectionType type)
        {
            if (type.ElementType is IStonUnionType) writer.Write('<');
            WriteType(writer, type.ElementType);
            if (type.ElementType is IStonUnionType) writer.Write('>');
            writer.Write("[]");
        }

        // writes a union type
        private void WriteType(StonTokenWriter writer, IStonUnionType type)
        {
            bool first = true;
            foreach (var permittedType in type.PermittedTypes)
            {
                if (!first) writer.Write('|');
                if (permittedType is IStonUnionType) writer.Write('<');
                WriteType(writer, permittedType);
                if (permittedType is IStonUnionType) writer.Write('>');
                first = false;
            }
        }

        #endregion

        #region Simple value writing

        // writes a simple value, depending on its data type
        private void WriteSimpleValue(StonTokenWriter writer, IStonSimpleValue value)
        {
            if (value == null) throw new StonException("A simple-valued entity cannot have a non-existing value.");
            switch (value.DataType)
            {
                case StonDataType.Null:
                    writer.Write("null");
                    break;
                case StonDataType.Number:
                    writer.WriteCanonicalNumberLiteral(value.Content);
                    break;
                case StonDataType.Binary:
                    writer.WriteBinaryLiteral(value.Content);
                    break;
                case StonDataType.Named:
                    writer.Write(value.Content);
                    break;
                case StonDataType.Text:
                    writer.WriteStringLiteral(value.Content, '"');
                    break;
                case StonDataType.Code:
                    writer.WriteStringLiteral(value.Content, '`');
                    break;
                default:
                    throw new StonException($"Unknown simple value data type: { (int)value.DataType }.");
            }
        }

        #endregion

        #region Complex value writing

        // writes a construction component of a complex value
        private void WriteConstruction(StonTokenWriter writer, IStonConstruction construction)
        {
            if (construction == null) return;
            if (construction.PositionalParameters == null) throw new StonException("A complex value construction cannot have a non-existing positional parameters collection.");
            if (construction.NamedParameters == null) throw new StonException("A complex value construction cannot have a non-existing named parameters collection.");

            writer.Write('(');

            // writing positional parameters
            WriteSequence(
                writer,
                construction.PositionalParameters,
                (w, p) => { w.Write(':'); WriteEntity(w, p); }
                );

            // a comma between positional and named parameters, if both types of parameters are present
            // it is necessary, because positional parameters sequence and named parameters sequence are written separately
            if (construction.PositionalParameters.Any() && construction.NamedParameters.Any()) writer.Write(',');

            // writing named parameters
            WriteSequence(
                writer,
                construction.NamedParameters,
                (w, p) => {
                    WriteStringLiteral(w, p.Key);
                    w.Write(':');
                    WriteEntity(w, p.Value);
                    }
                );

            writer.Write(')');
        }

        // writes a member initialization component of a complex value
        private void WriteMemberInit(StonTokenWriter writer, IStonMemberInit memberInit)
        {
            if (memberInit == null) return;
            if (memberInit.MemberBindings == null) throw new StonException("A complex value member initialization cannot have a non-existing member bindings collection.");

            writer.Write('{');

            WriteSequence(
                writer,
                memberInit.MemberBindings,
                (w, mb) => { WriteBindingKey(w, mb.Key); w.Write(':'); WriteEntity(w, mb.Value); }
                );

            writer.Write('}');
        }

        // writes any binding key
        private void WriteBindingKey(StonTokenWriter writer, IStonBindingKey bindingKey)
        {
            if (bindingKey == null) throw new StonException("A non-existing member binding key has been found in the structure to be written.");
            else if (bindingKey is IStonBindingName) WriteBindingKey(writer, bindingKey as IStonBindingName);
            else if (bindingKey is IStonBindingIndex) WriteBindingKey(writer, bindingKey as IStonBindingIndex);
            else throw new StonImplementationException(bindingKey.GetType(), typeof(IStonBindingKey), typeof(IStonBindingName), typeof(IStonBindingIndex));
        }

        // writes a binding name
        private void WriteBindingKey(StonTokenWriter writer, IStonBindingName bindingKey)
        {
            if (bindingKey.IsExtension) writer.Write('!');
            WriteStringLiteral(writer, bindingKey.Name);
        }

        // writes a binding index
        private void WriteBindingKey(StonTokenWriter writer, IStonBindingIndex bindingKey)
        {
            if (bindingKey.Parameters == null) throw new StonException("A member binding index cannot have non-existing parameters.");

            writer.Write('[');
            WriteSequence(writer, bindingKey.Parameters, WriteEntity);
            writer.Write(']');
        }

        // writes a collection inititialization component of a complex value
        private void WriteCollectionInit(StonTokenWriter writer, IStonCollectionInit collectionInit)
        {
            if (collectionInit == null) return;
            if (collectionInit.Elements == null) throw new StonException("A complex value collection initialization cannot have a non-existing elements collection.");

            writer.Write('[');
            WriteSequence(writer, collectionInit.Elements, WriteEntity);
            writer.Write(']');
        }

        #endregion

        #region Reference address writing

        // writes a reference address
        private void WriteReferenceAddress(StonTokenWriter writer, IStonAddress address)
        {
            if (address.InitialContext == null) throw new StonException("A reference address cannot have a non-existing initial context.");
            if (address.RelativePath == null) throw new StonException("A reference address cannot have a non-existing relative path.");

            WriteInitialContext(writer, address.InitialContext);
            foreach (var segment in address.RelativePath) WritePathSegment(writer, segment);
        }

        // writes any initial context
        private void WriteInitialContext(StonTokenWriter writer, IStonInitialContext context)
        {
            if (context is IStonAncestorInitialContext) WriteInitialContext(writer, context as IStonAncestorInitialContext);
            else if (context is IStonGlobalEntityInitialContext) WriteInitialContext(writer, context as IStonGlobalEntityInitialContext);
            else throw new StonImplementationException(context.GetType(), typeof(IStonInitialContext), typeof(IStonAncestorInitialContext), typeof(IStonGlobalEntityInitialContext));
        }

        // writes a reference defining initial context or an ancestor initial context
        private void WriteInitialContext(StonTokenWriter writer, IStonAncestorInitialContext context)
        {
            if (context.AncestorOrder == 0) writer.Write('$');
            else writer.Write(new string('^', context.AncestorOrder));
        }

        // writes a document core initial context or a globally identified entity initial context
        private void WriteInitialContext(StonTokenWriter writer, IStonGlobalEntityInitialContext context)
        {
            if (context.GlobalIdentifier == null) throw new StonException("A global entity initial context must have an existing global identifier.");
            else if (context.GlobalIdentifier == "") writer.Write("^*");
            else writer.Write("@" + context.GlobalIdentifier);
        }

        // writes any path segment
        private void WritePathSegment(StonTokenWriter writer, IStonPathSegment segment)
        {
            if (segment == null) throw new StonException("A non-existing path segment has been found in the structure to be written.");
            else if (segment is IStonAncestorPathSegment) WritePathSegment(writer, segment as IStonAncestorPathSegment);
            else if (segment is IStonMemberPathSegment) WritePathSegment(writer, segment as IStonMemberPathSegment);
            else if (segment is IStonCollectionElementPathSegment) WritePathSegment(writer, segment as IStonCollectionElementPathSegment);
            else throw new StonImplementationException(segment.GetType(), typeof(IStonPathSegment), typeof(IStonAncestorPathSegment), typeof(IStonMemberPathSegment), typeof(IStonCollectionElementPathSegment));
        }

        // writes an ancestor access path segment
        private void WritePathSegment(StonTokenWriter writer, IStonAncestorPathSegment segment)
        {
            writer.Write('.');
            writer.Write(new string('^', segment.AncestorOrder));
        }

        // writes a named member access path segment or an indexed member access path segment
        private void WritePathSegment(StonTokenWriter writer, IStonMemberPathSegment segment)
        {
            if (segment.BindingKey is IStonBindingName) writer.Write('.');
            WriteBindingKey(writer, segment.BindingKey);
        }

        // writes a collection element access path segment
        private void WritePathSegment(StonTokenWriter writer, IStonCollectionElementPathSegment segment)
        {
            writer.Write("[#");
            WriteEntity(writer, segment.ElementIndex);
            writer.Write(']');
        }

        #endregion
    }
}

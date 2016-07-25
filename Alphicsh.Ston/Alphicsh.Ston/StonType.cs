using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston
{
    /// <summary>
    /// Serves as a base class for STON type classes.
    /// </summary>
    public abstract class StonType : IStonType
    {
        /// <summary>
        /// Creates a structurally equivalent type from a given type.
        /// </summary>
        /// <param name="type">The type to copy the structure of.</param>
        /// <returns>A structurally equivalent copy of the given type.</returns>
        public static IStonType Copy(IStonType type)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (type is IStonNamedType) return StonNamedType.Copy(type as IStonNamedType);
            if (type is IStonCollectionType) return StonCollectionType.Copy(type as IStonCollectionType);
            if (type is IStonUnionType) return StonUnionType.Copy(type as IStonUnionType);
            throw new StonImplementationException(type.GetType(), typeof(IStonType), typeof(IStonNamedType), typeof(IStonCollectionType), typeof(IStonUnionType));
        }
    }

    /// <summary>
    /// Represents a STON named type, regular or extension.
    /// </summary>
    public sealed class StonNamedType : StonType, IStonNamedType
    {
        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the inner type parameters of the type.
        /// </summary>
        public IEnumerable<IStonType> TypeParameters { get; }

        /// <summary>
        /// Indicates whether the type is extension type or regular type.
        /// </summary>
        public bool IsExtension { get; }

        /// <summary>
        /// Creates a new STON named type, with a given name and optional sequence of type parameters, regular or extension.
        /// </summary>
        /// <param name="name">The name of the type.</param>
        /// <param name="typeParameters">The sequence of type parameters</param>
        /// <param name="isExtension">Whether the type is an extension type or a regular type.</param>
        public StonNamedType(string name, IEnumerable<IStonType> typeParameters = null, bool isExtension = false)
        {
            if (name == null) throw new ArgumentNullException("name");
            Name = name;
            TypeParameters = typeParameters?.Select(type => StonType.Copy(type)).ToList() ?? Enumerable.Empty<IStonType>();
            IsExtension = isExtension;
        }

        /// <summary>
        /// Creates a structurally equivalent named type from a given type.
        /// </summary>
        /// <param name="type">The type to copy the structure of.</param>
        public StonNamedType(IStonNamedType type)
            : this(type.Name, type.TypeParameters, type.IsExtension) { }

        /// <summary>
        /// Creates a structurally equivalent named type from a given type.
        /// </summary>
        /// <param name="type">The type to copy the structure of.</param>
        /// <returns>A structurally equivalent copy of the given type.</returns>
        public static IStonNamedType Copy(IStonNamedType type)
        {
            if (type == null) throw new ArgumentNullException("type");
            return new StonNamedType(type);
        }

        // override for general debugging purposes
        // it is not necessarily meant as a valid STON representation
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return (IsExtension ? "!" : "") + Name + (TypeParameters.Count() > 0 ? "<" + string.Join(",", TypeParameters.Select(t => t.ToString())) + ">" : "");
        }
    }

    /// <summary>
    /// Represents a STON collection type with specific element type.
    /// </summary>
    public sealed class StonCollectionType : StonType, IStonCollectionType
    {
        /// <summary>
        /// Gets the element type of the collection.
        /// </summary>
        public IStonType ElementType { get; }

        /// <summary>
        /// Creates a new STON collection type, with a given collection element type.
        /// </summary>
        /// <param name="elementType">The type of the collection elements.</param>
        public StonCollectionType(IStonType elementType)
        {
            if (elementType == null) throw new ArgumentNullException("elementType");
            ElementType = StonType.Copy(elementType);
        }

        /// <summary>
        /// Creates a structurally equivalent collection type from a given type.
        /// </summary>
        /// <param name="type">The type to copy the structure of.</param>
        public StonCollectionType(IStonCollectionType type)
            : this(type.ElementType) { }

        /// <summary>
        /// Creates a structurally equivalent collection type from a given type.
        /// </summary>
        /// <param name="type">The type to copy the structure of.</param>
        /// <returns>A structurally equivalent copy of the given type.</returns>
        public static IStonCollectionType Copy(IStonCollectionType type)
        {
            if (type == null) throw new ArgumentNullException("type");
            return new StonCollectionType(type);
        }

        // override for general debugging purposes
        // it is not necessarily meant as a valid STON representation
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return ElementType + "[]";
        }
    }

    /// <summary>
    /// Represents a STON union type with a list of permitted types.
    /// </summary>
    public sealed class StonUnionType : StonType, IStonUnionType
    {
        /// <summary>
        /// Gets the permitted types of the union type.
        /// </summary>
        public IEnumerable<IStonType> PermittedTypes { get; }

        /// <summary>
        /// Creates a new STON union type, with a given sequence of permitted types.
        /// </summary>
        /// <param name="permittedTypes">The sequence of permitted types.</param>
        public StonUnionType(IEnumerable<IStonType> permittedTypes)
        {
            if (permittedTypes == null) throw new ArgumentNullException("permittedTypes");
            PermittedTypes = permittedTypes.Select(type => StonType.Copy(type)).ToList();
        }

        /// <summary>
        /// Creates a structurally equivalent union type from a given type.
        /// </summary>
        /// <param name="type">The type to copy the structure of.</param>
        public StonUnionType(IStonUnionType type)
            : this(type.PermittedTypes) { }

        /// <summary>
        /// Creates a structurally equivalent union type from a given type.
        /// </summary>
        /// <param name="type">The type to copy the structure of.</param>
        /// <returns>A structurally equivalent copy of the given type.</returns>
        public static IStonUnionType Copy(IStonUnionType type)
        {
            if (type == null) throw new ArgumentNullException("type");
            return new StonUnionType(type);
        }

        // override for general debugging purposes
        // it is not necessarily meant as a valid STON representation
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return string.Join("|", PermittedTypes.Select(t => t.ToString()));
        }
    }
}

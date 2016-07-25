using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston
{
    /// <summary>
    /// Represents a STON type.
    /// It should be implemented either through IStonNamedType, IStonCollectionType or IStonUnionType.
    /// </summary>
    public interface IStonType { }

    /// <summary>
    /// Represents a STON named type, regular or extension.
    /// </summary>
    public interface IStonNamedType : IStonType
    {
        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the inner type parameters of the type.
        /// </summary>
        IEnumerable<IStonType> TypeParameters { get; }

        /// <summary>
        /// Indicates whether the type is extension type or regular type.
        /// </summary>
        bool IsExtension { get; }
    }

    /// <summary>
    /// Represents a STON collection type with specific element type.
    /// </summary>
    public interface IStonCollectionType : IStonType
    {
        /// <summary>
        /// Gets the element type of the collection.
        /// </summary>
        IStonType ElementType { get; }
    }

    /// <summary>
    /// Represents a STON union type with a list of permitted types.
    /// </summary>
    public interface IStonUnionType : IStonType
    {
        /// <summary>
        /// Gets the permitted types of the union type.
        /// </summary>
        IEnumerable<IStonType> PermittedTypes { get; }
    }
}

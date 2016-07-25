using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston
{
    /// <summary>
    /// Represents a relative path segment of a STON address.
    /// It should be implemented either through IStonAncestorPathSegment, IStonMemberPathSegment or IStonCollectionElementPathSegment.
    /// </summary>
    public interface IStonPathSegment { }

    /// <summary>
    /// Represents a STON address ancestor path segment with a given order.
    /// </summary>
    public interface IStonAncestorPathSegment : IStonPathSegment
    {
        /// <summary>
        /// Gets the order of the ancestor.
        /// </summary>
        int AncestorOrder { get; }
    }

    /// <summary>
    /// Represents a STON address member path segment with a given name or index.
    /// It is possible to represent regular and extension named members alike.
    /// </summary>
    public interface IStonMemberPathSegment : IStonPathSegment
    {
        /// <summary>
        /// Gets the member binding key.
        /// </summary>
        IStonBindingKey BindingKey { get; }
    }

    /// <summary>
    /// Represents a STON address collection element path segment with a given index.
    /// </summary>
    public interface IStonCollectionElementPathSegment : IStonPathSegment
    {
        /// <summary>
        /// Gets the entity representing the index of the element in collection.
        /// </summary>
        IStonEntity ElementIndex { get; }
    }
}

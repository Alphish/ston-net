using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston
{
    /// <summary>
    /// Serves as a base class for STON path segment classes.
    /// </summary>
    public abstract class StonPathSegment : IStonPathSegment
    {
        /// <summary>
        /// Creates a structurally equivalent path segment from a given segment.
        /// </summary>
        /// <param name="segment">The segment to copy the structure of.</param>
        /// <returns>A structurally equivalent copy of the given segment.</returns>
        public static IStonPathSegment Copy(IStonPathSegment segment)
        {
            if (segment == null) throw new ArgumentNullException("segment");
            if (segment is IStonAncestorPathSegment) return StonAncestorPathSegment.Copy(segment as IStonAncestorPathSegment);
            if (segment is IStonMemberPathSegment) return StonMemberPathSegment.Copy(segment as IStonMemberPathSegment);
            if (segment is IStonCollectionElementPathSegment) return StonCollectionElementPathSegment.Copy(segment as IStonCollectionElementPathSegment);
            throw new StonImplementationException(segment.GetType(), typeof(IStonPathSegment), typeof(IStonAncestorPathSegment), typeof(IStonMemberPathSegment), typeof(IStonCollectionElementPathSegment));
        }
    }

    /// <summary>
    /// Represents a STON address ancestor path segment with a given order.
    /// </summary>
    public sealed class StonAncestorPathSegment : StonPathSegment, IStonAncestorPathSegment
    {
        /// <summary>
        /// Gets the order of the ancestor.
        /// </summary>
        public int AncestorOrder { get; }

        /// <summary>
        /// Creates a new ancestor path segment, with a given ancestor order.
        /// </summary>
        /// <param name="ancestorOrder">The order of the ancestor to access.</param>
        public StonAncestorPathSegment(int ancestorOrder)
        {
            AncestorOrder = ancestorOrder;
        }

        /// <summary>
        /// Creates a structurally equivalent ancestor path segment from a given segment.
        /// </summary>
        /// <param name="segment">The segment to copy the structure from.</param>
        public StonAncestorPathSegment(IStonAncestorPathSegment segment)
            : this(segment.AncestorOrder) { }

        /// <summary>
        /// Creates a structurally equivalent ancestor path segment from a given segment.
        /// </summary>
        /// <param name="segment">The segment to copy the structure from.</param>
        /// <returns>A structurally equivalent copy of the given segment.</returns>
        public static IStonAncestorPathSegment Copy(IStonAncestorPathSegment segment)
        {
            if (segment == null) throw new ArgumentNullException("segment");
            return new StonAncestorPathSegment(segment);
        }

        // override for general debugging purposes
        // it is not necessarily meant as a valid STON representation
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return "." + new string('^', AncestorOrder);
        }
    }

    /// <summary>
    /// Represents a STON address member path segment with a given name or index.
    /// It is possible to represent regular and extension named members alike.
    /// </summary>
    public sealed class StonMemberPathSegment : StonPathSegment, IStonMemberPathSegment
    {
        /// <summary>
        /// Gets the member binding key.
        /// </summary>
        public IStonBindingKey BindingKey { get; }

        /// <summary>
        /// Creates a new member path segment, with a given binding key.
        /// </summary>
        /// <param name="bindingKey">The binding key of the member to access.</param>
        public StonMemberPathSegment(IStonBindingKey bindingKey)
        {
            if (bindingKey == null) throw new ArgumentNullException("bindingKey");
            BindingKey = StonBindingKey.Copy(bindingKey);
        }
        /// <summary>
        /// Creates a structurally equivalent member path segment from a given segment.
        /// </summary>
        /// <param name="segment">The segment to copy the structure from.</param>
        public StonMemberPathSegment(IStonMemberPathSegment segment)
            : this(segment.BindingKey) { }

        /// <summary>
        /// Creates a structurally equivalent member path segment from a given segment.
        /// </summary>
        /// <param name="segment">The segment to copy the structure from.</param>
        /// <returns>A structurally equivalent copy of the given segment.</returns>
        public static IStonMemberPathSegment Copy(IStonMemberPathSegment segment)
        {
            if (segment == null) throw new ArgumentNullException("segment");
            return new StonMemberPathSegment(segment);
        }

        // override for general debugging purposes
        // it is not necessarily meant as a valid STON representation
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return ((BindingKey is IStonBindingName) ? "." : "") + BindingKey.ToString();
        }
    }

    /// <summary>
    /// Represents a STON address collection element path segment with a given index.
    /// </summary>
    public sealed class StonCollectionElementPathSegment : StonPathSegment, IStonCollectionElementPathSegment
    {
        /// <summary>
        /// Gets the entity representing the index of the element in collection.
        /// </summary>
        public IStonEntity ElementIndex { get; }

        /// <summary>
        /// Creates a new collection element path segment, with a given element index entity.
        /// </summary>
        /// <param name="elementIndex">The entity representing the index of the element in collection to access.</param>
        public StonCollectionElementPathSegment(IStonEntity elementIndex)
        {
            if (elementIndex == null) throw new ArgumentNullException("elementIndex");
            ElementIndex = elementIndex;
        }

        /// <summary>
        /// Creates a structurally equivalent collection element path segment from a given segment.
        /// </summary>
        /// <param name="segment">The segment to copy the structure from.</param>
        public StonCollectionElementPathSegment(IStonCollectionElementPathSegment segment)
            : this(segment.ElementIndex) { }

        /// <summary>
        /// Creates a structurally equivalent collection element path segment from a given segment.
        /// </summary>
        /// <param name="segment">The segment to copy the structure from.</param>
        /// <returns>A structurally equivalent copy of the given segment.</returns>
        public static IStonCollectionElementPathSegment Copy(IStonCollectionElementPathSegment segment)
        {
            if (segment == null) throw new ArgumentNullException("segment");
            return new StonCollectionElementPathSegment(segment);
        }

        // override for general debugging purposes
        // it is not necessarily meant as a valid STON representation
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return "[#" + ElementIndex + "]";
        }
    }
}

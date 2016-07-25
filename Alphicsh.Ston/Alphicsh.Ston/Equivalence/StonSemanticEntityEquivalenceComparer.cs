using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston.Equivalence
{
    /// <summary>
    /// Provides methods to check STON entities for semantic equivalence, in the context of a given STON document.
    /// </summary>
    public sealed class StonSemanticEntityEquivalenceComparer : IStonEntityEquivalenceComparer
    {
        // The STON document whose entities are compared.
        private IStonDocument Document { get; }

        // The type equivalence comparer; right now, StonTypeEquivalenceComparer instance is used.
        private IStonTypeEquivalenceComparer TypeComparer { get; }

        /// <summary>
        /// Creates a new STON semantic entity equivalence comparer for a given STON document.
        /// </summary>
        /// <param name="document">The STON document whose entities are compared.</param>
        public StonSemanticEntityEquivalenceComparer(IStonDocument document)
        {
            if (document == null) throw new ArgumentNullException("document");
            Document = document;
            TypeComparer = StonTypeEquivalenceComparer.Instance;
        }

        #region IStonEntity equivalence

        /// <summary>
        /// Determines whether two entities are semantically equivalent.
        /// </summary>
        /// <param name="x">The first entity to compare.</param>
        /// <param name="y">The second entity to compare.</param>
        /// <returns>True when entities are semantically equivalent, false otherwise.</returns>
        public bool Equals(IStonEntity x, IStonEntity y)
        {
            if (x == y) return true;
            else if (x == null || y == null) return false;

            IStonValuedEntity xval, yval;

            if (x is IStonReferenceEntity) xval = Document.GetReferencedValue(x as IStonReferenceEntity);
            else xval = x as IStonValuedEntity;
            if (y is IStonReferenceEntity) yval = Document.GetReferencedValue(y as IStonReferenceEntity);
            else yval = y as IStonValuedEntity;

            if (xval == null || yval == null) return false;

            return Equals(xval, yval);
        }

        /// <summary>
        /// Returns a hash code for a given entity, applied to entities semantic equivalence.
        /// </summary>
        /// <param name="obj">The entity to get a hash code of.</param>
        /// <returns>The hash code for the entity.</returns>
        public int GetHashCode(IStonEntity obj)
        {
            if (obj == null) return 0;
            else if (obj is IStonReferenceEntity) return GetHashCode(obj as IStonReferenceEntity);
            else if (obj is IStonValuedEntity) return GetHashCode(obj as IStonValuedEntity);
            else return 0;
        }

        #endregion

        #region IStonValuedEntity equivalence

        /// <summary>
        /// Determines whether two valued entities are semantically equivalent.
        /// </summary>
        /// <param name="x">The first valued entity to compare.</param>
        /// <param name="y">The second valued entity to compare.</param>
        /// <returns>True when entities are semantically equivalent, false otherwise.</returns>
        public bool Equals(IStonValuedEntity x, IStonValuedEntity y)
        {
            if (x == y) return true;
            else if (x == null || y == null) return false;
            else if (x is IStonSimpleEntity && y is IStonSimpleEntity) return Equals(x as IStonSimpleEntity, y as IStonSimpleEntity);
            else return false;
        }

        /// <summary>
        /// Returns a hash code for a given valued entity, applied to entities semantic equivalence.
        /// </summary>
        /// <param name="obj">The valued entity to get a hash code of.</param>
        /// <returns>The hash code for the entity.</returns>
        public int GetHashCode(IStonValuedEntity obj)
        {
            if (obj == null) return 0;
            else if (obj is IStonSimpleEntity) return GetHashCode(obj as IStonSimpleEntity);
            else if (obj is IStonComplexEntity) return GetHashCode(obj as IStonComplexEntity);
            else return 0;
        }

        #endregion

        #region IStonSimpleEntity equivalence

        /// <summary>
        /// Determines whether two simple-valued entities are semantically equivalent.
        /// </summary>
        /// <param name="x">The first simple-valued entity to compare.</param>
        /// <param name="y">The second simple-valued entity to compare.</param>
        /// <returns>True when entities are semantically equivalent, false otherwise.</returns>
        public bool Equals(IStonSimpleEntity x, IStonSimpleEntity y)
        {
            if (x == y) return true;
            else if (x == null || y == null) return false;
            else if (!TypeComparer.Equals(x.Type, y.Type) || x.Value.DataType != y.Value.DataType) return false;
            else if (x.Value.DataType == StonDataType.Null) return true;
            else return (x.Value.Content == y.Value.Content);
        }

        /// <summary>
        /// Returns a hash code for a given simple-valued entity, applied to entities semantic equivalence.
        /// </summary>
        /// <param name="obj">The simple-valued entity to get a hash code of.</param>
        /// <returns>The hash code for the entity.</returns>
        public int GetHashCode(IStonSimpleEntity obj)
        {
            if (obj == null) return 0;

            int result = TypeComparer.GetHashCode(obj.Type);
            result = (result << 5) ^ (result >> 27);
            result ^= 0x1010101 * (byte)obj.Value.DataType;
            if (obj.Value.DataType != StonDataType.Null)
            {
                result = (result << 5) ^ (result >> 27);
                result ^= obj.Value.Content.GetHashCode();
            }

            return result;
        }

        #endregion

        #region IStonComplexEntity equivalence

        /// <summary>
        /// Determines whether two complex-valued entities are semantically equivalent.
        /// For two complex entities, it is the same as reference equality.
        /// </summary>
        /// <param name="x">The first complex-valued entity to compare.</param>
        /// <param name="y">The second complex-valued entity to compare.</param>
        /// <returns>True when entities are semantically equivalent, false otherwise.</returns>
        public bool Equals(IStonComplexEntity x, IStonComplexEntity y)
        {
            return ReferenceEquals(x, y);
        }

        /// <summary>
        /// Returns a hash code for a given complex-valued entity, applied to entities semantic equivalence.
        /// Among complex entities, it is the same as reference equality.
        /// </summary>
        /// <param name="obj">The complex-valued entity to get a hash code of.</param>
        /// <returns>The hash code for the entity.</returns>
        public int GetHashCode(IStonComplexEntity obj)
        {
            if (obj == null) return 0;
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }

        #endregion

        #region IStonReferenceEntity equivalence

        /// <summary>
        /// Determines whether two reference entities are semantically equivalent (whether they represent the same value).
        /// </summary>
        /// <param name="x">The first reference entity to compare.</param>
        /// <param name="y">The second reference entity to compare.</param>
        /// <returns>True when entities are semantically equivalent, false otherwise.</returns>
        public bool Equals(IStonReferenceEntity x, IStonReferenceEntity y)
        {
            if (x == y) return true;
            else if (x == null || y == null) return false;

            IStonValuedEntity xval, yval;
            xval = Document.GetReferencedValue(x);
            yval = Document.GetReferencedValue(y);

            if (xval == null || yval == null) return false;
            else return Equals(xval, yval);
        }

        /// <summary>
        /// Returns a hash code for a given reference entity, applied to entities semantic equivalence.
        /// It is the same as its referenced value hash code.
        /// </summary>
        /// <param name="obj">The reference entity to get a hash code of.</param>
        /// <returns>The hash code for the entity.</returns>
        public int GetHashCode(IStonReferenceEntity obj)
        {
            if (obj == null) return 0;
            return GetHashCode(Document.GetReferencedValue(obj));
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston.Equivalence
{
    /// <summary>
    /// Provides methods to check STON member binding keys for equivalence, depending on the method of checking entities equivalence.
    /// </summary>
    public sealed class StonBindingKeyEquivalenceComparer : IStonBindingKeyEquivalenceComparer
    {
        // The entities equivalence comparer used for index key parameters.
        private IStonEntityEquivalenceComparer IndexParameterComparer { get; }

        /// <summary>
        /// Creates a new STON member binding keys equivalence comparer, using a given entities equivalence comparer for index key parameters.
        /// </summary>
        /// <param name="indexParameterComparer">The entities equivalence comparer used for index key parameters.</param>
        public StonBindingKeyEquivalenceComparer(IStonEntityEquivalenceComparer indexParameterComparer)
        {
            if (indexParameterComparer == null) throw new ArgumentNullException("indexParameterComparer");
            IndexParameterComparer = indexParameterComparer;
        }

        #region IStonBindingKey equivalence

        /// <summary>
        /// Determines whether two binding keys are equivalent.
        /// </summary>
        /// <param name="x">The first binding key to compare.</param>
        /// <param name="y">The second binding key to compare.</param>
        /// <returns>True when binding keys are equivalent, false otherwise.</returns>
        public bool Equals(IStonBindingKey x, IStonBindingKey y)
        {
            if (x == y) return true;
            else if (x == null || y == null) return false;
            else if (x is IStonBindingName && y is IStonBindingName) return Equals(x as IStonBindingName, y as IStonBindingName);
            else if (x is IStonBindingIndex && y is IStonBindingIndex) return Equals(x as IStonBindingIndex, y as IStonBindingIndex);
            else return false;
        }

        /// <summary>
        /// Returns a hash code for a given binding key.
        /// </summary>
        /// <param name="obj">The binding key to get a hash code of.</param>
        /// <returns>The hash code for the binding key.</returns>
        public int GetHashCode(IStonBindingKey obj)
        {
            if (obj == null) return 0;
            else if (obj is IStonBindingName) return GetHashCode(obj as IStonBindingName);
            else if (obj is IStonBindingIndex) return GetHashCode(obj as IStonBindingIndex);
            else return 0;
        }

        #endregion

        #region IStonBindingName equivalence

        /// <summary>
        /// Determines whether two binding names are equivalent.
        /// </summary>
        /// <param name="x">The first binding name to compare.</param>
        /// <param name="y">The second binding name to compare.</param>
        /// <returns>True when binding names are equivalent, false otherwise.</returns>
        public bool Equals(IStonBindingName x, IStonBindingName y)
        {
            if (x == y) return true;
            else if (x == null || y == null) return false;

            return (x.IsExtension == y.IsExtension && x.Name == y.Name);
        }

        public int GetHashCode(IStonBindingName obj)
        {
            if (obj == null) return 0;

            int result = obj.Name.GetHashCode();
            if (obj.IsExtension) result ^= 0xf0f0f0f;

            return result;
        }

        #endregion

        #region IStonBindingIndex equivalence

        /// <summary>
        /// Determines whether two binding indices are equivalent by comparing corresponding entities.
        /// </summary>
        /// <param name="x">The first binding indices to compare.</param>
        /// <param name="y">The second binding indices to compare.</param>
        /// <returns>True when binding indices are equivalent, false otherwise.</returns>
        public bool Equals(IStonBindingIndex x, IStonBindingIndex y)
        {
            if (x == y) return true;
            else if (x == null || y == null) return false;

            return (x.Parameters.SequenceEqual(y.Parameters, IndexParameterComparer));
        }

        /// <summary>
        /// Returns a hash code for a given binding index.
        /// </summary>
        /// <param name="obj">The binding index to get a hash code of.</param>
        /// <returns>The hash code for the binding index.</returns>
        public int GetHashCode(IStonBindingIndex obj)
        {
            if (obj == null) return 0;

            unchecked
            {
                int result = 11;
                foreach (var parameter in obj.Parameters)
                {
                    result = result * 31 + IndexParameterComparer.GetHashCode(parameter);
                }

                return result;
            }
        }

        #endregion
    }
}

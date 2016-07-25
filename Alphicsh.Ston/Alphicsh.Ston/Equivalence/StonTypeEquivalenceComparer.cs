using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston.Equivalence
{
    /// <summary>
    /// Provides methods to check STON types for equivalence, based on equivalence of their type-specific properties.
    /// </summary>
    public sealed class StonTypeEquivalenceComparer : IStonTypeEquivalenceComparer
    {
        /// <summary>
        /// Exposes the StonTypeEquivalenceComparer functionality for checking STON types equivalence.
        /// </summary>
        public static IStonTypeEquivalenceComparer Instance { get; } = new StonTypeEquivalenceComparer();
        private StonTypeEquivalenceComparer() { }

        #region IStonType equivalence

        /// <summary>
        /// Determines whether two types are equivalent.
        /// </summary>
        /// <param name="x">The first type to compare.</param>
        /// <param name="y">The second type to compare.</param>
        /// <returns>True when types are equivalent, false otherwise.</returns>
        public bool Equals(IStonType x, IStonType y)
        {
            if (x == y) return true;
            else if (x == null || y == null) return false;
            else if (x is IStonNamedType && y is IStonNamedType) return Equals(x as IStonNamedType, y as IStonNamedType);
            else if (x is IStonCollectionType && y is IStonCollectionType) return Equals(x as IStonCollectionType, y as IStonCollectionType);
            else if (x is IStonUnionType && y is IStonUnionType) return Equals(x as IStonUnionType, y as IStonUnionType);
            else return false;
        }

        /// <summary>
        /// Returns a hash code for a given type.
        /// </summary>
        /// <param name="obj">The type to get a hash code of.</param>
        /// <returns>The hash code for the type.</returns>
        public int GetHashCode(IStonType obj)
        {
            if (obj == null) return 0;
            else if (obj is IStonNamedType) return GetHashCode(obj as IStonNamedType);
            else if (obj is IStonCollectionType) return GetHashCode(obj as IStonCollectionType);
            else if (obj is IStonUnionType) return GetHashCode(obj as IStonUnionType);
            else return 0;
        }

        #endregion

        #region IStonNamedType equivalence

        /// <summary>
        /// Determines whether two named types are equivalent.
        /// </summary>
        /// <param name="x">The first named type to compare.</param>
        /// <param name="y">The second named type to compare.</param>
        /// <returns>True when types are equivalent, false otherwise.</returns>
        public bool Equals(IStonNamedType x, IStonNamedType y)
        {
            if (x == y) return true;
            else if (x == null || y == null) return false;

            return (x.IsExtension == y.IsExtension && x.Name == y.Name && x.TypeParameters.SequenceEqual(y.TypeParameters, this));
        }

        /// <summary>
        /// Returns a hash code for a given named type.
        /// </summary>
        /// <param name="obj">The named type to get a hash code of.</param>
        /// <returns>The hash code for the type.</returns>
        public int GetHashCode(IStonNamedType obj)
        {
            if (obj == null) return 0;

            unchecked
            {
                int result = obj.IsExtension ? 19 : 23;
                result = result * 31 + obj.Name.GetHashCode();
                foreach (var parameter in obj.TypeParameters)
                {
                    result = result * 31 + GetHashCode(parameter);
                }

                return result;
            }
        }

        #endregion

        #region IStonCollectionType equivalence

        /// <summary>
        /// Determines whether two collection types are equivalent.
        /// </summary>
        /// <param name="x">The first collection type to compare.</param>
        /// <param name="y">The second collection type to compare.</param>
        /// <returns>True when types are equivalent, false otherwise.</returns>
        public bool Equals(IStonCollectionType x, IStonCollectionType y)
        {
            if (x == y) return true;
            else if (x == null || y == null) return false;

            return Equals(x.ElementType, y.ElementType);
        }

        /// <summary>
        /// Returns a hash code for a given collection type.
        /// </summary>
        /// <param name="obj">The collection type to get a hash code of.</param>
        /// <returns>The hash code for the type.</returns>
        public int GetHashCode(IStonCollectionType obj)
        {
            if (obj == null) return 0;

            int result = GetHashCode(obj.ElementType);
            return (result << 7) ^ (result >> 25) ^ 0x12345678;
        }

        #endregion

        #region IStonUnionType equivalence

        /// <summary>
        /// Determines whether two union types are equivalent.
        /// </summary>
        /// <param name="x">The first union type to compare.</param>
        /// <param name="y">The second union type to compare.</param>
        /// <returns>True when types are equivalent, false otherwise.</returns>
        public bool Equals(IStonUnionType x, IStonUnionType y)
        {
            if (x == y) return true;
            else if (x == null || y == null) return false;

            return (x.PermittedTypes.SequenceEqual(y.PermittedTypes, this));
        }

        /// <summary>
        /// Returns a hash code for a given union type.
        /// </summary>
        /// <param name="obj">The union type to get a hash code of.</param>
        /// <returns>The hash code for the type.</returns>
        public int GetHashCode(IStonUnionType obj)
        {
            if (obj == null) return 0;

            unchecked
            {
                int result = 17;
                foreach (var parameter in obj.PermittedTypes)
                {
                    result = result * 31 + GetHashCode(parameter);
                }

                return result;
            }
        }

        #endregion
    }
}

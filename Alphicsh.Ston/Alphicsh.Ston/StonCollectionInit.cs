using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alphicsh.Ston.Tokenization;

namespace Alphicsh.Ston
{
    /// <summary>
    /// Represents a collection initialization of a complex value.
    /// </summary>
    public sealed class StonCollectionInit : IStonCollectionInit
    {
        /// <summary>
        /// Gets the elements of collection initialization.
        /// </summary>
        public IEnumerable<IStonEntity> Elements { get; }

        /// <summary>
        /// Creates a new collection initialization of a complex value, given a sequence of elements.
        /// </summary>
        /// <param name="elements">The sequence of elements.</param>
        public StonCollectionInit(IEnumerable<IStonEntity> elements)
        {
            Elements = elements?.Select(e => StonEntity.Copy(e)).ToList() ?? Enumerable.Empty<IStonEntity>();
        }

        /// <summary>
        /// Creates a structurally equivalent collection initialization from a given collection initialization.
        /// </summary>
        /// <param name="collectionInit">The collection initialization to copy the structure of.</param>
        public StonCollectionInit(IStonCollectionInit collectionInit)
            : this(collectionInit.Elements) { }

        /// <summary>
        /// Creates a structurally equivalent collection initialization from a given collection initialization.
        /// </summary>
        /// <param name="collectionInit">The collection initialization to copy the structure of.</param>
        /// <returns>A structurally equivalent copy of the given collection initialization.</returns>
        public static IStonCollectionInit Copy(IStonCollectionInit collectionInit)
        {
            if (collectionInit == null) throw new ArgumentNullException("collectionInit");
            return new StonCollectionInit(collectionInit);
        }

        // override for general debugging purposes
        // it is not necessarily meant as a valid STON representation
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return "[ " + Elements.Count() + " element(s) ]";
        }
    }
}

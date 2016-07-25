using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston
{
    /// <summary>
    /// Represents a construction of a STON complex value.
    /// </summary>
    public sealed class StonConstruction : IStonConstruction
    {
        /// <summary>
        /// Gets the positional parameters of the construction.
        /// </summary>
        public IEnumerable<IStonEntity> PositionalParameters { get; }

        /// <summary>
        /// Gets the named parameters of the construction. The order is considered relevant.
        /// </summary>
        public IEnumerable<KeyValuePair<string, IStonEntity>> NamedParameters { get; }

        /// <summary>
        /// Creates a new construction of a complex value, given sequences of positional parameters and named parameters.
        /// </summary>
        /// <param name="positionalParameters">The sequence of positional construction parameters.</param>
        /// <param name="namedParameters">The sequence of named construction parameters.</param>
        public StonConstruction(IEnumerable<IStonEntity> positionalParameters, IEnumerable<KeyValuePair<string, IStonEntity>> namedParameters)
        {
            PositionalParameters = positionalParameters?.Select(p => StonEntity.Copy(p)).ToList() ?? Enumerable.Empty<IStonEntity>();
            NamedParameters = namedParameters?.Select(kvp => new KeyValuePair<string, IStonEntity>(kvp.Key, StonEntity.Copy(kvp.Value))).ToList() ?? Enumerable.Empty<KeyValuePair<string, IStonEntity>>();
        }

        /// <summary>
        /// Creates a structurally equivalent construction from a given construction.
        /// </summary>
        /// <param name="construction">The construction to copy the structure of.</param>
        public StonConstruction(IStonConstruction construction)
            : this(construction.PositionalParameters, construction.NamedParameters) { }

        /// <summary>
        /// Creates a structurally equivalent construction from a given construction.
        /// </summary>
        /// <param name="construction">The construction to copy the structure of.</param>
        /// <returns>A structurally equivalent copy of the given construction.</returns>
        public static IStonConstruction Copy(IStonConstruction construction)
        {
            if (construction == null) throw new ArgumentNullException("construction");
            return new StonConstruction(construction);
        }

        // override for general debugging purposes
        // it is not necessarily meant as a valid STON representation
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return "( " + PositionalParameters.Count() + "+" + NamedParameters.Count() + " parameter(s) )";
        }
    }
}

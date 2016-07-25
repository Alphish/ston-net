using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston
{
    /// <summary>
    /// Exposes the positional and named parameters of a complex value's construction.
    /// </summary>
    public interface IStonConstruction
    {
        /// <summary>
        /// Gets the positional parameters of the construction.
        /// </summary>
        IEnumerable<IStonEntity> PositionalParameters { get; }

        /// <summary>
        /// Gets the named parameters of the construction. The order is considered relevant.
        /// </summary>
        IEnumerable<KeyValuePair<string, IStonEntity>> NamedParameters { get; }
    }
}

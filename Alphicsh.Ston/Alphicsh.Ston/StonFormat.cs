using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston
{
    /// <summary>
    /// Describes available string formats, in relation to STON.
    /// </summary>
    public enum StonFormat : byte
    {
        /// <summary>
        /// The string can be arbitrary.
        /// </summary>
        Any = 0,
        /// <summary>
        /// The string is expected to be a valid regular STON string.
        /// </summary>
        RegularSton = 1,
        /// <summary>
        /// The string is expected to be a valid strict STON string.
        /// </summary>
        StrictSton = 2,
        /// <summary>
        /// The string is expected to be a valid canonical STON string.
        /// </summary>
        CanonicalSton = 3
    }
}

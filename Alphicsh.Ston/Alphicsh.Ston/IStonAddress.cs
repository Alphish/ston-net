using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston
{
    /// <summary>
    /// Exposes STON address structure.
    /// </summary>
    public interface IStonAddress
    {
        /// <summary>
        /// Gets the initial context of the address.
        /// </summary>
        IStonInitialContext InitialContext { get; }
        /// <summary>
        /// Gets the path segments of the address.
        /// </summary>
        IEnumerable<IStonPathSegment> RelativePath { get; }
    }
}

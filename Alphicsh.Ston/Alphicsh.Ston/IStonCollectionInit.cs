using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston
{
    /// <summary>
    /// Exposes the elements of a complex value's collection initialization.
    /// </summary>
    public interface IStonCollectionInit
    {
        /// <summary>
        /// Gets the elements of collection initialization.
        /// </summary>
        IEnumerable<IStonEntity> Elements { get; }
    }
}

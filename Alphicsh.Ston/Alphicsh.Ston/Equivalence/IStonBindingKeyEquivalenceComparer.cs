using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston.Equivalence
{
    /// <summary>
    /// Exposes methods to check STON member binding keys for equivalence.
    /// </summary>
    public interface IStonBindingKeyEquivalenceComparer :
        IEqualityComparer<IStonBindingKey>,
        IEqualityComparer<IStonBindingName>,
        IEqualityComparer<IStonBindingIndex>
    {
    }
}

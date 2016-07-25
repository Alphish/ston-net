using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston.Equivalence
{
    /// <summary>
    /// Exposes methods to check STON types for equivalence.
    /// </summary>
    public interface IStonTypeEquivalenceComparer :
        IEqualityComparer<IStonType>,
        IEqualityComparer<IStonNamedType>,
        IEqualityComparer<IStonCollectionType>,
        IEqualityComparer<IStonUnionType>
    {
    }
}

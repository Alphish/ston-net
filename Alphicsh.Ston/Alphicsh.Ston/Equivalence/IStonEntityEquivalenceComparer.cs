using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston.Equivalence
{
    /// <summary>
    /// Exposes methods to check STON entities for equivalence.
    /// </summary>
    public interface IStonEntityEquivalenceComparer :
        IEqualityComparer<IStonEntity>,
        IEqualityComparer<IStonValuedEntity>,
        IEqualityComparer<IStonSimpleEntity>,
        IEqualityComparer<IStonComplexEntity>,
        IEqualityComparer<IStonReferenceEntity>
    {
    }
}

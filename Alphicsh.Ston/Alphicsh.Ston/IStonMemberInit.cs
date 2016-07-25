using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston
{
    /// <summary>
    /// Exposes the member bindings of a complex value's member initialization.
    /// </summary>
    public interface IStonMemberInit
    {
        /// <summary>
        /// Gets the member bindings of the member initialization.
        /// </summary>
        IEnumerable<KeyValuePair<IStonBindingKey, IStonEntity>> MemberBindings { get; }
    }
}

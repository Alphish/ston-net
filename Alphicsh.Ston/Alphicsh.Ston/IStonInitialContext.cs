using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston
{
    /// <summary>
    /// Represents the initial context of a STON address.
    /// It should be implemented either through IStonAncestorInitialContext or IStonGlobalEntityInitialContext.
    /// </summary>
    public interface IStonInitialContext { }

    /// <summary>
    /// Represents the STON address ancestor initial context with a given order.
    /// When the ancestor order is zero, the initial context is the reference defining entity instead.
    /// </summary>
    public interface IStonAncestorInitialContext : IStonInitialContext
    {
        /// <summary>
        /// Gets the ancestor order of the initial context.
        /// When the initial context is the reference defining entity, the order is 0.
        /// </summary>
        int AncestorOrder { get; }
    }

    /// <summary>
    /// Represents the STON address globally identified entity initial context with a given identifier.
    /// When the identifier is empty, the initial cnotext is the document core instead.
    /// </summary>
    public interface IStonGlobalEntityInitialContext : IStonInitialContext
    {
        /// <summary>
        /// Gets the identifier of the globally identified entity.
        /// When the initial context is the document core, the identifier is empty.
        /// </summary>
        string GlobalIdentifier { get; }
    }
}

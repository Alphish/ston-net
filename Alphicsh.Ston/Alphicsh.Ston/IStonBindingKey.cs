using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston
{
    /// <summary>
    /// Represents a member binding key.
    /// </summary>
    public interface IStonBindingKey { }

    /// <summary>
    /// Represents a named member key.
    /// The key is composed of a name and member kind (regular/extension).
    /// </summary>
    public interface IStonBindingName : IStonBindingKey
    {
        /// <summary>
        /// Gets the name of the member.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Indicates whether the member is an extension member or a regular member.
        /// </summary>
        bool IsExtension { get; }
    }

    /// <summary>
    /// Represents an indexed member binding key.
    /// They key consists of an index parameters sequence.
    /// </summary>
    public interface IStonBindingIndex : IStonBindingKey
    {
        /// <summary>
        /// Gets the member index parameters.
        /// </summary>
        IEnumerable<IStonEntity> Parameters { get; }
    }
}

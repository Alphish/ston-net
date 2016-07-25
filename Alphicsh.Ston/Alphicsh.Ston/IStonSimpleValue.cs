using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston
{
    /// <summary>
    /// Exposes the data type and content string of a STON simple value.
    /// </summary>
    public interface IStonSimpleValue
    {
        /// <summary>
        /// Gets the data type of the value.
        /// </summary>
        StonDataType DataType { get; }

        /// <summary>
        /// Gets the content string of the value.
        /// </summary>
        string Content { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston
{
    /// <summary>
    /// Represents the kind of data stored in a simple value.
    /// </summary>
    public enum StonDataType : byte
    {
        /// <summary>
        /// Indicates that the value is missing.
        /// </summary>
        Null = 0,
        /// <summary>
        /// Indicates that the value is a floating point number.
        /// </summary>
        Number = 1,
        /// <summary>
        /// Indicates that the value is a sequence of bytes.
        /// </summary>
        Binary = 2,
        /// <summary>
        /// Indicates that the value is one of predefined, named values.
        /// </summary>
        Named = 3,
        /// <summary>
        /// Indicates that the value is a plain text.
        /// </summary>
        Text = 4,
        /// <summary>
        /// Indicates that the value is a code to be interpreted.
        /// </summary>
        Code = 5
    }
}

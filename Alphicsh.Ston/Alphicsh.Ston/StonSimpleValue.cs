using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alphicsh.Ston.Tokenization;

namespace Alphicsh.Ston
{
    /// <summary>
    /// Represents a STON simple value.
    /// </summary>
    public sealed class StonSimpleValue : IStonSimpleValue
    {
        /// <summary>
        /// Gets the data type of the value.
        /// </summary>
        public StonDataType DataType { get; set; }

        /// <summary>
        /// Gets the content string of the value.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Creates a new simple value, given a data type and content string.
        /// </summary>
        /// <param name="dataType">The data type of the value.</param>
        /// <param name="content">The content string of the value.</param>
        public StonSimpleValue(StonDataType dataType, string content)
        {
            DataType = dataType;
            Content = content;
        }

        /// <summary>
        /// Creates a structurally equivalent simple value from a given value.
        /// </summary>
        /// <param name="value">The value to copy the structure of.</param>
        public StonSimpleValue(IStonSimpleValue value)
            : this(value.DataType, value.Content) { }

        /// <summary>
        /// Creates a structurally equivalent simple value from a given value.
        /// </summary>
        /// <param name="value">The value to copy the structure of.</param>
        /// <returns>A structurally equivalent copy of the given value.</returns>
        public static IStonSimpleValue Copy(IStonSimpleValue value)
        {
            if (value == null) throw new ArgumentNullException("value");
            return new StonSimpleValue(value);
        }

        // override for general debugging purposes
        // it is not necessarily meant as a valid STON representation
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public override string ToString()
        {
            switch (DataType)
            {
                case StonDataType.Text:
                    return '"' + Content + '"';
                case StonDataType.Named:
                case StonDataType.Number:
                    return Content;
                case StonDataType.Binary:
                    if (Content == "") return "0n";
                    else if (Content.StartsWith("-")) return Content.Insert(1, "0x");
                    else return "0x" + Content;
                case StonDataType.Null:
                    return "null";
                default:
                    return "";
            }
        }
    }
}

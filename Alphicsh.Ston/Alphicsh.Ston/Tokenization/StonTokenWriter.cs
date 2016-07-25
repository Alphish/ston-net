using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston.Tokenization
{
    using Helpers;

    /// <summary>
    /// Provides the functionality of writing STON tokens to a text.
    /// </summary>
    public sealed class StonTokenWriter
    {
        // the inner text writer to write to
        private TextWriter InnerWriter { get; }

        /// <summary>
        /// Creates a new STON token writer from a given text writer.
        /// </summary>
        /// <param name="innerWriter">The text writer to write to.</param>
        public StonTokenWriter(TextWriter innerWriter)
        {
            if (innerWriter == null) throw new ArgumentNullException("innerWriter");
            InnerWriter = innerWriter;
        }

        #region Basic writing

        /// <summary>
        /// Writes a character to the text.
        /// </summary>
        /// <param name="value">The character to write.</param>
        public void Write(char value) => InnerWriter.Write(value);

        /// <summary>
        /// Writes a string to the text.
        /// </summary>
        /// <param name="value">The string to write.</param>
        public void Write(string value) => InnerWriter.Write(value);



        /// <summary>
        /// Begins a new line in the text.
        /// </summary>
        public void WriteLine() => InnerWriter.WriteLine();
        
        /// <summary>
        /// Writes a character and begins a new line in the text.
        /// </summary>
        /// <param name="value">The character to write before the new line.</param>
        public void WriteLine(char value) => InnerWriter.WriteLine(value);

        /// <summary>
        /// Writes a string and begins a new line in the text.
        /// </summary>
        /// <param name="value">The string to write before the new line.</param>
        public void WriteLine(string value) => InnerWriter.WriteLine(value);

        #endregion

        #region String literal writing

        /// <summary>
        /// Writes a STON string literal to the text, representing a given content and delimited with a given delimiter.
        /// If the delimiter is a double quote or a backtick, the resulting literal is a canonical text or code literal.
        /// </summary>
        /// <param name="content">The content to represent with the string literal.</param>
        /// <param name="delimiter">The delimiter to use.</param>
        public void WriteStringLiteral(string content, char delimiter = '"')
        {
            if (content == null) throw new ArgumentNullException("content");
            if (!delimiter.HasChartype(StonChartype.TextDelimiter | StonChartype.CodeDelimiter)) throw new ArgumentException("The given character is not a valid STON text or code literal delimiter.");

            Write(delimiter);

            // escaping the content characters where needed
            foreach (var ch in content)
            {
                switch (ch)
                {
                    case '\\':
                        Write("\\" + ch);
                        break;
                    case '\b':
                        Write("\\b");
                        break;
                    case '\f':
                        Write("\\f");
                        break;
                    case '\n':
                        Write("\\n");
                        break;
                    case '\r':
                        Write("\\r");
                        break;
                    case '\t':
                        Write("\\t");
                        break;

                    default:
                        if (ch < 32 || ch > 126)
                        {
                            Write("\\u");
                            Write((ch >> 12 & 0x0f).GetBase16Symbol());
                            Write((ch >> 8 & 0x0f).GetBase16Symbol());
                            Write((ch >> 4 & 0x0f).GetBase16Symbol());
                            Write((ch >> 0 & 0x0f).GetBase16Symbol());
                        }
                        else
                        {
                            if (ch == delimiter) Write('\\');
                            Write(ch);
                        }
                        break;
                }
            }

            Write(delimiter);
        }

        #endregion

        #region Binary literal writing

        /// <summary>
        /// Writes a STON binary literal to the text, representing a given content in a given base.
        /// </summary>
        /// <param name="content">The content to represent with the binary literal.</param>
        /// <param name="baseId">The identifier of the base used.</param>
        public void WriteBinaryLiteral(string content, char baseId = 'x')
        {
            CheckContentValidity(StonDataType.Binary, content);

            // empty content is always written as "0n", regardless of the base
            if (content == "")
            {
                Write("0n");
                return;
            }

            // building a non-empty content
            var builder = new StringBuilder();
            // handling and stripping the initial minus
            if (content[0] == '-')
            {
                builder.Append('-');
                content = content.Substring(1);
            }

            int charSize = 0;

            switch (baseId)
            {
                // base 2 literals can be obtained by turning hexadecimal digits into batches of 4 characters
                case 'b':
                case 'B':
                    builder.Append("0b");
                    builder.Append(string.Join("", content.Select(HexToBinary)));
                    Write(builder.ToString());
                    return;

                // base 8 literals are composed of 3-bit digits
                case 'o':
                case 'O':
                    builder.Append("0o");
                    charSize = 3;
                    break;

                // the content string is already in hexadecimal form
                case 'x':
                case 'X':
                    builder.Append("0x");
                    builder.Append(content);
                    Write(builder.ToString());
                    return;

                // base 64 literals are composed of 6-bit digits
                case 'z':
                case 'Z':
                    builder.Append("0z");
                    charSize = 6;
                    break;

                // empty binary literals should be handled at the beginning
                // if they weren't, it means the content is non-empty
                case 'n':
                case 'N':
                    throw new ArgumentException("An empty literal base identifier cannot be used for non-empty binary content.");
                
                default:
                    throw new ArgumentException($"Unknown base identfier: '{baseId}'");
            }

            // preparing to build the binary literal for base 8 and base 64
            int bitLength = 4 * content.Length;
            int remainder = bitLength % charSize;
            int offset = -charSize;
            bool isBase64 = (charSize == 6);

            if (!isBase64)
            {
                offset -= remainder;
                int test = content[0].GetBase16Value();
                // interestingly, not all byte sequences of a given length can be represented with sequence of base 8 digits
                // technically, it's true for base 64 as well, unless the padding is used (which it is)
                if ((test >= 8 && remainder >= 1) || (test >= 4 && remainder >= 2)) throw new ArgumentException("The given binary content cannot be represented as a base 8 literal.");
            }

            // representing the bits sequence with base-specific digits
            int value = 0;
            int digit;
            foreach (char c in content)
            {
                value = value << 4 | c.GetBase16Value();
                offset += 4;
                while (offset >= 0)
                {
                    digit = value >> offset;
                    builder.Append(isBase64 ? digit.GetBase64Symbol() : (char)('0' + digit));
                    value ^= digit << offset;
                    offset -= charSize;
                }
            }

            // adding padding to base 64 string, if necessary
            if (charSize == 6 && remainder != 0)
            {
                builder.Append((value << 6 - remainder).GetBase64Symbol());
                builder.Append(new string('=', (6 - remainder) / 2));
            }

            // writing the resulting literal
            Write(builder.ToString());
        }

        // converts base 16 digits to sequences of base 2 digits
        private string HexToBinary(char hex)
        {
            switch (hex)
            {
                case '0': return "0000";
                case '1': return "0001";
                case '2': return "0010";
                case '3': return "0011";
                case '4': return "0100";
                case '5': return "0101";
                case '6': return "0110";
                case '7': return "0111";
                case '8': return "1000";
                case '9': return "1001";
                case 'a': return "1010";
                case 'b': return "1011";
                case 'c': return "1100";
                case 'd': return "1101";
                case 'e': return "1110";
                default: return "1111";     // case 'f'
            }
        }

        #endregion

        #region Number literal writing

        /// <summary>
        /// Writes a STON number literal to the text, representing a given content canonically.
        /// </summary>
        /// <param name="content">The content to represent with the canonical number literal.</param>
        public void WriteCanonicalNumberLiteral(string content)
        {
            CheckContentValidity(StonDataType.Number, content);
            Write(content);
        }

        /// <summary>
        /// Writes a STON number literal to the text, representing a given content as an integer or decimal number, without explicit exponent.
        /// </summary>
        /// <param name="content">The content to represent with the plain number literal.</param>
        public void WritePlainNumberLiteral(string content)
        {
            CheckContentValidity(StonDataType.Number, content);

            // writing zero directly
            if (content == "0")
            {
                Write(content);
                return;
            }

            var builder = new StringBuilder();
            int eiof = content.IndexOf('e');
            builder.Append(content.Remove(eiof));       // writing the significand part to the builder

            string exponentString = content.Substring(eiof + 1);
            bool exponentMinus = (exponentString[0] == '-');
            if (exponentString.Length > 9 + (exponentMinus ? 1 : 0)) throw new NotSupportedException("A number that long cannot be written as a plain number literal.");

            int exponent = int.Parse(exponentString);
            if (exponent >= 0)
            {
                // when the exponent is positive, zeros are added to the significand
                builder.Append(new string('0', exponent));
            }
            else
            {
                // when the exponent is negative, decimal point is inserted
                // and possibly leading zeros are added
                int offset = (content[0] == '-') ? 1 : 0;
                if (offset < eiof + exponent) builder.Insert(eiof + exponent, '.');
                else
                {
                    builder.Insert(offset, new string('0', offset - exponent - eiof));
                    builder.Insert(offset, "0.");
                }
            }
            Write(builder.ToString());
        }

        /// <summary>
        /// Writes a STON number literal to the text, representing a given content in scientific notation.
        /// A minimum precision can be provided, to determine how many digits must be written before the exponent.
        /// If the content is zero, it is written with an exponent of zero.
        /// </summary>
        /// <param name="content">The content to represent with the scientific notation number literal.</param>
        /// <param name="minPrecision"></param>
        public void WriteScientificNumberLiteral(string content, int minPrecision = 1)
        {
            CheckContentValidity(StonDataType.Number, content);

            // handling zero content
            if (content == "0")
            {
                if (minPrecision <= 1) Write("0e0");
                else Write($"0.{ new string('0', minPrecision-1) }e0");
                return;
            }

            var builder = new StringBuilder();
            int eiof = content.IndexOf('e');
            builder.Append(content.Remove(eiof));       // writing the significand part to the builder

            int offset = (content[0] == '-') ? 1 : 0;
            int precision = eiof - offset;
            if (minPrecision > precision) builder.Append(new string('0', minPrecision - precision));        // adding trailing zeros to the required minimal precision
            if (precision > 1 || minPrecision > 1) builder.Insert(offset + 1, '.');                         // if the number has more than 1 digit (including trailing zeros), decimal point is inserted

            // writing the exponent, increased by the number of significant digits after the decimal point
            builder.Append('e');
            builder.Append(NumericStringCalculator.Add(content.Substring(eiof+1), precision-1));

            Write(builder.ToString());
        }

        #endregion

        // checks a content validity before trying to write it
        private void CheckContentValidity(StonDataType type, string content)
        {
            if (content == null) throw new ArgumentNullException("content");
            try
            {
                Validator.ValidateSimpleValue(new StonSimpleValue(type, content));
            }
            catch (StonException ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston.Tokenization
{
    using Helpers;

    /// <summary>
    /// Provides the functionality of reading tokens from a STON text.
    /// </summary>
    public sealed class StonTokenReader
    {
        // the inner text reader to read STON text from
        private TextReader InnerReader { get; }

        /// <summary>
        /// Gets the current position in the STON string.
        /// </summary>
        public int Position { get; private set; }

        /// <summary>
        /// Gets the current line in the STON string.
        /// </summary>
        public int Line { get; private set; }

        /// <summary>
        /// Gets the current column in the STON string, counted from the line beginning.
        /// </summary>
        public int Column { get; private set; }

        /// <summary>
        /// Creates a new STON token reader from a given text reader.
        /// </summary>
        /// <param name="innerReader">The text reader to read the tokens from.</param>
        public StonTokenReader(TextReader innerReader)
        {
            if (innerReader == null) throw new ArgumentNullException("innerReader");
            InnerReader = innerReader;
            Position = 0;
            Line = 0;
            Column = 0;
        }

        #region Character-by-character reading

        // advances the character position depending on the character provided
        private int AcceptChar(int ch)
        {
            Position++;

            // begin a new line if the character is a newline
            if (ch.HasChartype(StonChartype.Newline))
            {
                Line++;
                Column = 0;
            }
            else Column++;

            return ch;
        }

        /// <summary>
        /// Retrieves the next available character without advancing the reader's position.
        /// </summary>
        /// <returns>The next character to read.</returns>
        public int Peek()
        {
            return InnerReader.Peek();
        }

        /// <summary>
        /// Reads the next available character and advances the reader's position.
        /// </summary>
        /// <returns>The read character.</returns>
        public int Read()
        {
            return AcceptChar(InnerReader.Read());
        }

        /// <summary>
        /// Reads the next available character and confirms its character type.
        /// If the character type is invalid, throws an exception.
        /// </summary>
        /// <param name="chartype">The expected character type.</param>
        /// <returns>The read character.</returns>
        public int ExpectChartype(StonChartype chartype)
        {
            int ch = InnerReader.Peek();
            if (!ch.HasChartype(chartype)) throw MakeUnexpectedCharacterException(chartype);
            else return Read();
        }

        /// <summary>
        /// Checks whether the next available character has specific character type, and reads the character if that's the case.
        /// </summary>
        /// <param name="chartype">The suspected character type.</param>
        /// <returns>True if the character has the specified chartype, false otherwise.</returns>
        public bool TryChartype(StonChartype chartype)
        {
            int ch = InnerReader.Peek();
            if (!ch.HasChartype(chartype)) return false;
            else
            {
                Read();
                return true;
            }
        }

        #endregion

        #region Spacings reading

        /// <summary>
        /// Advances the reader right before the nearest character that is not a comment or whitespace and passes that character.
        /// </summary>
        /// <returns>The next significant character to read.</returns>
        public int PeekSignificant()
        {
            int ch = InnerReader.Peek();

            while (true)
            {
                // handle newlines
                if (ch.HasChartype(StonChartype.Newline))
                {
                    Read();
                    if (ch == '\r' && InnerReader.Peek() == '\n')
                    {
                        // exceptionally, position isn't changed through AcceptChar
                        // so that CRLF counts as a single line rather than two
                        if (Position > -1) Position++;
                        InnerReader.Read();
                    }
                }
                // handle whitespace characters
                else if (ch.HasChartype(StonChartype.Whitespace)) Read();
                // handle comments
                else if (ch.HasChartype(StonChartype.CommentOpen))
                {
                    Read();
                    ch = ExpectChartype(StonChartype.CommentDiscern);

                    // single line comments
                    if (ch == '/') while (!InnerReader.Peek().HasChartype(StonChartype.Newline | StonChartype.EOS)) Read();
                    // multiline comments
                    else if (ch == '*')
                    {
                        while (true)
                        {
                            ch = Read();
                            if (ch == '*')
                            {
                                if (InnerReader.Peek() == '/')
                                {
                                    Read();
                                    break;
                                }
                            }
                            else if (ch.HasChartype(StonChartype.EOS)) throw new StonUnexpectedCharacterParsingException(Position - 1, Line, Column - 1, ch, StonChartype.None);
                        }
                    }
                }
                else break;

                ch = InnerReader.Peek();
            }

            return ch;
        }

        /// <summary>
        /// Reads the next available character, then advances the reader until the next significant character.
        /// </summary>
        /// <returns>The initially read character.</returns>
        public int ReadAndSkip()
        {
            int result = Read();
            PeekSignificant();
            return result;
        }

        /// <summary>
        /// Reads the next available character and confirms its type, then advances the reader until the next significant character.
        /// </summary>
        /// <param name="chartype">The expected character type.</param>
        /// <returns>The initially read character.</returns>
        public int ExpectAndSkip(StonChartype chartype)
        {
            int result = ExpectChartype(chartype);
            PeekSignificant();
            return result;
        }

        /// <summary>
        /// Checks whether the next available character has specific character type, and advances the reader until the next significant character if that's the case.
        /// </summary>
        /// <param name="chartype">The suspected character type.</param>
        /// <returns>True if the character has the specified chartype, false otherwise.</returns>
        public bool TryAndSkip(StonChartype chartype)
        {
            var result = TryChartype(chartype);
            if (!result) return false;
            else
            {
                PeekSignificant();
                return true;
            }
        }

        #endregion

        #region CANUN tokens reading

        /// <summary>
        /// Reads a Common Alpha-Numeric and Underscore Name from the reader, and writes the name to a given builder.
        /// After that, advances the reader to the next significant character.
        /// </summary>
        /// <param name="builder">The builder to write the name to.</param>
        public void ReadCanun(StringBuilder builder)
        {
            builder.Append((char)ExpectChartype(StonChartype.CanunBegin));
            while (InnerReader.Peek().HasChartype(StonChartype.CanunContinue)) builder.Append((char)Read());
            PeekSignificant();
        }

        /// <summary>
        /// Reads a Common Alpha-Numeric and Underscore Name from the reader and passes the name.
        /// After that, advances the reader to the next significant character.
        /// </summary>
        /// <returns>The read CANUN name.</returns>
        public string ReadCanun()
        {
            var builder = new StringBuilder();
            ReadCanun(builder);
            return builder.ToString();
        }

        #endregion

        #region String literals reading

        /// <summary>
        /// Reads a string content from a string literal, text or code, and writes the content to a given builder.
        /// After that, advances the reader to the next significant character.
        /// </summary>
        /// <param name="builder">The builder to write the content to.</param>
        /// <param name="code">Whether the code literal is expected or a text literal.</param>
        public void ReadString(StringBuilder builder, bool code = false)
        {
            int delimiter = ExpectChartype(code ? StonChartype.CodeDelimiter : StonChartype.TextDelimiter);
            int ch = Peek();

            while (ch != delimiter)
            {
                if (ch > '\0' && ch < ' ') throw MakeUnexpectedCharacterException(StonChartype.None, $"Unexpected control character #{ch}. Control characters are not allowed in string literals.");
                if (ch.HasChartype(StonChartype.StringEscapeOpen))
                {
                    Read();
                    ch = Peek();
                    switch (ch)
                    {
                        case '\'':
                        case '"':
                        case '`':
                        case '\\':
                        case '/':
                            builder.Append((char)ch); break;
                        case 'b':
                            builder.Append('\b'); break;
                        case 'f':
                            builder.Append('\f'); break;
                        case 'n':
                            builder.Append('\n'); break;
                        case 'r':
                            builder.Append('\r'); break;
                        case 't':
                            builder.Append('\t'); break;
                        case '0':
                            builder.Append('\0'); break;
                        case 'u':
                            int uch = 0;
                            for (int i = 0; i < 4; i++)
                            {
                                Read();
                                ch = Peek();
                                if (!ch.HasChartype(StonChartype.Base16)) throw MakeUnexpectedCharacterException(StonChartype.Base16, "4 hexadecimal digits expected after Unicode escape code.");
                                else uch = 16 * uch + ch.GetBase16Value();
                            }
                            builder.Append((char)uch);
                            break;
                        case -1:
                            throw MakeUnexpectedCharacterException(StonChartype.None);
                        default:
                            throw MakeUnexpectedCharacterException(StonChartype.None, $"Character '{(char)ch}' is not a part of recognized escape sequence.");
                    }
                }
                else if (ch.HasChartype(StonChartype.EOS)) throw MakeUnexpectedCharacterException(StonChartype.None);
                else builder.Append((char)ch);
                Read();
                ch = Peek();
            }
            ReadAndSkip();
        }

        /// <summary>
        /// Reads a string content from a string literal, text or code, and passes the content.
        /// After that, advances the reader to the next significant character.
        /// </summary>
        /// <param name="code">Whether the code literal is expected or a text literal.</param>
        /// <returns>The content of the string literal.</returns>
        public string ReadString(bool code = false)
        {
            var builder = new StringBuilder();
            ReadString(builder, code);
            return builder.ToString();
        }

        #endregion

        #region Binary literals reading

        /// <summary>
        /// Reads a content string from a binary literal and passes the content.
        /// It is assumed that the possible minus sign and the initial zero were read earlier.
        /// After that, advances the reader to the next significant character.
        /// </summary>
        /// <param name="minus">Whether the binary value is negative or not.</param>
        /// <returns>The content string of the binary literal.</returns>
        public string ReadBinaryContent(bool minus)
        {
            int baseId = ExpectAndSkip(StonChartype.BaseIdentifier);
            StonChartype digitType;     // the digit chartype for the specific base
            int charSize;               // the number of bits each character corresponds to

            // determining the digit type and character size, based on the base identifier
            switch (baseId)
            {
                case 'b':
                case 'B':
                    digitType = StonChartype.Base2;
                    charSize = 1;
                    break;

                case 'o':
                case 'O':
                    digitType = StonChartype.Base8;
                    charSize = 3;
                    break;

                case 'x':
                case 'X':
                    digitType = StonChartype.Base16;
                    charSize = 4;
                    break;

                case 'z':
                case 'Z':
                    digitType = StonChartype.Base64;
                    charSize = 6;
                    break;

                default:
                    // the binary content is empty
                    return "";
            }

            // gathering the digits of the binary content
            var builder = new StringBuilder();
            while (Peek().HasChartype(digitType)) builder.Append((char)ReadAndSkip());

            if (builder.Length == 0) throw MakeUnexpectedCharacterException(digitType);     // at least one digit is expected

            // passing base16 string directly or nearly directly; no further processing required
            if (digitType == StonChartype.Base16)
            {
                if (builder.Length % 2 == 1) builder.Insert(0, '0');
                if (minus) builder.Insert(0, '-');
                return builder.ToString().ToLowerInvariant();
            }

            // converting other bases to the base16 string

            int bitsLength = builder.Length * charSize;

            // in base 64, one or two padding character are possible
            int padding = 0;
            if (digitType == StonChartype.Base64)
            {
                if (TryAndSkip(StonChartype.Base64Padding)) padding += 2;
                if (TryAndSkip(StonChartype.Base64Padding)) padding += 2;
                if (Peek().HasChartype(StonChartype.Base64Padding)) throw new StonParsingException(Position, Line, Column, "Only one or two padding characters are allowed at the end of base 64 binary literal.");
            }
            bitsLength -= padding;

            int offset = bitsLength % 8;
            if (offset == 0) offset = 8;
            offset -= charSize;

            int value = 0;

            // converting the digits into bits
            // which are then gathered into bytes and converted into base 16 digit pairs

            var contentBuilder = new StringBuilder();
            if (minus) contentBuilder.Append('-');

            bool isBase64 = digitType == StonChartype.Base64;

            foreach (int c in builder.ToString())
            {
                int cval = isBase64 ? c.GetBase64Value() : c - '0';

                if (offset >= 0) value |= cval << offset;
                else value |= cval >> -offset;

                if (offset <= 0)
                {
                    contentBuilder.Append(((value >> 4) & 0xf).GetBase16Symbol());
                    contentBuilder.Append((value & 0xf).GetBase16Symbol());
                    value = 0;
                    offset += 8;

                    if (offset < 8) value |= cval << offset;
                }
                offset -= charSize;
            }

            return contentBuilder.ToString();
        }

        #endregion

        #region Number literals reading

        /// <summary>
        /// Reads a content string from a number literal and passes the content.
        /// It is assumed that the possible minus sign was read earlier.
        /// After that, advances the reader to the next significant character.
        /// </summary>
        /// <param name="minus">Whether the number value is negative or not.</param>
        /// <returns>The content string of the number literal.</returns>
        public string ReadNumberContent(bool minus)
        {
            var significandBuilder = new StringBuilder();
            if (minus) significandBuilder.Append('-');

            int postDecimalCount = 0;       // tracks how many digits follow the decimal point
            int trailingZerosCount = -1;     // tracks how many zeros follow the last non-zero digit before the exponent

            // reading the significand digits before the decimal point
            while (Peek().HasChartype(StonChartype.Digit)) { ReadSignificandDigit(significandBuilder, ref trailingZerosCount); }

            if (TryAndSkip(StonChartype.DecimalPoint))
            {
                // at least one digit must follow the decimal point
                if (!Peek().HasChartype(StonChartype.Digit)) throw MakeUnexpectedCharacterException(StonChartype.Digit);
                // reading the significand digits after the decimal point
                while (Peek().HasChartype(StonChartype.Digit))
                {
                    ++postDecimalCount;
                    ReadSignificandDigit(significandBuilder, ref trailingZerosCount);
                }
            }

            // the significand is zero point zero
            if (significandBuilder.Length == (minus ? 1 : 0))
            {
                // with significand being zero, exponent doesn't matter anymore
                // but still must be read so that it's not mistaken for another token
                if (TryAndSkip(StonChartype.Exponent))
                {
                    TryAndSkip(StonChartype.Sign);
                    if (!Peek().HasChartype(StonChartype.Digit)) throw MakeUnexpectedCharacterException(StonChartype.Digit);
                    while (TryAndSkip(StonChartype.Digit)) { }
                }

                return "0";
            }

            // the number subtracted from the exponent
            // based on the digits after the decimal point, as well as the trailing zeros
            int exponentDiff = postDecimalCount - trailingZerosCount;

            // reading the exponent
            if (TryAndSkip(StonChartype.Exponent))
            {
                var exponentBuilder = new StringBuilder();

                if (Peek().HasChartype(StonChartype.Sign))
                {
                    if (ReadAndSkip() == '-') exponentBuilder.Append('-');
                }
                // the exponent must have at least one digit
                if (!Peek().HasChartype(StonChartype.Digit)) throw MakeUnexpectedCharacterException(StonChartype.Digit);

                // skipping the leading zeros
                while (TryAndSkip(StonChartype.ZeroDigit)) { }
                // adding the subsequent exponent digits, including the last zeros
                while (Peek().HasChartype(StonChartype.Digit)) exponentBuilder.Append((char)ReadAndSkip());

                // if declared exponent is non-zero, the exponent is calculated as a numeric string difference
                if (exponentBuilder.Length > 0 && (exponentBuilder.Length > 1 || exponentBuilder[0] != '-'))
                {
                    return significandBuilder.ToString() + "e" + NumericStringCalculator.Subtract(exponentBuilder.ToString(), exponentDiff);
                }
            }

            // the exponent was omitted or declared as zero
            return significandBuilder.ToString() + "e" + (-exponentDiff).ToString();
        }

        // reads a significand digit, taking the number of trailing zeros into account
        private void ReadSignificandDigit(StringBuilder builder, ref int trailingZerosCount)
        {
            // reading the trailing zeros
            if (Peek().HasChartype(StonChartype.ZeroDigit))
            {
                if (trailingZerosCount > -1) ++trailingZerosCount;
            }
            else
            {
                // the zeros read so far aren't, in fact, the trailing zeros
                if (trailingZerosCount > 0) builder.Append(new string('0', trailingZerosCount));

                trailingZerosCount = 0;
                builder.Append((char)Peek());
            }
            ReadAndSkip();
        }

        #endregion

        /// <summary>
        /// Reads a collection type suffix.
        /// After that, advances the reader to the next significant character.
        /// If full suffix is expected, return whether it has been read correctly or not.
        /// </summary>
        /// <param name="requireFull">Whether the collection suffix must use the full form to be valid.</param>
        /// <returns>True if a valid collection type suffix was read, false otherwise.</returns>
        public bool ReadCollectionTypeSuffix(bool requireFull)
        {
            ExpectAndSkip(StonChartype.CollectionSuffixBegin);
            if (InnerReader.Peek().HasChartype(StonChartype.CollectionSuffixEnd))
            {
                // what seems like collection type suffix is in fact an empty collection
                if (requireFull) return false;
                else
                {
                    // a short collection type suffix is found
                    ReadAndSkip();
                    return true;
                }
            }

            // what seems like collection type suffix is in fact a non-empty collection
            if (!InnerReader.Peek().HasChartype(StonChartype.CollectionSuffixContinue) && requireFull) return false;
            
            // with collection suffix continuing symbols it is either collection type suffix or nothing
            while (TryAndSkip(StonChartype.CollectionSuffixContinue)) { }
            ExpectAndSkip(StonChartype.CollectionSuffixEnd);
            return true;
        }

        #region Exception handling

        /// <summary>
        /// Creates a new STON unexpected character exception for a given expected character type, using the current position and character.
        /// </summary>
        /// <param name="expectedChartype">The expected character type.</param>
        /// <returns>The unexpected character exception to handle.</returns>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public StonUnexpectedCharacterParsingException MakeUnexpectedCharacterException(StonChartype expectedChartype)
            => new StonUnexpectedCharacterParsingException(Position, Line, Column, Peek(), expectedChartype);

        /// <summary>
        /// Creates a new STON unexpected character exception for a given expected character type, using the current position and character. Additionally, a custom message is provided.
        /// </summary>
        /// <param name="expectedChartype">The expected character type.</param>
        /// <param name="message">The custom message.</param>
        /// <returns>The unexpected character exception to handle.</returns>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public StonUnexpectedCharacterParsingException MakeUnexpectedCharacterException(StonChartype expectedChartype, string message)
            => new StonUnexpectedCharacterParsingException(Position, Line, Column, Peek(), expectedChartype, message);

        /// <summary>
        /// Creates a new STON unexpected character exception for a given expected character type, using the current position and character. Additionally, a custom message and inner exception is provided.
        /// </summary>
        /// <param name="expectedChartype">The expected character type.</param>
        /// <param name="message">The custom message.</param>
        /// <param name="innerException">The inner exception.</param>
        /// <returns>The unexpected character exception to handle.</returns>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public StonUnexpectedCharacterParsingException MakeUnexpectedCharacterException(StonChartype expectedChartype, string message, Exception innerException)
            => new StonUnexpectedCharacterParsingException(Position, Line, Column, Peek(), expectedChartype, message, innerException);

        #endregion
    }
}

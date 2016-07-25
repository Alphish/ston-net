using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston.Tokenization
{
    /// <summary>
    /// Defines the STON character types of specific characters.
    /// Also, provides functionality to check characters for their types, as well as obtaining character values in different bases.
    /// </summary>
    public static class StonChartable
    {
        // predefined array of STON character types
        private static readonly StonChartype[] Chartypes =
        {
            /* 00  NUL */    StonChartype.EOS,
            /* 01  SOH */    StonChartype.None,
            /* 02  STX */    StonChartype.None,
            /* 03  ETX */    StonChartype.None,
            /* 04  EOT */    StonChartype.None,
            /* 05  ENQ */    StonChartype.None,
            /* 06  ACK */    StonChartype.None,
            /* 07  BEL */    StonChartype.None,
            /* 08  BS  */    StonChartype.None,
            /* 09  HT  */    StonChartype.Whitespace,
            /* 0A  LF  */    StonChartype.Whitespace | StonChartype.Newline,
            /* 0B  VT  */    StonChartype.None,
            /* 0C  FF  */    StonChartype.None,
            /* 0D  CR  */    StonChartype.Whitespace | StonChartype.Newline,
            /* 0E  SO  */    StonChartype.None,
            /* 0F  SI  */    StonChartype.None,
            /* 10  DLE */    StonChartype.None,
            /* 11  DC1 */    StonChartype.None,
            /* 12  DC2 */    StonChartype.None,
            /* 13  DC3 */    StonChartype.None,
            /* 14  DC4 */    StonChartype.None,
            /* 15  NAK */    StonChartype.None,
            /* 16  SYN */    StonChartype.None,
            /* 17  ETB */    StonChartype.None,
            /* 18  CAN */    StonChartype.None,
            /* 19  EM  */    StonChartype.None,
            /* 1A  SUB */    StonChartype.None,
            /* 1B  ESC */    StonChartype.None,
            /* 1C  FS  */    StonChartype.None,
            /* 1D  GS  */    StonChartype.None,
            /* 1E  RS  */    StonChartype.None,
            /* 1F  US  */    StonChartype.None,
            /* 20      */    StonChartype.Whitespace,
            /* 21  !   */    StonChartype.Extension,
            /* 22  "   */    StonChartype.TextDelimiter,
            /* 23  #   */    StonChartype.PathSegmentCollection,
            /* 24  $   */    StonChartype.AddressBegin,
            /* 25  %   */    StonChartype.None,
            /* 26  &   */    StonChartype.IdentifierSigil,
            /* 27  '   */    StonChartype.TextDelimiter,
            /* 28  (   */    StonChartype.ConstructionOpen,
            /* 29  )   */    StonChartype.ConstructionClose,
            /* 2A  *   */    StonChartype.AddressBegin | StonChartype.CommentDiscern,
            /* 2B  +   */    StonChartype.Sign | StonChartype.StringChainContinue,
            /* 2C  ,   */    StonChartype.SequenceSeparator,
            /* 2D  -   */    StonChartype.Sign | StonChartype.Base64,
            /* 2E  .   */    StonChartype.DecimalPoint | StonChartype.NameSeparator | StonChartype.PathSegmentBegin | StonChartype.CollectionSuffixContinue,
            /* 2F  /   */    StonChartype.CommentOpen | StonChartype.CommentDiscern,
            /* 30  0   */    StonChartype.ZeroDigit | StonChartype.Base2 | StonChartype.Base8 | StonChartype.Base16 | StonChartype.Base64,
            /* 31  1   */    StonChartype.NonZeroDigit | StonChartype.Base2 | StonChartype.Base8 | StonChartype.Base16 | StonChartype.Base64,
            /* 32  2   */    StonChartype.NonZeroDigit | StonChartype.Base8 | StonChartype.Base16 | StonChartype.Base64,
            /* 33  3   */    StonChartype.NonZeroDigit | StonChartype.Base8 | StonChartype.Base16 | StonChartype.Base64,
            /* 34  4   */    StonChartype.NonZeroDigit | StonChartype.Base8 | StonChartype.Base16 | StonChartype.Base64,
            /* 35  5   */    StonChartype.NonZeroDigit | StonChartype.Base8 | StonChartype.Base16 | StonChartype.Base64,
            /* 36  6   */    StonChartype.NonZeroDigit | StonChartype.Base8 | StonChartype.Base16 | StonChartype.Base64,
            /* 37  7   */    StonChartype.NonZeroDigit | StonChartype.Base8 | StonChartype.Base16 | StonChartype.Base64,
            /* 38  8   */    StonChartype.NonZeroDigit | StonChartype.Base16 | StonChartype.Base64,
            /* 39  9   */    StonChartype.NonZeroDigit | StonChartype.Base16 | StonChartype.Base64,
            /* 3A  :   */    StonChartype.ValuePrompt,
            /* 3B  ;   */    StonChartype.None,
            /* 3C  <   */    StonChartype.TypeOpen,
            /* 3D  =   */    StonChartype.IdentifierAssign | StonChartype.Base64Padding,
            /* 3E  >   */    StonChartype.TypeClose | StonChartype.StringChainOpen | StonChartype.StringChainContinue,
            /* 3F  ?   */    StonChartype.None,
            /* 40  @   */    StonChartype.AddressBegin,
            /* 41  A   */    StonChartype.Letter | StonChartype.Base16 | StonChartype.Base64,
            /* 42  B   */    StonChartype.Letter | StonChartype.BaseIdentifier | StonChartype.Base16 | StonChartype.Base64,
            /* 43  C   */    StonChartype.Letter | StonChartype.Base16 | StonChartype.Base64,
            /* 44  D   */    StonChartype.Letter | StonChartype.Base16 | StonChartype.Base64,
            /* 45  E   */    StonChartype.Letter | StonChartype.Exponent | StonChartype.Base16 | StonChartype.Base64,
            /* 46  F   */    StonChartype.Letter | StonChartype.Base16 | StonChartype.Base64,
            /* 47  G   */    StonChartype.Letter | StonChartype.Base64,
            /* 48  H   */    StonChartype.Letter | StonChartype.Base64,
            /* 49  I   */    StonChartype.Letter | StonChartype.Base64,
            /* 4A  J   */    StonChartype.Letter | StonChartype.Base64,
            /* 4B  K   */    StonChartype.Letter | StonChartype.Base64,
            /* 4C  L   */    StonChartype.Letter | StonChartype.Base64,
            /* 4D  M   */    StonChartype.Letter | StonChartype.Base64,
            /* 4E  N   */    StonChartype.Letter | StonChartype.BaseIdentifier | StonChartype.Base64,
            /* 4F  O   */    StonChartype.Letter | StonChartype.BaseIdentifier | StonChartype.Base64,
            /* 50  P   */    StonChartype.Letter | StonChartype.Base64,
            /* 51  Q   */    StonChartype.Letter | StonChartype.Base64,
            /* 52  R   */    StonChartype.Letter | StonChartype.Base64,
            /* 53  S   */    StonChartype.Letter | StonChartype.Base64,
            /* 54  T   */    StonChartype.Letter | StonChartype.Base64,
            /* 55  U   */    StonChartype.Letter | StonChartype.Base64,
            /* 56  V   */    StonChartype.Letter | StonChartype.Base64,
            /* 57  W   */    StonChartype.Letter | StonChartype.Base64,
            /* 58  X   */    StonChartype.Letter | StonChartype.BaseIdentifier | StonChartype.Base64,
            /* 59  Y   */    StonChartype.Letter | StonChartype.Base64,
            /* 5A  Z   */    StonChartype.Letter | StonChartype.BaseIdentifier | StonChartype.Base64,
            /* 5B  [   */    StonChartype.IndexOpen | StonChartype.CollectionInitOpen | StonChartype.PathSegmentBegin | StonChartype.CollectionSuffixBegin,
            /* 5C  \   */    StonChartype.StringEscapeOpen,
            /* 5D  ]   */    StonChartype.IndexClose | StonChartype.CollectionInitClose | StonChartype.CollectionSuffixEnd,
            /* 5E  ^   */    StonChartype.AddressBegin | StonChartype.AncestorAccess,
            /* 5F  _   */    StonChartype.Underscore | StonChartype.Base64,
            /* 60  `   */    StonChartype.CodeDelimiter,
            /* 61  a   */    StonChartype.Letter | StonChartype.Base16 | StonChartype.Base64,
            /* 62  b   */    StonChartype.Letter | StonChartype.BaseIdentifier | StonChartype.Base16 | StonChartype.Base64,
            /* 63  c   */    StonChartype.Letter | StonChartype.Base16 | StonChartype.Base64,
            /* 64  d   */    StonChartype.Letter | StonChartype.Base16 | StonChartype.Base64,
            /* 65  e   */    StonChartype.Letter | StonChartype.Exponent | StonChartype.Base16 | StonChartype.Base64,
            /* 66  f   */    StonChartype.Letter | StonChartype.Base16 | StonChartype.Base64,
            /* 67  g   */    StonChartype.Letter | StonChartype.Base64,
            /* 68  h   */    StonChartype.Letter | StonChartype.Base64,
            /* 69  i   */    StonChartype.Letter | StonChartype.Base64,
            /* 6A  j   */    StonChartype.Letter | StonChartype.Base64,
            /* 6B  k   */    StonChartype.Letter | StonChartype.Base64,
            /* 6C  l   */    StonChartype.Letter | StonChartype.Base64,
            /* 6D  m   */    StonChartype.Letter | StonChartype.Base64,
            /* 6E  n   */    StonChartype.Letter | StonChartype.BaseIdentifier | StonChartype.Base64,
            /* 6F  o   */    StonChartype.Letter | StonChartype.BaseIdentifier | StonChartype.Base64,
            /* 70  p   */    StonChartype.Letter | StonChartype.Base64,
            /* 71  q   */    StonChartype.Letter | StonChartype.Base64,
            /* 72  r   */    StonChartype.Letter | StonChartype.Base64,
            /* 73  s   */    StonChartype.Letter | StonChartype.Base64,
            /* 74  t   */    StonChartype.Letter | StonChartype.Base64,
            /* 75  u   */    StonChartype.Letter | StonChartype.Base64,
            /* 76  v   */    StonChartype.Letter | StonChartype.Base64,
            /* 77  w   */    StonChartype.Letter | StonChartype.Base64,
            /* 78  x   */    StonChartype.Letter | StonChartype.BaseIdentifier | StonChartype.Base64,
            /* 79  y   */    StonChartype.Letter | StonChartype.Base64,
            /* 7A  z   */    StonChartype.Letter | StonChartype.BaseIdentifier | StonChartype.Base64,
            /* 7B  {   */    StonChartype.MemberInitOpen,
            /* 7C  |   */    StonChartype.UnionTypeSeparator,
            /* 7D  }   */    StonChartype.MemberInitClose,
            /* 7E  ~   */    StonChartype.None,
            /* 7F  DEL */    StonChartype.None,
        };

        /// <summary>
        /// Passes the STON character type associated with the given character.
        /// </summary>
        /// <param name="c">The character to get the type of.</param>
        /// <returns>The character type.</returns>
        public static StonChartype GetChartype(this char c)
        {
            if (c >= 128) return StonChartype.None;
            else return Chartypes[c];
        }

        /// <summary>
        /// Passes the STON character type associated with the given character.
        /// </summary>
        /// <param name="c">The character to get the type of.</param>
        /// <returns>The character type.</returns>
        public static StonChartype GetChartype(this int c)
        {
            if (c == -1) return Chartypes[0];
            else if (c < -1 || c >= 128) return StonChartype.None;
            else return Chartypes[c];
        }

        /// <summary>
        /// Checks whether the given character is of specific STON character type.
        /// </summary>
        /// <param name="c">The character to check the type of.</param>
        /// <param name="chartype">The character type.</param>
        /// <returns>True if the character matches the specified type, false otherwise.</returns>
        public static bool HasChartype(this char c, StonChartype chartype)
        {
            return (c.GetChartype() & chartype) > 0;
        }

        /// <summary>
        /// Checks whether the given character is of specific STON character type.
        /// </summary>
        /// <param name="c">The character to check the type of.</param>
        /// <param name="chartype">The character type.</param>
        /// <returns>True if the character matches the specified type, false otherwise.</returns>
        public static bool HasChartype(this int c, StonChartype chartype)
        {
            return (c.GetChartype() & chartype) > 0;
        }

        // predefined mapping between characters and their base 16 values
        private static readonly byte[] Base16Values =
        {
              0,  0,  0,  0,   0,  0,  0,  0,   0,  0,  0,  0,   0,  0,  0,  0,
              0,  0,  0,  0,   0,  0,  0,  0,   0,  0,  0,  0,   0,  0,  0,  0,

              0,  0,  0,  0,   0,  0,  0,  0,   0,  0,  0,  0,   0,  0,  0,  0,
              0,  1,  2,  3,   4,  5,  6,  7,   8,  9,  0,  0,   0,  0,  0,  0,

              0, 10, 11, 12,  13, 14, 15,  0,   0,  0,  0,  0,   0,  0,  0,  0,
              0,  0,  0,  0,   0,  0,  0,  0,   0,  0,  0,  0,   0,  0,  0,  0,

              0, 10, 11, 12,  13, 14, 15,  0,   0,  0,  0,  0,   0,  0,  0,  0,
              0,  0,  0,  0,   0,  0,  0,  0,   0,  0,  0,  0,   0,  0,  0,  0,
        };

        /// <summary>
        /// Gets the base 16 value associated with a given digit.
        /// </summary>
        /// <param name="c">The digit.</param>
        /// <returns>The value of the digit.</returns>
        public static byte GetBase16Value(this int c)
        {
            return Base16Values[c];
        }

        /// <summary>
        /// Gets the base 16 value associated with a given digit.
        /// </summary>
        /// <param name="c">The digit.</param>
        /// <returns>The value of the digit.</returns>
        public static byte GetBase16Value(this char c)
        {
            return Base16Values[c];
        }

        // predefined mapping between characters and their base 64 values
        private static readonly byte[] Base64Values =
        {
              0,  0,  0,  0,   0,  0,  0,  0,   0,  0,  0,  0,   0,  0,  0,  0,
              0,  0,  0,  0,   0,  0,  0,  0,   0,  0,  0,  0,   0,  0,  0,  0,

              0,  0,  0,  0,   0,  0,  0,  0,   0,  0,  0,  0,   0, 62,  0,  0,
             52, 53, 54, 55,  56, 57, 58, 59,  60, 61,  0,  0,   0,  0,  0,  0,

              0,  0,  1,  2,   3,  4,  5,  6,   7,  8,  9, 10,  11, 12, 13, 14,
             15, 16, 17, 18,  19, 20, 21, 22,  23, 24, 25,  0,   0,  0,  0, 63,

              0, 26, 27, 28,  29, 30, 31, 32,  33, 34, 35, 36,  37, 38, 39, 40,
             41, 42, 43, 44,  45, 46, 47, 48,  49, 50, 51,  0,   0,  0,  0,  0,
        };

        /// <summary>
        /// Gets the base 64 value associated with a given digit.
        /// </summary>
        /// <param name="c">The digit.</param>
        /// <returns>The value of the digit.</returns>
        public static byte GetBase64Value(this char c)
        {
            return Base64Values[c];
        }

        /// <summary>
        /// Gets the base 64 value associated with a given digit.
        /// </summary>
        /// <param name="c">The digit.</param>
        /// <returns>The value of the digit.</returns>
        public static byte GetBase64Value(this int c)
        {
            return Base64Values[c];
        }

        // predefined sequence of STON base 16 digits
        private const string Base16Symbols = "0123456789abcdef";

        /// <summary>
        /// Gets the base 16 digit associated with a given value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The base 16 digit.</returns>
        public static char GetBase16Symbol(this int value)
        {
            return Base16Symbols[value];
        }

        // predefined sequence of STON base 64 digits
        private const string Base64Symbols = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";

        /// <summary>
        /// Gets the base 64 digit associated with a given value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The base 16 digit.</returns>
        public static char GetBase64Symbol(this int value)
        {
            return Base64Symbols[value];
        }
    }
}

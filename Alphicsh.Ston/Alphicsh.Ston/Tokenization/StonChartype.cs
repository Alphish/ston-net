using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston.Tokenization
{
    /// <summary>
    /// Describes the role of a character in STON language.
    /// </summary>
    [Flags]
    public enum StonChartype : long
    {
        /// <summary>
        /// The character has no role.
        /// </summary>
        None = 0,

        /// <summary>
        /// The character marks end of STON string.
        /// </summary>
        EOS = 1,

        //WHITESPACES

        /// <summary>
        /// The character is a whitespace.
        /// </summary>
        Whitespace = EOS << 1,
        /// <summary>
        /// The character begins a new line.
        /// </summary>
        Newline = Whitespace << 1,
        /// <summary>
        /// The character starts a comment.
        /// </summary>
        CommentOpen = Newline << 1,
        /// <summary>
        /// The character determines if the comment is a single line or multiple line comment.
        /// </summary>
        CommentDiscern = CommentOpen << 1,

        //IDENTIFIERS

        /// <summary>
        /// The character begins a global identifier declaration.
        /// </summary>
        IdentifierSigil = CommentDiscern << 1,
        /// <summary>
        /// The character ends a global identifier declaration.
        /// </summary>
        IdentifierAssign = IdentifierSigil << 1,

        //TYPES

        /// <summary>
        /// The character opens a type definition or type wrapping.
        /// </summary>
        TypeOpen = IdentifierAssign << 1,
        /// <summary>
        /// The character closes a type definition or type wrapping.
        /// </summary>
        TypeClose = TypeOpen << 1,
        /// <summary>
        /// The character begins a collection type suffix.
        /// </summary>
        CollectionSuffixBegin = TypeClose << 1,
        /// <summary>
        /// The character appears in the middle of a collection type suffix.
        /// </summary>
        CollectionSuffixContinue = CollectionSuffixBegin << 1,
        /// <summary>
        /// The character ends a collection type suffix.
        /// </summary>
        CollectionSuffixEnd = CollectionSuffixContinue << 1,
        /// <summary>
        /// The character separates permitted types in the union type.
        /// </summary>
        UnionTypeSeparator = CollectionSuffixEnd << 1,

        //NAMES

        /// <summary>
        /// The character is a letter.
        /// </summary>
        Letter = UnionTypeSeparator << 1,
        /// <summary>
        /// The character is an underscore.
        /// </summary>
        Underscore = Letter << 1,
        /// <summary>
        /// The character separates CANUN names in CANUN path.
        /// </summary>
        NameSeparator = Underscore << 1,

        /// <summary>
        /// The character can be used as a Common Alpha-Numeric and Underscore Name beginning.
        /// </summary>
        CanunBegin = Letter | Underscore,
        /// <summary>
        /// The character can appear in a Common Alpha-Numeric and Underscore Name middle or end.
        /// </summary>
        CanunContinue = CanunBegin | Digit,

        //STRINGS

        /// <summary>
        /// The character is used as a text literal delimiter.
        /// </summary>
        TextDelimiter = NameSeparator << 1,
        /// <summary>
        /// The character is used as a code literal delimiter.
        /// </summary>
        CodeDelimiter = TextDelimiter << 1,
        /// <summary>
        /// The character begins an escape sequence within a string.
        /// </summary>
        StringEscapeOpen = CodeDelimiter << 1,
        /// <summary>
        /// The character can be used to start a string chain.
        /// </summary>
        StringChainOpen = StringEscapeOpen << 1,
        /// <summary>
        /// The character can be used to continue a string chain.
        /// </summary>
        StringChainContinue = StringChainOpen << 1,

        //NUMERIC

        /// <summary>
        /// The character is a zero digit.
        /// </summary>
        ZeroDigit = StringChainContinue << 1,
        /// <summary>
        /// The character is a decimal digit other than zero.
        /// </summary>
        NonZeroDigit = ZeroDigit << 1,
        /// <summary>
        /// The character identifies one of available binary literal bases.
        /// </summary>
        BaseIdentifier = NonZeroDigit << 1,
        /// <summary>
        /// The character is a valid base 2 digit.
        /// </summary>
        Base2 = BaseIdentifier << 1,
        /// <summary>
        /// The character is a valid base 8 digit.
        /// </summary>
        Base8 = Base2 << 1,
        /// <summary>
        /// The character is a valid base 16 digit.
        /// </summary>
        Base16 = Base8 << 1,
        /// <summary>
        /// The character is a valid base 64 digit.
        /// </summary>
        Base64 = Base16 << 1,
        /// <summary>
        /// The character can be used as a base 64 binary literal padding.
        /// </summary>
        Base64Padding = Base64 << 1,
        /// <summary>
        /// The character determines a number sign.
        /// </summary>
        Sign = Base64Padding << 1,
        /// <summary>
        /// The character is a decimal point.
        /// </summary>
        DecimalPoint = Sign << 1,
        /// <summary>
        /// The character marks the beginning of an exponent.
        /// </summary>
        Exponent = DecimalPoint << 1,

        /// <summary>
        /// The character is a decimal digit.
        /// </summary>
        Digit = ZeroDigit | NonZeroDigit,

        //COMPLEX

        /// <summary>
        /// The character opens a complex entity construction definition.
        /// </summary>
        ConstructionOpen = Exponent << 1,
        /// <summary>
        /// The character closes a complex entity construction definition.
        /// </summary>
        ConstructionClose = ConstructionOpen << 1,
        /// <summary>
        /// The character opens a complex entity member initialization definition.
        /// </summary>
        MemberInitOpen = ConstructionClose << 1,
        /// <summary>
        /// The character closes a complex entity member initialization definition.
        /// </summary>
        MemberInitClose = MemberInitOpen << 1,
        /// <summary>
        /// The character opens an index parameters list.
        /// </summary>
        IndexOpen = MemberInitClose << 1,
        /// <summary>
        /// The character closes an index parameters list.
        /// </summary>
        IndexClose = IndexOpen << 1,
        /// <summary>
        /// The character marks where a value is expected.
        /// </summary>
        ValuePrompt = IndexClose << 1,
        /// <summary>
        /// The character opens a complex entity collection initialization definition.
        /// </summary>
        CollectionInitOpen = ValuePrompt << 1,
        /// <summary>
        /// The character closes a complex entity collection initialization definition.
        /// </summary>
        CollectionInitClose = CollectionInitOpen << 1,

        //REFERENCES

        /// <summary>
        /// The character can be used at the beginning of a reference address initial context.
        /// </summary>
        AddressBegin = CollectionInitClose << 1,
        /// <summary>
        /// The character can be used at the beginning of a reference address path segment.
        /// </summary>
        PathSegmentBegin = AddressBegin << 1,
        /// <summary>
        /// The character marks that a given path segment is a collection element path segment.
        /// </summary>
        PathSegmentCollection = PathSegmentBegin << 1,
        /// <summary>
        /// The character is used to access an ancestor context.
        /// </summary>
        AncestorAccess = PathSegmentCollection << 1,

        //MISCELLANEOUS

        /// <summary>
        /// The character is an extension.
        /// </summary>
        Extension = AncestorAccess << 1,
        /// <summary>
        /// The character separates elements in a sequence. It can also terminate the sequence.
        /// </summary>
        SequenceSeparator = Extension << 1,
    }
}

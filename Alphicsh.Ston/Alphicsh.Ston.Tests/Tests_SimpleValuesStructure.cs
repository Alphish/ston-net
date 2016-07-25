using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Alphicsh.Ston.Tests
{
    using Tokenization;

    /// <summary>
    /// Collection of tests for STON simple values structure.
    /// </summary>
    [TestClass]
    public class Tests_SimpleValuesStructure
    {
        // checks that a given STON text parses to a simple value with the given data type and content string
        private void ExpectParsedValue(string ston, StonDataType dataType, string content)
        {
            var entity = RegularStonReader.Default.ParseEntity(ston) as IStonSimpleEntity;
            Assert.AreEqual(dataType, entity.Value.DataType);
            Assert.AreEqual(content, entity.Value.Content);
        }

        // checks that a given data type and content string form a valid simple value of a given canonical form
        private void ExpectValidValue(StonDataType dataType, string content, string canonicalForm)
        {
            try
            {
                new StonSimpleEntity(new StonSimpleValue(dataType, content)).ToCanonicalForm();
            }
            catch (StonException ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        // checks that a given data type and content string form an invalid simple value
        // and that the validation throws a specific error message
        private void ExpectInvalidValue(StonDataType dataType, string content, string message)
        {
            try
            {
                new StonSimpleEntity(new StonSimpleValue(dataType, content));
                Assert.Fail("The value is valid. This should *not* have happened.");
            }
            catch (StonException ex)
            {
                Assert.AreEqual(message, ex.Message);
            }
        }

        /// <summary>
        /// Test cases related to null simple values.
        /// </summary>
        [TestMethod]
        public void Test_NullValues()
        {
            AggregateTester.New()

                .Add("null", () => ExpectParsedValue("null", StonDataType.Null, null))
                .Add("Null", () => ExpectParsedValue("Null", StonDataType.Named, "Null"))
                .Add("NULL", () => ExpectParsedValue("NULL", StonDataType.Named, "NULL"))
                .Add("some null", () => ExpectInvalidValue(StonDataType.Null, "some", "A null simple value must be represented with a non-existing or empty content."))
                
                .Run();

            // tests of null or empty null are in Test_NullEmptyContent
        }

        /// <summary>
        /// Test cases related to number simple values.
        /// </summary>
        [TestMethod]
        public void Test_NumberValues()
        {
            AggregateTester.New()

                // lots and lots of zeros
                .Add("0", () => ExpectParsedValue("0", StonDataType.Number, "0"))
                .Add("-0", () => ExpectParsedValue("-0", StonDataType.Number, "0"))
                .Add("+000.000", () => ExpectParsedValue("000.000", StonDataType.Number, "0"))
                .Add("+0e999", () => ExpectParsedValue("0e999", StonDataType.Number, "0"))
                .Add("000.000e999", () => ExpectParsedValue("000.000e999", StonDataType.Number, "0"))
                .Add("-0e-999", () => ExpectParsedValue("0e-999", StonDataType.Number, "0"))
                .Add("-000.000e-999", () => ExpectParsedValue("000.000e-999", StonDataType.Number, "0"))
                
                // syntactically valid non-zero numbers
                .Add("1", () => ExpectParsedValue("1", StonDataType.Number, "1e0"))
                .Add("1.000", () => ExpectParsedValue("1.000", StonDataType.Number, "1e0"))
                .Add("+0.001", () => ExpectParsedValue("0.001", StonDataType.Number, "1e-3"))
                .Add("-0.0000000000000005e16", () => ExpectParsedValue("-0.0000000000000005e16", StonDataType.Number, "-5e0"))
                .Add("+1000", () => ExpectParsedValue("1000", StonDataType.Number, "1e3"))
                .Add("+1 000 000", () => ExpectParsedValue("1 000 000", StonDataType.Number, "1e6"))
                .Add("0001", () => ExpectParsedValue("0001", StonDataType.Number, "1e0"))
                .Add("1.234e000", () => ExpectParsedValue("1.234e000", StonDataType.Number, "1234e-3"))
                .Add("+1.234e002", () => ExpectParsedValue("1.234e002", StonDataType.Number, "1234e-1"))
                .Add("+1.234e004", () => ExpectParsedValue("1.234e004", StonDataType.Number, "1234e1"))
                .Add("1.234e5678", () => ExpectParsedValue("1.234e5678", StonDataType.Number, "1234e5675"))
                .Add("1000e999...", () => ExpectParsedValue("1000e999999999999999999999999", StonDataType.Number, "1e1000000000000000000000002"))
                .Add("1000e-1000...", () => ExpectParsedValue("1000e-1000000000000000000000000", StonDataType.Number, "1e-999999999999999999999997"))
                .Add("-1000e999...", () => ExpectParsedValue("-1000e999999999999999999999999", StonDataType.Number, "-1e1000000000000000000000002"))
                .Add("-1000e-1000...", () => ExpectParsedValue("-1000e-1000000000000000000000000", StonDataType.Number, "-1e-999999999999999999999997"))
                .Add("-12.34e-1000...", () => ExpectParsedValue("-12.34e-1000000000000000000000000", StonDataType.Number, "-1234e-1000000000000000000000002"))
                .Add("+10100100010000", () => ExpectParsedValue("10100100010000", StonDataType.Number, "1010010001e4"))

                // syntactically invalid numbers
                .Add("-?", () => Expect.UnexpectedCharacter("-?", '?', StonChartype.Digit, 0, 1, 1))
                .Add("++12", () => Expect.UnexpectedCharacter("++12", '+', StonChartype.Digit, 0, 1, 1))
                .Add("-3.e4", () => Expect.UnexpectedCharacter("-3.e4", 'e', StonChartype.Digit, 0, 3, 3))
                .Add("-3.0e?", () => Expect.UnexpectedCharacter("-3.0e?", '?', StonChartype.Digit, 0, 5, 5))
                .Add("-3.0e-?", () => Expect.UnexpectedCharacter("-3.0e-?", '?', StonChartype.Digit, 0, 6, 6))
                .Add("-000.0000e?", () => Expect.UnexpectedCharacter("-000.0000e?", '?', StonChartype.Digit, 0, 10, 10))
                .Add("-000.0000e-?", () => Expect.UnexpectedCharacter("-000.0000e-?", '?', StonChartype.Digit, 0, 11, 11))

                // directly created valid numbers
                .Add("=0", () => ExpectValidValue(StonDataType.Number, "0", "0"))
                .Add("=123e0", () => ExpectValidValue(StonDataType.Number, "123e0", "123e0"))
                .Add("=-123e0", () => ExpectValidValue(StonDataType.Number, "-123e0", "-123e0"))
                .Add("=123e456", () => ExpectValidValue(StonDataType.Number, "123e456", "123e456"))
                .Add("=-123e456", () => ExpectValidValue(StonDataType.Number, "-123e456", "-123e456"))
                .Add("=123e-456", () => ExpectValidValue(StonDataType.Number, "123e-456", "123e-456"))
                .Add("=-123e-456", () => ExpectValidValue(StonDataType.Number, "-123e-456", "-123e-456"))
                .Add("=123e100", () => ExpectValidValue(StonDataType.Number, "123e100", "123e100"))
                .Add("=-123e100", () => ExpectValidValue(StonDataType.Number, "-123e100", "-123e100"))

                // directly created invalid numbers
                .Add("=-", () => ExpectInvalidValue(StonDataType.Number, "-", "A number simple value significand must have at least one digit."))
                .Add("=-0", () => ExpectInvalidValue(StonDataType.Number, "-0", "A number simple value significand cannot have leading zeros."))
                .Add("=e", () => ExpectInvalidValue(StonDataType.Number, "e", "A number simple value significand must be represented with decimal digits."))
                .Add("=-e", () => ExpectInvalidValue(StonDataType.Number, "-e", "A number simple value significand must be represented with decimal digits."))
                .Add("=?", () => ExpectInvalidValue(StonDataType.Number, "?", "A number simple value significand must be represented with decimal digits."))
                .Add("=-?", () => ExpectInvalidValue(StonDataType.Number, "-?", "A number simple value significand must be represented with decimal digits."))
                .Add("=00", () => ExpectInvalidValue(StonDataType.Number, "00", "A number simple value significand cannot have leading zeros."))

                .Add("=123E0", () => ExpectInvalidValue(StonDataType.Number, "123E0", "A number simple value cannot have an uppercase exponent sign."))
                .Add("=123?", () => ExpectInvalidValue(StonDataType.Number, "123?", "A number simple value significand must be represented with decimal digits."))
                .Add("=123", () => ExpectInvalidValue(StonDataType.Number, "123", "A non-zero number simple value must have an exponent."))
                .Add("=1000e0", () => ExpectInvalidValue(StonDataType.Number, "1000e0", "A number simple value significand cannot have trailing zeros."))

                .Add("=123e", () => ExpectInvalidValue(StonDataType.Number, "123e", "A number simple value exponent must have at least one digit."))
                .Add("=123e-", () => ExpectInvalidValue(StonDataType.Number, "123e-", "A number simple value exponent must have at least one digit."))
                .Add("=123e-0", () => ExpectInvalidValue(StonDataType.Number, "123e-0", "A number simple value exponent cannot have leading zeros."))
                .Add("=123e?", () => ExpectInvalidValue(StonDataType.Number, "123e?", "A number simple value exponent must be represented with decimal digits."))
                .Add("=123e-?", () => ExpectInvalidValue(StonDataType.Number, "123e-?", "A number simple value exponent must be represented with decimal digits."))
                .Add("=123e00", () => ExpectInvalidValue(StonDataType.Number, "123e00", "A number simple value exponent cannot have leading zeros."))
                .Add("=123e45?", () => ExpectInvalidValue(StonDataType.Number, "123e45?", "A number simple value exponent must be represented with decimal digits."))

                .Run();
        }

        /// <summary>
        /// Test cases related to binary simple values.
        /// </summary>
        [TestMethod]
        public void Test_BinaryValues()
        {
            AggregateTester.New()

                // syntactically valid values of all possible bases
                .Add("0n", () => ExpectParsedValue("0n", StonDataType.Binary, ""))
                .Add("-0n", () => ExpectParsedValue("-0n", StonDataType.Binary, ""))
                .Add("0   n", () => ExpectParsedValue("0   n", StonDataType.Binary, ""))
                .Add("0b 1010", () => ExpectParsedValue("0b 1010", StonDataType.Binary, "0a"))
                .Add("-0b 1010", () => ExpectParsedValue("-0b 1010", StonDataType.Binary, "-0a"))
                .Add("0b 010010 00100001", () => ExpectParsedValue("0b 010010 00100001", StonDataType.Binary, "1221"))
                .Add("0o 644", () => ExpectParsedValue("0o 644", StonDataType.Binary, "01a4"))
                .Add("-0o 644", () => ExpectParsedValue("-0o 644", StonDataType.Binary, "-01a4"))
                .Add("0o 12345670", () => ExpectParsedValue("0o 12345670", StonDataType.Binary, "29cbb8"))
                .Add("0x Feed Bed", () => ExpectParsedValue("0x Feed Bed", StonDataType.Binary, "0feedbed"))
                .Add("-0x Feed Bed", () => ExpectParsedValue("-0x Feed Bed", StonDataType.Binary, "-0feedbed"))
                .Add("0x 0123456789ABCDEFabcdef", () => ExpectParsedValue("0x 0123456789ABCDEFabcdef", StonDataType.Binary, "0123456789abcdefabcdef"))
                .Add("0x 0000", () => ExpectParsedValue("0x 0000", StonDataType.Binary, "0000"))
                .Add("-0x 0000", () => ExpectParsedValue("-0x 0000", StonDataType.Binary, "-0000"))
                .Add("0z No-padding", () => ExpectParsedValue("0z No-padding", StonDataType.Binary, "0368fa969d7629e0"))
                .Add("-0z No-padding", () => ExpectParsedValue("-0z No-padding", StonDataType.Binary, "-0368fa969d7629e0"))
                .Add("0z One-padding=", () => ExpectParsedValue("0z One-padding=", StonDataType.Binary, "3a77bea5a75d8a78"))
                .Add("0z One-padding", () => ExpectParsedValue("0z One-padding", StonDataType.Binary, "00e9defa969d7629e0"))
                .Add("0z 2-paddingg==", () => ExpectParsedValue("0z 2-paddingg==", StonDataType.Binary, "dbea5a75d8a782"))
                .Add("0z 2-paddingg", () => ExpectParsedValue("0z 2-paddingg", StonDataType.Binary, "0dbea5a75d8a7820"))

                // syntactically invalid binary values
                .Add("0b2", () => Expect.UnexpectedCharacter("0b2", '2', StonChartype.Base2, 0, 2, 2))
                .Add("0o8", () => Expect.UnexpectedCharacter("0o8", '8', StonChartype.Base8, 0, 2, 2))
                .Add("0xG", () => Expect.UnexpectedCharacter("0xG", 'G', StonChartype.Base16, 0, 2, 2))
                .Add("0z", () => Expect.UnexpectedCharacter("0z", -1, StonChartype.Base64, 0, 2, 2))
                .Add("0zZzz===", () => Expect.Exception<StonParsingException>(() => RegularStonReader.Default.ParseEntity("0zZzz==="), "Only one or two padding characters are allowed at the end of base 64 binary literal."))

                // directly created binary values
                .Add("=0", () => ExpectInvalidValue(StonDataType.Binary, "0", "A binary simple value content must have an even number of hexadecimal digits."))
                .Add("=00", () => ExpectValidValue(StonDataType.Binary, "00", "0x00"))
                .Add("=00ff", () => ExpectValidValue(StonDataType.Binary, "00ff", "0x00ff"))
                .Add("=-", () => ExpectInvalidValue(StonDataType.Binary, "-", "An empty binary simple value cannot be negative."))
                .Add("=-0", () => ExpectInvalidValue(StonDataType.Binary, "-0", "A binary simple value content must have an even number of hexadecimal digits."))
                .Add("=-00", () => ExpectValidValue(StonDataType.Binary, "-00", "-0x00"))
                .Add("=-0000", () => ExpectValidValue(StonDataType.Binary, "-0000", "-0x0000"))
                .Add("=feedem", () => ExpectInvalidValue(StonDataType.Binary, "feedem", "A binary simple value content must be represented with hexadecimal digits."))
                .Add("=00FF", () => ExpectInvalidValue(StonDataType.Binary, "00FF", "A binary simple value content cannot have uppercase hexadecimal digits."))

                .Run();
        }

        /// <summary>
        /// Test cases related to named simple values.
        /// </summary>
        [TestMethod]
        public void Test_NamedValues()
        {
            AggregateTester.New()

                // parsed named values
                .Add("true", () => ExpectParsedValue("true", StonDataType.Named, "true"))
                .Add("false", () => ExpectParsedValue("false", StonDataType.Named, "false"))
                .Add("fa lse", () => ExpectParsedValue("fa lse", StonDataType.Named, "lse"))    // a named value "lse" of type "fa"
                .Add("One.Two.Three", () => ExpectParsedValue("One.Two.Three", StonDataType.Named, "One.Two.Three"))
                .Add("One  .Two.  Three", () => ExpectParsedValue("One  .Two.  Three", StonDataType.Named, "One.Two.Three"))
                .Add("One  .  Two  .  Three", () => ExpectParsedValue("One  .  Two  .  Three", StonDataType.Named, "One.Two.Three"))
                .Add("One  .  T  wo  .  Three", () => ExpectParsedValue("One  .  T  wo  .  Three", StonDataType.Named, "wo.Three"))     // a named value "wo.Three" of type "One.T"
                
                // directly created named values
                .Add("=single", () => ExpectValidValue(StonDataType.Named, "single", "single"))
                .Add("=dou.ble", () => ExpectValidValue(StonDataType.Named, "dou.ble", "dou.ble"))
                .Add("=.single", () => ExpectInvalidValue(StonDataType.Named, ".single", "A named simple value must be a valid CANUN path."))
                .Add("=single.", () => ExpectInvalidValue(StonDataType.Named, "single.", "A named simple value must be a valid CANUN path."))
                .Add("=dou..ble", () => ExpectInvalidValue(StonDataType.Named, "dou..ble", "A named simple value must be a valid CANUN path."))
                .Add("=0single", () => ExpectInvalidValue(StonDataType.Named, "0single", "A named simple value must be a valid CANUN path."))
                .Add("=dou.0ble", () => ExpectInvalidValue(StonDataType.Named, "dou.0ble", "A named simple value must be a valid CANUN path."))

                .Run();
        }

        /// <summary>
        /// Test cases related to text and code simple values.
        /// </summary>
        [TestMethod]
        public void Test_StringValues()
        {
            AggregateTester.New()
                // basic parsed string literals
                .Add("\"Hello, world!\"", () => ExpectParsedValue("\"Hello, world!\"", StonDataType.Text, "Hello, world!"))
                .Add("'Hello, world!'", () => ExpectParsedValue("'Hello, world!'", StonDataType.Text, "Hello, world!"))
                .Add("`Hello, world!`", () => ExpectParsedValue("`Hello, world!`", StonDataType.Code, "Hello, world!"))
                .Add("\"Contractions aren't hard!\"", () => ExpectParsedValue("\"Contractions aren't hard!\"", StonDataType.Text, "Contractions aren't hard!"))
                .Add("\"Contractions aren\\'t hard!\"", () => ExpectParsedValue("\"Contractions aren\\'t hard!\"", StonDataType.Text, "Contractions aren't hard!"))
                .Add("'Contractions aren\\'t hard!'", () => ExpectParsedValue("'Contractions aren\\'t hard!'", StonDataType.Text, "Contractions aren't hard!"))
                .Add("`Contractions aren't hard!`", () => ExpectParsedValue("`Contractions aren't hard!`", StonDataType.Code, "Contractions aren't hard!"))
                .Add("\"I'm an \\\"annoyance\\\".\"", () => ExpectParsedValue("\"I'm an \\\"annoyance\\\".\"", StonDataType.Text, "I'm an \"annoyance\"."))
                .Add("'I\\'m an \"annoyance\".'", () => ExpectParsedValue("'I\\'m an \"annoyance\".'", StonDataType.Text, "I'm an \"annoyance\"."))
                .Add("`I'm an \"annoyance\".`", () => ExpectParsedValue("`I'm an \"annoyance\".`", StonDataType.Code, "I'm an \"annoyance\"."))

                // non-regular characters string literals
                .Add("\"私は「面倒」です。\"", () => ExpectParsedValue("\"私は「面倒」です。\"", StonDataType.Text, "私は「面倒」です。"))
                .Add("'私は「面倒」です。'", () => ExpectParsedValue("'私は「面倒」です。'", StonDataType.Text, "私は「面倒」です。"))
                .Add("`私は「面倒」です。`", () => ExpectParsedValue("`私は「面倒」です。`", StonDataType.Code, "私は「面倒」です。"))
                .Add("'\\u79C1\\u306F\\u300C\\u9762\\u5012\\u300D\\u3067\\u3059\\u3002'", () => ExpectParsedValue("'\\u79C1\\u306F\\u300C\\u9762\\u5012\\u300D\\u3067\\u3059\\u3002'", StonDataType.Text, "私は「面倒」です。"))
                .Add("'\\u79c1\\u306f\\u300c\\u9762\\u5012\\u300d\\u3067\\u3059\\u3002'", () => ExpectParsedValue("'\\u79c1\\u306f\\u300c\\u9762\\u5012\\u300d\\u3067\\u3059\\u3002'", StonDataType.Text, "私は「面倒」です。"))
                .Add("'\\'\\\"\\`\\\\\\/\\b\\f\\n\\r\\t\\0'", () => ExpectParsedValue("'\\'\\\"\\`\\\\\\/\\b\\f\\n\\r\\t\\0'", StonDataType.Text, "'\"`\\/\b\f\n\r\t\0"))

                // string literal chains
                .Add(">'a'", () => ExpectParsedValue(">'a'", StonDataType.Text, "a"))
                .Add(">'a'>'b'>'c'", () => ExpectParsedValue(">'a'>'b'>'c'", StonDataType.Text, "a\nb\nc"))
                .Add(">'a'+'b'+'c'", () => ExpectParsedValue(">'a'+'b'+'c'", StonDataType.Text, "abc"))
                .Add(">'a'+'b'>'c'", () => ExpectParsedValue(">'a'+'b'>'c'", StonDataType.Text, "ab\nc"))
                .Add(">'a'>\"b\">'c'", () => ExpectParsedValue(">'a'>\"b\">'c'", StonDataType.Text, "a\nb\nc"))
                .Add(">'a'+\"b\"+'c'", () => ExpectParsedValue(">'a'+\"b\"+'c'", StonDataType.Text, "abc"))
                .Add(">'a'+\"b\">'c'", () => ExpectParsedValue(">'a'+\"b\">'c'", StonDataType.Text, "ab\nc"))

                .Add(">`a`", () => ExpectParsedValue(">'a'", StonDataType.Text, "a"))
                .Add(">`a`>`b`>`c`", () => ExpectParsedValue(">`a`>`b`>`c`", StonDataType.Code, "a\nb\nc"))
                .Add(">`a`+`b`+`c`", () => ExpectParsedValue(">`a`+`b`+`c`", StonDataType.Code, "abc"))
                .Add(">`a`+`b`>`c`", () => ExpectParsedValue(">`a`+`b`>`c`", StonDataType.Code, "ab\nc"))

                // syntactically invalid strings
                .Add("'String that is unfi", () => Expect.UnexpectedCharacter("'String that is unfi", -1, StonChartype.None, 0, 20, 20, "Unexpected end of string encountered when parsing a STON string."))
                .Add("'String that is unfi\0nished.'", () => Expect.UnexpectedCharacter("'String that is unfi\0nished.'", 0, StonChartype.None, 0, 20, 20, "Unexpected end of string encountered when parsing a STON string."))
                .Add("'String with\nnewline'", () => Expect.UnexpectedCharacter("'String with\nnewline'", '\n', StonChartype.None, 0, 12, 12, "Unexpected control character #10. Control characters are not allowed in string literals."))
                .Add("'Escape \\?'", () => Expect.UnexpectedCharacter("'Escape \\?'", '?', StonChartype.None, 0, 9, 9, "Character '?' is not a part of recognized escape sequence."))
                .Add("'Escape \\", () => Expect.UnexpectedCharacter("'Escape \\", -1, StonChartype.None, 0, 9, 9, "Unexpected end of string encountered when parsing a STON string."))
                .Add("'\\u", () => Expect.UnexpectedCharacter("'\\u", -1, StonChartype.Base16, 0, 3, 3, "4 hexadecimal digits expected after Unicode escape code."))
                .Add("'\\u?", () => Expect.UnexpectedCharacter("'\\u?", '?', StonChartype.Base16, 0, 3, 3, "4 hexadecimal digits expected after Unicode escape code."))
                .Add("'\\uB", () => Expect.UnexpectedCharacter("'\\uB", -1, StonChartype.Base16, 0, 4, 4, "4 hexadecimal digits expected after Unicode escape code."))
                .Add("'\\uB?", () => Expect.UnexpectedCharacter("'\\uB?", '?', StonChartype.Base16, 0, 4, 4, "4 hexadecimal digits expected after Unicode escape code."))
                .Add("'\\uBE", () => Expect.UnexpectedCharacter("'\\uBE", -1, StonChartype.Base16, 0, 5, 5, "4 hexadecimal digits expected after Unicode escape code."))
                .Add("'\\uBEE", () => Expect.UnexpectedCharacter("'\\uBEE", -1, StonChartype.Base16, 0, 6, 6, "4 hexadecimal digits expected after Unicode escape code."))

                .Add(">(a)", () => Expect.UnexpectedCharacter(">(a)", '(', StonChartype.TextDelimiter | StonChartype.CodeDelimiter, 0, 1, 1))
                .Add(">'a'>`b`", () => Expect.UnexpectedCharacter(">'a'>`b`", '`', StonChartype.TextDelimiter, 0, 5, 5))
                .Add(">`a`>'b'", () => Expect.UnexpectedCharacter(">`a`>'b'", '\'', StonChartype.CodeDelimiter, 0, 5, 5))


                // canonical representation
                .Add("=text I'm `an \"annoyance\".", () => ExpectValidValue(StonDataType.Text, "I'm `an \"annoyance\".", "\"I'm `an \\\"annoyance\\\"\""))
                .Add("=code I'm `an \"annoyance\".", () => ExpectValidValue(StonDataType.Code, "I'm `an \"annoyance\".", "`I'm \\`an \"annoyance\"`"))
                .Add("=text 私は「面倒」です。", () => ExpectValidValue(StonDataType.Text, "私は「面倒」です。", "\"\\u79c1\\u306f\\u300c\\u9762\\u5012\\u300d\\u3067\\u3059\\u3002\""))
                .Add("=text '\"`\\/\b\f\n\r\t\0", () => ExpectValidValue(StonDataType.Text, " '\"`\\/\b\f\n\r\t\0\u007f", "\" '\\\"`\\\\/\\b\\f\\n\\r\\t\\u0000\\u007f\""))

                .Run();
        }

        /// <summary>
        /// Test cases related to various simple values, in the context of null or empty content strings.
        /// </summary>
        [TestMethod]
        public void Test_NullEmptyContent()
        {
            AggregateTester.New()

                .Add("null null", () => ExpectValidValue(StonDataType.Null, null, "null"))
                .Add("null empty", () => ExpectValidValue(StonDataType.Null, "", "null"))

                .Add("number null", () => ExpectInvalidValue(StonDataType.Number, null, "A non-null simple value must be represented with an existing content."))
                .Add("number empty", () => ExpectInvalidValue(StonDataType.Number, "", "A number simple value cannot be empty."))

                .Add("binary null", () => ExpectInvalidValue(StonDataType.Binary, null, "A non-null simple value must be represented with an existing content."))
                .Add("binary empty", () => ExpectValidValue(StonDataType.Binary, "", "0n"))

                .Add("named null", () => ExpectInvalidValue(StonDataType.Named, null, "A non-null simple value must be represented with an existing content."))
                .Add("named empty", () => ExpectInvalidValue(StonDataType.Named, "", "A named simple value cannot be empty."))

                .Add("text null", () => ExpectInvalidValue(StonDataType.Text, null, "A non-null simple value must be represented with an existing content."))
                .Add("text empty", () => ExpectValidValue(StonDataType.Text, "", "\"\""))

                .Add("code null", () => ExpectInvalidValue(StonDataType.Code, null, "A non-null simple value must be represented with an existing content."))
                .Add("code empty", () => ExpectValidValue(StonDataType.Code, "", "``"))

                .Add("whatever null", () => ExpectInvalidValue((StonDataType)255, null, "A non-null simple value must be represented with an existing content."))
                .Add("whatever empty", () => ExpectInvalidValue((StonDataType)255, "", "Unknown simple value data type."))

                .Run();
        }
    }
}

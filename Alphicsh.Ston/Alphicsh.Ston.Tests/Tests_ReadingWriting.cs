using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.IO;

namespace Alphicsh.Ston.Tests
{
    using Tokenization;

    /// <summary>
    /// Collection of tests for STON reading and writing operations.
    /// </summary>
    [TestClass]
    public class Tests_ReadingWriting
    {
        #region Generic syntax errors

        /// <summary>
        /// Test cases related to syntax errors, not related to a specific STON structure.
        /// </summary>
        [TestMethod]
        public void Test_GenericSyntaxErrors()
        {
            AggregateTester.New()

                // blank documents
                // mostly for testing reader position
                .Add("empty", () => Expect.UnexpectedCharacter("", -1, StonChartype.None, 0, 0, 0))
                .Add("spaces", () => Expect.UnexpectedCharacter("    ", -1, StonChartype.None, 0, 4, 4))
                .Add("tabs", () => Expect.UnexpectedCharacter("\t\t", -1, StonChartype.None, 0, 2, 2))
                .Add("cr", () => Expect.UnexpectedCharacter("\r\r", -1, StonChartype.None, 2, 0, 2))
                .Add("lf", () => Expect.UnexpectedCharacter("\n\n", -1, StonChartype.None, 2, 0, 2))
                .Add("crlf", () => Expect.UnexpectedCharacter("\r\n", -1, StonChartype.None, 1, 0, 2))
                .Add("lfcr", () => Expect.UnexpectedCharacter("\n\r", -1, StonChartype.None, 2, 0, 2))
                .Add("line comment", () => Expect.UnexpectedCharacter("//line", -1, StonChartype.None, 0, 6, 6))
                .Add("block comment", () => Expect.UnexpectedCharacter("/*\nblock\n*/", -1, StonChartype.None, 2, 2, 11))
                .Add("block co", () => Expect.UnexpectedCharacter("/*\nblock", -1, StonChartype.None, 1, 5, 8))
                .Add("almost", () => Expect.UnexpectedCharacter("/*\nalmost\n*", -1, StonChartype.None, 2, 1, 11))
                .Add("no comment", () => Expect.UnexpectedCharacter("/no comment", 'n', StonChartype.CommentDiscern, 0, 1, 1))

                // how unexpected!
                .Add("whatever", () => Expect.UnexpectedCharacter("(Habla español)", 'ñ', position: 11, message: "Nobody expected the Spanish Inquisition."))

                // multiple entities in a single STON text
                .Add("multiple entities", () => Expect.UnexpectedCharacter("{a:0}{b:1}", '{', StonChartype.EOS, 0, 5, 5, "Unexpected character after the valid entity: '{'."))
                .Add("multiple entities in document", () => Expect.Exception<StonUnexpectedCharacterParsingException>(() => RegularStonReader.Default.ParseDocument("{a:0}{b:1}"), "Unexpected character after the valid entity: '{'."))

                .Run();
        }

        #endregion

        #region Values writing

        // checks that a given writing operation of StonTokenWriter causes an ArgumentException
        private void ExpectInvalidArgument(Action<StonTokenWriter> action, string message)
        {
            try
            {
                action(new StonTokenWriter(new StringWriter()));
                Assert.Fail("The action performed without throwing an ArgumentException.");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual(message, ex.Message);
            }
        }

        // checks that a given writing operation of StonTokenWriter produces a correct string
        private void ExpectString(Action<StonTokenWriter> action, string value)
        {
            var innerWriter = new StringWriter() { NewLine = "\n" };
            action(new StonTokenWriter(innerWriter));
            Assert.AreEqual(value, innerWriter.ToString());
        }

        // checks that a given canonical number content is correctly represented in plain notation, non-precise scientific notation and 5-digits precise scientific notation
        private void ExpectNumbers(string canonical, string plain, string scientific, string preciseScientific)
        {
            ExpectString((writer) => writer.WriteCanonicalNumberLiteral(canonical), canonical);
            ExpectString((writer) => writer.WritePlainNumberLiteral(canonical), plain);
            ExpectString((writer) => writer.WriteScientificNumberLiteral(canonical), scientific);
            ExpectString((writer) => writer.WriteScientificNumberLiteral(canonical, 5), preciseScientific);
        }

        /// <summary>
        /// Test cases related to representing values using StonTokenWriter.
        /// </summary>
        [TestMethod]
        public void Test_ValuesTokenWriting()
        {
            AggregateTester.New()

                // newlines
                .Add("newline", () => ExpectString((writer) => writer.WriteLine(), "\n"))
                .Add("c newline", () => ExpectString((writer) => writer.WriteLine('c'), "c\n"))
                .Add("string newline", () => ExpectString((writer) => writer.WriteLine("string"), "string\n"))

                // string literals
                .Add("string delimiter '", () => ExpectString((writer) => writer.WriteStringLiteral("", '"'), "\"\""))
                .Add("string delimiter \"", () => ExpectString((writer) => writer.WriteStringLiteral("", '\''), "''"))
                .Add("string delimiter `", () => ExpectString((writer) => writer.WriteStringLiteral("", '`'), "``"))
                .Add("string delimiter ?", () => ExpectInvalidArgument((writer) => writer.WriteStringLiteral("", '?'), "The given character is not a valid STON text or code literal delimiter."))

                // binary literals
                .Add("binary base a", () => ExpectInvalidArgument((writer) => writer.WriteBinaryLiteral("0123", 'a'), "Unknown base identfier: 'a'"))
                .Add("binary non-empty n", () => ExpectInvalidArgument((writer) => writer.WriteBinaryLiteral("0123", 'n'), "An empty literal base identifier cannot be used for non-empty binary content."))

                .Add("binary binary", () => ExpectString((writer) => writer.WriteBinaryLiteral("0123456789abcdef", 'b'), "0b0000000100100011010001010110011110001001101010111100110111101111"))
                
                .Add("binary misaligned octal", () => ExpectInvalidArgument((writer) => writer.WriteBinaryLiteral("80", 'o'), "The given binary content cannot be represented as a base 8 literal."))
                .Add("binary misaligned octal 2", () => ExpectInvalidArgument((writer) => writer.WriteBinaryLiteral("40", 'o'), "The given binary content cannot be represented as a base 8 literal."))
                .Add("binary misaligned octal 3", () => ExpectInvalidArgument((writer) => writer.WriteBinaryLiteral("8000", 'o'), "The given binary content cannot be represented as a base 8 literal."))
                .Add("binary aligned octal", () => ExpectString((writer) => writer.WriteBinaryLiteral("20", 'o'), "0o40"))
                .Add("binary aligned octal 2", () => ExpectString((writer) => writer.WriteBinaryLiteral("2000", 'o'), "0o20000"))
                .Add("binary aligned octal 3", () => ExpectString((writer) => writer.WriteBinaryLiteral("4000", 'o'), "0o40000"))

                .Add("binary hexadecimal", () => ExpectString((writer) => writer.WriteBinaryLiteral("0123456789abcdef", 'x'), "0x0123456789abcdef"))
                .Add("binary negative hexadecimal", () => ExpectString((writer) => writer.WriteBinaryLiteral("-0123456789abcdef", 'x'), "-0x0123456789abcdef"))
                .Add("binary uppercase", () => ExpectInvalidArgument((writer) => writer.WriteBinaryLiteral("0123456789ABCDEF", 'x'), "A binary simple value content cannot have uppercase hexadecimal digits."))
                .Add("binary unknown", () => ExpectInvalidArgument((writer) => writer.WriteBinaryLiteral("0123456789?", 'x'), "A binary simple value content must be represented with hexadecimal digits."))

                .Add("binary base64 no padding", () => ExpectString((writer) => writer.WriteBinaryLiteral("123456", 'z'), "0zEjRW"))
                .Add("binary base64 one padding", () => ExpectString((writer) => writer.WriteBinaryLiteral("789a", 'z'), "0zeJo="))
                .Add("binary base64 two paddings", () => ExpectString((writer) => writer.WriteBinaryLiteral("bc", 'z'), "0zvA=="))

                // number literals
                .Add("number zero", () => ExpectNumbers("0", "0", "0e0", "0.0000e0"))
                .Add("number one", () => ExpectNumbers("1e0", "1", "1e0", "1.0000e0"))
                .Add("number hundred", () => ExpectNumbers("1e2", "100", "1e2", "1.0000e2"))
                .Add("number over the moon", () => ExpectNumbers("384e3", "384000", "3.84e5", "3.8400e5"))
                .Add("number thousandth", () => ExpectNumbers("1e-3", "0.001", "1e-3", "1.0000e-3"))
                .Add("number 123 thousandths", () => ExpectNumbers("123e-3", "0.123", "1.23e-1", "1.2300e-1"))
                .Add("number 1234567 thousandths", () => ExpectNumbers("1234567e-3", "1234.567", "1.234567e3", "1.234567e3"))

                .Add("minus one", () => ExpectNumbers("-1e0", "-1", "-1e0", "-1.0000e0"))
                .Add("minus hundred", () => ExpectNumbers("-1e2", "-100", "-1e2", "-1.0000e2"))
                .Add("minus over the moon", () => ExpectNumbers("-384e3", "-384000", "-3.84e5", "-3.8400e5"))
                .Add("minus thousandth", () => ExpectNumbers("-1e-3", "-0.001", "-1e-3", "-1.0000e-3"))
                .Add("minus 123 thousandths", () => ExpectNumbers("-123e-3", "-0.123", "-1.23e-1", "-1.2300e-1"))
                .Add("minus 1234567 thousandths", () => ExpectNumbers("-1234567e-3", "-1234.567", "-1.234567e3", "-1.234567e3"))

                .Run();
        }

        #endregion

        #region Canonical form writing

        // checks that a given STON entity is converted to a proper canonical form
        private void ExpectCanonicalForm(string ston, string canonical)
        {
            try
            {
                Assert.AreEqual(canonical, RegularStonReader.Default.ParseEntity(ston).ToCanonicalForm());
            }
            catch (StonException ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        /// <summary>
        /// Test cases related to representing entities with their canonical form.
        /// </summary>
        [TestMethod]
        public void Test_CanonicalWriter()
        {
            AggregateTester.New()
                
                .Add("canonical types", () => ExpectCanonicalForm(
                    "megatype<<param<a,b[]>>|<union|type>|<<union|collection>[]>|!'weird name'>[...] []",
                    "<\"megatype\"<\"param\"<\"a\",\"b\"[]>|<\"union\"|\"type\">|<\"union\"|\"collection\">[]|!\"weird name\">[]>[]"
                    ))
                .Add("canonical complex structure", () => ExpectCanonicalForm(
                    "(a, b, 'parameter x':c, y:d, 'z e e':e)[ one, two, three ]{ mem1: trollface, mem2: slender man, 'mem3 wow': doge, !ext: whatever, [idx,0]: i0, [idy,0]: i1, [idy,1]: i2, [idx,1]: i3 }",
                    "(:a,:b,\"parameter x\":c,\"y\":d,\"z e e\":e){\"mem1\":trollface,\"mem2\":<\"slender\">man,\"mem3 wow\":doge,!\"ext\":whatever,[idx,0]:i0,[idy,0]:i1,[idy,1e0]:i2,[idx,1e0]:i3}[one,two,three]"
                    ))
                .Add("canonical references", () => ExpectCanonicalForm(
                    "{ r1: $.s1 . x . !'etc.', r2: ^* . r3 . ^^ [# 0b10] , r3: @GLOBAL[local], s1: { x: { '!etc.':wtvr, !'etc.':stuff } }, s2: [arr, GLOBAL = { [local]: [^^]}, r2val] }",
                    "{\"r1\":$.\"s1\".\"x\".!\"etc.\",\"r2\":^*.\"r3\".^^[#0x02],\"r3\":@GLOBAL[local],\"s1\":{\"x\":{\"!etc.\":wtvr,!\"etc.\":stuff}},\"s2\":[arr,&GLOBAL={[local]:[^^]},r2val]}"
                    ))

                .Run();
        }

        #endregion

        #region Entity and document writing extensions

        /// <summary>
        /// Testing IStonReader extension methods, i.e. parsing from string and from stream.
        /// </summary>
        [TestMethod]
        public void Test_IStonReaderExtensionMethods()
        {
            // with all these overly convoluted documents I made for test cases
            // (I particularly recommend spaghetti cases from Tests_ReferencesResolving >> Test_AdvancedReferences)
            // I really appreciate using some simple cases to do my tests
            var cfc = "[cheerful, friendly case]";
            var cfcc = "[cheerful,<\"friendly\">case]";

            // reading from string
            Assert.AreEqual(cfcc, (RegularStonReader.Default as IStonReader).ParseEntity(cfc).ToCanonicalForm());
            Assert.AreEqual(cfcc, RegularStonReader.Default.ParseEntity(cfc).ToCanonicalForm());

            Assert.AreEqual(cfcc, (RegularStonReader.Default as IStonReader).ParseDocument(cfc).ToCanonicalForm());
            Assert.AreEqual(cfcc, RegularStonReader.Default.ParseDocument(cfc).ToCanonicalForm());

            // reading from stream
            using (var s = new MemoryStream())
            {
                using (var sw = new StreamWriter(s, new System.Text.UTF8Encoding(false, true), 1024, true))
                {
                    sw.Write(cfc);
                }

                s.Seek(0, SeekOrigin.Begin);
                Assert.AreEqual(cfcc, (RegularStonReader.Default as IStonReader).LoadEntity(s).ToCanonicalForm());

                s.Seek(0, SeekOrigin.Begin);
                Assert.AreEqual(cfcc, RegularStonReader.Default.LoadEntity(s).ToCanonicalForm());

                s.Seek(0, SeekOrigin.Begin);
                Assert.AreEqual(cfcc, (RegularStonReader.Default as IStonReader).LoadDocument(s).ToCanonicalForm());

                s.Seek(0, SeekOrigin.Begin);
                Assert.AreEqual(cfcc, RegularStonReader.Default.LoadDocument(s).ToCanonicalForm());
            }
        }

        /// <summary>
        /// Testing IStonEntity extension methods for writing the entity to a string or stream.
        /// </summary>
        [TestMethod]
        public void Test_IStonEntityExtensionMethods()
        {
            var entity = RegularStonReader.Default.ParseEntity("[cheerful, friendly entity]");

            Assert.AreEqual("[cheerful,<\"friendly\">entity]", entity.ToCanonicalForm());
            Assert.AreEqual("[cheerful,<\"friendly\">entity]", entity.ToString<IStonEntity, IStonDocument>(CanonicalStonWriter.Instance));

            using (var s = new MemoryStream())
            {
                entity.SaveCanonicalForm(s);
                entity.Save<IStonEntity, IStonDocument>(s, CanonicalStonWriter.Instance);

                s.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(s, System.Text.Encoding.UTF8, true, 1024, false))
                {
                    var text = reader.ReadToEnd();
                    Assert.AreEqual("[cheerful,<\"friendly\">entity][cheerful,<\"friendly\">entity]", text);
                }
            }
        }

        /// <summary>
        /// Testing IStonDocument extension methods for writing the document to a string or stream.
        /// </summary>
        [TestMethod]
        public void Test_IStonDocumentExtensionMethods()
        {
            var document = RegularStonReader.Default.ParseDocument("[cheerful, friendly document]");

            Assert.AreEqual("[cheerful,<\"friendly\">document]", document.ToCanonicalForm());
            Assert.AreEqual("[cheerful,<\"friendly\">document]", document.ToString<IStonEntity, IStonDocument>(CanonicalStonWriter.Instance));

            using (var s = new MemoryStream())
            {
                document.SaveCanonicalForm(s);
                document.Save<IStonEntity, IStonDocument>(s, CanonicalStonWriter.Instance);

                s.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(s, System.Text.Encoding.UTF8, true, 1024, false))
                {
                    var text = reader.ReadToEnd();
                    Assert.AreEqual("[cheerful,<\"friendly\">document][cheerful,<\"friendly\">document]", text);
                }
            }
        }

        #endregion

        #region Loose ends

        /// <summary>
        /// Miscellaneous tests that check loose ends and complete code coverage.
        /// </summary>
        [TestMethod]
        public void Test_CodeCoverageStuffing()
        {
            Assert.AreEqual(RegularStonReader.Default.SupportedFormat, StonFormat.RegularSton);
            Assert.AreEqual(CanonicalStonWriter.Instance.OutputFormat, StonFormat.CanonicalSton);

            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
            for (int i = 0; i < chars.Length; i++)
            {
                Assert.AreEqual(i, chars[i].GetBase64Value());
            }

            Helpers.Validator.ValidateEntity(RegularStonReader.Default.ParseEntity("{}"));
        }

        #endregion
    }
}

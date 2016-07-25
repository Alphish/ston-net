using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Alphicsh.Ston.Tests
{
    using Tokenization;

    /// <summary>
    /// Collection of tests for STON types structure.
    /// </summary>
    [TestClass]
    public class Tests_TypesStructure
    {
        // checks that an entity parsed from a given text has a type with specific canonical form
        private void ExpectType(string ston, string canonicalType)
        {
            var entity = RegularStonReader.Default.ParseEntity(ston);
            var canonicalString = entity.ToCanonicalForm();
            if (canonicalType == null) Assert.IsNull((entity as IStonValuedEntity).Type);
            else Assert.AreEqual(canonicalType, canonicalString.Remove(canonicalString.LastIndexOf('>') + 1).Substring(canonicalString.IndexOf('<')));
        }

        /// <summary>
        /// Test cases related to type definitions wrapped between type wrappings.
        /// </summary>
        [TestMethod]
        public void Test_WrappedTypesDefinitions()
        {
            AggregateTester.New()

                .Add("<lorem> ipsum", () => ExpectType("lorem ipsum", "<\"lorem\">"))
                .Add("x = <y> z", () => ExpectType("x = <y> z", "<\"y\">"))
                .Add("<<<<<int>>>>> 0", () => ExpectType("<<<<<int>>>>> 0", "<\"int\">"))
                .Add("<<int|string>[...]> 0", () => ExpectType("<<int|string>[...]> 0", "<<\"int\"|\"string\">[]>"))
                .Add("<dictionary<string, int>> {}", () => ExpectType("<dictionary<string, int>> {}", "<\"dictionary\"<\"string\",\"int\">>"))
                .Add("<map<a|b, c|d>> {}", () => ExpectType("<map<a|b, c|d>> {}", "<\"map\"<\"a\"|\"b\",\"c\"|\"d\">>"))
                .Add("<map<a<<<b>>,c>[], d<e>|f[]>> {}", () => ExpectType("<map<a<<<b>>,c>[], d<e>|f[]>> {}", "<\"map\"<\"a\"<\"b\",\"c\">[],\"d\"<\"e\">|\"f\"[]>>"))

                .Run();
        }

        /// <summary>
        /// Test cases related to unwrapped type definitions.
        /// </summary>
        [TestMethod]
        public void Test_BareTypesDefinitions()
        {
            AggregateTester.New()

                .Add("<> implicit", () => ExpectType("<> implicit", null))
                .Add("lorem ipsum", () => ExpectType("lorem ipsum", "<\"lorem\">"))
                .Add("lorem (ipsum)", () => ExpectType("lorem (ipsum)", "<\"lorem\">"))
                .Add("lorem [ipsum]", () => ExpectType("lorem [ipsum]", "<\"lorem\">"))
                .Add("lorem|ipsum dolor", () => ExpectType("lorem|ipsum dolor", "<\"lorem\"|\"ipsum\">"))
                .Add("lorem|ipsum dolor", () => ExpectType("lorem|<ipsum> dolor", "<\"lorem\"|\"ipsum\">"))
                .Add("lorem|ipsum dolor", () => ExpectType("<lorem>|ipsum dolor", "<\"lorem\"|\"ipsum\">"))
                .Add("x = y z", () => ExpectType("x = y z", "<\"y\">"))

                // collection suffix can be tricky in bare types definition
                .Add("<int|string>[]", () => ExpectType("<int|string>[]", "<\"int\"|\"string\">"))
                .Add("<int|string>[...][]", () => ExpectType("<int|string>[...][]", "<<\"int\"|\"string\">[]>"))
                .Add("<int|string>[...][stuff]", () => ExpectType("<int|string>[...][stuff]", "<<\"int\"|\"string\">[]>"))
                .Add("<int|string>[][stuff]", () => Expect.UnexpectedCharacter("<int|string>[][stuff]", character: '[', position: 14))
                .Add("map<string, int> {}", () => ExpectType("map<string, int> {}", "<\"map\"<\"string\",\"int\">>"))
                .Add("map<string, int>[] {}", () => ExpectType("map<string, int>[] {}", "<\"map\"<\"string\",\"int\">>"))
                .Add("<<>|<type>> entity", () => Expect.UnexpectedCharacter("<<>|<type>> entity", character: '>', position: 2))

                .Add("map<string, int>[][]", () => Expect.UnexpectedCharacter("map<string, int>[][]", character: '[', position: 18))
                .Add("map<string, int>[...][...]", () => Expect.UnexpectedCharacter("map<string, int>[...][...]", character: -1))

                // miscellaneous type-related syntax errors 
                .Add("! 'string'", () => Expect.UnexpectedCharacter("! 'string'", '\'', StonChartype.CanunBegin, 0, 2, 2))
                .Add("<>|<type> entity", () => Expect.UnexpectedCharacter("<>|<type> entity", character: '|', position: 2))
                .Add("<type>|<> entity", () => Expect.Exception<StonParsingException>(() => RegularStonReader.Default.ParseEntity("<type>|<> entity"), "Implicit type declaration has been read as a part of union type."))

                .Run();
        }

        /// <summary>
        /// Testing basic validation of a union type, specifically that it requires at least two permitted types.
        /// </summary>
        [TestMethod]
        public void Test_UnionTypeValidation()
        {
            var type = new StonUnionType(new IStonType[] { });
            try
            {
                Helpers.Validator.ValidateType(type);
                Assert.Fail("no type:\n    The type is valid. This should *not* have happened.");
            }
            catch (StonException ex)
            {
                Assert.AreEqual(ex.Message, "A union type must have at least two permitted types.");
            }

            type = new StonUnionType(new IStonType[] { new StonNamedType("type") });
            try
            {
                Helpers.Validator.ValidateType(type);
                Assert.Fail("one type:\n    The type is valid. This should *not* have happened.");
            }
            catch (StonException ex)
            {
                Assert.AreEqual(ex.Message, "A union type must have at least two permitted types.");
            }

            type = new StonUnionType(new IStonType[] { new StonNamedType("type"), new StonNamedType("other type") });
            try
            {
                Helpers.Validator.ValidateType(type);
            }
            catch (StonException ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}

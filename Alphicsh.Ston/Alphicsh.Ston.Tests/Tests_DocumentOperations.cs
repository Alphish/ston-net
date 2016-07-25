using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
using System.Linq;

namespace Alphicsh.Ston.Tests
{
    /// <summary>
    /// Collection of tests for STON documents building and handling.
    /// References resolving is a can of worms so large, it has its own Tests_ReferencesResolving test class.
    /// </summary>
    [TestClass]
    public class Tests_DocumentOperations
    {
        #region Creating document from a reference entity (not)

        /// <summary>
        /// Confirms that a document cannot be created from a reference entity core.
        /// </summary>
        [TestMethod]
        public void Test_DocumentReferenceCore()
        {
            Expect.Exception<StonException>(() => RegularStonReader.Default.ParseDocument("Alice = @doofus"), "The parsed entity is not a valid document core.");
        }

        #endregion

        #region Duplicate global entities

        // checks that a given STON document has multiple entities with a specific global identifier
        private void ExpectDuplicateGlobalEntity(string ston, string identifier)
        {
            try
            {
                RegularStonReader.Default.ParseDocument(ston, null, null, s => true, s => true);
                Assert.Fail("The document is valid. This should *not* have happened.");
            }
            catch (StonDuplicateGlobalEntityException ex)
            {
                Assert.AreEqual(identifier, ex.GlobalIdentifier);
                Assert.AreEqual(identifier, ex.FirstEntity.GlobalIdentifier);
                Assert.AreEqual(identifier, ex.SecondEntity.GlobalIdentifier);
            }
        }

        /// <summary>
        /// Test cases related to duplicate global identifiers.
        /// </summary>
        [TestMethod]
        public void Test_DuplicateGlobalEntities()
        {
            AggregateTester.New()

                .Add("duplicate globals 1", () => ExpectDuplicateGlobalEntity("{ x: &DOUBLE = 0, y: &DOUBLE = 1 }", "DOUBLE"))
                .Add("duplicate globals 2", () => ExpectDuplicateGlobalEntity("{ x: &DOUBLE = 0, y: &DOUBLE = $.x }", "DOUBLE"))

                .Run();
        }

        #endregion

        #region Construction order

        // checks that a construction order provided by a given STON document is correct
        // it is performed by asserting that a globally identified entity NODE0 comes before NODE1,
        // then NODE2, etc. up to LAST[i]
        private void ExpectConstructionOrder(string ston)
        {
            try
            {
                var document = RegularStonReader.Default.ParseDocument(ston);
                int idx = 0;
                foreach (var entity in document.GetConstructionOrder())
                {
                    if (entity.GlobalIdentifier != null)
                    {
                        Assert.AreEqual(idx.ToString(), entity.GlobalIdentifier.Substring(4));
                        if (entity.GlobalIdentifier.StartsWith("LAST")) idx = -1;
                        else idx++;
                    }
                }
                Assert.AreEqual(-1, idx);
            }
            catch (StonException ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        // checks that a construction loop appears in a given STON document
        // and that cycle loops between globally identified entities NODE0_p0, NODE1_p1, etc. up to LASTi_pi
        // where pn in NODEn_pn is the position of the next entity in the cycle
        private void ExpectCircularConstruction(string ston)
        {
            try
            {
                RegularStonReader.Default.ParseDocument(ston, null, null, s => true, s => true);
                Assert.Fail("The document is valid. This should *not* have happened.");
            }
            catch (StonCircularConstructionException ex)
            {
                Assert.AreEqual(ex.ConstructedValue, ex.ConstructionCycle.First().Key);
                string globalIdentifier = ex.ConstructedValue.GlobalIdentifier;

                int expectIdx = int.Parse(globalIdentifier.Substring(4, globalIdentifier.IndexOf('_') - 4));
                int expectPosition;

                IStonEntity nextNode = null;

                foreach (var node in ex.ConstructionCycle)
                {
                    if (nextNode != null) Assert.AreEqual(nextNode, node.Key);
                    globalIdentifier = node.Key.GlobalIdentifier;

                    Assert.AreEqual(expectIdx, int.Parse(globalIdentifier.Substring(4, globalIdentifier.IndexOf('_') - 4)));

                    expectPosition = int.Parse(globalIdentifier.Substring(globalIdentifier.IndexOf('_') + 1));
                    nextNode = node.Key.Construction.PositionalParameters.Concat(node.Key.Construction.NamedParameters.Select(p => p.Value)).ElementAt(expectPosition);
                    if (nextNode is IStonReferenceEntity) nextNode = ex.Document.GetReferencedValue(nextNode as IStonReferenceEntity);

                    if (globalIdentifier.StartsWith("LAST")) expectIdx = 0;
                    else expectIdx++;
                }
                globalIdentifier = ex.ConstructedValue.GlobalIdentifier;
                Assert.AreEqual(expectIdx, int.Parse(globalIdentifier.Substring(4, globalIdentifier.IndexOf('_') - 4)));
                Assert.AreEqual(nextNode, ex.ConstructedValue);
            }
        }

        /// <summary>
        /// Test cases related to construction order and cicular construction.
        /// </summary>
        [TestMethod]
        public void Test_ConstructionOrder()
        {
            AggregateTester.New()

                .Add("valid construction", () => ExpectConstructionOrder("{ a: &LAST3 = ($.b, list: $.c), b: &NODE1 = ($[d]){ x: 0, y: 1 }, c: &NODE2 = ($.b)[3, 4, 5], [d]: &NODE0 = doodle }"))
                .Add("self-construction", () => ExpectCircularConstruction("&LAST0_1 = (null, ^*)"))
                .Add("constructioncursion", () => ExpectCircularConstruction("&NODE0_0 = (&NODE1_1 = (a, &NODE2_2 = (b, c, &LAST3_3 = (d, e, f, ^*))))"))
                .Add("construction pong", () => ExpectCircularConstruction("{ a: &NODE0_3 = (x, y, n1: z, n2: $.b), b: &NODE1_1 = (v, n3: &LAST2_0 = ($.a), n4: w)}"))

                .Run();
        }

        #endregion

        #region Extension types and members

        // checks that a given STON document build correctly, recognizing all inner extension types
        private void ExpectOkExtension(string ston, IEnumerable<string> knownTypes = null, IEnumerable<string> knownMembers = null, Func<string, bool> typesRule = null, Func<string, bool> membersRule = null)
        {
            try
            {
                RegularStonReader.Default.ParseDocument(ston, knownTypes, knownMembers, typesRule, membersRule);
            }
            catch (StonException ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        // checks that a given STON document encounters specific unknown extension, whether it's a name or a type
        private void ExpectUnknownExtension(string ston, string name, bool isType, IEnumerable<string> knownTypes = null, IEnumerable<string> knownMembers = null, Func<string,bool> typesRule = null, Func<string,bool> membersRule = null)
        {
            try
            {
                RegularStonReader.Default.ParseDocument(ston, knownTypes, knownMembers, typesRule, membersRule);
            }
            catch (StonUnknownExtensionTypeException ex)
            {
                Assert.IsTrue(isType, "It's type that is unknown.");
                Assert.AreEqual(name, ex.ExtensionType.Name);
            }
            catch (StonUnknownExtensionMemberException ex)
            {
                Assert.IsFalse(isType, "It's member that is unknown.");
                Assert.AreEqual(name, ex.MemberKey.Name);
            }
        }

        /// <summary>
        /// Test cases related to recognizing or denying extensions inside a document.
        /// </summary>
        [TestMethod]
        public void Test_StonExtensionElements()
        {
            AggregateTester.New()

                // all extensions unknown
                .Add("any extension type", () => ExpectUnknownExtension("!extension type", "extension", true))
                .Add("collection extension type", () => ExpectUnknownExtension("<!extension>[...] type", "extension", true))
                .Add("permitted extension type", () => ExpectUnknownExtension("!extension|regular type", "extension", true))
                .Add("nested extension type", () => ExpectUnknownExtension("regular<!extension> type", "extension", true))
                .Add("any extension member", () => ExpectUnknownExtension("{ !extension: member}", "extension", false))

                // extensions provided with whitelists
                .Add("known extension type", () => ExpectOkExtension("!extension type", new string[] { "extension" }, null, null, null))
                .Add("unknown extension type", () => ExpectUnknownExtension("!extension type", "extension", true, null, new string[] { "extension" }, null, null))
                .Add("known extension member", () => ExpectOkExtension("{ !extension: member}", null, new string[] { "extension" }, null, null))
                .Add("unknown extension member", () => ExpectUnknownExtension("{ !extension: member}", "extension", false, new string[] { "extension" }, null, null, null))
                .Add("both known extensions", () => ExpectOkExtension("{ !extension: !extension member}", new string[] { "extension" }, new string[] { "extension" }, null, null))

                // extensions provided with rules
                .Add("ruly extension type", () => ExpectOkExtension("!extension type", null, null, s => s.StartsWith("ex"), null))
                .Add("unruly extension type 1", () => ExpectUnknownExtension("!extension type", "extension", true, null, null, s => s.StartsWith("app"), null))
                .Add("unruly extension type 2", () => ExpectUnknownExtension("!extension type", "extension", true, null, null, null, s => s.StartsWith("ex")))
                .Add("ruly extension member", () => ExpectOkExtension("{ !extension: member}", null, null, null, s => s.StartsWith("ex")))
                .Add("unruly extension member 1", () => ExpectUnknownExtension("{ !extension: member}", "extension", false, null, null, null, s => s.StartsWith("app")))
                .Add("unruly extension member 2", () => ExpectUnknownExtension("{ !extension: member}", "extension", false, null, null, s => s.StartsWith("ex"), null))
                .Add("both ruly extensions", () => ExpectOkExtension("{ !extension: !extension member}", null, null, s => s.StartsWith("ex"), s => s.StartsWith("ex")))

                .Run();
        }

        #endregion

        #region Document-related methods

        /// <summary>
        /// Testing various operations, related to creating a document from entity as well as operating on existing documents.
        /// </summary>
        [TestMethod]
        public void Test_DocumentMethods()
        {
            var coreSource = RegularStonReader.Default.ParseEntity("(&CONSTRUCTED = 0){ a: 1, b: &STUFF = { c: 2, d: 3, !ext: <!app_wtvr[]> [9001] }, [c]: &C1 = 193, [&IDX = (c)]: &C2 = 194 }") as IStonValuedEntity;

            // invalid document extensions

            try { new StonDocument(coreSource); }
            catch (StonUnknownExtensionMemberException ex) { Assert.AreEqual("ext", ex.MemberKey.Name); }

            try { new StonDocument(coreSource, null, new string[] { "ext" }); }
            catch (StonUnknownExtensionTypeException ex) { Assert.AreEqual("app_wtvr", ex.ExtensionType.Name); }

            try { new StonDocument(coreSource, s => s.StartsWith("app"), null); }
            catch (StonUnknownExtensionMemberException ex) { Assert.AreEqual("ext", ex.MemberKey.Name); }

            // valid document extensions

            var document = new StonDocument(RegularStonReader.Default.ParseEntity("null") as IStonValuedEntity);
            document = new StonDocument(coreSource, new string[] { "app_wtvr" }, new string[] { "ext" });
            document = new StonDocument(coreSource, s => s.StartsWith("app"), s => s == "ext");
            document = new StonDocument(coreSource, null, new string[] { "ext" }, s => s.StartsWith("app"), null);

            // checking hierarchy of contexts

            Assert.AreEqual(document.Core, document.GetParentContext(document.GetGlobalEntity("STUFF")));
            Assert.AreEqual(null, document.GetParentContext(document.Core));
            Assert.AreEqual(null, document.GetParentContext(document.GetGlobalEntity("CONSTRUCTED")));

            // checking member access

            var bindingb = new StonBindingName("b");
            Assert.AreEqual(null, document.GetMember(coreSource as IStonComplexEntity, bindingb));      // in StonDocument, core is a copy of coreSource, not coreSource itself
            Assert.AreEqual(document.GetGlobalEntity("STUFF"), document.GetMember(document.Core as IStonComplexEntity, bindingb));

            var smallc = new StonSimpleEntity(new StonSimpleValue(StonDataType.Named, "c"));
            Assert.AreEqual(document.GetGlobalEntity("C1"), document.GetMember(document.Core as IStonComplexEntity, new StonBindingIndex(
                new IStonEntity[] { smallc }
                )));
            Assert.AreEqual(null, document.GetMember(document.Core as IStonComplexEntity, new StonBindingIndex(
                new IStonEntity[] { new StonComplexEntity(new StonConstruction(new IStonEntity[] { smallc }, null)) }
                )));
            Assert.AreEqual(null, document.GetMember(document.Core as IStonComplexEntity, new StonBindingIndex(
                new IStonEntity[] { document.GetGlobalEntity("IDX") }
                )));
            Assert.AreEqual(document.GetGlobalEntity("C2"), document.GetMember(document.Core as IStonComplexEntity, new ABindingIndex(
                new IStonEntity[] { document.GetGlobalEntity("IDX") }
                )));
        }

        #endregion
    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
using System.Linq;

namespace Alphicsh.Ston.Tests
{
    using Tokenization;

    /// <summary>
    /// Collection of tests for STON complex values structure.
    /// </summary>
    [TestClass]
    public class Tests_ComplexValuesStructure
    {
        #region Initialization shorthands

        // creates an empty text entity
        private IStonEntity _e(string content = "") => new StonSimpleEntity(new StonSimpleValue(StonDataType.Text, content));

        // creates a name-entity pair with a given name
        private KeyValuePair<string, IStonEntity> _nep(string name) => new KeyValuePair<string, IStonEntity>(name, _e());

        // creates a collection of entities
        private IEnumerable<IStonEntity> _colle(params IStonEntity[] result) => result;

        // creates a collection of name-value pairs
        private IEnumerable<KeyValuePair<string, IStonEntity>> _collnep(params KeyValuePair<string, IStonEntity>[] result) => result;

        #endregion

        #region Valid/invalid complex entities

        // checks that a given STON text parses to a correct entity, represented with a canonical form
        private void ExpectValidComplexEntity(string ston, string canonicalForm)
        {
            try
            {
                Assert.AreEqual(canonicalForm, RegularStonReader.Default.ParseEntity(ston).ToCanonicalForm());
            }
            catch (StonException ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        // checks that a given combination of complex value components is valid
        private void ExpectValidComplexEntity(IStonConstruction construction = null, IStonMemberInit memberInit = null, IStonCollectionInit collectionInit = null)
        {
            try
            {
                new StonComplexEntity(construction, memberInit, collectionInit);
            }
            catch (StonException ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        // checks that a given STON text parses to an incorrect structure, causing a specific error
        private void ExpectInvalidComplexEntity(string ston, string message)
        {
            try
            {
                RegularStonReader.Default.ParseEntity(ston);
                Assert.Fail("The value is valid. This should *not* have happened.");
            }
            catch (StonException ex)
            {
                Assert.AreEqual(message, ex.Message);
            }
        }

        // checks that a given combination of complex value components is invalid, and causes a specific error
        private void ExpectInvalidComplexEntity(IStonConstruction construction = null, IStonMemberInit memberInit = null, IStonCollectionInit collectionInit = null, string message = null)
        {
            try
            {
                new StonComplexEntity(construction, memberInit, collectionInit);
                Assert.Fail("The value is valid. This should *not* have happened.");
            }
            catch (StonException ex)
            {
                Assert.AreEqual(message, ex.Message);
            }
        }

        #endregion

        # region Valid/invalid documents

        // checks that a given STON text parses to a correct document
        private void ExpectValidComplexDocument(string ston)
        {
            try
            {
                RegularStonReader.Default.ParseDocument(ston, null, null, s => true, s => true);
            }
            catch (StonException ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        // checks that a given STON text parses to a document with a specific duplicate named member
        private void ExpectDuplicateMember(string ston, bool isExtension, string name)
        {
            var entity = RegularStonReader.Default.ParseEntity(ston) as IStonValuedEntity;
            try
            {
                new StonDocument(entity, null, null, s => true, s => true);
                Assert.Fail("The document is valid. This should *not* have happened.");
            }
            catch (StonDuplicateMemberException ex)
            {
                Assert.AreEqual("DOUBLE", ex.Entity.GlobalIdentifier);
                var key = new StonBindingName(name, isExtension);
                var comparer = new Equivalence.StonBindingKeyEquivalenceComparer(new Equivalence.StonSemanticEntityEquivalenceComparer(ex.Document));
                Assert.IsTrue(comparer.Equals(key, ex.FirstKey), "The first key doesn't match the expected key.");
                Assert.IsTrue(comparer.Equals(key, ex.SecondKey), "The second key doesn't match the expected key.");
            }
        }

        // checks that a given STON text parses to a document with a specific duplicate index
        private void ExpectDuplicateMember(string ston, params string[] parameterNames)
        {
            var entity = RegularStonReader.Default.ParseEntity(ston) as IStonValuedEntity;
            try
            {
                new StonDocument(entity, null, null, s => true, s => true);
                Assert.Fail("The document is valid. This should *not* have happened.");
            }
            catch (StonDuplicateMemberException ex)
            {
                Assert.AreEqual("DOUBLE", ex.Entity.GlobalIdentifier);
                var key = new ABindingIndex(parameterNames.Select(p => ex.Document.GetGlobalEntity(p)));
                var comparer = new Equivalence.StonBindingKeyEquivalenceComparer(new Equivalence.StonSemanticEntityEquivalenceComparer(ex.Document));
                Assert.IsTrue(comparer.Equals(key, ex.FirstKey), "The first key doesn't match the expected key.");
                Assert.IsTrue(comparer.Equals(key, ex.SecondKey), "The second key doesn't match the expected key.");
            }
        }

        #endregion

        /// <summary>
        /// Test cases related to complex entities with construction only.
        /// </summary>
        [TestMethod]
        public void Test_ConstructionStructure()
        {
            AggregateTester.New()

                // basic construction cases
                .Add("blank construction", () => ExpectValidComplexEntity(construction: new StonConstruction(null, null)))
                .Add("blank construction 2", () => ExpectValidComplexEntity("()", "()"))

                .Add("positional parameters only", () => ExpectValidComplexEntity(construction: new StonConstruction(_colle(_e(), _e(), _e()), null)))

                .Add("named parameters only", () => ExpectValidComplexEntity(construction: new StonConstruction(null, _collnep(_nep("a"), _nep("b"), _nep("lorem ipsum")))))
                .Add("named parameters null name", () => ExpectInvalidComplexEntity(construction: new StonConstruction(null, _collnep(_nep("a"), _nep(null), _nep("c"))), message: "A complex value construction cannot have a non-existing parameter name."))
                .Add("named parameters duplicate name", () => ExpectInvalidComplexEntity(construction: new StonConstruction(null, _collnep(_nep("a"), _nep("b"), _nep("a"))), message: "The complex value construction has a duplicate named parameter \"a\"."))
                .Add("named parameters duplicate name 2", () => ExpectInvalidComplexEntity("(x: 0, y: 0, x: 0)", message: "The complex value construction has a duplicate named parameter \"x\"."))
                
                .Add("both parameter kinds", () => ExpectValidComplexEntity(construction: new StonConstruction(_colle(_e(), _e(), _e()), _collnep(_nep("a"), _nep("b"), _nep("c")))))
                .Add("both parameter kinds 2", () => ExpectValidComplexEntity("(:0, :1, :2, a: 3, b: 4, c: 5)", "(:0,:1e0,:2e0,\"a\":3e0,\"b\":4e0,\"c\":5e0)"))

                // syntactically invalid constructions
                .Add("non-separated construction", () => Expect.UnexpectedCharacter("(a b c)", 'c', StonChartype.SequenceSeparator | StonChartype.ConstructionClose, 0, 5, 5))
                .Add("chain string construction parameter name", () => Expect.UnexpectedCharacter("(a, b, 'na'>'me': param)", ':', StonChartype.SequenceSeparator | StonChartype.ConstructionClose, 0, 16, 16))
                .Add("complex parameter name", () => Expect.UnexpectedCharacter("(a, b, ('name'): param)", ':', StonChartype.SequenceSeparator | StonChartype.ConstructionClose, 0, 15, 15))
                .Add("globally identified parameter name", () => Expect.UnexpectedCharacter("(a, b, &PAR = name: param)", ':', StonChartype.SequenceSeparator | StonChartype.ConstructionClose, 0, 18, 18))
                .Add("explicitly typed parameter name", () => Expect.UnexpectedCharacter("(a, b, <type> name: param)", ':', StonChartype.SequenceSeparator | StonChartype.ConstructionClose, 0, 18, 18))
                .Add("non-named value parameter name", () => Expect.UnexpectedCharacter("(a, b, 0z name: param)", ':', StonChartype.SequenceSeparator | StonChartype.ConstructionClose, 0, 14, 14))
                .Add("CANUN path value parameter name", () => Expect.UnexpectedCharacter("(a, b, na.dot.me: param)", ':', StonChartype.SequenceSeparator | StonChartype.ConstructionClose, 0, 16, 16))
                .Add("positional number after named parameter", () => Expect.UnexpectedCharacter("(0, 1, name: 2, 3)", '3', StonChartype.TextDelimiter | StonChartype.CanunBegin, 0, 16, 16))
                .Add("positional named value after named parameter", () => Expect.UnexpectedCharacter("(0, 1, name: 2, wtvr)", ')', StonChartype.ValuePrompt, 0, 20, 20))

                .Run();
        }

        /// <summary>
        /// Test cases related to complex entities with member initialization only.
        /// </summary>
        [TestMethod]
        public void Test_MemberInitStructure()
        {
            AggregateTester.New()

                .Add("blank member", () => ExpectValidComplexEntity(memberInit: new StonMemberInit(null as IEnumerable<KeyValuePair<IStonBindingKey, IStonEntity>>)))
                .Add("blank member 2", () => ExpectValidComplexEntity("{}", "{}"))
                .Add("empty index parameter", () => ExpectInvalidComplexEntity("{ []: whatever}", "A member binding index must be neither non-existing nor empty."))

                // duplicate members cases

                // since sometimes members can be determined only in the context of the document
                // (specifically, indexed members with reference parameters)
                // the duplicate member checks aren't performed before the document is built
                .Add("duplicate member falsely passing", () => ExpectValidComplexEntity("{ a: 0, a: 1}", "{\"a\":0,\"a\":1e0}"))
                .Add("duplicate member failing when document is built", () => ExpectDuplicateMember("&DOUBLE = { a: 0, a: 1 }", false, "a"))

                .Add("duplicate regular member", () => ExpectDuplicateMember("&DOUBLE = { 'a': 0, \"a\": 1 }", false, "a"))
                .Add("duplicate extension member", () => ExpectDuplicateMember("&DOUBLE = { !a: 0, !a: 1}", true, "a"))
                .Add("duplicate indexed member", () => ExpectDuplicateMember("&DOUBLE = { idx: &IDX = 'a', ['a']: 0, ['a']: 1 }", "IDX"))
                .Add("duplicate indexed member 2", () => ExpectDuplicateMember("&DOUBLE = { idx: &IDX = 'a', ['a']: 0, [@IDX]: 1 }", "IDX"))
                .Add("different members", () => ExpectValidComplexDocument("{ !a: 0, a: 1, '!a': 2, ['a']: 3, [string 'a']: 4}"))

                // two separately-defined complex entities are always considered different
                .Add("different, if same structured indices", () => ExpectValidComplexDocument("{ [(0, 0)]: 0, [(0, 0)]: 1 }"))
                // referenced complex entity is still the same entity
                .Add("same complex indices", () => ExpectDuplicateMember("&DOUBLE = { [&IDX = (0, 0)]: 0, [@IDX]: 1 }", "IDX"))
                .Add("same complex double indices", () => ExpectDuplicateMember("&DOUBLE = { [&A=(a), &D=(d)]: 0, [@A, &C=(c)]: 1, [&B=(b),@C]: 2, [@B,@D]: 3, [@A,@C]: 4 }", "A", "C"))
                .Add("same complex double indices, with reference", () => ExpectDuplicateMember("&DOUBLE = { x: $[@B, @D], [&A=(a), &D=(d)]: 0, [@A, &C=(c)]: 1, [&B=(b),@C]: 2, [@B,@D]: 3, [@A,@C]: 4 }", "A", "C"))

                // syntactically invalid member inits
                .Add("non-separated member init", () => Expect.UnexpectedCharacter("{'x':0 'y':1 'z':2]", '\'', StonChartype.SequenceSeparator | StonChartype.MemberInitClose, 0, 7, 7))
                .Add("non-keyed member init", () => Expect.UnexpectedCharacter("{0, 1, 2}", '0', StonChartype.IndexOpen | StonChartype.CanunBegin | StonChartype.TextDelimiter | StonChartype.Extension, 0, 1, 1))
                .Add("unnamed member key", () => Expect.UnexpectedCharacter("{! 1member: 'one' }", '1', StonChartype.TextDelimiter | StonChartype.CanunBegin, 0, 3, 3))

                .Run();
        }

        /// <summary>
        /// Test cases related to complex entities with collection initialization only.
        /// </summary>
        [TestMethod]
        public void Test_CollectionInitStructure()
        {
            AggregateTester.New()

                // basic blank collection inits
                .Add("blank collection", () => ExpectValidComplexEntity(collectionInit: new StonCollectionInit(null as IEnumerable<IStonEntity>)))
                .Add("blank collection 2", () => ExpectValidComplexEntity("[]", "[]"))

                // syntactically invalid collection inits
                .Add("non-separated collection", () => Expect.UnexpectedCharacter("[a b c]", 'c', StonChartype.SequenceSeparator | StonChartype.CollectionInitClose, 0, 5, 5))
                .Add("union type separator in collection", () => Expect.UnexpectedCharacter("type1|type2[|type3]", '|', StonChartype.None, 0, 12, 12))

                .Run();
        }

        /// <summary>
        /// Test cases related to complex entities with non-standard components combinations.
        /// </summary>
        [TestMethod]
        public void Test_NonSingleStructure()
        {
            AggregateTester.New()

                // empty structure
                .Add("empty", () => ExpectInvalidComplexEntity(message: "A complex-valued entity must have a construction, a member initialization or a collection initialization component."))

                // combined structures
                .Add("construction-member", () => ExpectValidComplexEntity("(){}", "(){}"))
                .Add("construction-collection", () => ExpectValidComplexEntity("()[]", "()[]"))
                .Add("member-collection", () => ExpectValidComplexEntity("{}[]", "{}[]"))
                .Add("collection-member", () => ExpectValidComplexEntity("[]{}", "{}[]"))
                .Add("member-construction", () => Expect.UnexpectedCharacter("{}()", character: '(', position: 2))
                .Add("collection-construction", () => Expect.UnexpectedCharacter("[]()", character: '(', position: 2))

                .Add("construction-member-collection", () => ExpectValidComplexEntity("(){}[]", "(){}[]"))
                .Add("construction-collection-member", () => ExpectValidComplexEntity("()[]{}", "(){}[]"))

                .Run();
        }
    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
using System.Linq;

namespace Alphicsh.Ston.Tests
{
    using Tokenization;

    /// <summary>
    /// Collection of tests for STON reference entities structure.
    /// The tests for interpreting the references is in Tests_ReferencesResolving test class.
    /// </summary>
    [TestClass]
    public class Tests_ReferencesStructure
    {
        #region Directly created initial contexts

        // checks that a given initial context has a correct canonical form
        private void ExpectValidContext(IStonInitialContext context, string canonicalForm)
        {
            try
            {
                Assert.AreEqual(canonicalForm, new StonReferenceEntity(new StonAddress(context, null)).ToCanonicalForm());
            }
            catch (StonException ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        // checks that a given initial context is invalid
        // and causes an error with a specific message
        private void ExpectInvalidContext(IStonInitialContext context, string message)
        {
            try
            {
                new StonReferenceEntity(new StonAddress(context, null));
                Assert.Fail("The reference is valid. This should *not* have happened.");
            }
            catch (StonException ex)
            {
                Assert.AreEqual(message, ex.Message);
            }
        }

        /// <summary>
        /// Test cases related to directly created STON initial contexts.
        /// </summary>
        [TestMethod]
        public void Test_DirectInitialContext()
        {
            AggregateTester.New()

                // reference defining initial context
                .Add("$", () => ExpectValidContext(new StonAncestorInitialContext(0), "$"))

                // ancestor initial context
                .Add("^", () => ExpectValidContext(new StonAncestorInitialContext(1), "^"))
                .Add("^^^^^", () => ExpectValidContext(new StonAncestorInitialContext(5), "^^^^^"))
                .Add("-^", () => ExpectInvalidContext(new StonAncestorInitialContext(-1), "An ancestor initial context must have a positive ancestor order."))
                .Add("-^^^...", () => ExpectInvalidContext(new StonAncestorInitialContext(int.MinValue), "An ancestor initial context must have a positive ancestor order."))

                // core initial context
                .Add("^*", () => ExpectValidContext(new StonGlobalEntityInitialContext(""), "^*"))
                
                // globally identified initial context
                .Add("agent007", () => ExpectValidContext(new StonGlobalEntityInitialContext("agent007"), "@agent007"))
                .Add("_GLOBAL", () => ExpectValidContext(new StonGlobalEntityInitialContext("_GLOBAL"), "@_GLOBAL"))
                .Add("007agent", () => ExpectInvalidContext(new StonGlobalEntityInitialContext("007agent"), "The global identifier \"007agent\" is not a valid CANUN identifier."))
                .Add("one.two", () => ExpectInvalidContext(new StonGlobalEntityInitialContext("one.two"), "The global identifier \"one.two\" is not a valid CANUN identifier."))
                .Add("what ever", () => ExpectInvalidContext(new StonGlobalEntityInitialContext("what ever"), "The global identifier \"what ever\" is not a valid CANUN identifier."))
                .Add(" ", () => ExpectInvalidContext(new StonGlobalEntityInitialContext(" "), "The global identifier \" \" is not a valid CANUN identifier."))
                .Add("☆", () => ExpectInvalidContext(new StonGlobalEntityInitialContext("☆"), "The global identifier \"☆\" is not a valid CANUN identifier."))

                .Run();
        }

        #endregion

        #region Directly created path segments

        // creates an entity from a given STON text
        private IStonEntity _e(string ston) => RegularStonReader.Default.ParseEntity(ston);

        // checks that a given STON path segment has a correct canonical form
        private void ExpectValidPathSegment(IStonPathSegment segment, string canonicalForm)
        {
            try
            {
                Assert.AreEqual(canonicalForm, new StonReferenceEntity(new StonAddress(new StonAncestorInitialContext(0), new IStonPathSegment[] { segment })).ToCanonicalForm().Substring(1));
            }
            catch (StonException ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        // checks that a given path segment is invalid
        // and causes an error with a specific message
        private void ExpectInvalidPathSegment(IStonPathSegment segment, string message)
        {
            try
            {
                new StonReferenceEntity(new StonAddress(new StonAncestorInitialContext(0), new IStonPathSegment[] { segment }));
                Assert.Fail("The reference is valid. This should *not* have happened.");
            }
            catch (StonException ex)
            {
                Assert.AreEqual(message, ex.Message);
            }
        }

        /// <summary>
        /// Test cases related to directly created STON path segments.
        /// </summary>
        [TestMethod]
        public void Test_DirectPathSegments()
        {
            AggregateTester.New()

                // ancestor path segment
                .Add(".", () => ExpectInvalidPathSegment(new StonAncestorPathSegment(0), "An ancestor path segment must have a positive ancestor order."))
                .Add(".^", () => ExpectValidPathSegment(new StonAncestorPathSegment(1), ".^"))
                .Add(".^^^^^", () => ExpectValidPathSegment(new StonAncestorPathSegment(5), ".^^^^^"))
                .Add(".-^", () => ExpectInvalidPathSegment(new StonAncestorPathSegment(-1), "An ancestor path segment must have a positive ancestor order."))
                .Add(".-^^^...", () => ExpectInvalidPathSegment(new StonAncestorPathSegment(int.MinValue), "An ancestor path segment must have a positive ancestor order."))

                // named member path segment
                .Add(".a", () => ExpectValidPathSegment(new StonMemberPathSegment(new StonBindingName("a")), ".\"a\""))
                .Add(".!''", () => ExpectValidPathSegment(new StonMemberPathSegment(new StonBindingName("", true)), ".!\"\""))
                .Add(".''", () => ExpectValidPathSegment(new StonMemberPathSegment(new StonBindingName("", false)), ".\"\""))
                .Add(".'in space'", () => ExpectValidPathSegment(new StonMemberPathSegment(new StonBindingName("in space")), ".\"in space\""))

                // indexed member path segment
                .Add("[]", () => ExpectInvalidPathSegment(new StonMemberPathSegment(new StonBindingIndex(Enumerable.Empty<IStonEntity>())), "A member binding index must be neither non-existing nor empty."))
                .Add("[&IDX = idx]", () => ExpectInvalidPathSegment(
                    new StonMemberPathSegment(new StonBindingIndex(new IStonEntity[] { _e("&IDX = idx") })),
                    "An indexed member path segment parameter cannot declare a global identifier."
                    ))
                .Add("[{idx: 0}]", () => ExpectInvalidPathSegment(
                    new StonMemberPathSegment(new StonBindingIndex(new IStonEntity[] { _e("{idx: 0}") })),
                    "An indexed member path segment parameter cannot be a complex-valued entity."
                    ))

                // collection element path segment
                .Add("[# &IDX = 0]", () => ExpectInvalidPathSegment(
                    new StonCollectionElementPathSegment( _e("&IDX = 0") ),
                    "A collection element path segment index cannot declare a global identifier."
                    ))
                .Add("[# {idx: 0}]", () => ExpectInvalidPathSegment(
                    new StonCollectionElementPathSegment(_e("{idx: 0}")),
                    "A collection element path segment index cannot be a complex-valued entity."
                    ))
                .Add("[# int 0]", () => ExpectInvalidPathSegment(
                    new StonCollectionElementPathSegment(_e("int 0")),
                    "A collection element path segment index must be implicitly typed."
                    ))

                // implicitly typed number/binary entities are fine
                .Add("[# 0]", () => ExpectValidPathSegment(new StonCollectionElementPathSegment(_e("0")), "[#0]"))
                .Add("[# 0x0]", () => ExpectValidPathSegment(new StonCollectionElementPathSegment(_e("0x0")), "[#0x00]"))
                // negative indices pass the basic validation
                // but when document is built, they'll cause an error
                // also, they cannot be parsed by StonReader
                .Add("[# -1]", () => ExpectValidPathSegment(new StonCollectionElementPathSegment(_e("-1")), "[#-1e0]"))
                .Add("[# -0x1]", () => ExpectValidPathSegment(new StonCollectionElementPathSegment(_e("-0x01")), "[#-0x01]"))
                // references are always fine, until at document resolving it turns out otherwise
                .Add("[# $.idx]", () => ExpectValidPathSegment(new StonCollectionElementPathSegment(_e("$.idx")), "[#$.\"idx\"]"))
                .Add("[# @IDX]", () => ExpectValidPathSegment(new StonCollectionElementPathSegment(_e("@IDX")), "[#@IDX]"))

                // non-number/non-binary simple values are not allowed
                .Add("[# null]", () => ExpectInvalidPathSegment(
                    new StonCollectionElementPathSegment(_e("null")),
                    "A collection element path segment index must have a number or binary value."
                    ))
                .Add("[# idx]", () => ExpectInvalidPathSegment(
                    new StonCollectionElementPathSegment(_e("idx")),
                    "A collection element path segment index must have a number or binary value."
                    ))
                .Add("[# '0']", () => ExpectInvalidPathSegment(
                    new StonCollectionElementPathSegment(_e("'0'")),
                    "A collection element path segment index must have a number or binary value."
                    ))
                .Add("[# `0`]", () => ExpectInvalidPathSegment(
                    new StonCollectionElementPathSegment(_e("`0`")),
                    "A collection element path segment index must have a number or binary value."
                    ))

                .Run();
        }

        #endregion

        #region Syntactically invalid references

        /// <summary>
        /// Test cases related to syntactically invalid reference entities.
        /// </summary>
        [TestMethod]
        public void Test_SyntacticallyInvalidReferences()
        {
            AggregateTester.New()

                // reference entities syntax errors
                .Add("explicitly typed reference entity", () => Expect.UnexpectedCharacter("{a: int $[0], [0]: value }", '$', StonChartype.None, 0, 8, 8))
                .Add("implicitly typed reference entity", () => Expect.UnexpectedCharacter("{a: <> $[0], [0]: value }", '$', StonChartype.None, 0, 7, 7))

                .Add("global index address subentity", () => Expect.UnexpectedCharacter("{a: $[&ID = 0], [0]: value }", '&', StonChartype.None, 0, 6, 6))
                .Add("global index address subentity 2", () => Expect.UnexpectedCharacter("{a: $[ID = 0], [0]: value }", '=', StonChartype.None, 0, 9, 9))
                .Add("mistyped index address subentity", () => Expect.UnexpectedCharacter("{a: $[char[] 'index'], [<char[]> 'index']: value }", ']', StonChartype.CollectionSuffixContinue, 0, 11, 11))
                .Add("complex index address subentity", () => Expect.UnexpectedCharacter("{a: $[(0,1)], [(0,1)]: value }", '(', StonChartype.None, 0, 6, 6))

                .Add("global collection element index", () => Expect.UnexpectedCharacter("{a: $[# &ID = 0] }[ ]", '&', StonChartype.Digit | StonChartype.Sign | StonChartype.AddressBegin, 0, 8, 8))
                .Add("global collection element index 2", () => Expect.UnexpectedCharacter("{a: $[# ID = 0] }[ ]", 'I', StonChartype.Digit | StonChartype.Sign | StonChartype.AddressBegin, 0, 8, 8))
                .Add("invalid collection element index type 1", () => Expect.UnexpectedCharacter("{a: $[# name], [name]: name}[ ]", 'n', StonChartype.Digit | StonChartype.AddressBegin | StonChartype.Sign, 0, 8, 8))
                .Add("invalid collection element index type 2", () => Expect.UnexpectedCharacter("{a: $[# 'string'], ['string']: 'string'}[ ]", '\'', StonChartype.Digit | StonChartype.AddressBegin | StonChartype.Sign, 0, 8, 8))
                .Add("invalid collection element index type 3", () => Expect.UnexpectedCharacter("{a: $[# `code`], [`code`]: `code`}[ ]", '`', StonChartype.Digit | StonChartype.AddressBegin | StonChartype.Sign, 0, 8, 8))
                .Add("invalid collection element index type 4", () => Expect.UnexpectedCharacter("{a: $[# (0, 0)], [(0, 0)]: (0, 0)}[ ]", '(', StonChartype.Digit | StonChartype.AddressBegin | StonChartype.Sign, 0, 8, 8))
                .Add("negative collection element index", () => Expect.Exception<StonParsingException>(() => RegularStonReader.Default.ParseEntity("{a: $[# -1], [-1]: -1}[ ]"), "Collection element index cannot be negative."))
                .Add("overly positive collection element index", () => Expect.UnexpectedCharacter("{a: $[# ++1], [1]: 1}[ 1 ]", '+', StonChartype.Digit, 0, 9, 9))
                .Add("invalid character collection element index", () => Expect.UnexpectedCharacter("{a: $[# +?], ['?']: '?'}[ '?' ]", '?', StonChartype.Digit, 0, 9, 9))

                .Run();
        }

        #endregion
    }
}

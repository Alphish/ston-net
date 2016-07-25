using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Linq;

namespace Alphicsh.Ston.Tests
{
    /// <summary>
    /// Collection of tests for STON reference entities resolving.
    /// </summary>
    [TestClass]
    public class Tests_ReferencesResolving
    {
        // checks that a globally identified reference entity TEST points to a named value of OK.
        private void ExpectOkReference(string ston)
        {
            var document = RegularStonReader.Default.ParseDocument(ston, null, null, s => true, s => true);
            var reference = document.GetGlobalEntity("TEST") as IStonReferenceEntity;
            if (reference == null) Assert.Fail("The TEST global entity is not a reference entity.");
            var result = document.GetReferencedValue(reference);
            if (!(result is IStonSimpleEntity)) Assert.Fail("The value referenced by TEST is not a simple entity.");

            var value = (result as IStonSimpleEntity).Value;
            Assert.AreEqual("Named: OK", value.DataType + ": " + value.Content);
        }

        // checks that a globally identified reference entity TEST points to an unknown value
        private void ExpectUnknownReference(string ston, int segmentIdx, string message = null)
        {
            try
            {
                RegularStonReader.Default.ParseDocument(ston, null, null, s => true, s => true);
                Assert.Fail("The document is valid. This should *not* have happened.");
            }
            catch (StonUnknownReferenceException ex)
            {
                Assert.AreEqual("TEST", ex.Entity.GlobalIdentifier);
                if (segmentIdx > -1) Assert.AreEqual("STOP", ex.Context.GlobalIdentifier);
                if (segmentIdx == -1) Assert.IsNull(ex.UnresolvedSegment);
                else Assert.AreEqual(ex.Entity.Address.RelativePath.ElementAt(segmentIdx), ex.UnresolvedSegment);
                if (message != null) Assert.AreEqual(message, ex.Message);
            }
        }

        // checks that a reference loop appears in a given STON document
        // and that reference loops between globally identified entities NODE0_s0, NODE1_s1, etc. up to LASTi_si
        // where sn in NODEn_sn is the segment index that leads to the next entity in cycle (or N it it's initial context)
        private void ExpectCircularReference(string ston)
        {
            try
            {
                RegularStonReader.Default.ParseDocument(ston, null, null, s => true, s => true);
                Assert.Fail("The document is valid. This should *not* have happened.");
            }
            catch (StonCircularReferenceException ex)
            {
                Assert.AreEqual(ex.Entity, ex.ReferenceCycle.First().Key);
                string globalIdentifier = ex.Entity.GlobalIdentifier;

                int expectIdx = int.Parse(globalIdentifier.Substring(4, globalIdentifier.IndexOf('_') - 4));
                string expectSegment;

                foreach (var node in ex.ReferenceCycle)
                {
                    globalIdentifier = node.Key.GlobalIdentifier;

                    Assert.AreEqual(expectIdx, int.Parse(globalIdentifier.Substring(4, globalIdentifier.IndexOf('_') - 4)));

                    expectSegment = globalIdentifier.Substring(globalIdentifier.IndexOf('_') + 1);
                    if (expectSegment == "N") Assert.IsNull(node.Value);
                    else Assert.AreEqual(node.Key.Address.RelativePath.ElementAt(int.Parse(expectSegment)), node.Value);

                    if (globalIdentifier.StartsWith("LAST")) expectIdx = 0;
                    else expectIdx++;
                }

                globalIdentifier = ex.Entity.GlobalIdentifier;
                Assert.AreEqual(expectIdx, int.Parse(globalIdentifier.Substring(4, globalIdentifier.IndexOf('_') - 4)));
            }
        }

        /// <summary>
        /// Test cases related to STON initial contexts.
        /// </summary>
        [TestMethod]
        public void Test_InitialContexts()
        {
            AggregateTester.New()
                
                // testing for member initialization
                .Add("member self", () => ExpectOkReference("{ a: { a: { a: { a: &TEST = $.b, b: OK}, b: PAR1 }, b: PAR2 }, b: PAR3 }"))
                .Add("member par1", () => ExpectOkReference("{ a: { a: { a: { a: &TEST = ^.b, b: SELF}, b: OK }, b: PAR2 }, b: PAR3 }"))
                .Add("member par2", () => ExpectOkReference("{ a: { a: { a: { a: &TEST = ^^.b, b: SELF}, b: PAR1 }, b: OK }, b: PAR3 }"))
                .Add("member par3", () => ExpectOkReference("{ a: { a: { a: { a: &TEST = ^^^.b, b: SELF}, b: PAR1 }, b: PAR2 }, b: OK }"))
                .Add("member !par4", () => ExpectUnknownReference("{ a: { a: { a: { a: &TEST = ^^^^.b, b: SELF}, b: PAR1 }, b: PAR2 }, b: PAR3 }", -1))
                .Add("member core", () => ExpectOkReference("{ a: { a: { a: { a: &TEST = ^*.b, b: SELF}, b: PAR1 }, b: PAR2 }, b: OK }"))
                .Add("member global1", () => ExpectOkReference("{ a: &global = { a: { a: { a: &TEST = @global.b, b: SELF}, b: PAR1 }, b: OK }, b: PAR3 }"))
                .Add("member global2", () => ExpectOkReference("&global = { a: { a: { a: { a: &TEST = @global.b, b: SELF}, b: PAR1 }, b: PAR2 }, b: OK }"))
                .Add("member !global3", () => ExpectUnknownReference("{ a: { a: { a: { a: &TEST = @global.b, b: SELF}, b: PAR1 }, b: PAR2 }, b: PAR3 }", -1))

                // testing for collection initialization
                .Add("collection self", () => ExpectOkReference("[ [ [ [&TEST = $[#1], OK], PAR1], PAR2 ], PAR3]"))
                .Add("collection par1", () => ExpectOkReference("[ [ [ [&TEST = ^[#1], SELF], OK], PAR2 ], PAR3]"))
                .Add("collection par2", () => ExpectOkReference("[ [ [ [&TEST = ^^[#1], SELF], PAR1], OK ], PAR3]"))
                .Add("collection par3", () => ExpectOkReference("[ [ [ [&TEST = ^^^[#1], SELF], PAR1], PAR2 ], OK]"))
                .Add("collection !par4", () => ExpectUnknownReference("[ [ [ [&TEST = ^^^^[#1], SELF], PAR1], PAR2 ], PAR3]", -1))
                .Add("collection core", () => ExpectOkReference("[ [ [ [&TEST = ^*[#1], SELF], PAR1], PAR2 ], OK]"))
                .Add("collection global1", () => ExpectOkReference("[ &global = [ [ [&TEST = @global[#1], SELF], PAR1], OK ], PAR3]"))
                .Add("collection global2", () => ExpectOkReference("&global = [ [ [ [&TEST = @global[#1], SELF], PAR1], PAR2 ], OK]"))
                .Add("collection !global3", () => ExpectUnknownReference("[ [ [ [&TEST = @global[#1], SELF], PAR1], PAR2 ], PAR3]", -1))

                // testing for construction
                .Add("construction self", () => ExpectOkReference("[ [ [ (&TEST = $[#1], PARAM), OK], PAR1 ], PAR2]"))
                .Add("construction par1", () => ExpectOkReference("[ [ [ (&TEST = ^[#1], PARAM), SELF], OK ], PAR2]"))
                .Add("construction par2", () => ExpectOkReference("[ [ [ (&TEST = ^^[#1], PARAM), SELF], PAR1 ], OK]"))
                .Add("construction !par3", () => ExpectUnknownReference("[ [ [ (&TEST = ^^^[#1], PARAM), SELF], PAR1 ], PAR2]", -1))
                .Add("core construction !self", () => ExpectUnknownReference("(&TEST = $[#0])[NOPE]", -1))
                .Add("core construction core", () => ExpectOkReference("(&TEST = ^*[#0])[OK]"))

                .Run();
        }

        /// <summary>
        /// Test cases related to basic usage of path segments.
        /// </summary>
        [TestMethod]
        public void Test_BasicPathSegments()
        {
            AggregateTester.New()

                // ancestor access path segment
                .Add("ancestor1", () => ExpectOkReference("{ a: &TEST = @inb.^.x, b: { in: &inb=[], x: OK }, c: { in: [&inc=[]], x: CX }, d: { in: [[&ind=[]]], x: DX }}"))
                .Add("ancestor2", () => ExpectOkReference("{ a: &TEST = @inc.^^.x, b: { in: &inb=[], x: BX }, c: { in: [&inc=[]], x: OK }, d: { in: [[&ind=[]]], x: DX }}"))
                .Add("ancestor3", () => ExpectOkReference("{ a: &TEST = @ind.^^^.x, b: { in: &inb=[], x: BX }, c: { in: [&inc=[]], x: CX }, d: { in: [[&ind=[]]], x: OK }}"))
                .Add("unknown ancestor", () => ExpectUnknownReference("{ a: &TEST = @STOP.^^^^^.x, b: { in: &inb=[], x: BX }, c: { in: [&inc=[]], x: CX }, d: { in: [[&STOP = []]], x: DX }}", 0))

                // named member access path segment
                .Add("regular name", () => ExpectOkReference("{ a: &TEST = $.ext, ext: OK, !ext: EXTENSION }"))
                .Add("extension name", () => ExpectOkReference("{ a: &TEST = $.!ext, ext: REUGLAR, !ext: OK }"))
                .Add("regular name single quote", () => ExpectOkReference("{ a: &TEST = $.'ext', ext: OK, !ext: EXTENSION }"))
                .Add("extension name single quote", () => ExpectOkReference("{ a: &TEST = $.!'ext', ext: REUGLAR, !ext: OK }"))
                .Add("regular name double quote", () => ExpectOkReference("{ a: &TEST = $.\"ext\", ext: OK, !ext: EXTENSION }"))
                .Add("extension name double quote", () => ExpectOkReference("{ a: &TEST = $.!\"ext\", ext: REUGLAR, !ext: OK }"))
                .Add("no regular name", () => ExpectUnknownReference("&STOP = { a: &TEST = $.ext, !ext: EXTENSION }", 0))
                .Add("no extension name", () => ExpectUnknownReference("&STOP = { a: &TEST = $.!ext, ext: REUGLAR }", 0))
                .Add("no exclamation mark name", () => ExpectUnknownReference("&STOP = { a: &TEST = $.'!ext', !ext: EXTENSION }", 0))
                .Add("exclamation mark name", () => ExpectOkReference("{ a: &TEST = $.'!ext', \"!ext\": OK }"))

                .Add("simple entity named member access", () => ExpectUnknownReference("{ a: &TEST = $.b.c, b: &STOP = c }", 1))

                // indexed member access path segment
                .Add("index1", () => ExpectOkReference("{ a: &TEST = $[0], [0]: OK, [1]: ONE, [2]: TWO}"))
                .Add("index2", () => ExpectOkReference("{ a: &TEST = $[1], [0]: ZERO, [1]: OK, [2]: TWO}"))
                .Add("index3", () => ExpectOkReference("{ a: &TEST = $[2], [0]: ZERO, [1]: ONE, [2]: OK}"))
                .Add("index non", () => ExpectOkReference("{ a: &TEST = $[<> 0], [0]: OK, [int 0]: INT, [long 0]: LONG}"))
                .Add("index int", () => ExpectOkReference("{ a: &TEST = $[<int> 0], [0]: NON, [int 0]: OK, [long 0]: LONG}"))
                .Add("index long", () => ExpectOkReference("{ a: &TEST = $[<long> 0], [0]: NON, [int 0]: INT, [long 0]: OK}"))
                .Add("index short", () => ExpectUnknownReference("&STOP = { a: &TEST = $[<short> 0], [0]: NON, [int 0]: INT, [long 0]: LONG}", 0))
                .Add("index map<string,int>", () => ExpectOkReference("{ a: &TEST = $[<map<string,int>> 0], [0]: NON, [map<string,int> 0]: OK}"))
                .Add("index int|long", () => ExpectOkReference("{ a: &TEST = $[<int|long> 0], [0]: NON, [int|long 0]: OK }"))
                .Add("index int[]", () => ExpectOkReference("{ a: &TEST = $[<int[]> 0], [0]: NON, [int[...] 0]: OK}"))
                .Add("index ac", () => ExpectOkReference("{ test: &TEST = $[a,c], [a,c]: OK, [a,d]: AD, [b,c]: BC }"))
                .Add("index ad", () => ExpectOkReference("{ test: &TEST = $[a,d], [a,c]: AC, [a,d]: OK, [b,c]: BC }"))
                .Add("index bc", () => ExpectOkReference("{ test: &TEST = $[b,c], [a,c]: AC, [a,d]: AD, [b,c]: OK }"))
                .Add("index bd", () => ExpectUnknownReference("&STOP = { test: &TEST = $[b,d], [a,c]: AC, [a,d]: AD, [b,c]: BC }", 0))

                .Add("simple entity indexed member access", () => ExpectUnknownReference("{ a: &TEST = $[0][1], [0]: &STOP = 1 }", 1))

                // collection element access path segment
                .Add("element0", () => ExpectOkReference("[OK, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, &TEST = $[# 0]]"))
                .Add("element1", () => ExpectOkReference("[0, OK, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, &TEST = $[# 1]]"))
                .Add("element10", () => ExpectOkReference("[0, 1, 2, 3, 4, 5, 6, 7, 8, 9, OK, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, &TEST = $[# +10]]"))
                .Add("element17", () => ExpectOkReference("[0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, OK, 18, 19, 20, &TEST = $[# 0o21]]"))
                .Add("element int.max", () => ExpectUnknownReference("&STOP = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, &TEST = $[# 0x7fffffff]]", 0))

                .Add("simple entity collection element access", () => ExpectUnknownReference("{ a: &TEST = $[#0][#1], [0]: [a, b, c] }[ &STOP = d, e, f ]", 1))
                .Add("collectionless entity collection element access", () => ExpectUnknownReference("&STOP = { a: &TEST = $[#0], [0]: NOPE }", 0))
                .Add("complex element index", () => ExpectUnknownReference("&STOP = { a: &TEST = $[# $[#0]] }[ (1), (2) ]", 0, "The collection element index must be a simple value."))
                .Add("explicitly typed element index", () => ExpectUnknownReference("&STOP = { a: &TEST = $[# $[#0]] }[ int 1, int 2 ]", 0, "The collection element index must be implicitly typed."))
                .Add("invalid index data type", () => ExpectUnknownReference("&STOP = { a: &TEST = $[# $[#0]] }[ '1', '2' ]", 0, "The collection element index value must be of number or binary data type."))
                .Add("negative element index", () => ExpectUnknownReference("&STOP = { a: &TEST = $[# $[#0]] }[ -1, -2 ]", 0, "The collection element index must not be negative."))
                .Add("fractional element index", () => ExpectUnknownReference("&STOP = { a: &TEST = $[# $[#0]] }[ 1.5, 2.5 ]", 0, "The collection element index must not be fractional."))

                .Run();
        }

        /// <summary>
        /// Test cases related to advance and sometimes messy usage of references.
        /// </summary>
        [TestMethod]
        public void Test_AdvancedReferences()
        {
            AggregateTester.New()

                .Add("simple chain", () => ExpectOkReference("{ a: &TEST = $.b, b: $.c, c: $.d, d: OK }"))
                .Add("complex chain", () => ExpectOkReference("{ a: &TEST = $.b.an.an.a, b: { an: ^.c }, c: {an: ^.d }, d: { a: OK } }"))
                .Add("nested indices", () => ExpectOkReference("{ a: &TEST = $[$[$[0]]], [0]: 1, [1]: 2, [2]: OK }"))
                .Add("nested elements", () => ExpectOkReference("{ a: &TEST = $[#$[#$[#$[#$[4]]]]] }[ OK, 0, 1, 2, 3]"))
                .Add("combined indices", () => ExpectOkReference("{ a: &TEST = $[$[0, 0], $[1, 1]], [0, 0]: 0, [1, 1]: 1, [0, 1]: OK }"))

                // yeah, whatever
                .Add("reference spaghetti", () => ExpectOkReference(
                    "{ a: &TEST = $[0].ok, x: $[3]"+
                    ", zero: $.x.zero, [$.zero]: { ok: OK }"+
                    ", one: $.x.one, [$.one]: wtvr"+
                    ", two: $.x.two, [$.two]: wtvr"+
                    ", three: 3, [$.three]: { zero: 0, one: 1, two: 2 } }"
                    ))
                .Add("more spaghetti", () => ExpectOkReference(
                    "{ a: &TEST = $.x.^[0].ok, x: $[3]" +
                    ", zero: $.x.zero, [$[$.one]]: { ok: OK }" +
                    ", one: $.x.one, [$.one]: 0" +
                    ", two: $.x.two, [$.two]: wtvr" +
                    ", three: 3, [$.three]: { zero: 0, one: 1, two: 2 } }"
                    ))
                .Add("even more spaghetti", () => ExpectOkReference(
                    "{ a: &TEST = $.x.^[0].ok, x: $[3]" +
                    ", zero: $.x.zero, [$[# $.zero]]: { ok: OK }" +
                    ", one: $.x.one, [$.one]: wtvr" +
                    ", two: $.x.two, [$.two]: wtvr" +
                    ", three: 3, [$.three]: { zero: 0, one: 1, two: 2 } }[ 0 ]"
                    ))

                .Run();
        }

        /// <summary>
        /// Test cases related to the usage of indexed member path segment as a collection element path segment.
        /// </summary>
        [TestMethod]
        public void Test_IndexAsCollectionElement()
        {
            AggregateTester.New()

                .Add("basic", () => ExpectOkReference("{ a: &TEST = $[0] }[ OK ]"))
                
                // direct numbers
                .Add("0 0", () => ExpectOkReference("{ a: &TEST = $[0], [0]: OK }[ NOPE ]"))
                .Add("0 0", () => ExpectOkReference("{ a: &TEST = $[0], [int 0]: NOPE }[ OK ]"))
                .Add("0x0 0", () => ExpectOkReference("{ a: &TEST = $[0x0], [0]: NOPE }[ OK ]"))
                .Add("#0 0", () => ExpectOkReference("{ a: &TEST = $[#0], [0]: NOPE }[ OK ]"))
                .Add("0 1", () => ExpectUnknownReference("&STOP = { a: &TEST = $[0], [1]: NOPE }[ NOPE ]", 0))
                .Add("0 0x0", () => ExpectOkReference("{ a: &TEST = $[0], [0x0]: NOPE }[ OK ]"))
                .Add("0x0 0x0", () => ExpectOkReference("{ a: &TEST = $[0x0], [0x0]: OK }[ NOPE ]"))
                .Add("0x0 0x0", () => ExpectOkReference("{ a: &TEST = $[0x0], [int 0x0]: NOPE }[ OK ]"))
                .Add("#0x0 0x0", () => ExpectOkReference("{ a: &TEST = $[#0x0], [0x0]: NOPE }[ OK ]"))
                .Add("0x0 0n", () => ExpectUnknownReference("&STOP = { a: &TEST = $[0x0], [0n]: NOPE }[ NOPE ]", 0))

                // references inside index path segment
                .Add("idx-0 0", () => ExpectOkReference("{ a: &TEST = $[$.idx], [0]: OK, idx: 0 }[ NOPE ]"))
                .Add("idx-0x0 0", () => ExpectOkReference("{ a: &TEST = $[$.idx], [0]: NOPE, idx: 0x0 }[ OK ]"))
                .Add("#idx-0 0", () => ExpectOkReference("{ a: &TEST = $[#$.idx], [0]: NOPE, idx: 0 }[ OK ]"))
                .Add("idx-0 1", () => ExpectUnknownReference("&STOP = { a: &TEST = $[$.idx], [1]: NOPE, idx: 0 }[ NOPE ]", 0))
                .Add("idx-0 0", () => ExpectOkReference("{ a: &TEST = $[$.idx], [0x0]: NOPE, idx: 0 }[ OK ]"))
                .Add("idx-0x0 0", () => ExpectOkReference("{ a: &TEST = $[$.idx], [0x0]: OK, idx: 0x0 }[ NOPE ]"))
                .Add("#idx-0 0", () => ExpectOkReference("{ a: &TEST = $[#$.idx], [0x0]: NOPE, idx: 0x0 }[ OK ]"))
                .Add("idx-0x0 0n", () => ExpectUnknownReference("&STOP = { a: &TEST = $[$.idx], [0n]: NOPE, idx: 0x0 }[ NOPE ]", 0))

                // references inside member index
                // the member index has an explicitly-typed string value
                // but since it's impossible to tell until the index reference is resolved
                // the indexed member path segment cannot be used as a collection element path segment
                .Add("0 ref", () => ExpectUnknownReference("&STOP = { a: &TEST = $[0], [$.ref]: NOPE, ref: string 'whatever' }[ NOPE ]", 0))
                .Add("0x0 ref", () => ExpectUnknownReference("&STOP = { a: &TEST = $[0x0], [$.ref]: NOPE, ref: string 'whatever' }[ NOPE ]", 0))
                .Add("#0 ref", () => ExpectOkReference("{ a: &TEST = $[#0], [$.ref]: NOPE, ref: string 'whatever' }[ OK ]"))
                .Add("#0x0 ref", () => ExpectOkReference("{ a: &TEST = $[#0x0], [$.ref]: NOPE, ref: string 'whatever' }[ OK ]"))
                .Add("idx-0 ref", () => ExpectUnknownReference("&STOP = { a: &TEST = $[$.idx], [$.ref]: NOPE, ref: string 'whatever', idx: 0 }[ NOPE ]", 0))
                .Add("idx-0x0 ref", () => ExpectUnknownReference("&STOP = { a: &TEST = $[$.idx], [$.ref]: NOPE, ref: 'string whatever', idx: 0x0 }[ NOPE ]", 0))
                .Add("#idx-0 ref", () => ExpectOkReference("{ a: &TEST = $[#$.idx], [$.ref]: NOPE, ref: string 'whatever', idx: 0 }[ OK ]"))
                .Add("#idx-0x0 ref", () => ExpectOkReference("{ a: &TEST = $[#$.idx], [$.ref]: NOPE, ref: string 'whatever', idx: 0x0 }[ OK ]"))

                .Run();
        }

        /// <summary>
        /// Test cases where circular references appear.
        /// </summary>
        [TestMethod]
        public void Test_CircularReferences()
        {
            AggregateTester.New()

                .Add("direct self-reference", () => ExpectCircularReference("{ x: $.y, y: $.a, a: &LAST0_N = @LAST0_N }"))
                .Add("simple self-reference", () => ExpectCircularReference("{ x: $.y, y: $.a, a: &LAST0_0 = $.a }"))
                .Add("double self-reference", () => ExpectCircularReference("{ x: $.y, y: $.a, a: &NODE0_0 = $.b, b: &LAST1_0 = $.a }"))
                .Add("chaining self-reference", () => ExpectCircularReference("{ x: $.y, y: $.a, a: &NODE0_0 = $.b, b: &NODE1_1 = $.c.in, c: { in: &NODE2_2 = ^.d.in.in }, d: { in: { in: &LAST3_0 = ^^.a }} }"))
                
                .Run();
        }
    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;

namespace Alphicsh.Ston.Tests
{
    using Equivalence;

    /// <summary>
    /// Collection of tests for STON equivalence comparers..
    /// </summary>
    [TestClass]
    public class Tests_EquivalenceComparing
    {
        // checks whether a given globally identified entities from a given document are equal or not
        private bool AreEqual<TEntity>(IStonDocument document, IStonEntityEquivalenceComparer comparer, string id1, string id2)
            where TEntity : class, IStonEntity
        {
            var e1 = id1 != null ? document.GetGlobalEntity(id1) : null;
            var e2 = id2 != null ? document.GetGlobalEntity(id2) : null;

            if (typeof(TEntity) == typeof(IStonValuedEntity)) return comparer.Equals(e1 as IStonValuedEntity, e2 as IStonValuedEntity);
            else if (typeof(TEntity) == typeof(IStonSimpleEntity)) return comparer.Equals(e1 as IStonSimpleEntity, e2 as IStonSimpleEntity);
            else if (typeof(TEntity) == typeof(IStonComplexEntity)) return comparer.Equals(e1 as IStonComplexEntity, e2 as IStonComplexEntity);
            else if (typeof(TEntity) == typeof(IStonReferenceEntity)) return comparer.Equals(e1 as IStonReferenceEntity, e2 as IStonReferenceEntity);
            else return comparer.Equals(e1, e2);
        }

        /// <summary>
        /// Test cases related to regular equivalence comparison and hashing.
        /// </summary>
        [TestMethod]
        public void Test_EntityEquivalenceComparing()
        {
            var document = RegularStonReader.Default.ParseDocument("{ a: A=0, b: B=0, c: C=(0,0), d: D=(0,0), e: E=int 0, ra: RA=@A, rb: RB=@B, rc: RC=@C, rd: RD=@D, re: RE=@E }");
            var entityComparer = new StonSemanticEntityEquivalenceComparer(document);
            var keyComparer = new StonBindingKeyEquivalenceComparer(entityComparer);

            AggregateTester.New()

                .Add("A == RA", () => Assert.IsTrue(AreEqual<IStonEntity>(document, entityComparer, "A", "RA")))
                .Add("B == RB", () => Assert.IsTrue(AreEqual<IStonEntity>(document, entityComparer, "B", "RB")))
                .Add("C == RC", () => Assert.IsTrue(AreEqual<IStonEntity>(document, entityComparer, "C", "RC")))
                .Add("D == RD", () => Assert.IsTrue(AreEqual<IStonEntity>(document, entityComparer, "D", "RD")))
                .Add("E == RE", () => Assert.IsTrue(AreEqual<IStonEntity>(document, entityComparer, "E", "RE")))

                .Add("A == B", () => Assert.IsTrue(AreEqual<IStonEntity>(document, entityComparer, "A", "B")))
                .Add("A == B 2", () => Assert.IsTrue(AreEqual<IStonSimpleEntity>(document, entityComparer, "A", "B")))
                .Add("RA == RB", () => Assert.IsTrue(AreEqual<IStonEntity>(document, entityComparer, "RA", "RB")))
                .Add("RA == RB 2", () => Assert.IsTrue(AreEqual<IStonReferenceEntity>(document, entityComparer, "RA", "RB")))

                .Add("A != E", () => Assert.IsFalse(AreEqual<IStonEntity>(document, entityComparer, "A", "E")))
                .Add("RA != RE", () => Assert.IsFalse(AreEqual<IStonEntity>(document, entityComparer, "RA", "RE")))
                
                .Add("C != D", () => Assert.IsFalse(AreEqual<IStonEntity>(document, entityComparer, "C", "D")))
                .Add("C != D 2", () => Assert.IsFalse(AreEqual<IStonComplexEntity>(document, entityComparer, "C", "D")))
                .Add("RC != RD", () => Assert.IsFalse(AreEqual<IStonEntity>(document, entityComparer, "RC", "RD")))
                .Add("RC != RD 2", () => Assert.IsFalse(AreEqual<IStonReferenceEntity>(document, entityComparer, "RC", "RD")))

                .Add("C == C", () => Assert.IsTrue(AreEqual<IStonEntity>(document, entityComparer, "C", "C")))
                .Add("C == C 2", () => Assert.IsTrue(AreEqual<IStonComplexEntity>(document, entityComparer, "C", "C")))
                .Add("RC == RC", () => Assert.IsTrue(AreEqual<IStonEntity>(document, entityComparer, "RC", "RC")))
                .Add("RC == RC 2", () => Assert.IsTrue(AreEqual<IStonReferenceEntity>(document, entityComparer, "RC", "RC")))

                // null-valued entities are always equal
                .Add("null value == null value", () => Assert.IsTrue(entityComparer.Equals(
                    new StonSimpleEntity(new StonSimpleValue(StonDataType.Null, null)),
                    new StonSimpleEntity(new StonSimpleValue(StonDataType.Null, ""))
                    )))

                // non-null references, one of which, being from outside, points to a null entity
                .Add("valid reference != invalid reference", () => Assert.IsFalse(entityComparer.Equals(document.GetGlobalEntity("RA"), new AReferenceEntity(null))))
                .Add("valid reference != invalid reference 2", () => Assert.IsFalse(entityComparer.Equals(document.GetGlobalEntity("RA") as IStonReferenceEntity, new AReferenceEntity(null))))

                .Run();

            // some additional assertions related to hash codes
            var set = new HashSet<IStonEntity>(entityComparer);
            Assert.IsTrue(set.Add(document.GetGlobalEntity("RA")));
            Assert.IsFalse(set.Add(document.GetGlobalEntity("A")));
            Assert.IsTrue(set.Add(document.GetGlobalEntity("E")));
            Assert.IsFalse(set.Add(document.GetGlobalEntity("RE")));
        }

        /// <summary>
        /// Test cases related to equivalence comparison with null and invalid values.
        /// </summary>
        [TestMethod]
        public void Test_NullInvalidEquivalenceComparing()
        {
            var document = RegularStonReader.Default.ParseDocument("{ a: A=0, b: B=0, c: C=(0,0), d: D=(0,0), e: E=int 0, ra: RA=@A, rb: RB=@B, rc: RC=@C, rd: RD=@D, re: RE=@E }");
            var typeComparer = StonTypeEquivalenceComparer.Instance;
            var entityComparer = new StonSemanticEntityEquivalenceComparer(document);
            var keyComparer = new StonBindingKeyEquivalenceComparer(entityComparer);

            AggregateTester.New()

                // comparing non-existing types

                .Add("null type == null type 1", () => Assert.IsTrue(typeComparer.Equals(null as IStonType, null as IStonType)))
                .Add("null type == null type 2", () => Assert.IsTrue(typeComparer.Equals(null as IStonNamedType, null as IStonNamedType)))
                .Add("null type == null type 3", () => Assert.IsTrue(typeComparer.Equals(null as IStonCollectionType, null as IStonCollectionType)))
                .Add("null type == null type 4", () => Assert.IsTrue(typeComparer.Equals(null as IStonUnionType, null as IStonUnionType)))

                .Add("null type != !null type 1", () => Assert.IsFalse(typeComparer.Equals(null as IStonType, new InvalidStonType())))
                .Add("null type != !null type 2", () => Assert.IsFalse(typeComparer.Equals(null as IStonNamedType, new ANamedType("", false, null))))
                .Add("null type != !null type 3", () => Assert.IsFalse(typeComparer.Equals(null as IStonCollectionType, new ACollectionType(null))))
                .Add("null type != !null type 4", () => Assert.IsFalse(typeComparer.Equals(null as IStonUnionType, new AUnionType(null))))
                .Add("invalid type != invalid type", () => Assert.IsFalse(typeComparer.Equals(new InvalidStonType(), new InvalidStonType())))

                /// hashing non-existing and invalid hashes
                .Add("null type hash 1", () => Assert.AreEqual(0, typeComparer.GetHashCode(null as IStonType)))
                .Add("null type hash 2", () => Assert.AreEqual(0, typeComparer.GetHashCode(null as IStonNamedType)))
                .Add("null type hash 3", () => Assert.AreEqual(0, typeComparer.GetHashCode(null as IStonCollectionType)))
                .Add("null type hash 4", () => Assert.AreEqual(0, typeComparer.GetHashCode(null as IStonUnionType)))
                .Add("invalid type hash", () => Assert.AreEqual(0, typeComparer.GetHashCode(new InvalidStonType())))

                // comparing non-existing and invalid entities
                .Add("null entity == null entity 1", () => Assert.IsTrue(entityComparer.Equals(null as IStonEntity, null as IStonEntity)))
                .Add("null entity == null entity 2", () => Assert.IsTrue(entityComparer.Equals(null as IStonValuedEntity, null as IStonValuedEntity)))
                .Add("null entity == null entity 3", () => Assert.IsTrue(entityComparer.Equals(null as IStonSimpleEntity, null as IStonSimpleEntity)))
                .Add("null entity == null entity 4", () => Assert.IsTrue(entityComparer.Equals(null as IStonComplexEntity, null as IStonComplexEntity)))
                .Add("null entity == null entity 5", () => Assert.IsTrue(entityComparer.Equals(null as IStonReferenceEntity, null as IStonReferenceEntity)))

                .Add("null entity != !null entity 1", () => Assert.IsFalse(entityComparer.Equals(null, new InvalidStonEntity())))
                .Add("null entity != !null entity 2", () => Assert.IsFalse(entityComparer.Equals(null, new InvalidStonValuedEntity())))
                .Add("null entity != !null entity 3", () => Assert.IsFalse(entityComparer.Equals(null, new ASimpleEntity(null))))
                .Add("null entity != !null entity 4", () => Assert.IsFalse(entityComparer.Equals(null, new AComplexEntity())))
                .Add("null entity != !null entity 5", () => Assert.IsFalse(entityComparer.Equals(null, new AReferenceEntity(null))))

                // hashing non-existing and invalid entities
                .Add("null entity hash 1", () => Assert.AreEqual(0, entityComparer.GetHashCode(null as IStonEntity)))
                .Add("null entity hash 2", () => Assert.AreEqual(0, entityComparer.GetHashCode(null as IStonValuedEntity)))
                .Add("null entity hash 3", () => Assert.AreEqual(0, entityComparer.GetHashCode(null as IStonSimpleEntity)))
                .Add("null entity hash 4", () => Assert.AreEqual(0, entityComparer.GetHashCode(null as IStonComplexEntity)))
                .Add("null entity hash 5", () => Assert.AreEqual(0, entityComparer.GetHashCode(null as IStonReferenceEntity)))
                .Add("invalid entity hash", () => Assert.AreEqual(0, entityComparer.GetHashCode(new InvalidStonEntity())))
                .Add("invalid valued entity hash", () => Assert.AreEqual(0, entityComparer.GetHashCode(new InvalidStonValuedEntity())))

                // comparing non-existing binding keys
                .Add("null key = null key", () => Assert.IsTrue(keyComparer.Equals(null as IStonBindingKey, null as IStonBindingKey)))
                .Add("null name = null name", () => Assert.IsTrue(keyComparer.Equals(null as IStonBindingName, null as IStonBindingName)))
                .Add("null index = null index", () => Assert.IsTrue(keyComparer.Equals(null as IStonBindingIndex, null as IStonBindingIndex)))

                .Add("null key != !null key", () => Assert.IsFalse(keyComparer.Equals(null as IStonBindingKey, new InvalidStonBindingKey())))
                .Add("null name != !null name", () => Assert.IsFalse(keyComparer.Equals(null as IStonBindingName, new StonBindingName("name", false))))
                .Add("null index != !null index", () => Assert.IsFalse(keyComparer.Equals(null as IStonBindingIndex, new ABindingIndex(null))))

                // hashing non-existing and invalid binding keys
                .Add("null key hash", () => Assert.AreEqual(0, keyComparer.GetHashCode(null as IStonBindingKey)))
                .Add("null name hash", () => Assert.AreEqual(0, keyComparer.GetHashCode(null as IStonBindingName)))
                .Add("null index hash", () => Assert.AreEqual(0, keyComparer.GetHashCode(null as IStonBindingIndex)))
                .Add("invalid key hash", () => Assert.AreEqual(0, keyComparer.GetHashCode(new InvalidStonBindingKey())))

                .Run();
        }
    }
}

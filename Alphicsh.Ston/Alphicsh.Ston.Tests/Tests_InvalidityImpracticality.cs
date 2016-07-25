using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Alphicsh.Ston.Tests
{
    using Equivalence;
    using Tokenization;
    using static Helpers.Validator;

    /// <summary>
    /// Collection of tests for variety of strictly invalid or not supported cases, as well as exceptions.
    /// </summary>
    [TestClass]
    public class Tests_InvalidityImpracticality
    {
        // creates an object to operate on, based on its type
        private TActor MakeActor<TActor>()
            where TActor : class
        {
            var t = typeof(TActor);
            if (t == typeof(IStonEntity)) return RegularStonReader.Default.ParseEntity("{}") as TActor;
            if (t == typeof(IStonDocument)) return RegularStonReader.Default.ParseDocument("{}") as TActor;
            if (t == typeof(StonTokenWriter)) return new StonTokenWriter(new StringWriter()) as TActor;
            throw new ArgumentException("TActor is not one of the recognized types.");
        }

        #region Null-related exceptions

        // checks that a given action causes an ArgumentNullException
        // with a given parameter name
        private void ExpectArgumentNull(Action action, string paramName)
        {
            try
            {
                action();
                Assert.Fail("The action performed without throwing an ArgumentNullException.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual(paramName, ex.ParamName);
            }
        }

        // checks that a given action of a specific object type causes an ArgumentNullException
        // with a given parameter name
        private void ExpectArgumentNull<TActor>(Action<TActor> action, string paramName)
            where TActor : class
        {
            try
            {
                action(MakeActor<TActor>());
                Assert.Fail("The action performed without throwing an ArgumentNullException.");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual(paramName, ex.ParamName);
            }
        }

        /// <summary>
        /// Test cases related to providing null arguments directly.
        /// </summary>
        [TestMethod]
        public void Test_ArgumentNull()
        {
            // things would have been a great deal easier if the non-nullable reference type was implemented already...

            AggregateTester.New()

                // STON elements construction
                .Add("new StonSimpleEntity.Copy(value)", () => ExpectArgumentNull(() => new StonSimpleEntity(null as IStonSimpleValue), "value"))
                .Add("new StonReferenceEntity(address,globalIdentifier)", () => ExpectArgumentNull(() => new StonReferenceEntity(null as IStonAddress), "address"))

                .Add("new StonNamedType.Copy(name,typeParameters,isExtension)", () => ExpectArgumentNull(() => new StonNamedType(null as string), "name"))
                .Add("new StonCollectionType.Copy(elementType)", () => ExpectArgumentNull(() => new StonCollectionType(null as IStonType), "elementType"))
                .Add("new StonUnionType(permittedTypes)", () => ExpectArgumentNull(() => new StonUnionType(null as IEnumerable<IStonType>), "permittedTypes"))

                .Add("new StonSimpleValue(value)", () => ExpectArgumentNull(() => StonSimpleValue.Copy(null), "value"))

                .Add("new StonBindingName(name,isExtension)", () => ExpectArgumentNull(() => new StonBindingName(null as string), "name"))
                .Add("new StonBindingIndex(bindingIndex)", () => ExpectArgumentNull(() => new StonBindingIndex(null as IEnumerable<IStonEntity>), "parameters"))

                .Add("new StonAddress(initialContext,relativePath)", () => ExpectArgumentNull(() => new StonAddress(null, null), "initialContext"))
                .Add("new StonGlobalEntityInitialContext(globalIdentifier)", () => ExpectArgumentNull(() => new StonGlobalEntityInitialContext(null as string), "globalIdentifier"))
                .Add("new StonMemberPathSegment(bindingKey)", () => ExpectArgumentNull(() => new StonMemberPathSegment(null as IStonBindingKey), "bindingKey"))
                .Add("new StonCollectionElementPathSegment(bindingKey)", () => ExpectArgumentNull(() => new StonCollectionElementPathSegment(null as IStonEntity), "elementIndex"))

                // STON elements copying
                .Add("StonEntity.Copy(entity)", () => ExpectArgumentNull(() => StonEntity.Copy(null), "entity"))
                .Add("StonValuedEntity.Copy(entity)", () => ExpectArgumentNull(() => StonValuedEntity.Copy(null), "entity"))
                .Add("StonSimpleEntity.Copy(entity)", () => ExpectArgumentNull(() => StonSimpleEntity.Copy(null), "entity"))
                .Add("StonComplexEntity.Copy(entity)", () => ExpectArgumentNull(() => StonComplexEntity.Copy(null), "entity"))
                .Add("StonReferenceEntity.Copy(entity)", () => ExpectArgumentNull(() => StonReferenceEntity.Copy(null), "entity"))

                .Add("StonType.Copy(type)", () => ExpectArgumentNull(() => StonType.Copy(null), "type"))
                .Add("StonNamedType.Copy(type)", () => ExpectArgumentNull(() => StonNamedType.Copy(null), "type"))
                .Add("StonCollectionType.Copy(type)", () => ExpectArgumentNull(() => StonCollectionType.Copy(null), "type"))
                .Add("StonUnionType.Copy(type)", () => ExpectArgumentNull(() => StonUnionType.Copy(null), "type"))

                .Add("StonSimpleValue.Copy(value)", () => ExpectArgumentNull(() => StonSimpleValue.Copy(null), "value"))

                .Add("StonConstruction.Copy(construction)", () => ExpectArgumentNull(() => StonConstruction.Copy(null), "construction"))
                .Add("StonMemberInit.Copy(memberInit)", () => ExpectArgumentNull(() => StonMemberInit.Copy(null), "memberInit"))
                .Add("StonBindingKey.Copy(bindingKey)", () => ExpectArgumentNull(() => StonBindingKey.Copy(null), "bindingKey"))
                .Add("StonBindingName.Copy(bindingName)", () => ExpectArgumentNull(() => StonBindingName.Copy(null), "bindingName"))
                .Add("StonBindingIndex.Copy(bindingIndex)", () => ExpectArgumentNull(() => StonBindingIndex.Copy(null), "bindingIndex"))
                .Add("StonCollectionInit.Copy(collectionInit)", () => ExpectArgumentNull(() => StonCollectionInit.Copy(null), "collectionInit"))

                .Add("StonAddress.Copy(address)", () => ExpectArgumentNull(() => StonAddress.Copy(null), "address"))
                .Add("StonInitialContext.Copy(context)", () => ExpectArgumentNull(() => StonInitialContext.Copy(null), "context"))
                .Add("StonAncestorInitialContext.Copy(context)", () => ExpectArgumentNull(() => StonAncestorInitialContext.Copy(null), "context"))
                .Add("StonGlobalEntityInitialContext.Copy(context)", () => ExpectArgumentNull(() => StonGlobalEntityInitialContext.Copy(null), "context"))
                .Add("StonPathSegment.Copy(segment)", () => ExpectArgumentNull(() => StonPathSegment.Copy(null), "segment"))
                .Add("StonAncestorPathSegment.Copy(segment)", () => ExpectArgumentNull(() => StonAncestorPathSegment.Copy(null), "segment"))
                .Add("StonMemberPathSegment.Copy(segment)", () => ExpectArgumentNull(() => StonMemberPathSegment.Copy(null), "segment"))
                .Add("StonCollectionElementPathSegment.Copy(segment)", () => ExpectArgumentNull(() => StonCollectionElementPathSegment.Copy(null), "segment"))

                // document methods
                .Add("StonDocument.GetParentContext(context)", () => ExpectArgumentNull<IStonDocument>((document) => document.GetParentContext(null), "context"))
                .Add("StonDocument.GetMember(entity?,memberKey)", () => ExpectArgumentNull<IStonDocument>((document) => document.GetMember(null, null), "entity"))
                .Add("StonDocument.GetMember(entity,memberKey?)", () => ExpectArgumentNull<IStonDocument>((document) => document.GetMember(document.Core as IStonComplexEntity, null), "memberKey"))
                .Add("StonDocument.GetReferencedValue(reference)", () => ExpectArgumentNull<IStonDocument>((document) => document.GetReferencedValue(null), "reference"))

                // reading/writing
                .Add("new StonReader(elementFactory?,documentFactory)", () => ExpectArgumentNull(() => new RegularStonReader(null, null), "elementFactory"))
                .Add("new StonReader(elementFactory,documentFactory?)", () => ExpectArgumentNull(() => new RegularStonReader(Building.CoreStonElementFactory.Instance, null), "documentFactory"))
                .Add("StonReader.ReadEntity(reader)", () => ExpectArgumentNull(() => RegularStonReader.Default.ReadEntity(null), "reader"))
                .Add("StonReader.ReadDocument(reader,...)", () => ExpectArgumentNull(() => RegularStonReader.Default.ReadDocument(null), "reader"))

                .Add("new StonTokenReader(innerReader)", () => ExpectArgumentNull(() => new StonTokenReader(null), "innerReader"))

                .Add("*IStonReader.ParseEntity(value)", () => ExpectArgumentNull(() => (RegularStonReader.Default as IStonReader).ParseEntity(null), "value"))
                .Add("*IStonReader.ParseEntity<>(value)", () => ExpectArgumentNull(() => RegularStonReader.Default.ParseEntity(null), "value"))
                .Add("*IStonReader.LoadEntity(stream)", () => ExpectArgumentNull(() => (RegularStonReader.Default as IStonReader).LoadEntity(null as Stream), "stream"))
                .Add("*IStonReader.LoadEntity<>(stream)", () => ExpectArgumentNull(() => RegularStonReader.Default.LoadEntity(null as Stream), "stream"))
                .Add("*IStonReader.ParseDocument(value)", () => ExpectArgumentNull(() => (RegularStonReader.Default as IStonReader).ParseDocument(null), "value"))
                .Add("*IStonReader.ParseDocument<>(value)", () => ExpectArgumentNull(() => RegularStonReader.Default.ParseDocument(null), "value"))
                .Add("*IStonReader.LoadDocument(stream)", () => ExpectArgumentNull(() => (RegularStonReader.Default as IStonReader).LoadDocument(null as Stream), "stream"))
                .Add("*IStonReader.LoadDocument<>(stream)", () => ExpectArgumentNull(() => RegularStonReader.Default.LoadDocument(null as Stream), "stream"))

                .Add("new StonTokenWriter(innerWriter)", () => ExpectArgumentNull(() => new StonTokenWriter(null), "innerWriter"))
                .Add("StonTokenWriter.WriteStringLiteral(content,delimiter)", () => ExpectArgumentNull((StonTokenWriter writer) => writer.WriteStringLiteral(null), "content"))
                .Add("StonTokenWriter.WriteBinaryLiteral(content,baseId)", () => ExpectArgumentNull((StonTokenWriter writer) => writer.WriteBinaryLiteral(null), "content"))
                .Add("StonTokenWriter.WriteCanonicalNumberLiteral(content)", () => ExpectArgumentNull((StonTokenWriter writer) => writer.WriteCanonicalNumberLiteral(null), "content"))
                .Add("StonTokenWriter.WritePlainNumberLiteral(content)", () => ExpectArgumentNull((StonTokenWriter writer) => writer.WritePlainNumberLiteral(null), "content"))
                .Add("StonTokenWriter.WriteScientificNumberLiteral(content)", () => ExpectArgumentNull((StonTokenWriter writer) => writer.WriteScientificNumberLiteral(null), "content"))

                .Add("CanonicalStonWriter.WriteEntity(writer?,entity)", () => ExpectArgumentNull(() => CanonicalStonWriter.Instance.WriteEntity(null, null), "writer"))
                .Add("CanonicalStonWriter.WriteEntity(writer,entity?)", () => ExpectArgumentNull(() => CanonicalStonWriter.Instance.WriteEntity(new System.IO.StringWriter(), null), "entity"))
                .Add("CanonicalStonWriter.WriteDocument(writer?,document)", () => ExpectArgumentNull(() => CanonicalStonWriter.Instance.WriteDocument(null, null), "writer"))
                .Add("CanonicalStonWriter.WriteDocument(writer,document?)", () => ExpectArgumentNull(() => CanonicalStonWriter.Instance.WriteDocument(new System.IO.StringWriter(), null), "document"))

                .Add("*IStonEntity.ToString(writer)", () => ExpectArgumentNull<IStonEntity>((entity) => entity.ToString(null), "writer"))
                .Add("*IStonEntity.ToString<>(writer)", () => ExpectArgumentNull<IStonEntity>((entity) => entity.ToString(null as IStonWriter<IStonEntity, IStonDocument>), "writer"))
                .Add("*IStonEntity.Save(stream?,writer)", () => ExpectArgumentNull<IStonEntity>((entity) => entity.Save(null as Stream, CanonicalStonWriter.Instance), "stream"))
                .Add("*IStonEntity.Save(stream,writer?)", () => ExpectArgumentNull<IStonEntity>((entity) => entity.Save(new MemoryStream(), null), "writer"))
                .Add("*IStonEntity.Save<>(stream?,writer)", () => ExpectArgumentNull<IStonEntity>((entity) => entity.Save(null as Stream, null as IStonWriter<IStonEntity, IStonDocument>), "stream"))
                .Add("*IStonEntity.Save<>(stream,writer?)", () => ExpectArgumentNull<IStonEntity>((entity) => entity.Save(new MemoryStream(), null as IStonWriter<IStonEntity, IStonDocument>), "writer"))
                .Add("*IStonDocument.ToString(writer)", () => ExpectArgumentNull<IStonDocument>((document) => document.ToString(null), "writer"))
                .Add("*IStonDocument.ToString<>(writer)", () => ExpectArgumentNull<IStonDocument>((document) => document.ToString(null as IStonWriter<IStonEntity, IStonDocument>), "writer"))
                .Add("*IStonDocument.Save(stream?,writer)", () => ExpectArgumentNull<IStonDocument>((document) => document.Save(null as Stream, null), "stream"))
                .Add("*IStonDocument.Save(stream,writer?)", () => ExpectArgumentNull<IStonDocument>((document) => document.Save(new MemoryStream(), null), "writer"))
                .Add("*IStonDocument.Save<>(stream?,writer)", () => ExpectArgumentNull<IStonDocument>((document) => document.Save(null as Stream, null as IStonWriter<IStonEntity, IStonDocument>), "stream"))
                .Add("*IStonDocument.Save<>(stream,writer?)", () => ExpectArgumentNull<IStonDocument>((document) => document.Save(new MemoryStream(), null as IStonWriter<IStonEntity, IStonDocument>), "writer"))

                // equivalence comparing
                .Add("new StonSemanticEntityEquivalenceComparer(document)", () => ExpectArgumentNull(() => new StonSemanticEntityEquivalenceComparer(null), "document"))
                .Add("new StonBindingKeyEquivalenceComparer(indexParameterComparer)", () => ExpectArgumentNull(() => new StonBindingKeyEquivalenceComparer(null), "indexParameterComparer"))

                // validation

                .Add("ValidateEntity(entity) 1", () => ExpectArgumentNull(() => ValidateEntity(null as IStonEntity), "entity"))
                .Add("ValidateEntity(entity) 2", () => ExpectArgumentNull(() => ValidateEntity(null as IStonValuedEntity), "entity"))
                .Add("ValidateEntity(entity) 3", () => ExpectArgumentNull(() => ValidateEntity(null as IStonSimpleEntity), "entity"))
                .Add("ValidateEntity(entity) 4", () => ExpectArgumentNull(() => ValidateEntity(null as IStonComplexEntity), "entity"))
                .Add("ValidateEntity(entity) 5", () => ExpectArgumentNull(() => ValidateEntity(null as IStonReferenceEntity), "entity"))

                .Add("ValidateGlobalIdentifier(globalIdentifier)", () => ExpectArgumentNull(() => ValidateGlobalIdentifier(null), "globalIdentifier"))

                .Add("ValidateType(type) 1", () => ExpectArgumentNull(() => ValidateType(null as IStonType), "type"))
                .Add("ValidateType(type) 2", () => ExpectArgumentNull(() => ValidateType(null as IStonNamedType), "type"))
                .Add("ValidateType(type) 3", () => ExpectArgumentNull(() => ValidateType(null as IStonCollectionType), "type"))
                .Add("ValidateType(type) 4", () => ExpectArgumentNull(() => ValidateType(null as IStonUnionType), "type"))

                .Add("ValidateSimpleValue(value)", () => ExpectArgumentNull(() => ValidateSimpleValue(null), "value"))

                .Add("ValidateConstruction(construction)", () => ExpectArgumentNull(() => ValidateConstruction(null), "construction"))
                .Add("ValidateMemberInit(memberInit)", () => ExpectArgumentNull(() => ValidateMemberInit(null), "memberInit"))
                .Add("ValidateBindingKey(bindingKey) 1", () => ExpectArgumentNull(() => ValidateBindingKey(null as IStonBindingKey), "bindingKey"))
                .Add("ValidateBindingKey(bindingKey) 2", () => ExpectArgumentNull(() => ValidateBindingKey(null as IStonBindingName), "bindingKey"))
                .Add("ValidateBindingKey(bindingKey) 3", () => ExpectArgumentNull(() => ValidateBindingKey(null as IStonBindingIndex), "bindingKey"))
                .Add("ValidateCollectionInit(collectionInit)", () => ExpectArgumentNull(() => ValidateCollectionInit(null), "collectionInit"))

                .Add("ValidateAddress(address)", () => ExpectArgumentNull(() => ValidateAddress(null), "address"))
                .Add("ValidateInitialContext(initialContext) 1", () => ExpectArgumentNull(() => ValidateInitialContext(null as IStonInitialContext), "initialContext"))
                .Add("ValidateInitialContext(initialContext) 2", () => ExpectArgumentNull(() => ValidateInitialContext(null as IStonAncestorInitialContext), "initialContext"))
                .Add("ValidateInitialContext(initialContext) 3", () => ExpectArgumentNull(() => ValidateInitialContext(null as IStonGlobalEntityInitialContext), "initialContext"))
                .Add("ValidatePathSegment(segment) 1", () => ExpectArgumentNull(() => ValidatePathSegment(null as IStonPathSegment), "segment"))
                .Add("ValidatePathSegment(segment) 2", () => ExpectArgumentNull(() => ValidatePathSegment(null as IStonAncestorPathSegment), "segment"))
                .Add("ValidatePathSegment(segment) 3", () => ExpectArgumentNull(() => ValidatePathSegment(null as IStonMemberPathSegment), "segment"))
                .Add("ValidatePathSegment(segment) 4", () => ExpectArgumentNull(() => ValidatePathSegment(null as IStonCollectionElementPathSegment), "segment"))

                // ironically, exceptions

                .Add("new StonImplementationException(elementType?,baseInterface,derivedInterfaces)", () => ExpectArgumentNull(() => new StonImplementationException(null, null, null as Type[]), "elementType"))
                .Add("new StonImplementationException(elementType,baseInterface?,derivedInterfaces)", () => ExpectArgumentNull(() => new StonImplementationException(typeof(object), null, null as Type[]), "baseInterface"))
                .Add("new StonImplementationException(elementType,baseInterface,derivedInterfaces?)", () => ExpectArgumentNull(() => new StonImplementationException(typeof(object), typeof(object), null as Type[]), "derivedInterfaces"))

                .Add("new StonDuplicateGlobalEntityException(firstEntity?,secondEntity)", () => ExpectArgumentNull(() => new StonDuplicateGlobalEntityException(null, null), "firstEntity"))
                .Add("new StonDuplicateGlobalEntityException(firstEntity,secondEntity?)", () => ExpectArgumentNull(() => new StonDuplicateGlobalEntityException(MakeActor<IStonEntity>(), null), "secondEntity"))
                .Add("new StonDuplicateGlobalEntityException(firstEntity?,secondEntity,message)", () => ExpectArgumentNull(() => new StonDuplicateGlobalEntityException(null, null, ""), "firstEntity"))
                .Add("new StonDuplicateGlobalEntityException(firstEntity,secondEntity?,message)", () => ExpectArgumentNull(() => new StonDuplicateGlobalEntityException(MakeActor<IStonEntity>(), null, ""), "secondEntity"))

                .Add("new StonCircularConstructionException(document?,constructionCycle)", () => ExpectArgumentNull(() => new StonCircularConstructionException(null, null), "document"))
                .Add("new StonCircularConstructionException(document,constructionCycle?)", () => ExpectArgumentNull<IStonDocument>((document) => new StonCircularConstructionException(document, null), "constructionCycle"))
                
                .Add("new StonExtensionTypeException(extensionType)", () => ExpectArgumentNull(() => new StonExtensionTypeException(null as IStonNamedType), "extensionType"))
                .Add("new StonExtensionTypeException(entity)", () => ExpectArgumentNull(() => new StonExtensionTypeException(null as IStonValuedEntity), "entity"))

                .Add("new StonExtensionTypeException(memberKey)", () => ExpectArgumentNull(() => new StonExtensionMemberException(null), "memberKey"))

                .Run();
        }

        /// <summary>
        /// Test cases related to providing structures with non-existing elements inside them.
        /// </summary>
        [TestMethod]
        public void Test_InnerNull()
        {
            AggregateTester.New()

                // entity writing
                .Add("writing null entity", () => Expect.Exception<StonException>(() => new AComplexEntity(collectionInit: new ACollectionInit(new IStonEntity[] { null })).ToCanonicalForm(), "A non-existing entity has been found in the structure to be written."))


                .Add("writing null type", () => Expect.Exception<StonException>(() => new ASimpleEntity(null, new ANamedType("name", false, new IStonType[] { null })).ToCanonicalForm(), "A non-existing type has been found in the structure to be written."))
                .Add("writing null name", () => Expect.Exception<StonException>(() => new ASimpleEntity(null, new ANamedType(null, false, new IStonType[] { null })).ToCanonicalForm(), "A non-existing string has been found in the structure to be written."))
                .Add("writing named parameterless type", () => Expect.Exception<StonException>(() => new ASimpleEntity(null, new ANamedType("", false, null)).ToCanonicalForm(), "A named type cannot have a non-existing type parameters collection."))


                .Add("writing simple entity w/o value", () => Expect.Exception<StonException>(() => new ASimpleEntity(null).ToCanonicalForm(), "A simple-valued entity cannot have a non-existing value."))
                .Add("writing simple entity with unknown data type", () => Expect.Exception<StonException>(() => new ASimpleEntity(new StonSimpleValue((StonDataType)42, null)).ToCanonicalForm(), "Unknown simple value data type: 42."))


                .Add("writing construction w/o positional parameters",
                    () => Expect.Exception<StonException>(() => new AComplexEntity(construction: new AConstruction(null, null)).ToCanonicalForm(),
                    "A complex value construction cannot have a non-existing positional parameters collection."))
                .Add("writing construction w/o named parameters",
                    () => Expect.Exception<StonException>(() => new AComplexEntity(construction: new AConstruction(new IStonEntity[] { }, null)).ToCanonicalForm(),
                    "A complex value construction cannot have a non-existing named parameters collection."
                    ))
                .Add("writing member init w/o bindings", () => Expect.Exception<StonException>(() => new AComplexEntity(memberInit: new AMemberInit(null)).ToCanonicalForm(), "A complex value member initialization cannot have a non-existing member bindings collection."))
                .Add("writing null binding key", () => Expect.Exception<StonException>(
                    () => new AComplexEntity(memberInit: new AMemberInit(new KeyValuePair<IStonBindingKey, IStonEntity>[] { new KeyValuePair<IStonBindingKey, IStonEntity>() })).ToCanonicalForm(),
                    "A non-existing member binding key has been found in the structure to be written.")
                    )
                .Add("writing index w/o parameters", () => Expect.Exception<StonException>(
                    () => new AComplexEntity(memberInit: new AMemberInit(new KeyValuePair<IStonBindingKey, IStonEntity>[] { new KeyValuePair<IStonBindingKey, IStonEntity>(new ABindingIndex(null), null) })).ToCanonicalForm(),
                    "A member binding index cannot have non-existing parameters."
                    ))
                .Add("writing null entity", () => Expect.Exception<StonException>(() => new AComplexEntity(collectionInit: new ACollectionInit(null)).ToCanonicalForm(), "A complex value collection initialization cannot have a non-existing elements collection."))


                .Add("writing address w/o initial context", () => Expect.Exception<StonException>(() => new AReferenceEntity(new AnAddress(null, null)).ToCanonicalForm(), "A reference address cannot have a non-existing initial context."))
                .Add("writing address w/o relative path", () => Expect.Exception<StonException>(() => new AReferenceEntity(new AnAddress(new StonAncestorInitialContext(0), null)).ToCanonicalForm(), "A reference address cannot have a non-existing relative path."))
                .Add("writing global initial context w/o identifier",
                    () => Expect.Exception<StonException>(() => new AReferenceEntity(new AnAddress(new AGlobalContext(null), new IStonPathSegment[] { })).ToCanonicalForm(),
                    "A global entity initial context must have an existing global identifier."
                    ))
                .Add("writing path w/o segments", () => Expect.Exception<StonException>(() => new AReferenceEntity(new AnAddress(new StonAncestorInitialContext(0), new IStonPathSegment[] { null })).ToCanonicalForm(), "A non-existing path segment has been found in the structure to be written."))


                // validation
                .Add("validating unnamed type", () => Expect.Exception<StonException>(() => ValidateType(new ANamedType(null, false, null) as IStonType), "A named type cannot have a non-existing name."))
                .Add("validating named parameterless type", () => Expect.Exception<StonException>(() => ValidateType(new ANamedType("", false, null) as IStonType), "A named type cannot have a non-existing type parameters collection."))
                .Add("validating type w/ null parameters", () => Expect.Exception<StonException>(() => ValidateType(new ANamedType("", false, new IStonType[] { null }) as IStonType), "A named type cannot have a non-existing type parameter."))
                .Add("validating collection type w/o element type", () => Expect.Exception<StonException>(() => ValidateType(new ACollectionType(null) as IStonType), "A collection type cannot have a non-existing element type."))
                .Add("validating union type w/o permitted types", () => Expect.Exception<StonException>(() => ValidateType(new AUnionType(null) as IStonType), "A union type must have at least two permitted types."))
                .Add("validating union type w/ one null type", () => Expect.Exception<StonException>(() => ValidateType(new AUnionType(new IStonType[] { null }) as IStonType), "A union type must have at least two permitted types."))
                .Add("validating union type w/ several null types", () => Expect.Exception<StonException>(() => ValidateType(new AUnionType(new IStonType[] { null, null }) as IStonType), "A union type cannot have a non-existing permitted type."))

                .Add("validating null value", () => Expect.Exception<StonException>(() => ValidateEntity(new ASimpleEntity(null) as IStonEntity), "A simple-valued entity cannot have a non-existing value."))

                .Add("validating construction w/o positional parameters", () => Expect.Exception<StonException>(() => ValidateConstruction(new AConstruction(null, null)), "A complex value construction cannot have a non-existing positional parameters collection."))
                .Add("validating construction w/o named parameters", () => Expect.Exception<StonException>(() => ValidateConstruction(new AConstruction(new IStonEntity[] { }, null)), "A complex value construction cannot have a non-existing named parameters collection."))
                .Add("validating construction w/ null positional parameters",
                    () => Expect.Exception<StonException>(() => ValidateConstruction(new AConstruction(new IStonEntity[] { null }, new KeyValuePair<string, IStonEntity>[] { })),
                    "A complex value construction cannot have a non-existing parameter value."
                    ))
                .Add("validating construction w/ null named parameters",
                    () => Expect.Exception<StonException>(() => ValidateConstruction(new AConstruction(new IStonEntity[] { }, new KeyValuePair<string, IStonEntity>[] { new KeyValuePair<string, IStonEntity>("", null) })),
                    "A complex value construction cannot have a non-existing parameter value."
                    ))

                .Add("validating member init w/o bindings", () => Expect.Exception<StonException>(() => ValidateMemberInit(new AMemberInit(null)), "A complex value member initialization cannot have a non-existing member bindings collection."))
                .Add("validating member init w/ null bindings", () => Expect.Exception<StonException>(
                    () => ValidateMemberInit(new AMemberInit(new KeyValuePair<IStonBindingKey, IStonEntity>[] { new KeyValuePair<IStonBindingKey, IStonEntity>(new ABindingName("", false), null) })),
                    "A complex value member initialization cannot have non-existing member values."
                    ))
                .Add("validating binding name w/o name", () => Expect.Exception<StonException>(() => ValidateBindingKey(new ABindingName(null, false) as IStonBindingKey), "A member binding name cannot have non-existing name."))
                .Add("validating binding index w/o parameters", () => Expect.Exception<StonException>(() => ValidateBindingKey(new ABindingIndex(null) as IStonBindingKey), "A member binding index must be neither non-existing nor empty."))
                .Add("validating binding name w/ null parameters", () => Expect.Exception<StonException>(() => ValidateBindingKey(new ABindingIndex(new IStonEntity[] { null }) as IStonBindingKey), "A member binding index cannot have non-existing index parameters."))

                .Add("validating collection init w/o elements", () => Expect.Exception<StonException>(() => ValidateCollectionInit(new ACollectionInit(null)), "A complex value collection initialization cannot have a non-existing elements collection."))
                .Add("validating collection init w/ null elements", () => Expect.Exception<StonException>(() => ValidateCollectionInit(new ACollectionInit(new IStonEntity[] { null })), "A complex value collection initialization cannot have non-existing elements."))

                .Add("validating null address", () => Expect.Exception<StonException>(() => ValidateEntity(new AReferenceEntity(null) as IStonEntity), "A reference entity cannot have a non-existing address."))
                .Add("validating address w/o initial context", () => Expect.Exception<StonException>(() => ValidateAddress(new AnAddress(null, null)), "A reference address cannot have a non-existing initial context."))
                .Add("validating address w/o relative path", () => Expect.Exception<StonException>(() => ValidateAddress(new AnAddress(new StonAncestorInitialContext(0), null)), "A reference address cannot have a non-existing relative path."))
                .Add("validating address w/ null path segments", () => Expect.Exception<StonException>(() => ValidateAddress(new AnAddress(new StonAncestorInitialContext(0), new IStonPathSegment[] { null })), "A reference address relative path cannot have non-existing path segments."))
                .Add("validating null global context", () => Expect.Exception<StonException>(() => ValidateInitialContext(new AGlobalContext(null) as IStonInitialContext), "A global entity initial context must have an existing global identifier."))
                .Add("validating member segment w/o key", () => Expect.Exception<StonException>(() => ValidatePathSegment(new AMemberSegment(null) as IStonPathSegment), "A member path segment cannot have a non-existing binding key."))
                .Add("validating element segment w/o key", () => Expect.Exception<StonException>(() => ValidatePathSegment(new AnElementSegment(null) as IStonPathSegment), "A collection element path segment cannot have a non-existing element index."))

                .Run();
        }

        #endregion

        #region Invalid implementations

        // checks that a given action throws a StonImplementationException
        // for a given element type, base interface and set of interfaces to derive
        private void ExpectImplementationException(Action action, Type elementType, Type baseInterface, params Type[] derivedInterfaces)
        {
            try
            {
                action();
                Assert.Fail("The implementation is valid. This should *not* have happened.");
            }
            catch (StonImplementationException ex)
            {
                Assert.AreEqual(elementType, ex.ElementType);
                Assert.AreEqual(baseInterface, ex.BaseInterface);
                Assert.IsTrue(derivedInterfaces.SequenceEqual(ex.DerivedInterfaces));
            }
        }

        /// <summary>
        /// Test cases related to invalid STON implementations.
        /// </summary>
        [TestMethod]
        public void Test_InvalidImplementations()
        {
            AggregateTester.New()

                // STON elements copying
                .Add("StonEntity.Copy(IStonEntity)", () => ExpectImplementationException(() => StonEntity.Copy(new InvalidStonEntity()), typeof(InvalidStonEntity), typeof(IStonEntity), typeof(IStonValuedEntity), typeof(IStonReferenceEntity)))
                .Add("StonValuedEntity.Copy(IStonValuedEntity)", () => ExpectImplementationException(() => StonValuedEntity.Copy(new InvalidStonValuedEntity()), typeof(InvalidStonValuedEntity), typeof(IStonValuedEntity), typeof(IStonSimpleEntity), typeof(IStonComplexEntity)))
                .Add("StonType.Copy(IStonType)", () => ExpectImplementationException(() => StonType.Copy(new InvalidStonType()), typeof(InvalidStonType), typeof(IStonType), typeof(IStonNamedType), typeof(IStonCollectionType), typeof(IStonUnionType)))
                .Add("StonBindingKey.Copy(IStonBindingKey)", () => ExpectImplementationException(() => StonBindingKey.Copy(new InvalidStonBindingKey()), typeof(InvalidStonBindingKey), typeof(IStonBindingKey), typeof(IStonBindingName), typeof(IStonBindingIndex)))
                .Add("StonInitialContext.Copy(IStonInitialContext)", () => ExpectImplementationException(() => StonInitialContext.Copy(new InvalidStonInitialContext()), typeof(InvalidStonInitialContext), typeof(IStonInitialContext), typeof(IStonAncestorInitialContext), typeof(IStonGlobalEntityInitialContext)))
                .Add("StonPathSegment.Copy(IStonPathSegment)", () => ExpectImplementationException(() => StonPathSegment.Copy(new InvalidStonPathSegment()), typeof(InvalidStonPathSegment), typeof(IStonPathSegment), typeof(IStonAncestorPathSegment), typeof(IStonMemberPathSegment), typeof(IStonCollectionElementPathSegment)))
                
                // entity writing
                .Add("write invalid entity", () => ExpectImplementationException(() => new InvalidStonEntity().ToCanonicalForm(), typeof(InvalidStonEntity), typeof(IStonEntity), typeof(IStonValuedEntity), typeof(IStonReferenceEntity)))
                .Add("write invalid valued entity", () => ExpectImplementationException(() => new InvalidStonValuedEntity().ToCanonicalForm(), typeof(InvalidStonValuedEntity), typeof(IStonValuedEntity), typeof(IStonSimpleEntity), typeof(IStonComplexEntity)))
                .Add("write invalid type", () => ExpectImplementationException(() => new ASimpleEntity(null, new InvalidStonType()).ToCanonicalForm(), typeof(InvalidStonType), typeof(IStonType), typeof(IStonNamedType), typeof(IStonCollectionType), typeof(IStonUnionType)))
                .Add("write invalid binding key", () => ExpectImplementationException(() =>
                    new AReferenceEntity(new AnAddress(new StonAncestorInitialContext(0), new IStonPathSegment[] { new AMemberSegment(new InvalidStonBindingKey() ) })).ToCanonicalForm(),
                    typeof(InvalidStonBindingKey), typeof(IStonBindingKey), typeof(IStonBindingName), typeof(IStonBindingIndex))
                    )
                .Add("write invalid initial context", () => ExpectImplementationException(() =>
                    new AReferenceEntity(new AnAddress(new InvalidStonInitialContext(), new IStonPathSegment[] { })).ToCanonicalForm(),
                    typeof(InvalidStonInitialContext), typeof(IStonInitialContext), typeof(IStonAncestorInitialContext), typeof(IStonGlobalEntityInitialContext))
                    )
                .Add("write invalid path segment", () => ExpectImplementationException(() =>
                    new AReferenceEntity(new AnAddress(new StonAncestorInitialContext(0), new IStonPathSegment[] { new InvalidStonPathSegment() })).ToCanonicalForm(),
                    typeof(InvalidStonPathSegment), typeof(IStonPathSegment), typeof(IStonAncestorPathSegment), typeof(IStonMemberPathSegment), typeof(IStonCollectionElementPathSegment))
                    )

                // validation
                .Add("Validator.ValidateEntity(IStonEntity)", () => ExpectImplementationException(() => ValidateEntity(new InvalidStonEntity()), typeof(InvalidStonEntity), typeof(IStonEntity), typeof(IStonValuedEntity), typeof(IStonReferenceEntity)))
                .Add("Validator.ValidateEntity(IStonValuedEntity)", () => ExpectImplementationException(() => ValidateEntity(new InvalidStonValuedEntity()), typeof(InvalidStonValuedEntity), typeof(IStonValuedEntity), typeof(IStonSimpleEntity), typeof(IStonComplexEntity)))
                .Add("Validator.ValidateType(IStonType)", () => ExpectImplementationException(() => ValidateType(new InvalidStonType()), typeof(InvalidStonType), typeof(IStonType), typeof(IStonNamedType), typeof(IStonCollectionType), typeof(IStonUnionType)))
                .Add("Validator.ValidateBindingKey(IStonValuedEntity)", () => ExpectImplementationException(() => ValidateBindingKey(new InvalidStonBindingKey()), typeof(InvalidStonBindingKey), typeof(IStonBindingKey), typeof(IStonBindingName), typeof(IStonBindingIndex)))
                .Add("Validator.ValidateInitialContext(IStonEntity)", () => ExpectImplementationException(() => ValidateInitialContext(new InvalidStonInitialContext()), typeof(InvalidStonInitialContext), typeof(IStonInitialContext), typeof(IStonAncestorInitialContext), typeof(IStonGlobalEntityInitialContext)))
                .Add("Validator.ValidatePathSegment(IStonValuedEntity)", () => ExpectImplementationException(() => ValidatePathSegment(new InvalidStonPathSegment()), typeof(InvalidStonPathSegment), typeof(IStonPathSegment), typeof(IStonAncestorPathSegment), typeof(IStonMemberPathSegment), typeof(IStonCollectionElementPathSegment)))

                .Run();
        }

        #endregion

        #region Exceptions

        /// <summary>
        /// Testing correctly created exceptions that aren't tested anywhere else.
        /// </summary>
        [TestMethod]
        public void Test_RegularExceptions()
        {
            // in the current implementation, no document-side extensions are implemented
            // so StonExtensionTypeException/StonExtensionMemberException aside from unknown extension exceptions
            // aren't really practical yet, unless in specific applications

            var entity = RegularStonReader.Default.ParseEntity("!extension entity") as IStonValuedEntity;
            Assert.AreEqual(entity, new StonExtensionTypeException(entity).Entity);

            var complexEntity = RegularStonReader.Default.ParseEntity("{ !extension: entity }") as IStonComplexEntity;
            var memberBinding = complexEntity.MemberInit.MemberBindings.First();
            Assert.AreEqual(memberBinding.Value, new StonExtensionMemberException(memberBinding.Key as IStonBindingName, memberBinding.Value).MemberValue);
        }

        /// <summary>
        /// Test cases related to creating malformed exceptions.
        /// </summary>
        [TestMethod]
        public void Test_MalformedExceptions()
        {
            var glob1 = RegularStonReader.Default.ParseEntity("glob1");
            var glob2 = RegularStonReader.Default.ParseEntity("glob2");
            var glob3 = RegularStonReader.Default.ParseEntity("&GLOB3 = glob3");
            var glob4 = RegularStonReader.Default.ParseEntity("&GLOB4 = glob4");

            AggregateTester.New()

                // implementation exceptions
                .Add("null derived implementation interface", () => Expect.Exception<ArgumentException>(() => new StonImplementationException(typeof(object), typeof(object), typeof(object), null), "The derived interfaces list cannot contain any null."))

                // duplicate global entity exceptions
                .Add("non-duplicate global entity", () => Expect.Exception<ArgumentException>(() => new StonDuplicateGlobalEntityException(glob3, glob4), "The given entities have different global identifiers."))
                .Add("non-duplicate global entity", () => Expect.Exception<ArgumentException>(() => new StonDuplicateGlobalEntityException(glob3, glob4, "glob3 duplicates glob4"), "The given entities have different global identifiers."))
                .Add("non-duplicate global entity", () => Expect.Exception<ArgumentException>(() => new StonDuplicateGlobalEntityException(glob1, glob4), "The given entities have different global identifiers."))
                .Add("non-duplicate global entity", () => Expect.Exception<ArgumentException>(() => new StonDuplicateGlobalEntityException(glob1, glob4, "glob1 duplicates glob4"), "The given entities have different global identifiers."))
                .Add("duplicate non-global entity", () => Expect.Exception<ArgumentException>(() => new StonDuplicateGlobalEntityException(glob1, glob2), "The given entities have no global identifiers."))
                .Add("duplicate non-global entity", () => Expect.Exception<ArgumentException>(() => new StonDuplicateGlobalEntityException(glob1, glob2, "glob1 duplicates glob2"), "The given entities have no global identifiers."))

                // extensions exceptions
                .Add("non-named extension type", () => Expect.Exception<ArgumentException>(() => new StonExtensionTypeException(RegularStonReader.Default.ParseEntity("!array[...] []") as IStonValuedEntity), "The given entity has no extension type."))
                .Add("non-extension extension type", () => Expect.Exception<ArgumentException>(() => new StonExtensionTypeException(RegularStonReader.Default.ParseEntity("just type") as IStonValuedEntity), "The given entity has no extension type."))
                .Add("non-extension extension type 2", () => Expect.Exception<ArgumentException>(() => new StonExtensionTypeException((RegularStonReader.Default.ParseEntity("just type") as IStonValuedEntity).Type as IStonNamedType), "The given type is not an extension type."))

                .Add("non-extension extension member", () => Expect.Exception<ArgumentException>(() => new StonExtensionMemberException(new StonBindingName("name")), "The given member name is not an extension member name."))
                
                .Run();
        }

        #endregion

        #region Impractical scale

        /// <summary>
        /// Test cases related to operating on arguments so large that they cannot be reasonably handled.
        /// </summary>
        [TestMethod]
        public void Test_ImpracticalScale()
        {
            AggregateTester.New()

                // accessing absurd collection indices
                .Add("colossal number collection index 1", () => Expect.Exception<NotSupportedException>(() => RegularStonReader.Default.ParseDocument("{ a: $[#2147483648] }[ 0 ]"), "The collection element index cannot be larger than maximum 32-bit signed integer value (2147483647)."))
                .Add("colossal number collection index 2", () => Expect.Exception<NotSupportedException>(() => RegularStonReader.Default.ParseDocument("{ a: $[# 1e11] }[ 0 ]"), "The collection element index cannot be larger than maximum 32-bit signed integer value (2147483647)."))
                .Add("colossal number collection index 3", () => Expect.Exception<NotSupportedException>(() => RegularStonReader.Default.ParseDocument("{ a: $[# 1234e9] }[ 0 ]"), "The collection element index cannot be larger than maximum 32-bit signed integer value (2147483647)."))
                .Add("colossal binary collection index", () => Expect.Exception<NotSupportedException>(() => RegularStonReader.Default.ParseDocument("{ a: $[# 0x80000000] }[ 0 ]"), "The collection element index cannot be larger than maximum 32-bit signed integer value (2147483647)."))
                
                // writing plain numbers
                .Add("plain colossal number", () => Expect.Exception<NotSupportedException>(() => MakeActor<StonTokenWriter>().WritePlainNumberLiteral("1e1000000000"), "A number that long cannot be written as a plain number literal."))
                .Add("plain tiny number", () => Expect.Exception<NotSupportedException>(() => MakeActor<StonTokenWriter>().WritePlainNumberLiteral("1e-1000000000"), "A number that long cannot be written as a plain number literal."))

                .Run();
        }

        #endregion
    }
}

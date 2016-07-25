using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston.Building
{
    /// <summary>
    /// Provides the functionality of creating STON entities and their elements.
    /// </summary>
    public sealed class CoreStonElementFactory : IStonElementFactory
    {
        /// <summary>
        /// Exposes the CoreStonFactory functionality for creating STON elements.
        /// </summary>
        public static IStonElementFactory Instance { get; } = new CoreStonElementFactory();
        private CoreStonElementFactory() { }

        #region Entity building

        /// <summary>
        /// Creates a new simple-valued entity, with a given value and optional declared type and global identifier.
        /// </summary>
        /// <param name="value">The value of the entity.</param>
        /// <param name="type">The declared type of the entity.</param>
        /// <param name="globalIdentifier">The global identifier of the entity.</param>
        /// <returns>The new STON entity.</returns>
        public IStonSimpleEntity CreateSimpleEntity(IStonSimpleValue value, IStonType type = null, string globalIdentifier = null)
            => new StonSimpleEntity(value, type, globalIdentifier);

        /// <summary>
        /// Creates a new complex-valued entity, with a given value and optional declared type and global identifier.
        /// </summary>
        /// <param name="construction">The construction part of the complex value.</param>
        /// <param name="memberInit">The member initialization part of the complex value.</param>
        /// <param name="collectionInit">The collection initialization part of the complex value.</param>
        /// <param name="type">The declared type of the entity.</param>
        /// <param name="globalIdentifier">The global identifier of the entity.</param>
        /// <returns>The new STON entity.</returns>
        public IStonComplexEntity CreateComplexEntity(IStonConstruction construction = null, IStonMemberInit memberInit = null, IStonCollectionInit collectionInit = null, IStonType type = null, string globalIdentifier = null)
            => new StonComplexEntity(construction, memberInit, collectionInit, type, globalIdentifier);

        /// <summary>
        /// Creates a new reference entity, with a given reference address and optional global identifier.
        /// </summary>
        /// <param name="address">The address of the referenced value.</param>
        /// <param name="globalIdentifier">The global identifier of the entity.</param>
        /// <returns>The new STON entity.</returns>
        public IStonReferenceEntity CreateReferenceEntity(IStonAddress address, string globalIdentifier = null)
            => new StonReferenceEntity(address, globalIdentifier);

        #endregion

        #region Type building

        /// <summary>
        /// Creates a new STON named type, with a given name and optional sequence of type parameters, regular or extension.
        /// </summary>
        /// <param name="name">The name of the type.</param>
        /// <param name="typeParameters">The sequence of type parameters</param>
        /// <param name="isExtension">Whether the type is an extension type or a regular type.</param>
        /// <returns>The new STON type.</returns>
        public IStonNamedType CreateNamedType(string name, IEnumerable<IStonType> typeParameters = null, bool isExtension = false)
            => new StonNamedType(name, typeParameters, isExtension);

        /// <summary>
        /// Creates a new STON collection type, with a given collection element type.
        /// </summary>
        /// <param name="elementType">The type of the collection elements.</param>
        /// <returns>The new STON type.</returns>
        public IStonUnionType CreateUnionType(IEnumerable<IStonType> permittedTypes)
            => new StonUnionType(permittedTypes);

        /// <summary>
        /// Creates a new STON union type, with a given sequence of permitted types.
        /// </summary>
        /// <param name="permittedTypes">The sequence of permitted types.</param>
        /// <returns>The new STON type.</returns>
        public IStonCollectionType CreateCollectionType(IStonType elementType)
            => new StonCollectionType(elementType);

        #endregion

        #region Simple value building

        /// <summary>
        /// Creates a new simple value, given a data type and content string.
        /// </summary>
        /// <param name="dataType">The data type of the value.</param>
        /// <param name="content">The content string of the value.</param>
        /// <returns>The new STON simple value.</returns>
        public IStonSimpleValue CreateSimpleValue(StonDataType dataType, string content)
            => new StonSimpleValue(dataType, content);

        #endregion

        #region Complex value building

        /// <summary>
        /// Creates a new construction of a complex value, given sequences of positional parameters and named parameters.
        /// </summary>
        /// <param name="positionalParameters">The sequence of positional construction parameters.</param>
        /// <param name="namedParameters">The sequence of named construction parameters.</param>
        /// <returns>The new STON complex value's construction.</returns>
        public IStonConstruction CreateConstruction(IEnumerable<IStonEntity> positionalParameters, IEnumerable<KeyValuePair<string, IStonEntity>> namedParameters)
            => new StonConstruction(positionalParameters, namedParameters);

        /// <summary>
        /// Creates a new member initialization of a complex value, given a sequence of member bindings.
        /// </summary>
        /// <param name="memberBindings">The sequence of member bindings.</param>
        /// <returns>The new STON complex value's member initialization.</returns>
        public IStonMemberInit CreateMemberInit(IEnumerable<KeyValuePair<IStonBindingKey, IStonEntity>> memberBindings)
            => new StonMemberInit(memberBindings);

        /// <summary>
        /// Creates a new member binding name, with a given name, regular or extension.
        /// </summary>
        /// <param name="name">The name of the member.</param>
        /// <param name="isExtension">Whether the member is an extension member or a regular member.</param>
        /// <returns>The new STON binding name.</returns>
        public IStonBindingName CreateBindingName(string name, bool isExtension = false)
            => new StonBindingName(name, isExtension);

        /// <summary>
        /// Creates a new member binding index, with given parameters.
        /// </summary>
        /// <param name="parameters">The index parameters sequence.</param>
        /// <returns>The new STON binding index.</returns>
        public IStonBindingIndex CreateBindingIndex(IEnumerable<IStonEntity> parameters)
            => new StonBindingIndex(parameters);

        /// <summary>
        /// Creates a new collection initialization of a complex value, given a sequence of elements.
        /// </summary>
        /// <param name="elements">The sequence of elements.</param>
        /// <returns>The new STON complex value's collection initialization.</returns>
        public IStonCollectionInit CreateCollectionInit(IEnumerable<IStonEntity> elements)
            => new StonCollectionInit(elements);

        #endregion

        #region Reference address building

        /// <summary>
        /// Creates a new STON address, with a given initial context and path segments.
        /// </summary>
        /// <param name="initialContext">The initial context to start from.</param>
        /// <param name="relativePath">The path segments leading to the destination.</param>
        /// <returns>The new STON reference address.</returns>
        public IStonAddress CreateAddress(IStonInitialContext initialContext, IEnumerable<IStonPathSegment> relativePath)
            => new StonAddress(initialContext, relativePath);



        /// <summary>
        /// Creates a new ancestor initial context, with a given order.
        /// </summary>
        /// <param name="ancestorOrder">The order of the ancestor, or 0 for the reference defining entity context.</param>
        /// <returns>The new STON initial context.</returns>
        public IStonAncestorInitialContext CreateAncestorInitialContext(int ancestorOrder)
            => new StonAncestorInitialContext(ancestorOrder);

        /// <summary>
        /// Creates a globally identified entity initial context with a given identifier.
        /// </summary>
        /// <param name="globalIdentifier">The identifier of the entity, or empty string for document core.</param>
        /// <returns>The new STON initial context.</returns>
        public IStonGlobalEntityInitialContext CreateGlobalEntityInitialContext(string globalIdentifier)
            => new StonGlobalEntityInitialContext(globalIdentifier);



        /// <summary>
        /// Creates a new ancestor path segment, with a given ancestor order.
        /// </summary>
        /// <param name="ancestorOrder">The order of the ancestor to access.</param>
        /// <returns>The new STON path segment.</returns>
        public IStonAncestorPathSegment CreateAncestorPathSegment(int ancestorOrder)
            => new StonAncestorPathSegment(ancestorOrder);

        /// <summary>
        /// Creates a new member path segment, with a given binding key.
        /// </summary>
        /// <param name="bindingKey">The binding key of the member to access.</param>
        /// <returns>The new STON path segment.</returns>
        public IStonMemberPathSegment CreateMemberPathSegment(IStonBindingKey bindingKey)
            => new StonMemberPathSegment(bindingKey);

        /// <summary>
        /// Creates a new collection element path segment, with a given element index.
        /// </summary>
        /// <param name="elementIndex">The entity representing the index of the element in collection to access.</param>
        /// <returns>The new STON path segment.</returns>
        public IStonCollectionElementPathSegment CreateCollectionElementPathSegment(IStonEntity elementIndex)
            => new StonCollectionElementPathSegment(elementIndex);

        #endregion
    }
}

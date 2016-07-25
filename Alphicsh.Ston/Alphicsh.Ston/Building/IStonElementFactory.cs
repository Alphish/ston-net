using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston.Building
{
    /// <summary>
    /// Exposes the functionality of creating STON entities and their elements.
    /// </summary>
    public interface IStonElementFactory
    {
        #region Entity building

        /// <summary>
        /// Creates a new simple-valued entity, with a given value and optional declared type and global identifier.
        /// </summary>
        /// <param name="value">The value of the entity.</param>
        /// <param name="type">The declared type of the entity.</param>
        /// <param name="globalIdentifier">The global identifier of the entity.</param>
        /// <returns>The new STON entity.</returns>
        IStonSimpleEntity CreateSimpleEntity(IStonSimpleValue value, IStonType type = null, string globalIdentifier = null);

        /// <summary>
        /// Creates a new complex-valued entity, with a given value and optional declared type and global identifier.
        /// </summary>
        /// <param name="construction">The construction part of the complex value.</param>
        /// <param name="memberInit">The member initialization part of the complex value.</param>
        /// <param name="collectionInit">The collection initialization part of the complex value.</param>
        /// <param name="type">The declared type of the entity.</param>
        /// <param name="globalIdentifier">The global identifier of the entity.</param>
        /// <returns>The new STON entity.</returns>
        IStonComplexEntity CreateComplexEntity(IStonConstruction construction = null, IStonMemberInit memberInit = null, IStonCollectionInit collectionInit = null, IStonType type = null, string globalIdentifier = null);

        /// <summary>
        /// Creates a new reference entity, with a given reference address and optional global identifier.
        /// </summary>
        /// <param name="address">The address of the referenced value.</param>
        /// <param name="globalIdentifier">The global identifier of the entity.</param>
        /// <returns>The new STON entity.</returns>
        IStonReferenceEntity CreateReferenceEntity(IStonAddress address, string globalIdentifier = null);

        #endregion

        #region Type building

        /// <summary>
        /// Creates a new STON named type, with a given name and optional sequence of type parameters, regular or extension.
        /// </summary>
        /// <param name="name">The name of the type.</param>
        /// <param name="typeParameters">The sequence of type parameters</param>
        /// <param name="isExtension">Whether the type is an extension type or a regular type.</param>
        /// <returns>The new STON type.</returns>
        IStonNamedType CreateNamedType(string name, IEnumerable<IStonType> typeParameters = null, bool isExtension = false);

        /// <summary>
        /// Creates a new STON collection type, with a given collection element type.
        /// </summary>
        /// <param name="elementType">The type of the collection elements.</param>
        /// <returns>The new STON type.</returns>
        IStonCollectionType CreateCollectionType(IStonType elementType);

        /// <summary>
        /// Creates a new STON union type, with a given sequence of permitted types.
        /// </summary>
        /// <param name="permittedTypes">The sequence of permitted types.</param>
        /// <returns>The new STON type.</returns>
        IStonUnionType CreateUnionType(IEnumerable<IStonType> permittedTypes);

        #endregion

        #region Simple value building

        /// <summary>
        /// Creates a new simple value, given a data type and content string.
        /// </summary>
        /// <param name="dataType">The data type of the value.</param>
        /// <param name="content">The content string of the value.</param>
        /// <returns>The new STON simple value.</returns>
        IStonSimpleValue CreateSimpleValue(StonDataType dataType, string content);

        #endregion

        #region Complex value building

        /// <summary>
        /// Creates a new construction of a complex value, given sequences of positional parameters and named parameters.
        /// </summary>
        /// <param name="positionalParameters">The sequence of positional construction parameters.</param>
        /// <param name="namedParameters">The sequence of named construction parameters.</param>
        /// <returns>The new STON complex value's construction.</returns>
        IStonConstruction CreateConstruction(IEnumerable<IStonEntity> positionalParameters, IEnumerable<KeyValuePair<string, IStonEntity>> namedParameters);

        /// <summary>
        /// Creates a new member initialization of a complex value, given a sequence of member bindings.
        /// </summary>
        /// <param name="memberBindings">The sequence of member bindings.</param>
        /// <returns>The new STON complex value's member initialization.</returns>
        IStonMemberInit CreateMemberInit(IEnumerable<KeyValuePair<IStonBindingKey, IStonEntity>> memberBindings);

        /// <summary>
        /// Creates a new member binding name, with a given name, regular or extension.
        /// </summary>
        /// <param name="name">The name of the member.</param>
        /// <param name="isExtension">Whether the member is an extension member or a regular member.</param>
        /// <returns>The new STON binding name.</returns>
        IStonBindingName CreateBindingName(string name, bool isExtension = false);

        /// <summary>
        /// Creates a new member binding index, with given parameters.
        /// </summary>
        /// <param name="parameters">The index parameters sequence.</param>
        /// <returns>The new STON binding index.</returns>
        IStonBindingIndex CreateBindingIndex(IEnumerable<IStonEntity> parameters);

        /// <summary>
        /// Creates a new collection initialization of a complex value, given a sequence of elements.
        /// </summary>
        /// <param name="elements">The sequence of elements.</param>
        /// <returns>The new STON complex value's collection initialization.</returns>
        IStonCollectionInit CreateCollectionInit(IEnumerable<IStonEntity> elements);

        #endregion

        #region Reference address building

        /// <summary>
        /// Creates a new STON address, with a given initial context and path segments.
        /// </summary>
        /// <param name="initialContext">The initial context to start from.</param>
        /// <param name="relativePath">The path segments leading to the destination.</param>
        /// <returns>The new STON reference address.</returns>
        IStonAddress CreateAddress(IStonInitialContext initialContext, IEnumerable<IStonPathSegment> relativePath);



        /// <summary>
        /// Creates a new ancestor initial context, with a given order.
        /// </summary>
        /// <param name="ancestorOrder">The order of the ancestor, or 0 for the reference defining entity context.</param>
        /// <returns>The new STON initial context.</returns>
        IStonAncestorInitialContext CreateAncestorInitialContext(int ancestorOrder);

        /// <summary>
        /// Creates a globally identified entity initial context with a given identifier.
        /// </summary>
        /// <param name="globalIdentifier">The identifier of the entity, or empty string for document core.</param>
        /// <returns>The new STON initial context.</returns>
        IStonGlobalEntityInitialContext CreateGlobalEntityInitialContext(string globalIdentifier);



        /// <summary>
        /// Creates a new ancestor path segment, with a given ancestor order.
        /// </summary>
        /// <param name="ancestorOrder">The order of the ancestor to access.</param>
        /// <returns>The new STON path segment.</returns>
        IStonAncestorPathSegment CreateAncestorPathSegment(int ancestorOrder);

        /// <summary>
        /// Creates a new member path segment, with a given binding key.
        /// </summary>
        /// <param name="bindingKey">The binding key of the member to access.</param>
        /// <returns>The new STON path segment.</returns>
        IStonMemberPathSegment CreateMemberPathSegment(IStonBindingKey bindingKey);

        /// <summary>
        /// Creates a new collection element path segment, with a given element index.
        /// </summary>
        /// <param name="elementIndex">The entity representing the index of the element in collection to access.</param>
        /// <returns>The new STON path segment.</returns>
        IStonCollectionElementPathSegment CreateCollectionElementPathSegment(IStonEntity elementIndex);

        #endregion
    }
}

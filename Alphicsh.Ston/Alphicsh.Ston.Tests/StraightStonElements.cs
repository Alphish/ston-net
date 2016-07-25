using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston.Tests
{
    // collection of STON elements that are created with provided values directly
    // without any prior validation whatsoever
    
    // they are rather cute and friendly
    // but with their naivety and unconditional trust
    // they wouldn't last an hour in the cruel production environment

    #region Entities

    public class ASimpleEntity : IStonSimpleEntity
    {
        public string GlobalIdentifier { get; }

        public IStonType Type { get; }

        public IStonSimpleValue Value { get; }

        public ASimpleEntity(IStonSimpleValue value, IStonType type = null, string globalIdentifier = null)
        {
            GlobalIdentifier = globalIdentifier;
            Type = type;
            Value = value;
        }
    }

    public class AComplexEntity : IStonComplexEntity
    {
        public string GlobalIdentifier { get; }

        public IStonType Type { get; }

        public IStonConstruction Construction { get; }

        public IStonMemberInit MemberInit { get; }

        public IStonCollectionInit CollectionInit { get; }

        public AComplexEntity(IStonConstruction construction = null, IStonMemberInit memberInit = null, IStonCollectionInit collectionInit = null, IStonType type = null, string globalIdentifier = null)
        {
            GlobalIdentifier = globalIdentifier;
            Type = type;

            Construction = construction;
            MemberInit = memberInit;
            CollectionInit = collectionInit;
        }
    }

    public class AReferenceEntity : IStonReferenceEntity
    {
        public string GlobalIdentifier { get; }

        public IStonAddress Address { get; }

        public AReferenceEntity(IStonAddress address, string globalIdentifier = null)
        {
            Address = address;
            GlobalIdentifier = globalIdentifier;
        }
    }

    #endregion

    #region Types

    public class ANamedType : IStonNamedType
    {
        public string Name { get; }

        public bool IsExtension { get; }

        public IEnumerable<IStonType> TypeParameters { get; }

        public ANamedType(string name, bool isExtension, IEnumerable<IStonType> typeParameters)
        {
            Name = name;
            IsExtension = isExtension;
            TypeParameters = typeParameters;
        }
    }

    public class ACollectionType : IStonCollectionType
    {
        public IStonType ElementType { get; }

        public ACollectionType(IStonType elementType)
        {
            ElementType = elementType;
        }
    }

    public class AUnionType : IStonUnionType
    {
        public IEnumerable<IStonType> PermittedTypes { get; }

        public AUnionType(IEnumerable<IStonType> permittedTypes)
        {
            PermittedTypes = permittedTypes;
        }
    }

    #endregion

    // ASimpleValue is not added, because StonSimpleValue performs no copying or validation already

    #region Complex values

    public class AConstruction : IStonConstruction
    {
        public IEnumerable<IStonEntity> PositionalParameters { get; }

        public IEnumerable<KeyValuePair<string, IStonEntity>> NamedParameters { get; }

        public AConstruction(IEnumerable<IStonEntity> positionalParameters, IEnumerable<KeyValuePair<string, IStonEntity>> namedParameters)
        {
            PositionalParameters = positionalParameters;
            NamedParameters = namedParameters;
        }
    }

    public class AMemberInit : IStonMemberInit
    {
        public IEnumerable<KeyValuePair<IStonBindingKey, IStonEntity>> MemberBindings { get; }

        public AMemberInit(IEnumerable<KeyValuePair<IStonBindingKey, IStonEntity>> memberBindings)
        {
            MemberBindings = memberBindings;
        }
    }

    public class ABindingName : IStonBindingName
    {
        public string Name { get; }

        public bool IsExtension { get; }

        public ABindingName(string name, bool isExtension)
        {
            Name = name;
            IsExtension = isExtension;
        }
    }

    public class ABindingIndex : IStonBindingIndex
    {
        public IEnumerable<IStonEntity> Parameters { get; }

        public ABindingIndex(IEnumerable<IStonEntity> parameters)
        {
            Parameters = parameters;
        }
    }

    public class ACollectionInit : IStonCollectionInit
    {
        public IEnumerable<IStonEntity> Elements { get; }

        public ACollectionInit(IEnumerable<IStonEntity> elements)
        {
            Elements = elements;
        }
    }

    #endregion

    #region Reference addresses

    public class AnAddress : IStonAddress
    {
        public IStonInitialContext InitialContext { get; }

        public IEnumerable<IStonPathSegment> RelativePath { get; }

        public AnAddress(IStonInitialContext initialContext, IEnumerable<IStonPathSegment> relativePath)
        {
            InitialContext = initialContext;
            RelativePath = relativePath;
        }

    }

    // AnAncestorContext is not added, because StonAncestorInitialContext performs no copying or validation already

    public class AGlobalContext : IStonGlobalEntityInitialContext
    {
        public string GlobalIdentifier { get; }

        public AGlobalContext(string globalIdentifier)
        {
            GlobalIdentifier = globalIdentifier;
        }
    }

    // PassStonAncestorPathSegment is not added, because StonAncestorPathSegment performs no copying or validation already

    public class AMemberSegment : IStonMemberPathSegment
    {
        public IStonBindingKey BindingKey { get; }

        public AMemberSegment(IStonBindingKey bindingKey)
        {
            BindingKey = bindingKey;
        }
    }

    public class AnElementSegment : IStonCollectionElementPathSegment
    {
        public IStonEntity ElementIndex { get; }

        public AnElementSegment(IStonEntity elementIndex)
        {
            ElementIndex = elementIndex;
        }
    }

    #endregion
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alphicsh.Ston.Tokenization;

namespace Alphicsh.Ston.Helpers
{
    /// <summary>
    /// Provides the functionality of validating STON elements.
    /// </summary>
    public static class Validator
    {
        #region Entity validation

        /// <summary>
        /// Checks a validity of a given STON entity.
        /// </summary>
        /// <param name="entity">The entity to check the validity of.</param>
        public static void ValidateEntity(IStonEntity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            else if (entity is IStonValuedEntity) ValidateEntity(entity as IStonValuedEntity);
            else if (entity is IStonReferenceEntity) ValidateEntity(entity as IStonReferenceEntity);
            else throw new StonImplementationException(entity.GetType(), typeof(IStonEntity), typeof(IStonValuedEntity), typeof(IStonReferenceEntity));
        }

        /// <summary>
        /// Checks a validity of a given STON valued entity.
        /// </summary>
        /// <param name="entity">The entity to check the validity of.</param>
        public static void ValidateEntity(IStonValuedEntity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            else if (entity is IStonSimpleEntity) ValidateEntity(entity as IStonSimpleEntity);
            else if (entity is IStonComplexEntity) ValidateEntity(entity as IStonComplexEntity);
            else throw new StonImplementationException(entity.GetType(), typeof(IStonValuedEntity), typeof(IStonSimpleEntity), typeof(IStonComplexEntity));
        }

        /// <summary>
        /// Checks a validity of a given STON simple-valued entity.
        /// </summary>
        /// <param name="entity">The entity to check the validity of.</param>
        public static void ValidateEntity(IStonSimpleEntity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            try
            {
                if (entity.GlobalIdentifier != null) ValidateGlobalIdentifier(entity.GlobalIdentifier);
                if (entity.Type != null) ValidateType(entity.Type);

                if (entity.Value == null) throw new StonException("A simple-valued entity cannot have a non-existing value.");
                ValidateSimpleValue(entity.Value);
            }
            catch (StonException ex)
            {
                throw new StonValueException(entity, ex.Message);
            }
        }

        /// <summary>
        /// Checks a validity of a given STON complex-valued entity.
        /// The validation does not include check for duplicate members, as that might require resolving references in a STON document.
        /// </summary>
        /// <param name="entity">The entity to check the validity of.</param>
        public static void ValidateEntity(IStonComplexEntity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            try
            {
                if (entity.GlobalIdentifier != null) ValidateGlobalIdentifier(entity.GlobalIdentifier);
                if (entity.Type != null) ValidateType(entity.Type);

                if (entity.Construction == null && entity.MemberInit == null && entity.CollectionInit == null)
                {
                    throw new StonException("A complex-valued entity must have a construction, a member initialization or a collection initialization component.");
                }
                if (entity.Construction != null) ValidateConstruction(entity.Construction);
                if (entity.MemberInit != null) ValidateMemberInit(entity.MemberInit);
                if (entity.CollectionInit != null) ValidateCollectionInit(entity.CollectionInit);
            }
            catch (StonException ex)
            {
                throw new StonValueException(entity, ex.Message);
            }
        }

        /// <summary>
        /// Checks a validity of a given STON reference entity.
        /// </summary>
        /// <param name="entity">The entity to check the validity of.</param>
        public static void ValidateEntity(IStonReferenceEntity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            try
            {
                if (entity.GlobalIdentifier != null) ValidateGlobalIdentifier(entity.GlobalIdentifier);

                if (entity.Address == null) throw new StonException("A reference entity cannot have a non-existing address.");
                ValidateAddress(entity.Address);
            }
            catch (StonException ex)
            {
                throw new StonReferenceException(entity, ex.Message);
            }
        }

        #endregion

        #region Global identifier validation

        /// <summary>
        /// Checks the validity of a given global identifier.
        /// </summary>
        /// <param name="globalIdentifier">The global identifier to check the validity of.</param>
        public static void ValidateGlobalIdentifier(string globalIdentifier)
        {
            if (globalIdentifier == null) throw new ArgumentNullException("globalIdentifier");
            if (!globalIdentifier[0].HasChartype(StonChartype.CanunBegin) || globalIdentifier.Any(c => !c.HasChartype(StonChartype.CanunContinue)))
            {
                throw new StonException($"The global identifier \"{globalIdentifier}\" is not a valid CANUN identifier.");
            }
        }

        #endregion

        #region Type validation

        /// <summary>
        /// Checks the validity of a given STON type.
        /// </summary>
        /// <param name="type">The type to check the validity of.</param>
        public static void ValidateType(IStonType type)
        {
            if (type == null) throw new ArgumentNullException("type");
            else if (type is IStonNamedType) ValidateType(type as IStonNamedType);
            else if (type is IStonCollectionType) ValidateType(type as IStonCollectionType);
            else if (type is IStonUnionType) ValidateType(type as IStonUnionType);
            else throw new StonImplementationException(type.GetType(), typeof(IStonType), typeof(IStonNamedType), typeof(IStonCollectionType), typeof(IStonUnionType));
        }

        /// <summary>
        /// Checks the validity of a given STON named type.
        /// </summary>
        /// <param name="type">The named type to check the validity of.</param>
        public static void ValidateType(IStonNamedType type)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (type.Name == null) throw new StonException("A named type cannot have a non-existing name.");
            if (type.TypeParameters == null) throw new StonException("A named type cannot have a non-existing type parameters collection.");
            foreach (var parameter in type.TypeParameters)
            {
                if (parameter == null) throw new StonException("A named type cannot have a non-existing type parameter.");
                ValidateType(parameter);
            }
        }

        /// <summary>
        /// Checks the validity of a given STON collection type.
        /// </summary>
        /// <param name="type">The type to check the validity of.</param>
        public static void ValidateType(IStonCollectionType type)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (type.ElementType == null) throw new StonException("A collection type cannot have a non-existing element type.");
        }

        /// <summary>
        /// Checks the validity of a given STON union type.
        /// </summary>
        /// <param name="type">The type to check the validity of.</param>
        public static void ValidateType(IStonUnionType type)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (type.PermittedTypes == null || !type.PermittedTypes.Skip(1).Any()) throw new StonException("A union type must have at least two permitted types.");
            if (type.PermittedTypes.Contains(null)) throw new StonException("A union type cannot have a non-existing permitted type.");
        }

        #endregion

        #region Simple value validation

        /// <summary>
        /// Checks the validity of a given STON simple value.
        /// </summary>
        /// <param name="value">The simple value to check the validity of.</param>
        public static void ValidateSimpleValue(IStonSimpleValue value)
        {
            if (value == null) throw new ArgumentNullException("value");
            
            // handling the null value
            if (value.DataType == StonDataType.Null)
            {
                if (!string.IsNullOrEmpty(value.Content)) throw new StonException("A null simple value must be represented with a non-existing or empty content.");
                else return;
            }
            else if (value.Content == null) throw new StonException("A non-null simple value must be represented with an existing content.");

            switch (value.DataType)
            {
                // significand and exponent are expected
                // or zero, if the number represented is zero
                case StonDataType.Number:
                    ValidateNumberContent(value.Content);
                    break;

                // hexadecimal digits pairs with optional initial minus are expected
                case StonDataType.Binary:
                    ValidateBinaryContent(value.Content);
                    break;

                // a CANUN path is expected
                case StonDataType.Named:
                    ValidateNamedValueContent(value.Content);
                    break;

                // all non-empty text is valid
                case StonDataType.Text:
                    break;

                // all non-empty code is valid
                case StonDataType.Code:
                    break;

                default:
                    throw new StonException("Unknown simple value data type.");
            }
        }

        // validates the structure of a number content string
        private static void ValidateNumberContent(string content)
        {
            var enumerator = content.GetEnumerator();
            if (!enumerator.MoveNext()) throw new StonException("A number simple value cannot be empty.");

            // preapring the significand
            if (enumerator.Current == '-')
            {
                if (!enumerator.MoveNext()) throw new StonException("A number simple value significand must have at least one digit.");
                // minus zero is not allowed
                if (enumerator.Current == '0') throw new StonException("A number simple value significand cannot have leading zeros.");
            }

            if (!enumerator.Current.HasChartype(StonChartype.Digit)) throw new StonException("A number simple value significand must be represented with decimal digits.");
            if (enumerator.Current == '0')
            {
                if (enumerator.MoveNext()) throw new StonException("A number simple value significand cannot have leading zeros.");
                else return;    // the number is plain zero
            }

            // reading the significand digits
            bool trailingZeros = false;
            while (enumerator.Current != 'e')
            {
                if (enumerator.Current == 'E') throw new StonException("A number simple value cannot have an uppercase exponent sign.");
                if (!enumerator.Current.HasChartype(StonChartype.Digit)) throw new StonException("A number simple value significand must be represented with decimal digits.");
                trailingZeros = enumerator.Current.HasChartype(StonChartype.ZeroDigit);
                if (!enumerator.MoveNext()) throw new StonException("A non-zero number simple value must have an exponent.");
            }
            if (trailingZeros) throw new StonException("A number simple value significand cannot have trailing zeros.");

            // preparing the exponent
            if (!enumerator.MoveNext()) throw new StonException("A number simple value exponent must have at least one digit.");
            if (enumerator.Current == '-')
            {
                if (!enumerator.MoveNext()) throw new StonException("A number simple value exponent must have at least one digit.");
                // minus zero is not allowed
                if (enumerator.Current == '0') throw new StonException("A number simple value exponent cannot have leading zeros.");
            }

            if (!enumerator.Current.HasChartype(StonChartype.Digit)) throw new StonException("A number simple value exponent must be represented with decimal digits.");
            if (enumerator.Current == '0')
            {
                if (enumerator.MoveNext()) throw new StonException("A number simple value exponent cannot have leading zeros.");
                else return;    // the exponent is plain zero
            }

            do
            {
                if (!enumerator.Current.HasChartype(StonChartype.Digit)) throw new StonException("A number simple value exponent must be represented with decimal digits.");
            } while (enumerator.MoveNext());
        }

        // validates the structure of a binary content string
        private static void ValidateBinaryContent(string content)
        {
            if (content == "") return;
            if (content == "-") throw new StonException("An empty binary simple value cannot be negative.");

            bool first = true;
            bool even = true;

            foreach (char c in content)
            {
                if (first && c == '-')
                {
                    first = false;
                    continue;
                }
                if (!c.HasChartype(StonChartype.Base16)) throw new StonException("A binary simple value content must be represented with hexadecimal digits.");
                if (c >= 'A' && c <= 'F') throw new StonException("A binary simple value content cannot have uppercase hexadecimal digits.");
                even = !even;
                first = false;
            }
            if (!even) throw new StonException("A binary simple value content must have an even number of hexadecimal digits.");
        }

        // validates the structure of a named value content string
        private static void ValidateNamedValueContent(string content)
        {
            if (content == "") throw new StonException("A named simple value cannot be empty.");

            StonChartype expectedChartype = StonChartype.CanunBegin;
            foreach (char c in content)
            {
                if (!c.HasChartype(expectedChartype)) throw new StonException("A named simple value must be a valid CANUN path.");
                if (c == '.') expectedChartype = StonChartype.CanunBegin;
                else expectedChartype = StonChartype.CanunContinue | StonChartype.NameSeparator;
            }
            // the CANUN path cannot end with a dot
            if (expectedChartype == StonChartype.CanunBegin) throw new StonException("A named simple value must be a valid CANUN path.");
        }

        #endregion

        #region Complex value validation

        /// <summary>
        /// Checks the validity of a given STON complex value construction.
        /// </summary>
        /// <param name="construction">The construction to check the validity of.</param>
        public static void ValidateConstruction(IStonConstruction construction)
        {
            if (construction == null) throw new ArgumentNullException("construction");
            if (construction.PositionalParameters == null) throw new StonException("A complex value construction cannot have a non-existing positional parameters collection.");
            if (construction.NamedParameters == null) throw new StonException("A complex value construction cannot have a non-existing named parameters collection.");

            foreach (var parameter in construction.PositionalParameters)
            {
                if (parameter == null) throw new StonException("A complex value construction cannot have a non-existing parameter value.");
            }
            var names = new HashSet<string>();
            foreach (var namedParameter in construction.NamedParameters)
            {
                if (namedParameter.Key == null) throw new StonException("A complex value construction cannot have a non-existing parameter name.");
                if (!names.Add(namedParameter.Key)) throw new StonException($"The complex value construction has a duplicate named parameter \"{namedParameter.Key}\".");
                if (namedParameter.Value == null) throw new StonException("A complex value construction cannot have a non-existing parameter value.");
            }
        }

        /// <summary>
        /// Checks the validity of a given STON complex value member initialization.
        /// The validation does not include check for duplicate members, as that might require resolving references in a STON document.
        /// </summary>
        /// <param name="memberInit">The member initialization to check the validity of.</param>
        public static void ValidateMemberInit(IStonMemberInit memberInit)
        {
            if (memberInit == null) throw new ArgumentNullException("memberInit");
            if (memberInit.MemberBindings == null) throw new StonException("A complex value member initialization cannot have a non-existing member bindings collection.");

            foreach (var memberBinding in memberInit.MemberBindings)
            {
                ValidateBindingKey(memberBinding.Key);
                if (memberBinding.Value == null) throw new StonException("A complex value member initialization cannot have non-existing member values.");
            }
        }

        /// <summary>
        /// Checks the validity of a given STON member binding key.
        /// </summary>
        /// <param name="bindingKey">The member binding key to check the validity of.</param>
        public static void ValidateBindingKey(IStonBindingKey bindingKey)
        {
            if (bindingKey == null) throw new ArgumentNullException("bindingKey");
            else if (bindingKey is IStonBindingName) ValidateBindingKey(bindingKey as IStonBindingName);
            else if (bindingKey is IStonBindingIndex) ValidateBindingKey(bindingKey as IStonBindingIndex);
            else throw new StonImplementationException(bindingKey.GetType(), typeof(IStonBindingKey), typeof(IStonBindingName), typeof(IStonBindingIndex));
        }

        /// <summary>
        /// Checks the validity of a given STON member binding name.
        /// </summary>
        /// <param name="bindingKey">The member binding name to check the validity of.</param>
        public static void ValidateBindingKey(IStonBindingName bindingKey)
        {
            if (bindingKey == null) throw new ArgumentNullException("bindingKey");
            if (bindingKey.Name == null) throw new StonException("A member binding name cannot have non-existing name.");
        }

        /// <summary>
        /// Checks the validity of a given STON member binding index.
        /// </summary>
        /// <param name="bindingKey">The member binding index to check the validity of.</param>
        public static void ValidateBindingKey(IStonBindingIndex bindingKey)
        {
            if (bindingKey == null) throw new ArgumentNullException("bindingKey");
            if (bindingKey.Parameters == null || !bindingKey.Parameters.Any()) throw new StonException("A member binding index must be neither non-existing nor empty.");
            
            foreach (var parameter in bindingKey.Parameters)
            {
                if (parameter == null) throw new StonException("A member binding index cannot have non-existing index parameters.");
            }
        }

        /// <summary>
        /// Checks the validity of a given STON complex value collection initialization.
        /// </summary>
        /// <param name="collectionInit">The collection initialization to check the validity of.</param>
        public static void ValidateCollectionInit(IStonCollectionInit collectionInit)
        {
            if (collectionInit == null) throw new ArgumentNullException("collectionInit");
            if (collectionInit.Elements == null) throw new StonException("A complex value collection initialization cannot have a non-existing elements collection.");

            foreach (var element in collectionInit.Elements)
            {
                if (element == null) throw new StonException("A complex value collection initialization cannot have non-existing elements.");
            }
        }

        #endregion

        #region Reference address validation

        /// <summary>
        /// Checks the validity of a given STON reference address.
        /// </summary>
        /// <param name="address">The reference address to check the validity of.</param>
        public static void ValidateAddress(IStonAddress address)
        {
            if (address == null) throw new ArgumentNullException("address");
            if (address.InitialContext == null) throw new StonException("A reference address cannot have a non-existing initial context.");
            if (address.RelativePath == null) throw new StonException("A reference address cannot have a non-existing relative path.");

            ValidateInitialContext(address.InitialContext);
            foreach (var segment in address.RelativePath)
            {
                if (segment == null) throw new StonException("A reference address relative path cannot have non-existing path segments.");
                ValidatePathSegment(segment);
            }
        }

        /// <summary>
        /// Checks the validity of a given STON initial context.
        /// </summary>
        /// <param name="initialContext">The initial context to check the validity of.</param>
        public static void ValidateInitialContext(IStonInitialContext initialContext)
        {
            if (initialContext == null) throw new ArgumentNullException("initialContext");
            else if (initialContext is IStonAncestorInitialContext) ValidateInitialContext(initialContext as IStonAncestorInitialContext);
            else if (initialContext is IStonGlobalEntityInitialContext) ValidateInitialContext(initialContext as IStonGlobalEntityInitialContext);
            else throw new StonImplementationException(initialContext.GetType(), typeof(IStonInitialContext), typeof(IStonAncestorInitialContext), typeof(IStonGlobalEntityInitialContext));
        }

        /// <summary>
        /// Checks the validity of a given STON ancestor initial context.
        /// </summary>
        /// <param name="initialContext">The initial context to check the validity of.</param>
        public static void ValidateInitialContext(IStonAncestorInitialContext initialContext)
        {
            if (initialContext == null) throw new ArgumentNullException("initialContext");
            if (initialContext.AncestorOrder < 0) throw new StonException("An ancestor initial context must have a positive ancestor order.");
        }

        /// <summary>
        /// Checks the validity of a given STON global entity initial context.
        /// </summary>
        /// <param name="initialContext">The initial context to check the validity of.</param>
        public static void ValidateInitialContext(IStonGlobalEntityInitialContext initialContext)
        {
            if (initialContext == null) throw new ArgumentNullException("initialContext");
            if (initialContext.GlobalIdentifier == null) throw new StonException("A global entity initial context must have an existing global identifier.");
            if (initialContext.GlobalIdentifier == "") return;
            ValidateGlobalIdentifier(initialContext.GlobalIdentifier);
        }

        /// <summary>
        /// Checks the validity of a given STON path segment.
        /// </summary>
        /// <param name="segment">The path segment to check the validity of.</param>
        public static void ValidatePathSegment(IStonPathSegment segment)
        {
            if (segment == null) throw new ArgumentNullException("segment");
            else if (segment is IStonAncestorPathSegment) ValidatePathSegment(segment as IStonAncestorPathSegment);
            else if (segment is IStonMemberPathSegment) ValidatePathSegment(segment as IStonMemberPathSegment);
            else if (segment is IStonCollectionElementPathSegment) ValidatePathSegment(segment as IStonCollectionElementPathSegment);
            else throw new StonImplementationException(segment.GetType(), typeof(IStonPathSegment), typeof(IStonAncestorPathSegment), typeof(IStonMemberPathSegment), typeof(IStonCollectionElementPathSegment));
        }

        /// <summary>
        /// Checks the validity of a given STON ancestor path segment.
        /// </summary>
        /// <param name="segment">The path segment to check the validity of.</param>
        public static void ValidatePathSegment(IStonAncestorPathSegment segment)
        {
            if (segment == null) throw new ArgumentNullException("segment");
            if (segment.AncestorOrder <= 0) throw new StonException("An ancestor path segment must have a positive ancestor order.");
        }

        /// <summary>
        /// Checks the validity of a given STON member path segment.
        /// </summary>
        /// <param name="segment">The path segment to check the validity of.</param>
        public static void ValidatePathSegment(IStonMemberPathSegment segment)
        {
            if (segment == null) throw new ArgumentNullException("segment");
            if (segment.BindingKey == null) throw new StonException("A member path segment cannot have a non-existing binding key.");
            ValidateBindingKey(segment.BindingKey);

            if (segment.BindingKey is IStonBindingIndex)
            {
                foreach (var parameter in (segment.BindingKey as IStonBindingIndex).Parameters)
                {
                    if (parameter.GlobalIdentifier != null) throw new StonException("An indexed member path segment parameter cannot declare a global identifier.");
                    if (parameter is IStonComplexEntity) throw new StonException("An indexed member path segment parameter cannot be a complex-valued entity.");
                }
            }
        }

        /// <summary>
        /// Checks the validity of a given STON collection element path segment.
        /// </summary>
        /// <param name="segment">The path segment to check the validity of.</param>
        public static void ValidatePathSegment(IStonCollectionElementPathSegment segment)
        {
            if (segment == null) throw new ArgumentNullException("segment");
            if (segment.ElementIndex == null) throw new StonException("A collection element path segment cannot have a non-existing element index.");

            if (segment.ElementIndex.GlobalIdentifier != null) throw new StonException("A collection element path segment index cannot declare a global identifier.");
            if (segment.ElementIndex is IStonComplexEntity) throw new StonException("A collection element path segment index cannot be a complex-valued entity.");
            else if (segment.ElementIndex is IStonSimpleEntity)
            {
                if ((segment.ElementIndex as IStonSimpleEntity).Type != null) throw new StonException("A collection element path segment index must be implicitly typed.");
                var value = (segment.ElementIndex as IStonSimpleEntity).Value;
                if (value.DataType != StonDataType.Binary && value.DataType != StonDataType.Number) throw new StonException("A collection element path segment index must have a number or binary value.");
            }
        }

        #endregion
    }
}

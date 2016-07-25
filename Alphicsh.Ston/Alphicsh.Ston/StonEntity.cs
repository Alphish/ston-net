using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston
{
    /// <summary>
    /// Serves as a base class for STON entities.
    /// </summary>
    public abstract class StonEntity : IStonEntity
    {
        /// <summary>
        /// Gets the global identifier of the entity, or null if the global identifier does not exist.
        /// </summary>
        public string GlobalIdentifier { get; }

        /// <summary>
        /// Creates a new STON entity, with a given global identifier.
        /// </summary>
        /// <param name="globalIdentifier">The global identifier of the entity.</param>
        protected StonEntity(string globalIdentifier)
        {
            GlobalIdentifier = globalIdentifier;
        }

        /// <summary>
        /// Creates a structurally equivalent entity from a given entity.
        /// </summary>
        /// <param name="entity">The entity to copy the structure of.</param>
        /// <returns>A structurally equivalent copy of the given entity.</returns>
        public static IStonEntity Copy(IStonEntity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            if (entity is IStonValuedEntity) return StonValuedEntity.Copy(entity as IStonValuedEntity);
            if (entity is IStonReferenceEntity) return StonReferenceEntity.Copy(entity as IStonReferenceEntity);
            throw new StonImplementationException(entity.GetType(), typeof(IStonEntity), typeof(IStonValuedEntity), typeof(IStonReferenceEntity));
        }

        // override for general debugging purposes
        // it is not necessarily meant as a valid STON representation
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public override string ToString()
        {
            if (GlobalIdentifier == null) return "";
            else return "&" + GlobalIdentifier + " = ";
        }
    }

    /// <summary>
    /// Serves as a base class for STON valued entities.
    /// </summary>
    public abstract class StonValuedEntity : StonEntity, IStonValuedEntity
    {
        /// <summary>
        /// Gets the type declared for the entity, or null if the entity is implicitly typed.
        /// </summary>
        public IStonType Type { get; }

        /// <summary>
        /// Creates a new STON valued entity, with a given declared type and global identifier.
        /// </summary>
        /// <param name="type">The declared type or the entity.</param>
        /// <param name="globalIdentifier">The global identifier of the entity.</param>
        protected StonValuedEntity(IStonType type, string globalIdentifier)
            : base(globalIdentifier)
        {
            Type = type != null ? StonType.Copy(type) : null;
        }

        /// <summary>
        /// Creates a structurally equivalent valued entity from a given entity.
        /// </summary>
        /// <param name="entity">The entity to copy the structure of.</param>
        /// <returns>A structurally equivalent copy of the given entity.</returns>
        public static IStonValuedEntity Copy(IStonValuedEntity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            if (entity is IStonSimpleEntity) return StonSimpleEntity.Copy(entity as IStonSimpleEntity);
            if (entity is IStonComplexEntity) return StonComplexEntity.Copy(entity as IStonComplexEntity);
            throw new StonImplementationException(entity.GetType(), typeof(IStonValuedEntity), typeof(IStonSimpleEntity), typeof(IStonComplexEntity));
        }

        // override for general debugging purposes
        // it is not necessarily meant as a valid STON representation
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public override string ToString()
        {
            if (Type == null) return base.ToString();
            else return base.ToString() + Type + " ";
        }
    }

    /// <summary>
    /// Represents a simple-valued STON entity.
    /// </summary>
    public sealed class StonSimpleEntity : StonValuedEntity, IStonSimpleEntity
    {
        /// <summary>
        /// Gets the simple value of the entity.
        /// </summary>
        public IStonSimpleValue Value { get; }

        /// <summary>
        /// Creates a new simple-valued entity, with a given value and optional declared type and global identifier.
        /// </summary>
        /// <param name="value">The value of the entity.</param>
        /// <param name="type">The declared type of the entity.</param>
        /// <param name="globalIdentifier">The global identifier of the entity.</param>
        public StonSimpleEntity(IStonSimpleValue value, IStonType type = null, string globalIdentifier = null)
            : base(type, globalIdentifier)
        {
            if (value == null) throw new ArgumentNullException("value");
            Value = StonSimpleValue.Copy(value);

            Helpers.Validator.ValidateEntity(this);
        }

        /// <summary>
        /// Creates a structurally equivalent simple-valued entity from a given entity.
        /// </summary>
        /// <param name="entity">The entity to copy the structure of.</param>
        public StonSimpleEntity(IStonSimpleEntity entity)
            : this(entity.Value, entity.Type, entity.GlobalIdentifier) { }

        /// <summary>
        /// Creates a structurally equivalent simple-valued entity from a given entity.
        /// </summary>
        /// <param name="entity">The entity to copy the structure of.</param>
        /// <returns>A structurally equivalent copy of the given entity.</returns>
        public static IStonSimpleEntity Copy(IStonSimpleEntity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return new StonSimpleEntity(entity);
        }

        // override for general debugging purposes
        // it is not necessarily meant as a valid STON representation
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return base.ToString() + Value.ToString();
        }
    }

    /// <summary>
    /// Represents a complex-valued STON entity.
    /// </summary>
    public sealed class StonComplexEntity : StonValuedEntity, IStonComplexEntity
    {
        /// <summary>
        /// Gets the construction of the complex value, or null if no construction is declared.
        /// </summary>
        public IStonConstruction Construction { get; }

        /// <summary>
        /// Gets the member initialization of the complex value, or null if no member initialization is declared.
        /// </summary>
        public IStonMemberInit MemberInit { get; }

        /// <summary>
        /// Gets the collection initialization of the complex value, or null if no collection initialization is declared.
        /// </summary>
        public IStonCollectionInit CollectionInit { get; }

        /// <summary>
        /// Creates a new complex-valued entity, with a given value and optional declared type and global identifier.
        /// </summary>
        /// <param name="construction">The construction part of the complex value.</param>
        /// <param name="memberInit">The member initialization part of the complex value.</param>
        /// <param name="collectionInit">The collection initialization part of the complex value.</param>
        /// <param name="type">The declared type of the entity.</param>
        /// <param name="globalIdentifier">The global identifier of the entity.</param>
        public StonComplexEntity(IStonConstruction construction = null, IStonMemberInit memberInit = null, IStonCollectionInit collectionInit = null, IStonType type = null, string globalIdentifier = null)
            : base(type, globalIdentifier)
        {
            Construction = construction != null ? StonConstruction.Copy(construction) : null;
            MemberInit = memberInit != null ? StonMemberInit.Copy(memberInit) : null;
            CollectionInit = collectionInit != null ? StonCollectionInit.Copy(collectionInit) : null;

            Helpers.Validator.ValidateEntity(this);
        }

        /// <summary>
        /// Creates a structurally equivalent complex-valued entity from a given entity.
        /// </summary>
        /// <param name="entity">The entity to copy the structure of.</param>
        public StonComplexEntity(IStonComplexEntity entity)
            : this(entity.Construction, entity.MemberInit, entity.CollectionInit, entity.Type, entity.GlobalIdentifier) { }

        /// <summary>
        /// Creates a structurally equivalent complex-valued entity from a given entity.
        /// </summary>
        /// <param name="entity">The entity to copy the structure of.</param>
        /// <returns>A structurally equivalent copy of the given entity.</returns>
        public static IStonComplexEntity Copy(IStonComplexEntity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return new StonComplexEntity(entity);
        }

        // override for general debugging purposes
        // it is not necessarily meant as a valid STON representation
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return base.ToString() + (Construction?.ToString() ?? "") + (MemberInit?.ToString() ?? "") + (CollectionInit?.ToString() ?? "");
        }
    }

    /// <summary>
    /// Represents a reference STON entity.
    /// </summary>
    public sealed class StonReferenceEntity : StonEntity, IStonReferenceEntity
    {
        /// <summary>
        /// Gets the address of the references value.
        /// </summary>
        public IStonAddress Address { get; }

        /// <summary>
        /// Creates a new reference entity, with a given reference address and optional global identifier.
        /// </summary>
        /// <param name="address">The address of the referenced value.</param>
        /// <param name="globalIdentifier">The global identifier of the entity.</param>
        public StonReferenceEntity(IStonAddress address, string globalIdentifier = null)
            : base(globalIdentifier)
        {
            if (address == null) throw new ArgumentNullException("address");
            Address = StonAddress.Copy(address);

            Helpers.Validator.ValidateEntity(this);
        }

        /// <summary>
        /// Creates a structurally equivalent reference entity from a given entity.
        /// </summary>
        /// <param name="entity">The entity to copy the structure of.</param>
        public StonReferenceEntity(IStonReferenceEntity entity)
            : this(entity.Address, entity.GlobalIdentifier) { }

        /// <summary>
        /// Creates a structurally equivalent reference entity from a given entity.
        /// </summary>
        /// <param name="entity">The entity to copy the structure of.</param>
        /// <returns>A structurally equivalent copy of the given entity.</returns>
        public static IStonReferenceEntity Copy(IStonReferenceEntity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return new StonReferenceEntity(entity);
        }

        // override for general debugging purposes
        // it is not necessarily meant as a valid STON representation
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return base.ToString() + Address.ToString();
        }
    }
}

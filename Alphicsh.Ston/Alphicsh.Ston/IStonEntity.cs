using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston
{
    /// <summary>
    /// Represents a basic STON entity.
    /// It should be implemented either through IStonValuedEntity or IStonReferenceEntity.
    /// </summary>
    public interface IStonEntity
    {
        /// <summary>
        /// Gets the global identifier of the entity, or null if the global identifier does not exist.
        /// </summary>
        string GlobalIdentifier { get; }
    }

    /// <summary>
    /// Represents a valued STON entity.
    /// It should be implemented either through IStonSimpleEntity or IStonComplexEntity.
    /// </summary>
    public interface IStonValuedEntity : IStonEntity
    {
        /// <summary>
        /// Gets the type declared for the entity, or null if the entity is implicitly typed.
        /// </summary>
        IStonType Type { get; }
    }

    /// <summary>
    /// Represents a simple-valued STON entity.
    /// </summary>
    public interface IStonSimpleEntity : IStonValuedEntity
    {
        /// <summary>
        /// Gets the simple value of the entity.
        /// </summary>
        IStonSimpleValue Value { get; }
    }

    /// <summary>
    /// Represents a complex-valued STON entity.
    /// </summary>
    public interface IStonComplexEntity : IStonValuedEntity
    {
        /// <summary>
        /// Gets the construction of the complex value, or null if no construction is declared.
        /// </summary>
        IStonConstruction Construction { get; }

        /// <summary>
        /// Gets the member initialization of the complex value, or null if no member initialization is declared.
        /// </summary>
        IStonMemberInit MemberInit { get; }

        /// <summary>
        /// Gets the collection initialization of the complex value, or null if no collection initialization is declared.
        /// </summary>
        IStonCollectionInit CollectionInit { get; }
    }

    /// <summary>
    /// Represents a reference STON entity.
    /// </summary>
    public interface IStonReferenceEntity : IStonEntity
    {
        /// <summary>
        /// Gets the address of the references value.
        /// </summary>
        IStonAddress Address { get; }
    }
}

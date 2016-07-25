using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston
{
    /// <summary>
    /// Exposes the STON document data.
    /// </summary>
    public interface IStonDocument
    {
        /// <summary>
        /// Gets the STON document core.
        /// </summary>
        IStonValuedEntity Core { get; }

        /// <summary>
        /// Gets the globally identified entity with a given name.
        /// </summary>
        /// <param name="globalIdentifier">The identifier of the entity.</param>
        /// <returns>The globally identified entity.</returns>
        IStonEntity GetGlobalEntity(string globalIdentifier);

        /// <summary>
        /// Gets the parent (defining) context of a given entity context, as defined in the hierarchy of contexts.
        /// </summary>
        /// <param name="context">The entity context whose parent should be found.</param>
        /// <returns>The parent (defining) context of the entity context.</returns>
        IStonComplexEntity GetParentContext(IStonEntity context);

        /// <summary>
        /// Gets a member entity of a complex entity by a given name or index. If the key is not present, returns null.
        /// </summary>
        /// <param name="entity">The entity to get the member of.</param>
        /// <param name="memberKey">The name or index of the member.</param>
        /// <returns>The member entity associated with the name or index.</returns>
        IStonEntity GetMember(IStonComplexEntity entity, IStonBindingKey memberKey);

        /// <summary>
        /// Gets the value referenced by a given reference entity from the document.
        /// </summary>
        /// <param name="reference">The reference entity.</param>
        /// <returns>The referenced value.</returns>
        IStonValuedEntity GetReferencedValue(IStonReferenceEntity reference);

        /// <summary>
        /// Enumerates the construction order of the document's valued entities.
        /// </summary>
        /// <returns>One of valid construction orders.</returns>
        IEnumerable<IStonValuedEntity> GetConstructionOrder();
    }
}

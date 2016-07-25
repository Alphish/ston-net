using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston.Building
{
    /// <summary>
    /// Exposes the functionality of creating STON documents.
    /// </summary>
    public interface IStonDocumentFactory
    {
        /// <summary>
        /// Builds a STON document using a given entity as a core source, with whitelists of known application extensions and rules determining valid application extension names.
        /// </summary>
        /// <param name="coreSource">The entity to use as a core source.</param>
        /// <param name="knownApplicationExtensionTypes">The whitelist of known application extension types.</param>
        /// <param name="knownApplicationExtensionMembers">The whitelist of known application extension members.</param>
        /// <param name="extensionTypesRule">The rule determining if a given name is a valid application extension type name.</param>
        /// <param name="extensionMembersRule">The rule determining if a given name is a valid application extension member name.</param>
        /// <returns>The built STON document.</returns>
        IStonDocument CreateDocument(
            IStonValuedEntity coreSource,
            IEnumerable<string> knownApplicationExtensionTypes = null, IEnumerable<string> knownApplicationExtensionMembers = null,
            Func<string, bool> extensionTypesRule = null, Func<string, bool> extensionMembersRule = null
            );
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston.Building
{
    /// <summary>
    /// Provides the functionality of creating STON documents.
    /// </summary>
    public sealed class CoreStonDocumentFactory : IStonDocumentFactory
    {
        /// <summary>
        /// Exposes the CoreStonDocumentFactory functionality for creating STON documents.
        /// </summary>
        public static IStonDocumentFactory Instance { get; } = new CoreStonDocumentFactory();
        private CoreStonDocumentFactory() { }

        /// <summary>
        /// Builds a STON document using a copy of a given entity as a source, with whitelists of known application extensions and rules determining valid application extension names.
        /// </summary>
        /// <param name="coreSource">The entity to copy core's structure from.</param>
        /// <param name="knownApplicationExtensionTypes">The whitelist of known application extension types.</param>
        /// <param name="knownApplicationExtensionMembers">The whitelist of known application extension members.</param>
        /// <param name="extensionTypesRule">The rule determining if a given name is a valid application extension type name.</param>
        /// <param name="extensionMembersRule">The rule determining if a given name is a valid application extension member name.</param>
        /// <returns>The built STON document.</returns>
        public IStonDocument CreateDocument(IStonValuedEntity coreSource, IEnumerable<string> knownApplicationExtensionTypes, IEnumerable<string> knownApplicationExtensionMembers, Func<string, bool> extensionTypesRule, Func<string, bool> extensionMembersRule)
        {
            return new StonDocument(coreSource, knownApplicationExtensionTypes, knownApplicationExtensionMembers, extensionTypesRule, extensionMembersRule);
        }
    }
}

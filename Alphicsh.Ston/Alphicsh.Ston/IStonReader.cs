using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston
{
    /// <summary>
    /// Exposes the functionality of reading a STON structure from a STON text.
    /// </summary>
    public interface IStonReader
    {
        /// <summary>
        /// Gets the STON format supported by the reader.
        /// </summary>
        StonFormat SupportedFormat { get; }

        /// <summary>
        /// Reads a STON entity from a given text reader.
        /// </summary>
        /// <param name="reader">The text reader to read the entity from.</param>
        /// <returns>The read STON entity.</returns>
        IStonEntity ReadEntity(TextReader reader);

        /// <summary>
        /// Reads a STON document from a given text reader, with whitelists of known application extensions and rules determining valid application extension names.
        /// </summary>
        /// <param name="reader">The text reader to read the document from.</param>
        /// <param name="knownApplicationExtensionTypes">The whitelist of known application extension types.</param>
        /// <param name="knownApplicationExtensionMembers">The whitelist of known application extension members.</param>
        /// <param name="extensionTypesRule">The rule determining if a given name is a valid application extension type name.</param>
        /// <param name="extensionMembersRule">The rule determining if a given name is a valid application extension member name.</param>
        /// <returns>The read STON document.</returns>
        IStonDocument ReadDocument(
            TextReader reader,
            IEnumerable<string> knownApplicationExtensionTypes = null, IEnumerable<string> knownApplicationExtensionMembers = null,
            Func<string, bool> extensionTypesRule = null, Func<string, bool> extensionMembersRule = null
            );
    }

    /// <summary>
    /// Exposes the functionality of reading a STON structure from a STON text.
    /// </summary>
    /// <typeparam name="TEntity">Type of the entity read by the reader.</typeparam>
    /// <typeparam name="TDocument">Type of the document read by the reader.</typeparam>
    public interface IStonReader<out TEntity, out TDocument> : IStonReader
        where TEntity : IStonEntity
        where TDocument : IStonDocument
    {
        /// <summary>
        /// Reads a STON entity from a given text reader.
        /// </summary>
        /// <param name="reader">The text reader to read the entity from.</param>
        /// <returns>The read STON entity.</returns>
        new TEntity ReadEntity(TextReader reader);

        /// <summary>
        /// Reads a STON document from a given text reader, with whitelists of known application extensions and rules determining valid application extension names.
        /// </summary>
        /// <param name="reader">The text reader to read the document from.</param>
        /// <param name="knownApplicationExtensionTypes">The whitelist of known application extension types.</param>
        /// <param name="knownApplicationExtensionMembers">The whitelist of known application extension members.</param>
        /// <param name="extensionTypesRule">The rule determining if a given name is a valid application extension type name.</param>
        /// <param name="extensionMembersRule">The rule determining if a given name is a valid application extension member name.</param>
        /// <returns>The read STON document.</returns>
        new TDocument ReadDocument(
            TextReader reader,
            IEnumerable<string> knownApplicationExtensionTypes = null, IEnumerable<string> knownApplicationExtensionMembers = null,
            Func<string, bool> extensionTypesRule = null, Func<string, bool> extensionMembersRule = null
            );
    }
}

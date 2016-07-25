using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston
{
    /// <summary>
    /// Provides extension methods for common STON readers operations.
    /// </summary>
    public static class IStonReader_Extensions
    {
        #region Plain IStonReader extensions

        /// <summary>
        /// Parses a STON entity from a given STON text.
        /// </summary>
        /// <param name="reader">The STON reader to read the entity with.</param>
        /// <param name="value">The text to read the entity from.</param>
        /// <returns>The parsed STON entity.</returns>
        public static IStonEntity ParseEntity(this IStonReader reader, string value)
        {
            if (value == null) throw new ArgumentNullException("value");
            return reader.ReadEntity(new StringReader(value));
        }

        /// <summary>
        /// Parses a STON document from a given STON text, with whitelists of known application extensions and rules determining valid application extension names.
        /// </summary>
        /// <param name="reader">The STON reader to read the document with.</param>
        /// <param name="value">The text to read the document from.</param>
        /// <param name="knownApplicationExtensionTypes">The whitelist of known application extension types.</param>
        /// <param name="knownApplicationExtensionMembers">The whitelist of known application extension members.</param>
        /// <param name="extensionTypesRule">The rule determining if a given name is a valid application extension type name.</param>
        /// <param name="extensionMembersRule">The rule determining if a given name is a valid application extension member name.</param>
        /// <returns>The parsed STON document.</returns>
        public static IStonDocument ParseDocument(
            this IStonReader reader, string value,
            IEnumerable<string> knownApplicationExtensionTypes = null, IEnumerable<string> knownApplicationExtensionMembers = null,
            Func<string, bool> extensionTypesRule = null, Func<string, bool> extensionMembersRule = null
            )
        {
            if (value == null) throw new ArgumentNullException("value");
            return reader.ReadDocument(new StringReader(value), knownApplicationExtensionTypes, knownApplicationExtensionMembers, extensionTypesRule, extensionMembersRule);
        }

        /// <summary>
        /// Loads a STON entity from a given STON file.
        /// </summary>
        /// <param name="reader">The STON reader to read the entity with.</param>
        /// <param name="path">The path to the file to read the entity from.</param>
        /// <returns>The loaded STON entity.</returns>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public static IStonEntity LoadEntity(this IStonReader reader, string path)
        {
            if (path == null) throw new ArgumentNullException("value");
            using (var streamReader = new StreamReader(path))
            {
                return reader.ReadEntity(streamReader);
            }
        }

        /// <summary>
        /// Loads a STON document from a given STON file, with whitelists of known application extensions and rules determining valid application extension names.
        /// </summary>
        /// <param name="reader">The STON reader to read the document with.</param>
        /// <param name="path">The path to the file to read the document from.</param>
        /// <param name="knownApplicationExtensionTypes">The whitelist of known application extension types.</param>
        /// <param name="knownApplicationExtensionMembers">The whitelist of known application extension members.</param>
        /// <param name="extensionTypesRule">The rule determining if a given name is a valid application extension type name.</param>
        /// <param name="extensionMembersRule">The rule determining if a given name is a valid application extension member name.</param>
        /// <returns>The loaded STON document.</returns>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public static IStonDocument LoadDocument(
            this IStonReader reader, string path,
            IEnumerable<string> knownApplicationExtensionTypes = null, IEnumerable<string> knownApplicationExtensionMembers = null,
            Func<string, bool> extensionTypesRule = null, Func<string, bool> extensionMembersRule = null
            )
        {
            if (path == null) throw new ArgumentNullException("value");
            using (var streamReader = new StreamReader(path))
            {
                return reader.ReadDocument(streamReader, knownApplicationExtensionTypes, knownApplicationExtensionMembers, extensionTypesRule, extensionMembersRule);
            }
        }

        /// <summary>
        /// Loads a STON entity from a given stream, leaving the stream open.
        /// </summary>
        /// <param name="reader">The STON reader to read the entity with.</param>
        /// <param name="stream">The stream to read the entity from.</param>
        /// <returns>The loaded STON entity.</returns>
        public static IStonEntity LoadEntity(this IStonReader reader, Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            using (var streamReader = new StreamReader(stream, Encoding.UTF8, true, 1024, true))
            {
                return reader.ReadEntity(streamReader);
            }
        }

        /// <summary>
        /// Loads a STON document from a given stream, with whitelists of known application extensions and rules determining valid application extension names, leaving the stream open.
        /// </summary>
        /// <param name="reader">The STON reader to read the document with.</param>
        /// <param name="stream">The stream to read the document from.</param>
        /// <param name="knownApplicationExtensionTypes">The whitelist of known application extension types.</param>
        /// <param name="knownApplicationExtensionMembers">The whitelist of known application extension members.</param>
        /// <param name="extensionTypesRule">The rule determining if a given name is a valid application extension type name.</param>
        /// <param name="extensionMembersRule">The rule determining if a given name is a valid application extension member name.</param>
        /// <returns>The loaded STON document.</returns>
        public static IStonDocument LoadDocument(
            this IStonReader reader, Stream stream,
            IEnumerable<string> knownApplicationExtensionTypes = null, IEnumerable<string> knownApplicationExtensionMembers = null,
            Func<string, bool> extensionTypesRule = null, Func<string, bool> extensionMembersRule = null
            )
        {
            if (stream == null) throw new ArgumentNullException("stream");
            using (var streamReader = new StreamReader(stream, Encoding.UTF8, true, 1024, true))
            {
                return reader.ReadDocument(streamReader, knownApplicationExtensionTypes, knownApplicationExtensionMembers, extensionTypesRule, extensionMembersRule);
            }
        }

        #endregion

        #region IStonReader<TEntity, TDocument> extensions

        /// <summary>
        /// Parses a STON entity from a given STON text.
        /// </summary>
        /// <param name="reader">The STON reader to read the entity with.</param>
        /// <param name="value">The text to read the entity from.</param>
        /// <returns>The parsed STON entity.</returns>
        public static TEntity ParseEntity<TEntity, TDocument>(this IStonReader<TEntity, TDocument> reader, string value)
            where TEntity : IStonEntity
            where TDocument : IStonDocument
        {
            if (value == null) throw new ArgumentNullException("value");
            return reader.ReadEntity(new StringReader(value));
        }

        /// <summary>
        /// Parses a STON document from a given STON text, with whitelists of known application extensions and rules determining valid application extension names.
        /// </summary>
        /// <param name="reader">The STON reader to read the document with.</param>
        /// <param name="value">The text to read the document from.</param>
        /// <param name="knownApplicationExtensionTypes">The whitelist of known application extension types.</param>
        /// <param name="knownApplicationExtensionMembers">The whitelist of known application extension members.</param>
        /// <param name="extensionTypesRule">The rule determining if a given name is a valid application extension type name.</param>
        /// <param name="extensionMembersRule">The rule determining if a given name is a valid application extension member name.</param>
        /// <returns>The parsed STON document.</returns>
        public static TDocument ParseDocument<TEntity, TDocument>(
            this IStonReader<TEntity, TDocument> reader, string value,
            IEnumerable<string> knownApplicationExtensionTypes = null, IEnumerable<string> knownApplicationExtensionMembers = null,
            Func<string, bool> extensionTypesRule = null, Func<string, bool> extensionMembersRule = null
            )
            where TEntity : IStonEntity
            where TDocument : IStonDocument
        {
            if (value == null) throw new ArgumentNullException("value");
            return reader.ReadDocument(new StringReader(value), knownApplicationExtensionTypes, knownApplicationExtensionMembers, extensionTypesRule, extensionMembersRule);
        }

        /// <summary>
        /// Loads a STON entity from a given STON file.
        /// </summary>
        /// <param name="reader">The STON reader to read the entity with.</param>
        /// <param name="path">The path to the file to read the entity from.</param>
        /// <returns>The loaded STON entity.</returns>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public static TEntity LoadEntity<TEntity, TDocument>(this IStonReader<TEntity, TDocument> reader, string path)
            where TEntity : IStonEntity
            where TDocument : IStonDocument
        {
            if (path == null) throw new ArgumentNullException("value");
            using (var streamReader = new StreamReader(path))
            {
                return reader.ReadEntity(streamReader);
            }
        }

        /// <summary>
        /// Loads a STON document from a given STON file, with whitelists of known application extensions and rules determining valid application extension names.
        /// </summary>
        /// <param name="reader">The STON reader to read the document with.</param>
        /// <param name="path">The path to the file to read the document from.</param>
        /// <param name="knownApplicationExtensionTypes">The whitelist of known application extension types.</param>
        /// <param name="knownApplicationExtensionMembers">The whitelist of known application extension members.</param>
        /// <param name="extensionTypesRule">The rule determining if a given name is a valid application extension type name.</param>
        /// <param name="extensionMembersRule">The rule determining if a given name is a valid application extension member name.</param>
        /// <returns>The loaded STON document.</returns>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public static TDocument LoadDocument<TEntity, TDocument>(
            this IStonReader<TEntity, TDocument> reader, string path,
            IEnumerable<string> knownApplicationExtensionTypes = null, IEnumerable<string> knownApplicationExtensionMembers = null,
            Func<string, bool> extensionTypesRule = null, Func<string, bool> extensionMembersRule = null
            )
            where TEntity : IStonEntity
            where TDocument : IStonDocument
        {
            if (path == null) throw new ArgumentNullException("value");
            using (var streamReader = new StreamReader(path))
            {
                return reader.ReadDocument(streamReader, knownApplicationExtensionTypes, knownApplicationExtensionMembers, extensionTypesRule, extensionMembersRule);
            }
        }

        /// <summary>
        /// Loads a STON entity from a given stream, leaving the stream open.
        /// </summary>
        /// <param name="reader">The STON reader to read the entity with.</param>
        /// <param name="stream">The stream to read the entity from.</param>
        /// <returns>The loaded STON entity.</returns>
        public static TEntity LoadEntity<TEntity, TDocument>(this IStonReader<TEntity, TDocument> reader, Stream stream)
            where TEntity : IStonEntity
            where TDocument : IStonDocument
        {
            if (stream == null) throw new ArgumentNullException("stream");
            using (var streamReader = new StreamReader(stream, Encoding.UTF8, true, 1024, true))
            {
                return reader.ReadEntity(streamReader);
            }
        }

        /// <summary>
        /// Loads a STON document from a given stream, with whitelists of known application extensions and rules determining valid application extension names, leaving the stream open.
        /// </summary>
        /// <param name="reader">The STON reader to read the document with.</param>
        /// <param name="stream">The stream to read the document from.</param>
        /// <param name="knownApplicationExtensionTypes">The whitelist of known application extension types.</param>
        /// <param name="knownApplicationExtensionMembers">The whitelist of known application extension members.</param>
        /// <param name="extensionTypesRule">The rule determining if a given name is a valid application extension type name.</param>
        /// <param name="extensionMembersRule">The rule determining if a given name is a valid application extension member name.</param>
        /// <returns>The loaded STON document.</returns>
        public static TDocument LoadDocument<TEntity, TDocument>(
            this IStonReader<TEntity, TDocument> reader, Stream stream,
            IEnumerable<string> knownApplicationExtensionTypes = null, IEnumerable<string> knownApplicationExtensionMembers = null,
            Func<string, bool> extensionTypesRule = null, Func<string, bool> extensionMembersRule = null
            )
            where TEntity : IStonEntity
            where TDocument : IStonDocument
        {
            if (stream == null) throw new ArgumentNullException("stream");
            using (var streamReader = new StreamReader(stream, Encoding.UTF8, true, 1024, true))
            {
                return reader.ReadDocument(streamReader, knownApplicationExtensionTypes, knownApplicationExtensionMembers, extensionTypesRule, extensionMembersRule);
            }
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston
{
    public static class IStonDocument_Extensions
    {
        #region Writing with plain IStonWriter

        /// <summary>
        /// Returns a string representation of a STON document, using specific STON writer.
        /// </summary>
        /// <param name="document">The document to represent as a string.</param>
        /// <param name="writer">The writer used to represent the entity.</param>
        /// <returns>The string representation of the entity.</returns>
        public static string ToString(this IStonDocument document, IStonWriter writer)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            var stringWriter = new StringWriter();
            writer.WriteDocument(stringWriter, document);
            return stringWriter.ToString();
        }

        /// <summary>
        /// Returns a canonical string representation of a STON document.
        /// </summary>
        /// <param name="document">The document to represent in its canonical form.</param>
        /// <returns>The canonical representation of the entity.</returns>
        public static string ToCanonicalForm(this IStonDocument document) => ToString(document, CanonicalStonWriter.Instance);

        /// <summary>
        /// Writes a string representation of a STON document to a file, using specific STON writer.
        /// </summary>
        /// <param name="document">The document to write.</param>
        /// <param name="path">The path of the file to write to.</param>
        /// <param name="writer">The writer used to write the entity.</param>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public static void Save(this IStonDocument document, string path, IStonWriter writer)
        {
            if (path == null) throw new ArgumentNullException("path");
            if (writer == null) throw new ArgumentNullException("writer");
            using (var streamWriter = new StreamWriter(path))
            {
                writer.WriteDocument(streamWriter, document);
            }
        }

        /// <summary>
        /// Writes a canonical representation of a STON document to a file.
        /// </summary>
        /// <param name="document">The document to write.</param>
        /// <param name="path">The path of the file to write to.</param>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public static void SaveCanonicalForm(this IStonDocument document, string path) => Save(document, path, CanonicalStonWriter.Instance);

        /// <summary>
        /// Writes a string representation of a STON document to a stream, using specific STON writer, leaving the stream open.
        /// </summary>
        /// <param name="document">The document to write.</param>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="writer">The writer used to write the entity.</param>
        public static void Save(this IStonDocument document, Stream stream, IStonWriter writer)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (writer == null) throw new ArgumentNullException("writer");
            using (var streamWriter = new StreamWriter(stream, new UTF8Encoding(false, true), 1024, true))
            {
                writer.WriteDocument(streamWriter, document);
            }
        }

        /// <summary>
        /// Writes a canonical string representation of a STON document to a stream.
        /// </summary>
        /// <param name="document">The document to write.</param>
        /// <param name="stream">The stream to write to.</param>
        public static void SaveCanonicalForm(this IStonDocument document, Stream stream) => Save(document, stream, CanonicalStonWriter.Instance);

        #endregion

        #region Writing with IStonWriter<TEntity, TDocument>

        /// <summary>
        /// Returns a string representation of a STON document, using specific STON writer.
        /// </summary>
        /// <param name="document">The document to represent as a string.</param>
        /// <param name="writer">The writer used to represent the entity.</param>
        /// <returns>The string representation of the entity.</returns>
        public static string ToString<TEntity, TDocument>(this TDocument document, IStonWriter<TEntity, TDocument> writer)
            where TEntity : IStonEntity
            where TDocument : IStonDocument
        {
            if (writer == null) throw new ArgumentNullException("writer");
            var stringWriter = new StringWriter();
            writer.WriteDocument(stringWriter, document);
            return stringWriter.ToString();
        }

        /// <summary>
        /// Writes a string representation of a STON document to a file, using specific STON writer.
        /// </summary>
        /// <param name="document">The document to write.</param>
        /// <param name="path">The path of the file to write to.</param>
        /// <param name="writer">The writer used to write the entity.</param>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public static void Save<TEntity, TDocument>(this TDocument document, string path, IStonWriter<TEntity, TDocument> writer)
            where TEntity : IStonEntity
            where TDocument : IStonDocument
        {
            if (path == null) throw new ArgumentNullException("path");
            if (writer == null) throw new ArgumentNullException("writer");
            using (var streamWriter = new StreamWriter(path))
            {
                writer.WriteDocument(streamWriter, document);
            }
        }

        /// <summary>
        /// Writes a string representation of a STON document to a stream, using specific STON writer, leaving the stream open.
        /// </summary>
        /// <param name="document">The document to write.</param>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="writer">The writer used to write the entity.</param>
        public static void Save<TEntity, TDocument>(this TDocument document, Stream stream, IStonWriter<TEntity, TDocument> writer)
            where TEntity : IStonEntity
            where TDocument : IStonDocument
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (writer == null) throw new ArgumentNullException("writer");
            using (var streamWriter = new StreamWriter(stream, new UTF8Encoding(false, true), 1024, true))
            {
                writer.WriteDocument(streamWriter, document);
            }
        }

        #endregion
    }
}

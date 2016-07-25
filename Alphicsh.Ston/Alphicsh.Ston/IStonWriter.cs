using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston
{
    /// <summary>
    /// Exposes the functionality of writing a STON structure to a specific text, STON or not.
    /// </summary>
    /// <typeparam name="TEntity">The expected type of the entity to write.</typeparam>
    /// <typeparam name="TDocument">The expected type of the document to write.</typeparam>
    public interface IStonWriter<in TEntity, in TDocument>
        where TEntity : IStonEntity
        where TDocument : IStonDocument
    {
        /// <summary>
        /// Gets the format written by the writer.
        /// </summary>
        StonFormat OutputFormat { get; }

        /// <summary>
        /// Writes a STON entity to a given text writer.
        /// </summary>
        /// <param name="writer">The writer to write the entity to.</param>
        /// <param name="entity">The entity to write.</param>
        void WriteEntity(TextWriter writer, TEntity entity);

        /// <summary>
        /// Writes a STON document to a given text writer.
        /// </summary>
        /// <param name="writer">The writer to write the document to.</param>
        /// <param name="document">The document to write.</param>
        void WriteDocument(TextWriter writer, TDocument document);
    }

    /// <summary>
    /// Exposes the functionality of writing a STON structure to a specific text, STON or not.
    /// </summary>
    public interface IStonWriter : IStonWriter<IStonEntity, IStonDocument>
    {
    }
}

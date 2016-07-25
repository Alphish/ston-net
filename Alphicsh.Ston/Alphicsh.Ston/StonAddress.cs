using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston
{
    /// <summary>
    /// Represents a STON reference address.
    /// </summary>
    public sealed class StonAddress : IStonAddress
    {
        /// <summary>
        /// Gets the initial context of the address.
        /// </summary>
        public IStonInitialContext InitialContext { get; }
        /// <summary>
        /// Gets the path segments of the address.
        /// </summary>
        public IEnumerable<IStonPathSegment> RelativePath { get; }

        /// <summary>
        /// Creates a new STON address, with a given initial context and path segments.
        /// </summary>
        /// <param name="initialContext">The initial context to start from.</param>
        /// <param name="relativePath">The path segments leading to the destination.</param>
        public StonAddress(IStonInitialContext initialContext, IEnumerable<IStonPathSegment> relativePath = null)
        {
            if (initialContext == null) throw new ArgumentNullException("initialContext");
            InitialContext = StonInitialContext.Copy(initialContext);
            RelativePath = relativePath?.Select(segment => StonPathSegment.Copy(segment)).ToList() ?? Enumerable.Empty<IStonPathSegment>();
        }

        /// <summary>
        /// Creates a new STON address, with a given initial context and path segments.
        /// </summary>
        /// <param name="initialContext">The initial context to start of.</param>
        /// <param name="relativePath">The path segments leading to the destination.</param>
        public StonAddress(IStonInitialContext initialContext, params IStonPathSegment[] relativePath)
            : this(initialContext, relativePath as IEnumerable<IStonPathSegment>) { }

        /// <summary>
        /// Creates a structurally equivalent STON address from a given address.
        /// </summary>
        /// <param name="address">The address to copy the structure from.</param>
        public StonAddress(IStonAddress address)
            : this(address.InitialContext, address.RelativePath) { }

        /// <summary>
        /// Creates a structurally equivalent STON address from a given address.
        /// </summary>
        /// <param name="address">The address to copy the structure of.</param>
        /// <returns>A structurally equivalent copy of the given address.</returns>
        public static IStonAddress Copy(IStonAddress address)
        {
            if (address == null) throw new ArgumentNullException("address");
            return new StonAddress(address);
        }

        // override for general debugging purposes
        // it is not necessarily meant as a valid STON representation
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return InitialContext.ToString() + string.Join("", RelativePath.Select(segment => segment.ToString()));
        }
    }
}

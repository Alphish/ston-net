using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston
{
    /// <summary>
    /// Serves as a base class for STON initial context classes.
    /// </summary>
    public abstract class StonInitialContext : IStonInitialContext
    {
        /// <summary>
        /// Creates a structurally equivalent initial context from a given context.
        /// </summary>
        /// <param name="context">The context to copy the structure of.</param>
        /// <returns>A structurally equivalent copy of the given context.</returns>
        public static IStonInitialContext Copy(IStonInitialContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (context is IStonAncestorInitialContext) return StonAncestorInitialContext.Copy(context as IStonAncestorInitialContext);
            if (context is IStonGlobalEntityInitialContext) return StonGlobalEntityInitialContext.Copy(context as IStonGlobalEntityInitialContext);
            throw new StonImplementationException(context.GetType(), typeof(IStonInitialContext), typeof(IStonAncestorInitialContext), typeof(IStonGlobalEntityInitialContext));
        }
    }

    /// <summary>
    /// Represents the STON address ancestor initial context with a given order.
    /// When the ancestor order is zero, the initial context is the reference defining entity instead.
    /// </summary>
    public sealed class StonAncestorInitialContext : StonInitialContext, IStonAncestorInitialContext
    {
        /// <summary>
        /// Gets the ancestor order of the initial context.
        /// When the initial context is the reference defining entity, the order is 0.
        /// </summary>
        public int AncestorOrder { get; }

        /// <summary>
        /// Creates a new ancestor initial context, with a given order.
        /// </summary>
        /// <param name="ancestorOrder">The order of the ancestor, or 0 for the reference defining entity context.</param>
        public StonAncestorInitialContext(int ancestorOrder)
        {
            AncestorOrder = ancestorOrder;
        }

        /// <summary>
        /// Creates a structurally equivalent ancestor initial context from a given context.
        /// </summary>
        /// <param name="context">The context to copy the structure from.</param>
        public StonAncestorInitialContext(IStonAncestorInitialContext context)
            : this(context.AncestorOrder) { }

        /// <summary>
        /// Creates a structurally equivalent ancestor initial context from a given context.
        /// </summary>
        /// <param name="context">The context to copy the structure from.</param>
        /// <returns>A structurally equivalent copy of the given segment.</returns>
        public static IStonAncestorInitialContext Copy(IStonAncestorInitialContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            return new StonAncestorInitialContext(context);
        }

        // override for general debugging purposes
        // it is not necessarily meant as a valid STON representation
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return AncestorOrder == 0 ? "$" : new string('^', AncestorOrder);
        }
    }

    /// <summary>
    /// Represents the STON address globally identified entity initial context with a given identifier.
    /// When the identifier is empty, the initial cnotext is the document core instead.
    /// </summary>
    public sealed class StonGlobalEntityInitialContext : StonInitialContext, IStonGlobalEntityInitialContext
    {
        /// <summary>
        /// Gets the identifier of the globally identified entity.
        /// When the initial context is the document core, the identifier is empty.
        /// </summary>
        public string GlobalIdentifier { get; }

        /// <summary>
        /// Creates a globally identified entity initial context with a given identifier.
        /// </summary>
        /// <param name="globalIdentifier">The identifier of the entity, or empty string for document core.</param>
        public StonGlobalEntityInitialContext(string globalIdentifier)
        {
            if (globalIdentifier == null) throw new ArgumentNullException("globalIdentifier");
            GlobalIdentifier = globalIdentifier;
        }

        /// <summary>
        /// Creates a structurally equivalent globally identified entity initial context from a given context.
        /// </summary>
        /// <param name="context">The context to copy the structure from.</param>
        public StonGlobalEntityInitialContext(IStonGlobalEntityInitialContext context)
            : this(context.GlobalIdentifier) { }

        /// <summary>
        /// Creates a structurally equivalent globally identified entity initial context from a given context.
        /// </summary>
        /// <param name="context">The context to copy the structure from.</param>
        /// <returns>A structurally equivalent copy of the given context.</returns>
        public static IStonGlobalEntityInitialContext Copy(IStonGlobalEntityInitialContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            return new StonGlobalEntityInitialContext(context);
        }

        // override for general debugging purposes
        // it is not necessarily meant as a valid STON representation
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return GlobalIdentifier == "" ? "^*" : "@" + GlobalIdentifier;
        }
    }
}

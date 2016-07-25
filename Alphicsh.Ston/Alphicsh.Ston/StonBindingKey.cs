using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston
{
    /// <summary>
    /// Serves as a base class for STON member binding key classes.
    /// </summary>
    public abstract class StonBindingKey : IStonBindingKey
    {
        /// <summary>
        /// Creates a structurally equivalent member binding key from a given binding key.
        /// </summary>
        /// <param name="bindingKey">The binding key to copy the structure of.</param>
        /// <returns>A structurally equivalent copy of the given binding key.</returns>
        public static IStonBindingKey Copy(IStonBindingKey bindingKey)
        {
            if (bindingKey == null) throw new ArgumentNullException("bindingKey");
            if (bindingKey is IStonBindingName) return StonBindingName.Copy(bindingKey as IStonBindingName);
            if (bindingKey is IStonBindingIndex) return StonBindingIndex.Copy(bindingKey as IStonBindingIndex);
            throw new StonImplementationException(bindingKey.GetType(), typeof(IStonBindingKey), typeof(IStonBindingName), typeof(IStonBindingIndex));
        }
    }

    /// <summary>
    /// Represents a named member key.
    /// The key is composed of a name and member kind (regular/extension).
    /// </summary>
    public sealed class StonBindingName : StonBindingKey, IStonBindingName
    {
        /// <summary>
        /// Gets the name of the member.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Indicates whether the member is an extension member or a regular member.
        /// </summary>
        public bool IsExtension { get; }

        /// <summary>
        /// Creates a new member binding name, with a given name, regular or extension.
        /// </summary>
        /// <param name="name">The name of the member.</param>
        /// <param name="isExtension">Whether the member is an extension member or a regular member.</param>
        public StonBindingName(string name, bool isExtension = false)
        {
            if (name == null) throw new ArgumentNullException("name");
            Name = name;
            IsExtension = isExtension;
        }

        /// <summary>
        /// Creates a structurally equivalent member binding name from a given binding name.
        /// </summary>
        /// <param name="bindingName">The binding name to copy the structure from.</param>
        public StonBindingName(IStonBindingName bindingName)
            : this(bindingName.Name, bindingName.IsExtension) { }

        /// <summary>
        /// Creates a structurally equivalent member binding name from a given binding name.
        /// </summary>
        /// <param name="bindingName">The binding name to copy the structure from.</param>
        /// <returns>A structurally equivalent copy of the given binding name.</returns>
        public static IStonBindingName Copy(IStonBindingName bindingName)
        {
            if (bindingName == null) throw new ArgumentNullException("bindingName");
            return new StonBindingName(bindingName);
        }

        // override for general debugging purposes
        // it is not necessarily meant as a valid STON representation
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return (IsExtension ? "!" : "") + '"' + Name + '"';
        }
    }

    /// <summary>
    /// Represents an indexed member binding key.
    /// They key consists of an index parameters sequence.
    /// </summary>
    public sealed class StonBindingIndex : StonBindingKey, IStonBindingIndex
    {
        /// <summary>
        /// Gets the member index parameters.
        /// </summary>
        public IEnumerable<IStonEntity> Parameters { get; }

        /// <summary>
        /// Creates a new member binding index, with given parameters.
        /// </summary>
        /// <param name="parameters">The index parameters sequence.</param>
        public StonBindingIndex(IEnumerable<IStonEntity> parameters)
        {
            if (parameters == null) throw new ArgumentNullException("parameters");
            Parameters = parameters.Select(p => StonEntity.Copy(p)).ToList();
        }

        /// <summary>
        /// Creates a structurally equivalent member binding index from a given binding index.
        /// </summary>
        /// <param name="bindingIndex">The binding index to copy the structure from.</param>
        public StonBindingIndex(IStonBindingIndex bindingIndex)
            : this(bindingIndex.Parameters) { }

        /// <summary>
        /// Creates a structurally equivalent member binding index from a given binding index.
        /// </summary>
        /// <param name="bindingIndex">The binding index to copy the structure from.</param>
        /// <returns>A structurally equivalent copy of the given binding index.</returns>
        public static IStonBindingIndex Copy(IStonBindingIndex bindingIndex)
        {
            if (bindingIndex == null) throw new ArgumentNullException("bindingIndex");
            return new StonBindingIndex(bindingIndex);
        }

        // override for general debugging purposes
        // it is not necessarily meant as a valid STON representation
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return "[ " + Parameters.Count() + " parameter(s) ]";
        }
    }
}

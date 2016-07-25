using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston
{
    /// <summary>
    /// Represents a member initialization of a complex value.
    /// </summary>
    public sealed class StonMemberInit : IStonMemberInit
    {
        /// <summary>
        /// Gets the member bindings of the member initialization.
        /// </summary>
        public IEnumerable<KeyValuePair<IStonBindingKey, IStonEntity>> MemberBindings { get; }

        /// <summary>
        /// Creates a new member initialization of a complex value, given a sequence of member bindings.
        /// </summary>
        /// <param name="memberBindings">The sequence of member bindings.</param>
        public StonMemberInit(IEnumerable<KeyValuePair<IStonBindingKey, IStonEntity>> memberBindings)
        {
            MemberBindings = memberBindings?.Select(kvp => new KeyValuePair<IStonBindingKey, IStonEntity>(StonBindingKey.Copy(kvp.Key), StonEntity.Copy(kvp.Value))).ToList() ?? new List<KeyValuePair<IStonBindingKey, IStonEntity>>();
        }

        /// <summary>
        /// Creates a structurally equivalent member initialization from a given member initialization.
        /// </summary>
        /// <param name="memberInit">The member initialization to copy the structure of.</param>
        public StonMemberInit(IStonMemberInit memberInit)
            : this(memberInit.MemberBindings) { }

        /// <summary>
        /// Creates a structurally equivalent member initialization from a given member initialization.
        /// </summary>
        /// <param name="memberInit">The member initialization to copy the structure of.</param>
        /// <returns>A structurally equivalent copy of the given member initialization.</returns>
        public static IStonMemberInit Copy(IStonMemberInit memberInit)
        {
            if (memberInit == null) throw new ArgumentNullException("memberInit");
            return new StonMemberInit(memberInit);
        }

        // override for general debugging purposes
        // it is not necessarily meant as a valid STON representation
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return "{ " + MemberBindings.Count() + " member(s) }";
        }
    }
}

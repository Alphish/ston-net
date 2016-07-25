using System;
using System.Diagnostics.CodeAnalysis;

namespace Alphicsh.Ston.Tests
{
    // collection of STON elements that implement STON interfaces incorrectly

    public class InvalidStonEntity : IStonEntity
    {
        public string GlobalIdentifier
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }

    public class InvalidStonValuedEntity : IStonValuedEntity
    {
        public string GlobalIdentifier
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IStonType Type
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }

    public class InvalidStonType : IStonType { }

    public class InvalidStonBindingKey : IStonBindingKey { }

    public class InvalidStonInitialContext : IStonInitialContext { }

    public class InvalidStonPathSegment : IStonPathSegment { }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston.Tests
{
    /// <summary>
    /// A utility class to perform simple assertions in parallel, and show their results in bulk.
    /// </summary>
    public class AggregateTester
    {
        /// <summary>
        /// Creates a new aggregate tester, ready to add new tests to.
        /// </summary>
        /// <returns>The ready to use aggregate tester.</returns>
        public static AggregateTester New() => new AggregateTester();

        // test cases to run
        private List<KeyValuePair<string, Action>> Cases { get; } = new List<KeyValuePair<string, Action>>();

        // the gathered failed assertions
        private List<AssertFailedException> FailedAssertions { get; } = new List<AssertFailedException>();

        // the hidden constructor
        private AggregateTester() { }

        /// <summary>
        /// Schedules a test case for an aggregate tester, then passes the tester to perform further commands.
        /// </summary>
        /// <param name="name">The name of the test.</param>
        /// <param name="test">The code performed during the test.</param>
        /// <returns>The aggregate tester, awaiting further commands.</returns>
        public AggregateTester Add(string name, Action test)
        {
            Cases.Add(new KeyValuePair<string, Action>(name, test));
            return this;
        }

        /// <summary>
        /// Runs all scheduled test cases, prints out encountered errors and fails assertion if at least one test failed.
        /// </summary>
        public void Run()
        {
            foreach (var @case in Cases)
            {
                try
                {
                    @case.Value();
                }
                catch(AssertFailedException ex)
                {
                    System.Diagnostics.Trace.WriteLine(@case.Key + ":");
                    System.Diagnostics.Trace.WriteLine("    " + ex.Message);
                    FailedAssertions.Add(ex);
                }
            }
            if (FailedAssertions.Any()) Assert.Fail($"The test failed in {FailedAssertions.Count} cases.");
        }
    }
}

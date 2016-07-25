using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Alphicsh.Ston.Tests
{
    using Tokenization;

    /// <summary>
    /// A collection of common test scenarios.
    /// </summary>
    public static class Expect
    {
        /// <summary>
        /// Checks that parsing a given STON text results in an unexpected character parsing exception, for a specific character, expected chartype, position and message.
        /// </summary>
        /// <param name="ston">The STON text that causes an error.</param>
        /// <param name="character">The asserted unexpected character.</param>
        /// <param name="expectedType">The asserted expected character type.</param>
        /// <param name="line">The asserted line where the error occurs.</param>
        /// <param name="column">The asserted column in the line where the error occurs.</param>
        /// <param name="position">The asserted position in the string where the error occurs.</param>
        /// <param name="message">The asserted error message.</param>
        public static void UnexpectedCharacter(string ston, int? character = null, StonChartype? expectedType = null, int line = -1, int column = -1, int position = -1, string message = null)
        {
            try
            {
                RegularStonReader.Default.ParseEntity(ston);
                Assert.Fail("The entity has been read properly. This should *not* have happened.");
            }
            catch (StonUnexpectedCharacterParsingException ex)
            {
                if (character != null) Assert.AreEqual(character, ex.Character);
                if (expectedType != null) Assert.AreEqual(expectedType, ex.ExpectedType);
                if (line > -1) Assert.AreEqual(line, ex.Line);
                if (column > -1) Assert.AreEqual(column, ex.Column);
                if (position > -1) Assert.AreEqual(position, ex.Position);
                if (message != null) Assert.AreEqual(message, ex.Message);
            }
        }

        /// <summary>
        /// Checks that performing a given action results in a specific exception being thrown.
        /// </summary>
        /// <typeparam name="TException">The type of the expected exception.</typeparam>
        /// <param name="action">The action causing the exception.</param>
        /// <param name="message">The expected error message.</param>
        public static void Exception<TException>(Action action, string message)
            where TException : Exception
        {
            try
            {
                action();
                Assert.Fail($"The action has executed without throwing{typeof(TException).Name}. This should *not* have happened.");
            }
            catch (TException ex)
            {
                Assert.AreEqual(message, ex.Message);
            }
        }
    }
}

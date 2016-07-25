using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphicsh.Ston.Helpers
{
    /// <summary>
    /// Provides the functionality of adding and subtracting arbitrarily large integers represented with base10 strings.
    /// </summary>
    public static class NumericStringCalculator
    {
        // function used internally to skip leading zeros
        private static bool IsLeadingZero(char c) => c == '0';

        /// <summary>
        /// Adds an integer to a given numeric string.
        /// </summary>
        /// <param name="x">The numeric string.</param>
        /// <param name="y">The integer to add.</param>
        /// <returns>The numeric string representing the sum.</returns>
        public static string Add(string x, int y)
        {
            if (y == 0) return x;

            if (x[0] == '-')
            {
                if (y < 0) return '-' + Add(x.Skip(1).SkipWhile(IsLeadingZero).ToArray(), (-y).ToString().ToCharArray());
                else return Subtract(y.ToString().ToCharArray(), x.Skip(1).SkipWhile(IsLeadingZero).ToArray());
            }
            else
            {
                if (y < 0) return Subtract(x.SkipWhile(IsLeadingZero).ToArray(), (-y).ToString().ToCharArray());
                else return Add(x.SkipWhile(IsLeadingZero).ToArray(), y.ToString().ToCharArray());
            }
        }

        // adds two numeric strings together, represented as characters arrays
        // returns the numeric string representing the sum
        // used internally by other Add functions
        private static string Add(char[] x, char[] y)
        {
            // creating the resulting array
            int maxLength = Math.Max(x.Length, y.Length);
            char[] result = new char[maxLength + 1];

            // initializing
            int xpos = x.Length;
            int ypos = y.Length;
            int rpos = result.Length;
            int num = 0;

            // adding digits pairs, starting from the least significant
            while (--rpos >= 0)
            {
                if (--xpos >= 0) num += x[xpos] - '0';
                if (--ypos >= 0) num += y[ypos] - '0';

                // sets the result digit to the remainder of accumulated number
                result[rpos] = (char)('0' + num % 10);
                // divides the accumulated number to carry to the next digit
                num /= 10;
            }

            return new string(result.SkipWhile(IsLeadingZero).ToArray());
        }

        /// <summary>
        /// Subtracts an integer from a given numeric string.
        /// </summary>
        /// <param name="x">The numeric string.</param>
        /// <param name="y">The integer to subtract.</param>
        /// <returns>The numeric string representing the difference.</returns>
        public static string Subtract(string x, int y)
        {
            if (y == 0) return x;
            else return Add(x, -y);
        }

        // performs subtraction of two numeric strings, represented as characters arrays
        // returns the numeric string representing the difference
        // used internally by other Subtract functions
        private static string Subtract(char[] x, char[] y)
        {
            // if x < y, then -(y-x) is returned
            if (x.Length < y.Length) return "-" + Subtract(y, x);

            if (x.Length == y.Length)
            {
                for (var i = 0; i < x.Length; i++)
                {
                    if (x[i] < y[i]) return "-" + Subtract(y, x);
                    if (x[i] > y[i]) break;

                    // if corresponding digits of x and y are the same
                    // their difference is zero
                    if (i + 1 == x.Length) return "0";
                }
            }

            // subtracting y from x, with y being the smaller value
            int xpos = x.Length;
            int ypos = y.Length;
            int num = 0;

            // subtracting digit pairs, starting from the least significant
            while (--ypos >= 0 || num < 0)
            {
                num += x[--xpos] - '0';
                if (ypos >= 0) num -= y[ypos] - '0';

                if (num >= 0)
                {
                    // if accumulated subtraction result is non-negative
                    // the entire result is used as the digit
                    x[xpos] = (char)('0' + num);
                    num = 0;
                }
                else
                {
                    // if the accumulated subtraction result is negative
                    // its value increased by 10 is used as the digit
                    // and the negative value is carried over
                    x[xpos] = (char)('0' + (num + 10));
                    num = -1;
                }
            }

            return new string(x.SkipWhile(IsLeadingZero).ToArray());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobCommon.Utils
{
    using System.Runtime.CompilerServices;

    public static class StringUtils {
        /// <summary>
        /// contains subset of English alphabet in order to prevent swearwords generation
        /// </summary>
        private static readonly char[] alphaBet = "abcdevwxyz0123456789".ToCharArray();
        private static readonly Random random = new Random();

        /// <summary>
        /// Determines whether the specified string is empty.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEmpty(string str) {
            return string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// Determines whether the specified string is not empty.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotEmpty(string str) {
            return !IsEmpty(str);
        }

        /// <summary>
        /// Generates the random English string.
        /// </summary>
        /// <param name="stringLength">Length of the string.</param>
        /// <remarks>
        /// Used for verification code generation
        /// </remarks>
        /// <returns></returns>
        public static string GenerateRandomEnglishString(int stringLength = 5) {
            stringLength = Math.Max(1, stringLength);
            stringLength = Math.Min(stringLength, alphaBet.Length);

            var chars = Enumerable.Repeat(alphaBet, stringLength)
                .Select(ab => ab[random.Next(ab.Length)])
                .ToArray();

            return new string(chars);
        }
    }
}

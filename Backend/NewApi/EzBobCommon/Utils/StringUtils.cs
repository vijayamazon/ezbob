using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobCommon.Utils
{
    using System.Runtime.CompilerServices;

    public static class StringUtils
    {
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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobCommon.Utils
{
    using System.Runtime.CompilerServices;

    public static class NullableUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsTrue(this bool? val) {
            return val.HasValue && val.Value;
        }
    }
}

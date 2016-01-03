using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobCommon {
    /// <summary>
    /// equality comparer suitable for Linq
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EqComparer<T> : IEqualityComparer<T> {

        private Func<T, T, bool> equals;
        private Func<T, int> getHashCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public EqComparer(Func<T, T, bool> equals, Func<T, int> getHashCode) {
            this.equals = equals;
            this.getHashCode = getHashCode;
        }

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        /// <param name="x">The first object of type <paramref name="T"/> to compare.</param><param name="y">The second object of type <paramref name="T"/> to compare.</param>
        public bool Equals(T x, T y) {
            return this.equals(x, y);
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <returns>
        /// A hash code for the specified object.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param><exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
        public int GetHashCode(T obj) {
            return this.getHashCode(obj);
        }
    }
}

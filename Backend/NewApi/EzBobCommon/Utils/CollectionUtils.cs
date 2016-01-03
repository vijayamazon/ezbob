namespace EzBobCommon.Utils
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Contains methods to manipulate collections
    /// </summary>
    public class CollectionUtils
    {
        /// <summary>
        /// Determines whether the specified collection is empty.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">The collection.</param>
        /// <returns></returns>
        public static bool IsEmpty<T>(IEnumerable<T> collection) {
            return collection == null || !collection.Any();
        }

        /// <summary>
        /// Determines whether the specified collection is empty or not.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">The collection.</param>
        /// <returns></returns>
        public static bool IsNotEmpty<T>(IEnumerable<T> collection)
        {
            return !IsEmpty(collection);
        }

        /// <summary>
        /// Determines whether the specified collection is empty.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <returns></returns>
        public static bool IsEmpty(IEnumerable collection) {
            return collection == null || collection.GetEnumerator()
                .MoveNext();
        }

        /// <summary>
        /// Determines whether the specified collection is not empty.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <returns></returns>
        public static bool IsNotEmpty(IEnumerable collection) {
            return !IsEmpty(collection);
        }

        /// <summary>
        /// Gets the empty list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IList<T> GetEmptyList<T>() {
            return new List<T>();
        }
    }
}

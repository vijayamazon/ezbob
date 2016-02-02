using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobCommon {
    using System.Runtime.Caching;
    using EzBobCommon.Utils;

    public static class LinqExtensions {
        /// <summary>
        /// Divides enumerable into batches of same size.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> Batch<T>(
            this IEnumerable<T> source, int size) {
            T[] bucket = null;
            var count = 0;

            foreach (var item in source) {
                if (bucket == null)
                    bucket = new T[size];

                bucket[count++] = item;

                if (count != size)
                    continue;

                yield return bucket;

                bucket = null;
                count = 0;
            }

            // Return the last bucket with all remaining elements
            if (bucket != null && count > 0)
                yield return bucket.Take(count);
        }

        /// <summary>
        /// Applies provided action to each item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumeration">The enumeration.</param>
        /// <param name="action">The action.</param>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> enumeration, Action<T> action) {
            foreach (T item in enumeration) {
                action(item);
            }

            return enumeration;
        }

        public static Optional<T> AsOptional<T>(this T item) {
            return item;
        }

        /// <summary>
        /// Determines whether the collection is empty.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">The collection.</param>
        /// <returns></returns>
        public static bool IsEmpty<T>(this IEnumerable<T> collection) {
            return CollectionUtils.IsEmpty(collection);
        }

        /// <summary>
        ///Determines whether the collection is not empty.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">The collection.</param>
        /// <returns></returns>
        public static bool IsNotEmpty<T>(this IEnumerable<T> collection) {
            return CollectionUtils.IsNotEmpty(collection);
        }

        /// <summary>
        /// Returns enumerable consisting of a single item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public static IEnumerable<T> ToEnumerable<T>(this T item) {
            return Enumerable.Repeat(item, 1);
        } 
    }
}

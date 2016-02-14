using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobCommon {
    using System.Runtime.Caching;
    using Common.Logging.Configuration;
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

        /// <summary>
        /// Converts everything to optional.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public static Optional<T> AsOptional<T>(this T item) {
            return item;//implicit casting
        }

        /// <summary>
        /// Converts collection the to collection of optionals.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public static IEnumerable<Optional<T>> AsOptionals<T>(this IEnumerable<T> items) {
            if (items.IsEmpty()) {
                return Enumerable.Empty<Optional<T>>();
            }

            return items.Select(o => o.AsOptional());
        }

        /// <summary>
        /// Maps collection of optionals of type T, to collection of optionals of type V, by applying map() method to each optional of type T.
        /// </summary>
        /// <typeparam name="V">Mapped optional type.</typeparam>
        /// <typeparam name="T">Original optional type.</typeparam>
        /// <param name="optionals">The optionals.</param>
        /// <param name="map">The map method.</param>
        /// <returns>Enumerator of optionals.</returns>
        /// <exception cref="System.ArgumentException">Got empty 'map' method</exception>
        public static IEnumerable<Optional<V>> MapMany<V, T>(this IEnumerable<Optional<T>> optionals, Func<T, V> map) {

            if (map == null) {
                throw new ArgumentException("Got empty 'map' method");
            }
            
            if (optionals != null) {
                foreach (var optional in optionals) {
                    yield return optional.Map(map);
                }
            }
        }

        /// <summary>
        /// Call 'action()' method if finds at least one none-empty optional.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="optionals">The optionals.</param>
        /// <param name="action">The action.</param>
        /// <returns>original collection</returns>
        public static IEnumerable<Optional<T>> IfAnyNotEmpty<T>(this IEnumerable<Optional<T>> optionals, Action action) {
            if (optionals != null && optionals.Any(o => o.HasValue)) {
                action();
            }

            return optionals;
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
        /// Determines whether specified string is empty.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static bool IsEmpty(this string str) {
            return StringUtils.IsEmpty(str);
        }

        /// <summary>
        /// Determines whether specified string is not empty.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static bool IsNotEmpty(this string str) {
            return StringUtils.IsNotEmpty(str);
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

        /// <summary>
        /// Converts nullable to string, if nullable has no value returns null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nullable">The nullable.</param>
        /// <returns>null - if there is no value. Otherwise value.ToString()</returns>
        public static string AsString<T>(this T? nullable) where T : struct {
            return nullable.HasValue ? nullable.Value.ToString() : null;
        }

        /// <summary>
        /// Determines whether this instance is true.
        /// </summary>
        /// <param name="nullable">The nullable.</param>
        /// <returns></returns>
        public static bool IsTrue(this bool? nullable) {
            return nullable.HasValue && nullable.Value;
        }
    }
}

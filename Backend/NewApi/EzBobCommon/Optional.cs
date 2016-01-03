using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobCommon {
    using System.Collections;
    using System.Reflection;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Designates optionality.
    /// Implements IEnumerable to support Linq
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [JsonConverter(typeof(OptionalJsonSerializer))]
    public class Optional<T> : IEnumerable<T> {

        private static readonly Optional<T> emptyOptional = new Optional<T>();

        private readonly T item;
        private readonly bool hasValue = false;

        private Optional() {}

        private Optional(T val) {
            this.item = val;
            this.hasValue = true;
        }

        public bool HasValue
        {
            get { return this.hasValue; }
        }

        public T GetValue() {
            if (this.hasValue) {
                return this.item;
            }

            return default(T);
        }

        public static Optional<T> Of(T item) {

            if (default(T) == null && item == null) {
                return emptyOptional;
            }

            return new Optional<T>(item);
        }

        public static Optional<T> Empty() {
            return emptyOptional;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator() {
            if (this.hasValue) {
                yield return this.item;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="T"/> to <see cref="Optional{T}"/>.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Optional<T>(T obj) {
            return Of(obj);
        }
    }
}

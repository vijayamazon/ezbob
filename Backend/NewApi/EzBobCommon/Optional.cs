using System.Collections.Generic;

namespace EzBobCommon {
    using System;
    using System.Collections;
    using Newtonsoft.Json;

    /// <summary>
    /// Designates optionality.<br/>
    /// Implements IEnumerable to support Linq.<br/>
    /// Implements implicit casting to Optional.<br/>
    /// Implements explicit casting to T.<br/>
    /// Look also on <see cref="IfNotEmpty"/>, <see cref="IfEmpty"/> and <see cref="Map{V}"/>.<br/>
    /// There is an extension method <code>AsOptional()</code> to convert anything to optional
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

        /// <summary>
        /// Gets a value indicating whether this instance has value.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has value; otherwise, <c>false</c>.
        /// </value>
        public bool HasValue
        {
            get { return this.hasValue; }
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public T Value
        {
            get { return this.item; }
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <returns></returns>
        public T GetValue() {
            return this.item;
        }

        /// <summary>
        /// Creates optional from the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public static Optional<T> Of(T item) {

            if (default(T) == null && item == null) {
                return emptyOptional;
            }

            return new Optional<T>(item);
        }

        /// <summary>
        /// Returns an empty optional.
        /// </summary>
        /// <returns></returns>
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
        /// If not empty calls specified action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public Optional<T> IfNotEmpty(Action<T> action) {
            if (this.hasValue) {
                action(this.item);
            }

            return this;
        }

        /// <summary>
        /// If empty calls specified action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public Optional<T> IfEmpty(Action action) {
            if (!this.hasValue) {
                action();
            }

            return this;
        }

        /// <summary>
        /// Maps optional of T to optional of V by calling map method.
        /// <remarks>
        /// Empty optional of type T maps to empty optional of type V. (Without calling 'map' method)
        /// </remarks>
        /// </summary>
        /// <typeparam name="V">Type to map 'T' to</typeparam>
        /// <param name="map">The mapping method.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">got null map method.</exception>
        public Optional<V> Map<V>(Func<T, V> map) {

            if (map == null) {
                throw new ArgumentException("got null map method.");
            }

            if (this.hasValue) {
                return map(this.item);
            }

            return Optional<V>.Empty();
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

        /// <summary>
        /// Performs an explicit conversion from <see cref="Optional{T}"/> to <see cref="T"/>.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator T(Optional<T> item) {
            return item.Value;
        }
    }
}

namespace Ezbob.Integration.LogicalGlue.Models {
	using System.Collections;
	using System.Collections.Generic;
	using Ezbob.Integration.LogicalGlue.Interface;

	public class Inference : IInference {
		public Inference() {
			Data = new Dictionary<RequestType, IModelOutput>();
		} // constructor

		public Dictionary<RequestType, IModelOutput> Data { get; set; }

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<KeyValuePair<RequestType, IModelOutput>> GetEnumerator() {
			return Data.GetEnumerator();
		} // GetEnumerator

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		} // GetEnumerator

		/// <summary>
		/// Gets the number of elements in the collection.
		/// </summary>
		/// <returns>
		/// The number of elements in the collection. 
		/// </returns>
		public int Count {
			get { return Data.Count; }
		} // Count

		/// <summary>
		/// Determines whether the read-only dictionary contains an element that has the specified key.
		/// </summary>
		/// <returns>
		/// true if the read-only dictionary contains an element that has the specified key; otherwise, false.
		/// </returns>
		/// <param name="key">The key to locate.</param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception>
		public bool ContainsKey(RequestType key) {
			return Data.ContainsKey(key);
		} // ContainsKey

		/// <summary>
		/// Gets the value that is associated with the specified key.
		/// </summary>
		/// <returns>
		/// true if the object that implements the <see cref="T:System.Collections.Generic.IReadOnlyDictionary`2"/>
		/// interface contains an element that has the specified key; otherwise, false.
		/// </returns>
		/// <param name="key">The key to locate.</param>
		/// <param name="value">When this method returns, the value associated with the specified key, if the key is found;
		/// otherwise, the default value for the type of the <paramref name="value"/> parameter. This parameter is passed
		/// uninitialized.</param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception>
		public bool TryGetValue(RequestType key, out IModelOutput value) {
			return Data.TryGetValue(key, out value);
		} // TryGetValue

		/// <summary>
		/// Gets the element that has the specified key in the read-only dictionary.
		/// </summary>
		/// <returns>
		/// The element that has the specified key in the read-only dictionary.
		/// </returns>
		/// <param name="key">The key to locate.</param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception>
		/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">
		/// The property is retrieved and <paramref name="key"/> is not found.
		/// </exception>
		public IModelOutput this[RequestType key] {
			get { return Data[key]; }
		} // indexer

		/// <summary>
		/// Gets an enumerable collection that contains the keys in the read-only dictionary. 
		/// </summary>
		/// <returns>
		/// An enumerable collection that contains the keys in the read-only dictionary.
		/// </returns>
		public IEnumerable<RequestType> Keys {
			get { return Data.Keys; }
		} // Keys

		/// <summary>
		/// Gets an enumerable collection that contains the values in the read-only dictionary.
		/// </summary>
		/// <returns>
		/// An enumerable collection that contains the values in the read-only dictionary.
		/// </returns>
		public IEnumerable<IModelOutput> Values {
			get { return Data.Values; }
		} // Values
	} // class Inference
} // namespace

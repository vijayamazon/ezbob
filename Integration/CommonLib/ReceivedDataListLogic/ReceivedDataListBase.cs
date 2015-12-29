namespace EzBob.CommonLib.ReceivedDataListLogic {
	using System;
	using System.Collections;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Linq;

	public abstract class ReceivedDataListBase<T> : IReceivedDataList, IEnumerable<T> where T : class {
		public RequestsCounterData RequestsCounter { get; set; }

		public DateTime SubmittedDate { get; set; }

		public void IncrementRequests(string method = null, string details = null) {
			if (RequestsCounter == null)
				RequestsCounter = new RequestsCounterData();

			RequestsCounter.IncrementRequests(method, details);
		} // IncrementRequests

		public bool HasData {
			get { return Count > 0; }
		} // HasData

		public int Count {
			get { return this.data == null ? 0 : this.data.Count; }
		} // Count

		public void Add(T item) {
			if (item == null)
				return;

			this.data.Add(item);
		} // Add

		public void AddRange(IEnumerable<T> range) {
			if (range == null)
				return;

			range.ToList().ForEach(Add);
		} // AddRange

		public IEnumerator<T> GetEnumerator() {
			return this.data.GetEnumerator();
		} // GetEnumerator

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		} // IEnumerable.GetEnumerator

		protected readonly ConcurrentBag<T> data;

		protected ReceivedDataListBase(DateTime submittedDate, IEnumerable<T> collection = null) {
			this.data = new ConcurrentBag<T>();
			SubmittedDate = submittedDate;
			AddRange(collection);
		} // constructor
	} // class ReceivedDataListBase
} // namespace
namespace EzBob.CommonLib {
	using System;
	using System.Collections;
	using System.Collections.Concurrent;
	using System.Collections.Generic;

	public class RequestsCounterData : IEnumerable<RequestsCounterItem> {
		public RequestsCounterData(IEnumerable<RequestsCounterItem> requestsCounter = null) {
			this.data = new ConcurrentBag<RequestsCounterItem>();
			Add(requestsCounter);
		} // constructor

		public bool IsEmpty { get { return this.data.Count < 1; } } // IsEmpty

		public void IncrementRequests(string methodName = null, string details = null) {
			Add(new RequestsCounterItem(DateTime.UtcNow, methodName, details));
		} // IncrementRequests

		public IEnumerator<RequestsCounterItem> GetEnumerator() {
			return this.data.GetEnumerator();
		} // GetEnumerator

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		} // IEnumerable.GetEnumerator

		public void Add(IEnumerable<RequestsCounterItem> requestsCounter) {
			if (requestsCounter == null)
				return;

			foreach (var requestInfo in requestsCounter)
				Add(requestInfo);
		} // Add

		private void Add(RequestsCounterItem requestsCounter) {
			this.data.Add(requestsCounter);
		} // Add

		private readonly ConcurrentBag<RequestsCounterItem> data;
	} // class RequestsCounterData

	public class RequestsCounterItem {
		public RequestsCounterItem(DateTime created, string method, string details) {
			Created = created;
			Method = method;
			Details = details;
		} // constructor

		public DateTime Created { get; private set; }
		public string Method { get; private set; }
		public string Details { get; private set; }
	} // class RequestsCounterItem
} // namespace

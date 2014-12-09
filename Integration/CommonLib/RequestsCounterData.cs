using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace EzBob.CommonLib
{
	public class RequestsCounterData : IEnumerable<RequestsCounterItem>
	{
		private readonly ConcurrentBag<RequestsCounterItem> _Data = new ConcurrentBag<RequestsCounterItem>();

		public RequestsCounterData()
		{
		}

		public RequestsCounterData(IEnumerable<RequestsCounterItem> requestsCounter)
		{
			Add( requestsCounter );
		}

		public void IncrementRequests( string methodName = null, string details = null )
		{
			Add( new RequestsCounterItem( DateTime.UtcNow, methodName, details ) );
		}

		public IEnumerator<RequestsCounterItem> GetEnumerator()
		{
			return _Data.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add( IEnumerable<RequestsCounterItem> requestsCounter )
		{
			if ( requestsCounter == null || !requestsCounter.Any() )
			{
				return;
			}

			foreach (var requestInfo in requestsCounter)
			{
				Add( requestInfo );	
			}

		}

		private void Add(RequestsCounterItem requestsCounter)
		{
			_Data.Add( requestsCounter );
		}
	}

	public class RequestsCounterItem
	{
		public DateTime Created { get; private set; }
		public string Method { get; private set; }
		public string Details { get; private set; }

		public RequestsCounterItem( DateTime created, string method, string details )
		{
			Created = created;
			Method = method;
			Details = details;
		}

	}
}

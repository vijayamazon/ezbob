using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace EzBob.CommonLib.ReceivedDataListLogic
{
	public abstract class ReceivedDataListBase<T> : IReceivedDataList, IEnumerable<T> 
		where T : class
	{
		protected readonly ConcurrentBag<T> _Data = new ConcurrentBag<T>();

		protected ReceivedDataListBase(DateTime submittedDate, IEnumerable<T> collection = null)
		{
			SubmittedDate = submittedDate;

			AddRange( collection );

		}

		public RequestsCounterData RequestsCounter { get; set; }

		public DateTime SubmittedDate { get; set; }

		public void IncrementRequests( string method = null, string details = null )
		{
			if ( RequestsCounter == null )
			{
				RequestsCounter = new RequestsCounterData();
			}
			RequestsCounter.IncrementRequests(method, details);
		}

		public bool HasData
		{
			get { return Count > 0; }
		}

		public int Count
		{
			get { return _Data == null ? 0 : _Data.Count; }
		}

		public void Add(T item)
		{
			if ( item == null )
			{
				return;
			}
			_Data.Add( item );
		}

		public void AddRange( IEnumerable<T> data )
		{
			if ( data == null || !data.Any() )
			{
				return;
			}

			data.ToList().ForEach( Add );
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _Data.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
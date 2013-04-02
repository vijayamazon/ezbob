using System;
using System.Collections;
using System.Collections.Generic;

namespace EzBob.CommonLib
{
	public enum ElapsedDataMemberType
	{
		RetrieveDataFromExternalService,
		RetrieveDataFromDatabase,
		StoreDataToDatabase,
		AggregateData,
		StoreAggregatedData
	}

	public class ElapsedTimeInfo : IEnumerable<ElapsedDataMemberType>
	{
		private readonly Dictionary<ElapsedDataMemberType, long> _Data = new Dictionary<ElapsedDataMemberType, long>();

		public ElapsedTimeInfo()
		{
			foreach ( ElapsedDataMemberType type in Enum.GetValues( typeof( ElapsedDataMemberType ) ) )
			{
				_Data.Add( type, 0 );
			}
		}

		public void MergeData( ElapsedTimeInfo other )
		{
			foreach (var key in other)
			{
				IncreateData( key, other.GetValue( key ) );
			}
		}

		internal void IncreateData( ElapsedDataMemberType type, long value )
		{
			_Data[type] += value;
		}

		public long GetValue( ElapsedDataMemberType type )
		{
			return _Data[type];
		}

		public IEnumerator<ElapsedDataMemberType> GetEnumerator()
		{
			return _Data.Keys.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
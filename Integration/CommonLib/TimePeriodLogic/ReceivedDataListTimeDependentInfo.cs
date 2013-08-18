namespace EzBob.CommonLib.TimePeriodLogic
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using ReceivedDataListLogic;

	public class ReceivedDataListTimeDependentInfo<T>: IEnumerable<T>
		where T : class
	{
		public ReceivedDataListTimeDependentInfo( ReceivedDataListTimeDependentBase<T> data, TimePeriodEnum timePeriodType, int countMonths )
		{
			Data = data;
			CountMonths = countMonths;
			TimePeriodType = timePeriodType;
		}

		public ReceivedDataListTimeDependentBase<T> Data { get; private set; }
		public int CountMonths { get; private set; }
		public TimePeriodEnum TimePeriodType { get; private set; }

		public DateTime SubmittedDate 
		{
			get { return Data != null ? Data.SubmittedDate : DateTime.MinValue; }
		}

		public int CountData
		{
			get { return Data != null ? Data.Count : 0; }
		}

		public bool HasData
		{
			get { return Data != null && Data.HasData; }
		}

		public IEnumerator<T> GetEnumerator()
		{
			return Data.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public override string ToString()
		{
			return string.Format( "[{1}] {0} - {2}: {3}", CountData, SubmittedDate, TimePeriodType, Data );
		}
	}
}
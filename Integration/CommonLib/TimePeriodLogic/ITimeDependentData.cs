using System;

namespace EzBob.CommonLib.TimePeriodLogic
{
	public interface ITimeDependentData
	{
		DateTime RecordTime { get; }		
	}

	public interface ITimeRangedData
	{		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="fromDate">from date</param>
		/// <param name="toDate">to date</param>
		/// <returns>dates in requests interval</returns>
		bool InRange( DateTime fromDate, DateTime toDate );

		bool Include( DateTime fromDate, DateTime toDate );

		DateTime LeftBoundary { get; }

		DateTime RightBoundary { get; }
	}

	public abstract class TimeDependentRangedDataBase : ITimeRangedData, ITimeDependentData
	{
		public bool InRange(DateTime fromDate, DateTime toDate)
		{
			return ( fromDate == toDate && fromDate == RecordTime ) || ( fromDate <= LeftBoundary && RightBoundary <= toDate );
		}

		public bool Include(DateTime fromDate, DateTime toDate)
		{
			return InRange( fromDate, toDate );
			//return ( fromDate == toDate && fromDate == RecordTime ) || ( fromDate <= LeftBoundary && RightBoundary <= toDate );
		}

		public DateTime LeftBoundary
		{
			get { return RecordTime; }
		}

		public DateTime RightBoundary
		{
			get { return RecordTime; }
		}

		public abstract DateTime RecordTime { get; }

		public override string ToString()
		{
			return RecordTime.ToString();
		}
	}


}
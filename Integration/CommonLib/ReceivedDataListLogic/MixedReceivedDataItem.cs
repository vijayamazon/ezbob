using System;
using EzBob.CommonLib.TimePeriodLogic;

namespace EzBob.CommonLib.ReceivedDataListLogic
{
	public class MixedReceivedDataItem : ITimeRangedData
	{
		public MixedReceivedDataItem(ITimeRangedData data)
		{
			Data = data;
		}

		public ITimeRangedData Data { get; private set; }

		public bool InRange(DateTime fromDate, DateTime toDate)
		{
			return Data.InRange( fromDate, toDate );
		}

		public bool Include(DateTime fromDate, DateTime toDate)
		{
			return Data.Include( fromDate, toDate );
		}

		public DateTime LeftBoundary
		{
			get { return Data.LeftBoundary; }
		}

		public DateTime RightBoundary
		{
			get { return Data.RightBoundary; }
		}

		public override string ToString()
		{
			return string.Format( "[ {0} ; {1} ]", LeftBoundary, RightBoundary );
		}
	}
}
using System;

namespace EzBob.TeraPeakServiceLib.Requests.ResearchResult
{
	[Serializable]
	public class DateRangeInfo
	{
		private DateTime? _EndDate;
		private string _DateRange;

		public DateRangeInfo()
		{
		}

		public DateRangeInfo( DateTime? endDate )
		{
			_EndDate = endDate;
			_DateRange = "1";
		}

		public string EndDate
		{
			get { return _EndDate.HasValue ? _EndDate.Value.ToString("yyyy-MM-dd") : null; }
			set { }
		}

		public string DateRange
		{
			get { return _EndDate.HasValue? _DateRange: null; }
			set { _DateRange = value; }
		}
	}
}

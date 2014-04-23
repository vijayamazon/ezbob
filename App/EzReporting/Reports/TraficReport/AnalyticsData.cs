
namespace Reports.TraficReport
{
	using System;
	using Ezbob.Logger;
	using Ezbob.Database;

	public class AnalyticsData: AStoredProc
	{
		public AnalyticsData(AConnection oDB, ASafeLog oLog = null) : base(oDB, oLog)
		{
		}

		public DateTime DateStart { get; set; }
		public DateTime DateEnd { get; set; }

		public override bool HasValidParameters()
		{
			return true;
		}

		public class ResultRow : AResultRow
		{
			public string Source { get; set; }
			public int Visits { get; set; }
			public int Visitors { get; set; }
		} // class ResultRow

		protected override string GetName()
		{
			return "RptTraficReport_Analytics";
		}
	}
}

namespace Reports.TraficReport
{
	using Ezbob.Database;
	using System;
	using Ezbob.Logger;

	class CustomersData: AStoredProc
	{
		
		public CustomersData(AConnection oDB, ASafeLog oLog = null) : base(oDB, oLog)
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
			public int Customers { get; set; }
			public int Applications { get; set; }
			public int NumOfApproved { get; set; }
			public int NumOfRejected { get; set; }
			public int NumOfLoans { get; set; }
			public int LoanAmount { get; set; }
			public string ReferenceSource { get; set; }
			public string GoogleCookie { get; set; }
		} // class ResultRow

		protected override string GetName()
		{
			return "RptTraficReport_Customers";
		}
	}
}

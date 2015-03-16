namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;

	internal class LoanMetaData : AResultRow {
		public int CashRequestID { get; set; }
		public int LoanID { get; set; }
		public string LoanSourceName { get; set; }
		public DateTime LoanDate { get; set; }
		public decimal LoanAmount { get; set; }
		public string Status {
			get { return LoanStatus.ToString(); }
			set {
				LoanStatus ls;
				Enum.TryParse(value, true, out ls);
				LoanStatus = ls;
			} // set
		} // Status
		public decimal RepaidPrincipal { get; set; }

		public int MaxLateDays { get; set; }

		public LoanStatus LoanStatus { get; protected set; }
	} // class LoanMetaData
} // namespace

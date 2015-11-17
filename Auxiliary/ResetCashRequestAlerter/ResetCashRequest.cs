namespace ResetCashRequestAlerter {
	using System;
	using Ezbob.Database;

	class ResetCashRequest : AResultRow {
		public long CashRequestID { get; set; }
		public DateTime DecisionTime { get; set; }
		public int CustomerID { get; set; }
		public string CustomerName { get; set; }
		public string CustomerEmail { get; set; }
		public int UnderwriterID { get; set; }
		public string UnderwriterName { get; set; }

		public string DecisionName {
			get { return Decision.ToString(); }
			set { Decision = (DecisionAction)Enum.Parse(typeof(DecisionAction), value); }
		} // DecisionName

		public DecisionAction Decision { get; private set; } // Decision
	} // class ResetCashRequest
} // namespace

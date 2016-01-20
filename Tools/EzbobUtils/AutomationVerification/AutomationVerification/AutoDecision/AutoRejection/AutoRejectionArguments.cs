namespace AutomationCalculator.AutoDecision.AutoRejection {
	using System;
	using AutomationCalculator.AutoDecision.AutoRejection.Models;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class AutoRejectionArguments {
		public int CustomerID { get; set; }
		public int CompanyID { get; set; }
		public int MonthlyPayment { get; set; }

		public long? CashRequestID { get; set; }
		public long? NLCashRequestID { get; set; }
		public string Tag { get; set; }
		public DateTime Now { get; set; }

		public RejectionConfigs Configs { get; set; }

		public AConnection DB { get; set; }
		public ASafeLog Log { get; set; }
	} // class AutoRejectionArguments
} // namespace

namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	internal class SpLoadCashRequestsForAutomationReport : AStoredProc {
		public SpLoadCashRequestsForAutomationReport(AConnection db, ASafeLog log) : base(db, log) {
		} // constructor

		public override bool HasValidParameters() {
			return (CustomerID == null) || (CustomerID.Value > 0);
		} // HasValidParameters

		public int? CustomerID { get; set; }

		public DateTime? DateFrom { get; set; }

		public class ResultRow : AResultRow {
			public long CashRequestID { get; set; }
			public int CustomerID { get; set; }
			public int BrokerID { get; set; }
			public DateTime DecisionDate { get; set; }
			public bool IsApproved { get; set; }
			public int Amount { get; set; }
			public decimal InterestRate { get; set; }
			public int RepaymentPeriod { get; set; }
			public int UseSetupFee { get; set; }
			public bool UseBrokerSetupFee { get; set; }
			public decimal ManualSetupFeePercent { get; set; }
			public int ManualSetupFeeAmount { get; set; }
			public string MedalName { get; set; }
			public decimal EzbobScore { get; set; }
			public bool IsCampaign { get; set; }
			public int LoanCount { get; set; }
		} // class ResultRow
	} // class SpLoadCashRequestsForAutomationReport
} // namespace

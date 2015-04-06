namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class SpLoadCashRequestsForAutomationReport : AStoredProc {
		public SpLoadCashRequestsForAutomationReport(AConnection db, ASafeLog log) : base(db, log) {
		} // constructor

		public override bool HasValidParameters() {
			return (CustomerID == null) || (CustomerID.Value > 0);
		} // HasValidParameters

		public int? CustomerID { get; set; }

		public DateTime? DateFrom { get; set; }

		public DateTime? DateTo { get; set; }

		public class ResultRow : AResultRow {
			public long CashRequestID { get; set; }
			public int CustomerID { get; set; }
			public int? BrokerID { get; set; }
			public DateTime DecisionTime { get; set; }
			public bool IsApproved { get; set; }
			public int ApprovedAmount { get; set; }
			public decimal InterestRate { get; set; }
			public int RepaymentPeriod { get; set; }
			public int UseSetupFee { get; set; }
			public bool UseBrokerSetupFee { get; set; }
			public decimal? ManualSetupFeePercent { get; set; }
			public int? ManualSetupFeeAmount { get; set; }
			public string MedalName { get; set; }
			public decimal? EzbobScore { get; set; }
			public int PreviousLoanCount { get; set; }
			public int CrLoanCount { get; set; }
			public bool IsDefault { get; set; }

			protected internal virtual void CopyTo(ResultRow other) {
				if (other == null)
					return;

				other.CashRequestID = CashRequestID;
				other.CustomerID = CustomerID;
				other.BrokerID = BrokerID;
				other.DecisionTime = DecisionTime;
				other.IsApproved = IsApproved;
				other.ApprovedAmount = ApprovedAmount;
				other.InterestRate = InterestRate;
				other.RepaymentPeriod = RepaymentPeriod;
				other.UseSetupFee = UseSetupFee;
				other.UseBrokerSetupFee = UseBrokerSetupFee;
				other.ManualSetupFeePercent = ManualSetupFeeAmount;
				other.ManualSetupFeeAmount = ManualSetupFeeAmount;
				other.MedalName = MedalName;
				other.EzbobScore = EzbobScore;
				other.PreviousLoanCount = PreviousLoanCount;
				other.CrLoanCount = CrLoanCount;
				other.IsDefault = IsDefault;
			} // CopyTo
		} // class ResultRow
	} // class SpLoadCashRequestsForAutomationReport
} // namespace

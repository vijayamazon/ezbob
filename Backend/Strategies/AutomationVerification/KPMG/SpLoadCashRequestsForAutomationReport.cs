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

		public class ResultRow : AResultRow {
			public virtual long CashRequestID { get; set; }
			public virtual int CustomerID { get; set; }
			public virtual int? BrokerID { get; set; }
			public virtual DateTime DecisionTime { get; set; }
			public virtual bool IsApproved { get; set; }
			public virtual int ApprovedAmount { get; set; }
			public virtual decimal InterestRate { get; set; }
			public virtual int RepaymentPeriod { get; set; }
			public virtual int UseSetupFee { get; set; }
			public virtual bool UseBrokerSetupFee { get; set; }
			public virtual decimal? ManualSetupFeePercent { get; set; }
			public virtual int? ManualSetupFeeAmount { get; set; }
			public virtual string MedalName { get; set; }
			public virtual decimal? EzbobScore { get; set; }
			public virtual int LoanCount { get; set; }

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
				other.LoanCount = LoanCount;
			} // CopyTo
		} // class ResultRow
	} // class SpLoadCashRequestsForAutomationReport
} // namespace

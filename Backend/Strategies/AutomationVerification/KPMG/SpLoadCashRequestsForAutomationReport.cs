namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class SpLoadCashRequestsForAutomationReport : AStoredProc {
		public SpLoadCashRequestsForAutomationReport(AConnection db, ASafeLog log) : base(db, log) {
			this.requestedCustomers = new List<int>();
		} // constructor

		public override bool HasValidParameters() {
			return true;
		} // HasValidParameters

		public DateTime? DateFrom { get; set; }

		public DateTime? DateTo { get; set; }

		public List<int> RequestedCustomers {
			get { return this.requestedCustomers; }
			// Making the property read-only but with public set.
			// ReSharper disable once ValueParameterNotUsed
			set { }
		} // RequestedCustomers

		public class ResultRow : AResultRow {
			public long CashRequestID { get; set; }
			public long? NLCashRequestID { get; set; }
			public int CustomerID { get; set; }
			public int? BrokerID { get; set; }
			public DateTime DecisionTime { get; set; }
			public bool IsApproved { get; set; }

			public virtual int ApprovedAmount { get; set; }
			public virtual decimal InterestRate { get; set; }
			public virtual int RepaymentPeriod { get; set; }

			public int UseSetupFee { get; set; }
			public bool UseBrokerSetupFee { get; set; }
			public decimal? ManualSetupFeePercent { get; set; }
			public int? ManualSetupFeeAmount { get; set; }
			public string MedalName { get; set; }
			public decimal? EzbobScore { get; set; }
			public int PreviousLoanCount { get; set; }
			public int CrLoanCount { get; set; }
			public bool IsDefault { get; set; }
			public string LoanSourceName { get; set; }

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
				other.ManualSetupFeePercent = ManualSetupFeePercent;
				other.ManualSetupFeeAmount = ManualSetupFeeAmount;
				other.MedalName = MedalName;
				other.EzbobScore = EzbobScore;
				other.PreviousLoanCount = PreviousLoanCount;
				other.CrLoanCount = CrLoanCount;
				other.IsDefault = IsDefault;
				other.LoanSourceName = LoanSourceName;
			} // CopyTo
		} // class ResultRow

		private readonly List<int> requestedCustomers;
	} // class SpLoadCashRequestsForAutomationReport
} // namespace

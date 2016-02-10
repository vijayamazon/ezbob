namespace Ezbob.Backend.Strategies.MainStrategy.Helpers {
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Database;
	using Ezbob.Logger;

	internal class MainStrategySetApproved : AStoredProcedure {
		public MainStrategySetApproved(
			int customerID,
			long cashRequestID,
			bool overrideApprovedRejected,
			AutoDecisionResponse autoDecisionResponse,
			AConnection db,
			ASafeLog log
		) : base(db, log) {
			CustomerID = customerID;
			CashRequestID = cashRequestID;
			OverrideApprovedRejected = overrideApprovedRejected;
			this.autoDecisionResponse = autoDecisionResponse;
		} // constructor

		public override bool HasValidParameters() {
			return CustomerID > 0 && CashRequestID > 0;
		} // HasValidParameters

		public int CustomerID { get; set; }

		public long CashRequestID { get; set; }

		public bool OverrideApprovedRejected { get; set; }

		public string CreditResult {
			get {
				return this.autoDecisionResponse.CreditResult.HasValue
					? this.autoDecisionResponse.CreditResult.Value.ToString()
					: null;
			} // get
			set { }
		} // CreditResult

		public string CustomerStatus {
			get {
				return this.autoDecisionResponse.UserStatus.HasValue
					? this.autoDecisionResponse.UserStatus.Value.ToString()
					: null;
			} // get
			set { }
		} // CustomerStatus

		public string SystemDecision {
			get {
				return this.autoDecisionResponse.SystemDecision.HasValue
					? this.autoDecisionResponse.SystemDecision.Value.ToString()
					: null;
			} // get
			set { }
		} // SystemDecision

		private readonly AutoDecisionResponse autoDecisionResponse;
	} // class MainStrategySetApproved
} // namespace

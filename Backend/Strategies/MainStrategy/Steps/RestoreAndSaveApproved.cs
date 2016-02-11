namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using System;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Backend.Strategies.MainStrategy.Helpers;

	internal class RestoreAndSaveApproved : ASaveDecisionBase {
		public RestoreAndSaveApproved(
			string outerContextDescription,
			int underwriterID,
			int customerID,
			long cashRequestID,
			long nlCashRequestID,
			bool overrideApprovedRejected,
			int offerValidForHours,
			AutoDecisionResponse autoDecisionResponse,
			WriteDecisionOutput approvalToRestore
		) : base(
			outerContextDescription,
			underwriterID,
			cashRequestID,
			nlCashRequestID,
			offerValidForHours,
			autoDecisionResponse
		) {
			this.customerID = customerID;
			this.overrideApprovedRejected = overrideApprovedRejected;
			this.approvalToRestore = approvalToRestore;
			this.outcome = "'not executed'";
		} // constructor

		public override string Outcome { get { return this.outcome; } }

		protected override StepResults Run() {
			if (this.approvalToRestore == null) {
				Log.Alert(
					"Cannot restore approval status after finding investor for {0}: approval status is NULL.",
					OuterContextDescription
				);

				this.outcome = "'failed - no data'";
				return StepResults.Failed;
			} // if

			this.autoDecisionResponse.CreditResult = this.approvalToRestore.CreditResult;
			this.autoDecisionResponse.UserStatus = this.approvalToRestore.UserStatus;
			this.autoDecisionResponse.SystemDecision = this.approvalToRestore.SystemDecision;

			new MainStrategySetApproved(
				this.customerID,
				this.cashRequestID,
				this.overrideApprovedRejected,
				this.autoDecisionResponse,
				DB,
				Log
			).ExecuteNonQuery();

			AddNLDecisionOffer(DateTime.UtcNow);

			this.outcome = "'completed'";
			return StepResults.Success;
		} // Run

		private readonly WriteDecisionOutput approvalToRestore;
		private readonly bool overrideApprovedRejected;
		private readonly int customerID;

		private string outcome;
	} // class RestoreAndSaveApproved
} // namespace

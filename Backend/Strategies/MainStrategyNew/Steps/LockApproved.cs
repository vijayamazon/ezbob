namespace Ezbob.Backend.Strategies.MainStrategyNew.Steps {
	using DbConstants;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Backend.Strategies.OfferCalculation;
	using EZBob.DatabaseLib.Model.Database;

	internal class LockApproved : AMainStrategyStep {
		public LockApproved(
			string outerContextDescription,
			AutoDecisionResponse autoDecisionResponse,
			bool autoApproveIsSilent,
			OfferResult offerResult,
			int loanSourceID,
			bool isEmailSendingBanned
		) : base(outerContextDescription) {
			this.autoDecisionResponse = autoDecisionResponse;
			this.autoApproveIsSilent = autoApproveIsSilent;
			this.offerResult = offerResult;
			this.loanSourceID = loanSourceID;
			this.isEmailSendingBanned = isEmailSendingBanned;
		} // constructor

		protected override string Outcome { get { return this.outcome; } }

		protected override StepResults Run() {
			if (this.autoDecisionResponse.DecisionIsLocked) {
				this.outcome = "'already locked'";
				return StepResults.Success;
			} // if

			if (this.offerResult == null) {
				Log.Alert("No offer found for approved {0}, switching to manual.", OuterContextDescription);

				this.outcome = "'no offer'";
				return StepResults.Failed;
			} // if

			if (this.offerResult.IsMismatch) {
				Log.Alert("Mismatch in offer detected for approved {0}, switching to manual.", OuterContextDescription);

				this.outcome = "'mismatch in offer'";
				return StepResults.Failed;
			} // if

			if (this.offerResult.IsError) {
				Log.Alert("Error in offer detected for approved {0}, switching to manual.", OuterContextDescription);

				this.outcome = "'error in offer'";
				return StepResults.Failed;
			} // if

			if (this.loanSourceID <= 0) {
				Log.Alert("No loan source found for approved {0}, switching to manual.", OuterContextDescription);

				this.outcome = "'no loan source'";
				return StepResults.Failed;
			} // if

			this.autoDecisionResponse.DecisionIsLocked = true;
			this.autoDecisionResponse.HasApprovalChance = true;

			if (this.autoApproveIsSilent) {
				this.autoDecisionResponse.CreditResult = CreditResultStatus.WaitingForDecision;
				this.autoDecisionResponse.UserStatus = Status.Manual;
				this.autoDecisionResponse.SystemDecision = SystemDecision.Manual;

				this.outcome = "'silent approve'";
			} else {
				this.autoDecisionResponse.CreditResult = CreditResultStatus.Approved;
				this.autoDecisionResponse.UserStatus = Status.Approved;
				this.autoDecisionResponse.SystemDecision = SystemDecision.Approve;

				this.autoDecisionResponse.DecisionName = "Approval";
				this.autoDecisionResponse.Decision = DecisionActions.Approve;
				this.autoDecisionResponse.LoanOfferEmailSendingBannedNew = this.isEmailSendingBanned;

				this.autoDecisionResponse.RepaymentPeriod = this.offerResult.Period;
				this.autoDecisionResponse.LoanSourceID = this.loanSourceID;
				this.autoDecisionResponse.LoanTypeID = this.offerResult.LoanTypeId;
				this.autoDecisionResponse.InterestRate = this.offerResult.InterestRate / 100M;
				this.autoDecisionResponse.SetupFee = this.offerResult.SetupFee / 100M;
				
				this.outcome = "'success'";
			} // if

			return StepResults.Success;
		} // ExecuteStep

		private readonly AutoDecisionResponse autoDecisionResponse;
		private readonly bool autoApproveIsSilent;
		private readonly OfferResult offerResult;
		private readonly int loanSourceID;
		private readonly bool isEmailSendingBanned;

		private string outcome;
	} // class LockReapproved
} // namespace

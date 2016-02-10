namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using System;
	using DbConstants;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.ReApproval;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database;

	internal class LockReapproved : AMainStrategyStep {
		public LockReapproved(
			string outerContextDescription,
			AutoDecisionResponse autoDecisionResponse,
			AutoReapprovalOutput output
		) : base(outerContextDescription) {
			this.autoDecisionResponse = autoDecisionResponse;
			this.output = output;
		} // constructor

		public override string Outcome { get { return this.outcome; } }

		protected override StepResults Run() {
			if (this.autoDecisionResponse.DecisionIsLocked) {
				this.outcome = "'already locked'";
				return StepResults.Success;
			} // if

			if (!this.output.IsValid()) {
				Log.Debug("Cannot lock re-approval for {0}: {1}.", OuterContextDescription, this.output.Stringify());

				this.outcome = "'failure'";
				return StepResults.Failed;
			} // if

			SafeReader sr;

			try {
				sr = DB.GetFirst(
					"RapprovalGetLastApproveTerms",
					CommandSpecies.StoredProcedure,
					new QueryParameter("LacrID", this.output.LastApprovedCashRequestID)
				);
			} catch (Exception e) {
				Log.Alert(
					e,
					"Failed to load last approved terms for {0}, auto decision process aborted.",
					OuterContextDescription
				);

				this.outcome = "'failure'";
				return StepResults.Failed;
			} // try

			if (sr.IsEmpty) {
				Log.Alert(
					"Load last approved terms were not found for {0}, auto decision process aborted.",
					OuterContextDescription
				);

				this.outcome = "'failure'";
				return StepResults.Failed;
			} // if

			this.autoDecisionResponse.DecisionIsLocked = true;

			this.autoDecisionResponse.HasApprovalChance = true;
			this.autoDecisionResponse.ApprovedAmount = this.output.ApprovedAmount;
			this.autoDecisionResponse.Decision = DecisionActions.ReApprove;
			this.autoDecisionResponse.CreditResult = CreditResultStatus.Approved;
			this.autoDecisionResponse.UserStatus = Status.Approved;
			this.autoDecisionResponse.SystemDecision = SystemDecision.Approve;
			this.autoDecisionResponse.DecisionName = "Re-Approval";
			this.autoDecisionResponse.AppValidFor = this.output.AppValidFor;
			this.autoDecisionResponse.LoanOfferEmailSendingBannedNew = this.output.IsEmailSendingBanned;

			this.autoDecisionResponse.InterestRate = sr["InterestRate"];
			this.autoDecisionResponse.RepaymentPeriod = sr["RepaymentPeriod"];
			this.autoDecisionResponse.SetupFee = sr["ManualSetupFeePercent"];
			this.autoDecisionResponse.LoanTypeID = sr["LoanTypeID"];
			this.autoDecisionResponse.LoanSourceID = sr["LoanSourceID"];
			this.autoDecisionResponse.IsCustomerRepaymentPeriodSelectionAllowed =
				sr["IsCustomerRepaymentPeriodSelectionAllowed"];
			this.autoDecisionResponse.BrokerSetupFeePercent = sr["BrokerSetupFeePercent"];
			this.autoDecisionResponse.SpreadSetupFee = sr["SpreadSetupFee"];
			this.autoDecisionResponse.ProductSubTypeID = sr["ProductSubTypeID"];

			this.outcome = "'success'";
			return StepResults.Success;
		} // ExecuteStep

		private readonly AutoDecisionResponse autoDecisionResponse;
		private readonly AutoReapprovalOutput output;
		private string outcome;
	} // class LockReapproved
} // namespace

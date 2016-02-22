namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using System;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Backend.Strategies.MainStrategy.Helpers;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using EZBob.DatabaseLib.Model.Database;

	internal class WriteDecisionDown : ASaveDecisionBase {
		public WriteDecisionDown(
			string outerContextDescription,
			int underwriterID,
			int customerID,
			long cashRequestID,
			long nlCashRequestID,
			int offerValidForHours,
			AutoDecisionResponse autoDecisionResponse,
			MedalResult medal,
			bool overrideApprovedRejected,
			int experianConsumerScore
		) : base(
			outerContextDescription,
			underwriterID,
			cashRequestID,
			nlCashRequestID,
			offerValidForHours,
			autoDecisionResponse
		) {
			this.customerID = customerID;
			this.medal = medal;
			this.overrideApprovedRejected = overrideApprovedRejected;
			this.experianConsumerScore = experianConsumerScore;

			this.outcome = "'not executed'";
			WriteDecisionOutput = null;
			CashRequestWasWritten = false;
		} // constructor

		public override string Outcome { get { return this.outcome; } }

		[StepOutput]
		public bool CashRequestWasWritten { get; private set; }

		[StepOutput]
		public WriteDecisionOutput WriteDecisionOutput { get; private set; }

		protected override StepResults Run() {
			if (this.autoDecisionResponse == null) {
				Log.Alert("Cannot save decision for {0}: decision response is NULL.", OuterContextDescription);
				this.outcome = "'failed - decision response is NULL'";
				return StepResults.Failed;
			} // if

			if (this.medal == null) {
				Log.Alert("Cannot save decision for {0}: medal is NULL.", OuterContextDescription);
				this.outcome = "'failed - medal is NULL'";
				return StepResults.Failed;
			} // if

			StepResults result;

			if (this.autoDecisionResponse.DecidedToApprove) {
				WriteDecisionOutput = new WriteDecisionOutput {
					CreditResult = this.autoDecisionResponse.CreditResult,
					UserStatus = this.autoDecisionResponse.UserStatus,
					SystemDecision = this.autoDecisionResponse.SystemDecision,
				};

				this.autoDecisionResponse.CreditResult = CreditResultStatus.PendingInvestor;
				this.autoDecisionResponse.UserStatus = Status.Manual;
				this.autoDecisionResponse.SystemDecision = SystemDecision.Manual;

				this.outcome = "pending investor";
				result = StepResults.Approved;
			} else {
				this.outcome = this.autoDecisionResponse.DecidedToReject ? "rejected" : "manual";
				result = StepResults.RejectedManual;
			} // if

			DateTime now = DateTime.UtcNow;

			AddOldDecisionOffer(now);

			AddNLDecisionOffer(now);

			CashRequestWasWritten = true;

			this.outcome = string.Format("'completed - {0}'", this.outcome);
			return result;
		} // Run

		private void AddOldDecisionOffer(DateTime now) {
			var sp = new MainStrategyUpdateCrC(
				now,
				this.customerID,
				this.cashRequestID,
				this.autoDecisionResponse,
				DB,
				Log
			) {
				OverrideApprovedRejected = this.overrideApprovedRejected,
				MedalClassification = this.medal.MedalClassification.ToString(),
				OfferedCreditLine = this.autoDecisionResponse.ApprovedAmount,
				SystemCalculatedSum = this.medal.RoundOfferedAmount(),
				TotalScoreNormalized = this.medal.TotalScoreNormalized,
				ExperianConsumerScore = this.experianConsumerScore,
				AnnualTurnover = (int)this.medal.AnnualTurnover,
			};

			sp.ExecuteNonQuery();
		} // AddOldDecisionOffer

		private string outcome;

		private readonly int customerID;
		private readonly MedalResult medal;
		private readonly bool overrideApprovedRejected;
		private readonly int experianConsumerScore;
	} // class WriteDecisionDown
} // namespace

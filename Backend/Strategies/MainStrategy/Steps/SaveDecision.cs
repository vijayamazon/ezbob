namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using System;
	using DbConstants;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Backend.Strategies.MainStrategy.Helpers;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Backend.Strategies.NewLoan;
	using Ezbob.Utils.Extensions;

	internal class SaveDecision : AOneExitStep {
		public SaveDecision(
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
		) : base(outerContextDescription) {
			this.underwriterID = underwriterID;
			this.customerID = customerID;
			this.cashRequestID = cashRequestID;
			this.nlCashRequestID = nlCashRequestID;
			this.offerValidForHours = offerValidForHours;
			this.autoDecisionResponse = autoDecisionResponse;
			this.medal = medal;
			this.overrideApprovedRejected = overrideApprovedRejected;
			this.experianConsumerScore = experianConsumerScore;
		} // constructor

		protected override void ExecuteStep() {
			bool goodToGo = true;

			if (this.autoDecisionResponse == null) {
				Log.Alert("Cannot save decision for {0}: decision response is NULL.", OuterContextDescription);
				goodToGo = false;
			} // if

			if (this.medal == null) {
				Log.Alert("Cannot save decision for {0}: medal is NULL.", OuterContextDescription);
				goodToGo = false;
			} // if

			if (!goodToGo)
				return;

			DateTime now = DateTime.UtcNow;

			AddOldDecisionOffer(now);

			AddNLDecisionOffer(now);
		} // ExecuteStep

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

		private void AddNLDecisionOffer(DateTime now) {
			if (!this.autoDecisionResponse.HasAutoDecided)
				return;

			AddDecision addDecisionStra = new AddDecision(new NL_Decisions {
				DecisionNameID = this.autoDecisionResponse.DecisionCode ?? (int)DecisionActions.Waiting,
				DecisionTime = now,
				Notes = this.autoDecisionResponse.CreditResult.HasValue
					? this.autoDecisionResponse.CreditResult.Value.DescriptionAttr()
					: string.Empty,
				CashRequestID = this.nlCashRequestID,
				UserID = this.underwriterID,
			}, this.cashRequestID, null);

			addDecisionStra.Execute();
			long decisionID = addDecisionStra.DecisionID;

			Log.Debug("Added NL decision: {0}", decisionID);

			if (this.autoDecisionResponse.DecidedToApprove) {
				NL_OfferFees setupFee = new NL_OfferFees {
					LoanFeeTypeID = (int)NLFeeTypes.SetupFee,
					Percent = this.autoDecisionResponse.SetupFee,
					OneTimePartPercent = 1,
					DistributedPartPercent = 0
				};

				if (this.autoDecisionResponse.SpreadSetupFee) {
					setupFee.LoanFeeTypeID = (int)NLFeeTypes.ServicingFee;
					setupFee.OneTimePartPercent = 0;
					setupFee.DistributedPartPercent = 1;
				} // if

				NL_OfferFees[] ofeerFees = { setupFee };

				AddOffer addOfferStrategy = new AddOffer(new NL_Offers {
					DecisionID = decisionID,
					Amount = this.autoDecisionResponse.ApprovedAmount,
					StartTime = now,
					EndTime = now.AddHours(this.offerValidForHours),
					CreatedTime = now,
					DiscountPlanID = this.autoDecisionResponse.DiscountPlanIDToUse,
					LoanSourceID = this.autoDecisionResponse.LoanSource.ID,
					LoanTypeID = this.autoDecisionResponse.LoanTypeID,
					RepaymentIntervalTypeID = (int)RepaymentIntervalTypes.Month, 
					MonthlyInterestRate = this.autoDecisionResponse.InterestRate,
					RepaymentCount = this.autoDecisionResponse.RepaymentPeriod,
					BrokerSetupFeePercent = this.autoDecisionResponse.BrokerSetupFeePercent,
					IsLoanTypeSelectionAllowed = this.autoDecisionResponse.IsCustomerRepaymentPeriodSelectionAllowed,
					IsRepaymentPeriodSelectionAllowed = this.autoDecisionResponse.IsCustomerRepaymentPeriodSelectionAllowed,
					SendEmailNotification = !this.autoDecisionResponse.LoanOfferEmailSendingBannedNew,
					// ReSharper disable once PossibleInvalidOperationException
					Notes = "Auto decision: " + this.autoDecisionResponse.Decision.Value,
				}, ofeerFees);

				addOfferStrategy.Execute();

				Log.Debug("Added NL offer: {0}", addOfferStrategy.OfferID);
			} // if
		} // AddNLDecisionOffer

		private readonly int underwriterID;
		private readonly int customerID;
		private readonly long cashRequestID;
		private readonly long nlCashRequestID;
		private readonly int offerValidForHours;
		private readonly AutoDecisionResponse autoDecisionResponse;
		private readonly MedalResult medal;
		private readonly bool overrideApprovedRejected;
		private readonly int experianConsumerScore;
	} // class SaveDecision
} // namespace

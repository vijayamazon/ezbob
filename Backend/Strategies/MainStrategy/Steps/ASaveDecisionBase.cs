namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using System;
	using DbConstants;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Backend.Strategies.NewLoan;
	using Ezbob.Utils.Extensions;

	internal abstract class ASaveDecisionBase : AMainStrategyStep {
		protected ASaveDecisionBase(
			string outerContextDescription,
			int underwriterID,
			long cashRequestID,
			long nlCashRequestID,
			int offerValidForHours,
			AutoDecisionResponse autoDecisionResponse
		) : base(outerContextDescription) {
			this.underwriterID = underwriterID;
			this.cashRequestID = cashRequestID;
			this.nlCashRequestID = nlCashRequestID;
			this.offerValidForHours = offerValidForHours;
			this.autoDecisionResponse = autoDecisionResponse;
		} // constructor

		protected void AddNLDecisionOffer(DateTime now) {
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

			if (!this.autoDecisionResponse.DecidedToApprove)
				return;

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
		} // AddNLDecisionOffer

		protected readonly AutoDecisionResponse autoDecisionResponse;
		protected readonly long cashRequestID;

		private readonly int underwriterID;
		private readonly long nlCashRequestID;
		private readonly int offerValidForHours;
	} // class ASaveDecisionBase
} // namespace

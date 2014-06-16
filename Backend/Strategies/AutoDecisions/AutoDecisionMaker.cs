namespace EzBob.Backend.Strategies.AutoDecisions
{
	using Ezbob.Database;
	using Ezbob.Logger;

	public class AutoDecisionMaker
	{
		public static AutoDecisionResponse MakeDecision(
			int customerId,
			int minExperianScore,
			int maxExperianScore, 
			int maxCompanyScore, 
			double totalSumOfOrders1YTotalForRejection, 
			double totalSumOfOrders3MTotalForRejection,
			double yodlee1YForRejection,
			double yodlee3MForRejection,
			int offeredCreditLine, 
			double marketplaceSeniorityDays,
			bool enableAutomaticReRejection, 
			bool enableAutomaticRejection, 
			bool enableAutomaticReApproval, 
			bool enableAutomaticApproval, 
			decimal loanOfferReApprovalFullAmount, 
			decimal loanOfferReApprovalRemainingAmount,
			decimal loanOfferReApprovalFullAmountOld, 
			decimal loanOfferReApprovalRemainingAmountOld,
			bool customerStatusIsEnabled,
			bool customerStatusIsWarning, 
			bool isBrokerCustomer,
			bool isLimitedCompany,
			int companySeniorityDays,
			bool isOffline,
			string customerStatusName,
			AConnection oDb, ASafeLog oLog)
		{
			oLog.Info("Starting auto decision");
			var autoDecisionResponse = new AutoDecisionResponse();

			if (new ReRejection(customerId, enableAutomaticReRejection, oDb, oLog).MakeDecision(autoDecisionResponse))
			{
				return autoDecisionResponse;
			}

			if (new ReApproval(customerId, enableAutomaticReApproval, loanOfferReApprovalFullAmount, loanOfferReApprovalRemainingAmount, loanOfferReApprovalFullAmountOld, 
				loanOfferReApprovalRemainingAmountOld, oDb, oLog).MakeDecision(autoDecisionResponse))
			{
				return autoDecisionResponse;
			}

			if (new Approval(customerId, minExperianScore, offeredCreditLine, enableAutomaticApproval, oDb, oLog).MakeDecision(autoDecisionResponse))
			{
				return autoDecisionResponse;
			}

			if (new BankBasedApproval(customerId, oDb, oLog).MakeDecision(autoDecisionResponse))
			{
				return autoDecisionResponse;
			}

			var rejection = new Rejection(customerId, totalSumOfOrders1YTotalForRejection, totalSumOfOrders3MTotalForRejection,
			                              yodlee1YForRejection,
			                              yodlee3MForRejection, marketplaceSeniorityDays, enableAutomaticRejection,
			                              maxExperianScore, maxCompanyScore, customerStatusIsEnabled,
			                              customerStatusIsWarning, isBrokerCustomer, isLimitedCompany, companySeniorityDays,
										  isOffline, customerStatusName, oDb, oLog);
			var isRejected = rejection.MakeDecision(autoDecisionResponse);
			oLog.Debug(rejection.ToString());
			if (isRejected)
			{
				return autoDecisionResponse;
			}

			autoDecisionResponse.CreditResult = "WaitingForDecision";
			autoDecisionResponse.UserStatus = "Manual";
			autoDecisionResponse.SystemDecision = "Manual";

			return autoDecisionResponse;
		}
	}
}

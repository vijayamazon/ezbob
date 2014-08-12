namespace EzBob.Backend.Strategies.AutoDecisions
{
	using System;
	using System.Collections.Generic;
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
			List<string> consumerCaisDetailWorstStatuses,
			AConnection oDb,
			ASafeLog oLog)
		{
			oLog.Info("Starting auto decision");
			var autoDecisionResponse = new AutoDecisionResponse();
			string decisionName = "Manual";
			var conditions = new List<AutoDecisionCondition>();

			if (new ReRejection(customerId, enableAutomaticReRejection, oDb, oLog).MakeDecision(autoDecisionResponse))
			{
				decisionName = "Re-rejection";
			}
			else if (new ReApproval(customerId, enableAutomaticReApproval, loanOfferReApprovalFullAmount, loanOfferReApprovalRemainingAmount, loanOfferReApprovalFullAmountOld, 
				loanOfferReApprovalRemainingAmountOld, oDb, oLog).MakeDecision(autoDecisionResponse))
			{
				decisionName = "Re-Approval";
			}
			else if (new Approval(customerId, minExperianScore, offeredCreditLine, enableAutomaticApproval, consumerCaisDetailWorstStatuses, oDb, oLog).MakeDecision(autoDecisionResponse))
			{
				decisionName = "Approval";
			}
			else if (new BankBasedApproval(customerId, oDb, oLog).MakeDecision(autoDecisionResponse))
			{
				decisionName = "Bank Based Approval";
			}
			else
			{
				var rejection = new Rejection(conditions, customerId, totalSumOfOrders1YTotalForRejection,
				                              totalSumOfOrders3MTotalForRejection,
				                              yodlee1YForRejection,
				                              yodlee3MForRejection, marketplaceSeniorityDays, enableAutomaticRejection,
				                              maxExperianScore, maxCompanyScore, customerStatusIsEnabled,
				                              customerStatusIsWarning, isBrokerCustomer, isLimitedCompany, companySeniorityDays,
				                              isOffline, customerStatusName, oDb, oLog);
				var isRejected = rejection.MakeDecision(autoDecisionResponse);
				oLog.Debug(rejection.ToString());
				if (isRejected)
				{
					decisionName = "Rejection";
				}
			}

			if (decisionName == "Manual")
			{
				autoDecisionResponse.CreditResult = "WaitingForDecision";
				autoDecisionResponse.UserStatus = "Manual";
				autoDecisionResponse.SystemDecision = "Manual";
			}

			int decisionId = oDb.ExecuteScalar<int>(
				"AutoDecisionRecord",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("DecisionName", decisionName),
				new QueryParameter("Date", DateTime.UtcNow)
			);

			foreach (AutoDecisionCondition condition in conditions)
			{
				oDb.ExecuteNonQuery(
					"AutoDecisionConditionRecord",
					CommandSpecies.StoredProcedure,
					new QueryParameter("DecisionId", decisionId),
					new QueryParameter("DecisionName", condition.DecisionName),
					new QueryParameter("Satisfied", condition.Satisfied),
					new QueryParameter("Description", condition.Description)
				);
			}

			return autoDecisionResponse;
		}
	}
}

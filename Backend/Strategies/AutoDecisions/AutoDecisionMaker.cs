namespace EzBob.Backend.Strategies.AutoDecisions
{
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class AutoDecisionMaker
	{
		private readonly AConnection db;
		private readonly ASafeLog log;

		public AutoDecisionMaker(AConnection db, ASafeLog log)
		{
			this.db = db;
			this.log = log;
		}

		public AutoDecisionRejectionResponse MakeRejectionDecision(
			int customerId,
			int maxExperianScore,
			int maxCompanyScore, 
			double totalSumOfOrders1YTotalForRejection,
			double totalSumOfOrders3MTotalForRejection,
			double yodlee1YForRejection,
			double yodlee3MForRejection,
			double marketplaceSeniorityDays,
			bool enableAutomaticReRejection,
			bool enableAutomaticRejection,
			bool customerStatusIsEnabled,
			bool customerStatusIsWarning,
			bool isBrokerCustomer,
			bool isLimitedCompany,
			int companySeniorityDays,
			bool isOffline,
			string customerStatusName)
		{
			log.Info("Starting auto decision");
			var autoDecisionResponse = new AutoDecisionRejectionResponse();

			if (new ReRejection(customerId, enableAutomaticReRejection, db, log).MakeDecision(autoDecisionResponse))
			{
				autoDecisionResponse.DecisionName = "Re-rejection";
			}
			else
			{
				var rejection = new Rejection(customerId, totalSumOfOrders1YTotalForRejection,
											  totalSumOfOrders3MTotalForRejection,
											  yodlee1YForRejection,
											  yodlee3MForRejection, marketplaceSeniorityDays, enableAutomaticRejection,
											  maxExperianScore, maxCompanyScore, customerStatusIsEnabled,
											  customerStatusIsWarning, isBrokerCustomer, isLimitedCompany, companySeniorityDays,
											  isOffline, customerStatusName, db, log);
				var isRejected = rejection.MakeDecision(autoDecisionResponse);
				log.Debug(rejection.ToString());
				if (isRejected)
				{
					autoDecisionResponse.DecisionName = "Rejection";
				}
			}

			return autoDecisionResponse;
		}

		public AutoDecisionResponse MakeDecision(
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
			var autoDecisionResponse = new AutoDecisionResponse {DecisionName = "Manual"};

			if (new ReApproval(customerId, enableAutomaticReApproval, loanOfferReApprovalFullAmount, loanOfferReApprovalRemainingAmount, loanOfferReApprovalFullAmountOld, 
				loanOfferReApprovalRemainingAmountOld, oDb, oLog).MakeDecision(autoDecisionResponse))
			{
				autoDecisionResponse.DecisionName = "Re-Approval";
			}
			else if (new Approval(customerId, minExperianScore, offeredCreditLine, enableAutomaticApproval, consumerCaisDetailWorstStatuses, oDb, oLog).MakeDecision(autoDecisionResponse))
			{
				autoDecisionResponse.DecisionName = "Approval";
			}
			else if (new BankBasedApproval(customerId, oDb, oLog).MakeDecision(autoDecisionResponse))
			{
				autoDecisionResponse.DecisionName = "Bank Based Approval";
			}
			else
			{
				autoDecisionResponse.CreditResult = "WaitingForDecision";
				autoDecisionResponse.UserStatus = "Manual";
				autoDecisionResponse.SystemDecision = "Manual";
			}

			return autoDecisionResponse;
		}

		public void LogDecision(int customerId, AutoDecisionRejectionResponse autoDecisionRejectionResponse, AutoDecisionResponse autoDecisionResponse)
		{
			string decisionName = autoDecisionRejectionResponse.DecidedToReject ? autoDecisionRejectionResponse.DecisionName : autoDecisionResponse.DecisionName;

			int decisionId = db.ExecuteScalar<int>(
				"AutoDecisionRecord",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("DecisionName", decisionName),
				new QueryParameter("Date", DateTime.UtcNow)
			);

			foreach (AutoDecisionCondition condition in autoDecisionRejectionResponse.RejectionConditions)
			{
				db.ExecuteNonQuery(
					"AutoDecisionConditionRecord",
					CommandSpecies.StoredProcedure,
					new QueryParameter("DecisionId", decisionId),
					new QueryParameter("DecisionName", condition.DecisionName),
					new QueryParameter("Satisfied", condition.Satisfied),
					new QueryParameter("Description", condition.Description)
				);
			}
		}
	}
}

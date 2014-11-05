namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions
{
	using System.Collections.Generic;
	using EzBob.Models;
	using Ezbob.Database;
	using System;
	using Ezbob.Logger;
	using Misc;

	public class Approval
	{
		private readonly StrategyHelper strategyHelper = new StrategyHelper();
		private readonly AConnection db;
		private readonly int minExperianScore;
		private readonly int minCompanyScore;
		private readonly int offeredCreditLine;
		private readonly ASafeLog log;
		private readonly int customerId;
		private readonly List<string> consumerCaisDetailWorstStatuses;

		public Approval(int customerId, int minExperianScore, int minCompanyScore, int offeredCreditLine, List<string> consumerCaisDetailWorstStatuses, AConnection db, ASafeLog log)
		{
			this.db = db;
			this.log = log;
			this.minExperianScore = minExperianScore;
			this.minCompanyScore = minCompanyScore;
			this.offeredCreditLine = offeredCreditLine;
			this.customerId = customerId;
			this.consumerCaisDetailWorstStatuses = consumerCaisDetailWorstStatuses;
		}

		public bool MakeDecision(AutoDecisionResponse response)
		{
			try
			{
				var configSafeReader = db.GetFirst("GetApprovalConfigs", CommandSpecies.StoredProcedure);

				bool autoApproveIsSilent = configSafeReader["AutoApproveIsSilent"];
				string autoApproveSilentTemplateName = configSafeReader["AutoApproveSilentTemplateName"];
				string autoApproveSilentToAddress = configSafeReader["AutoApproveSilentToAddress"];
				decimal minLoanAmount = configSafeReader["MinLoanAmount"];
				
				var instance = new GetAvailableFunds(db, log);
				instance.Execute();
				decimal availableFunds = instance.AvailableFunds;

				response.AutoApproveAmount = strategyHelper.AutoApproveCheck(customerId, offeredCreditLine, minExperianScore, instance.ReservedAmount, consumerCaisDetailWorstStatuses);
				response.AutoApproveAmount = (int)(Math.Round(response.AutoApproveAmount / minLoanAmount, 0, MidpointRounding.AwayFromZero) * minLoanAmount);
				log.Info("Decided to auto approve rounded amount:{0}", response.AutoApproveAmount);

				if (response.AutoApproveAmount != 0)
				{
					if (availableFunds > response.AutoApproveAmount)
					{
						if (autoApproveIsSilent)
						{
							strategyHelper.NotifyAutoApproveSilentMode(customerId, response.AutoApproveAmount, autoApproveSilentTemplateName, autoApproveSilentToAddress);

							response.CreditResult = "WaitingForDecision";
							response.UserStatus = "Manual";
							response.SystemDecision = "Manual";
						}
						else
						{
							SafeReader sr = db.GetFirst(
								"GetLastOfferDataForApproval",
								CommandSpecies.StoredProcedure,
								new QueryParameter("CustomerId", customerId),
								new QueryParameter("Now", DateTime.UtcNow)
							);

							bool loanOfferEmailSendingBanned = sr["EmailSendingBanned"];
							DateTime loanOfferOfferStart = sr["OfferStart"];
							DateTime loanOfferOfferValidUntil = sr["OfferValidUntil"];

							response.CreditResult = "Approved";
							response.UserStatus = "Approved";
							response.SystemDecision = "Approve";
							response.LoanOfferUnderwriterComment = "Auto Approval";
							response.DecisionName = "Approval";
							response.AppValidFor = DateTime.UtcNow.AddDays((loanOfferOfferValidUntil - loanOfferOfferStart).TotalDays);
							response.IsAutoApproval = true;
							response.LoanOfferEmailSendingBannedNew = loanOfferEmailSendingBanned;
						}
					}
					else
					{
						response.CreditResult = "WaitingForDecision";
						response.UserStatus = "Manual";
						response.SystemDecision = "Manual";
					}

					return true;
				}

				return false;
			}
			catch (Exception e)
			{
				log.Error("Exception during approval:{0}", e);
				return false;
			}
		}
	}
}

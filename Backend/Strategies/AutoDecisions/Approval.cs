namespace EzBob.Backend.Strategies.AutoDecisions
{
	using System.Collections.Generic;
	using EzBob.Models;
	using Ezbob.Database;
	using System;
	using System.Data;
	using Ezbob.Logger;
	using Misc;

	public class Approval
	{
		private readonly StrategyHelper strategyHelper = new StrategyHelper();
		private readonly AConnection db;
		private readonly int minExperianScore;
		private readonly int offeredCreditLine;
		private readonly ASafeLog log;
		private readonly bool enableAutomaticApproval;
		private readonly int customerId;
		private readonly List<string> consumerCaisDetailWorstStatuses;

		public Approval(int customerId, int minExperianScore, int offeredCreditLine, bool enableAutomaticApproval, List<string> consumerCaisDetailWorstStatuses, AConnection db, ASafeLog log)
		{
			this.db = db;
			this.log = log;
			this.minExperianScore = minExperianScore;
			this.offeredCreditLine = offeredCreditLine;
			this.enableAutomaticApproval = enableAutomaticApproval;
			this.customerId = customerId;
			this.consumerCaisDetailWorstStatuses = consumerCaisDetailWorstStatuses;
		}

		public bool MakeDecision(AutoDecisionResponse response)
		{
			try
			{
				DataTable configsDataTable = db.ExecuteReader("GetApprovalConfigs", CommandSpecies.StoredProcedure);
				var configSafeReader = new SafeReader(configsDataTable.Rows[0]);

				bool autoApproveIsSilent = configSafeReader["AutoApproveIsSilent"];
				string autoApproveSilentTemplateName = configSafeReader["AutoApproveSilentTemplateName"];
				string autoApproveSilentToAddress = configSafeReader["AutoApproveSilentToAddress"];
				decimal minLoanAmount = configSafeReader["MinLoanAmount"];
				if (enableAutomaticApproval)
				{
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
								DataTable dt = db.ExecuteReader(
									"GetLastOfferDataForApproval",
									CommandSpecies.StoredProcedure,
									new QueryParameter("CustomerId", customerId),
									new QueryParameter("Now", DateTime.UtcNow)
									);

								var sr = new SafeReader(dt.Rows[0]);
								bool loanOfferEmailSendingBanned = sr["EmailSendingBanned"];
								DateTime loanOfferOfferStart = sr["OfferStart"];
								DateTime loanOfferOfferValidUntil = sr["OfferValidUntil"];

								response.CreditResult = "Approved";
								response.UserStatus = "Approved";
								response.SystemDecision = "Approve";
								response.LoanOfferUnderwriterComment = "Auto Approval";
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

namespace EzBob.Backend.Strategies.AutoDecisions
{
	using EzBob.Models;
	using Ezbob.Database;
	using System;
	using System.Data;
	using Ezbob.Logger;

	public class Approval
	{
		private readonly StrategyHelper strategyHelper = new StrategyHelper();
		private readonly AConnection Db;
		private readonly int minExperianScore;
		private readonly int offeredCreditLine;
		private readonly ASafeLog log;
		private readonly bool enableAutomaticApproval;
		private readonly int customerId;

		public Approval(int customerId, int minExperianScore, int offeredCreditLine, bool enableAutomaticApproval, AConnection oDb, ASafeLog oLog)
		{
			Db = oDb;
			log = oLog;
			this.minExperianScore = minExperianScore;
			this.offeredCreditLine = offeredCreditLine;
			this.enableAutomaticApproval = enableAutomaticApproval;
			this.customerId = customerId;
		} // constructor

		public bool MakeDecision(AutoDecisionResponse response)
		{
			try
			{
				DataTable configsDataTable = Db.ExecuteReader("GetApprovalConfigs", CommandSpecies.StoredProcedure);
				var configSafeReader = new SafeReader(configsDataTable.Rows[0]);

				bool autoApproveIsSilent = configSafeReader["AutoApproveIsSilent"];
				string autoApproveSilentTemplateName = configSafeReader["AutoApproveSilentTemplateName"];
				string autoApproveSilentToAddress = configSafeReader["AutoApproveSilentToAddress"];
				if (enableAutomaticApproval)
				{
					response.AutoApproveAmount = strategyHelper.AutoApproveCheck(customerId, offeredCreditLine, minExperianScore);

					if (response.AutoApproveAmount != 0)
					{
						DataTable dt = Db.ExecuteReader("GetAvailableFunds", CommandSpecies.StoredProcedure);
						var sr = new SafeReader(dt.Rows[0]);
						decimal availableFunds = sr["AvailableFunds"];

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
								dt = Db.ExecuteReader(
									"GetLastOfferDataForApproval",
									CommandSpecies.StoredProcedure,
									new QueryParameter("CustomerId", customerId),
									new QueryParameter("Now", DateTime.UtcNow)
									);

								sr = new SafeReader(dt.Rows[0]);
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

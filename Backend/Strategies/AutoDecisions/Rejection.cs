namespace EzBob.Backend.Strategies.AutoDecisions
{
	using System;
	using Ezbob.Database;
	using System.Data;
	using Ezbob.Logger;

	public class Rejection
	{
		private int autoRejectionExceptionAnualTurnover;
		private int rejectDefaultsCreditScore;
		private int rejectMinimalSeniority;
		private int lowCreditScore;
		private int rejectDefaultsAccountsNum;
		private int autoRejectionExceptionCreditScore;
		private bool hasAccountingAccounts;
		private int errorMPsNum;
		private int loanOfferApprovalNum;
		private int numOfDefaultAccounts;
		private readonly AConnection Db;
		private readonly ASafeLog log;
		private readonly double totalSumOfOrders1YTotal;
		private readonly double totalSumOfOrders3MTotal;
		private readonly double marketplaceSeniorityDays;
		private readonly bool enableAutomaticRejection;
		private int lowTotalAnnualTurnover;
		private int lowTotalThreeMonthTurnover;
		private readonly double maxExperianConsumerScore;
		private readonly int customerId;

		public Rejection(int customerId, double totalSumOfOrders1YTotal, double totalSumOfOrders3MTotal, double marketplaceSeniorityDays, bool enableAutomaticRejection, double maxExperianConsumerScore, AConnection oDb, ASafeLog oLog)
		{
			Db = oDb;
			log = oLog;
			this.totalSumOfOrders1YTotal = totalSumOfOrders1YTotal;
			this.totalSumOfOrders3MTotal = totalSumOfOrders3MTotal;
			this.marketplaceSeniorityDays = marketplaceSeniorityDays;
			this.enableAutomaticRejection = enableAutomaticRejection;
			this.maxExperianConsumerScore = maxExperianConsumerScore;
			this.customerId = customerId;
		}

		private bool IsException()
		{
			if (loanOfferApprovalNum > 0 || totalSumOfOrders1YTotal > autoRejectionExceptionAnualTurnover ||
				maxExperianConsumerScore > autoRejectionExceptionCreditScore || errorMPsNum > 0 ||
				(decimal)maxExperianConsumerScore == 0 || hasAccountingAccounts)
			{
				return true;
			}

			return false;
		}

		private void Init()
		{

			DataTable dt = Db.ExecuteReader("GetRejectionConfigs", CommandSpecies.StoredProcedure);
			var sr = new SafeReader(dt.Rows[0]);

			autoRejectionExceptionAnualTurnover = sr["AutoRejectionException_AnualTurnover"];
			rejectDefaultsCreditScore = sr["Reject_Defaults_CreditScore"];
			rejectMinimalSeniority = sr["Reject_Minimal_Seniority"];
			lowCreditScore = sr["LowCreditScore"];
			rejectDefaultsAccountsNum = sr["Reject_Defaults_AccountsNum"];
			autoRejectionExceptionCreditScore = sr["AutoRejectionException_CreditScore"];
			int rejectDefaultsMonths = sr["Reject_Defaults_MonthsNum"];
			int rejectDefaultsAmount = sr["Reject_Defaults_Amount"];
			lowTotalAnnualTurnover = sr["LowTotalAnnualTurnover"];
			lowTotalThreeMonthTurnover = sr["LowTotalThreeMonthTurnover"];

			dt = Db.ExecuteReader(
				"GetCustomerRejectionData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("Reject_Defaults_Months", rejectDefaultsMonths),
				new QueryParameter("Reject_Defaults_Amount", rejectDefaultsAmount)
			);

			sr = new SafeReader(dt.Rows[0]);

			hasAccountingAccounts = sr["HasAccountingAccounts"];
			errorMPsNum = sr["ErrorMPsNum"];
			loanOfferApprovalNum = sr["ApprovalNum"];
			numOfDefaultAccounts = sr["NumOfDefaultAccounts"];
		}

		public bool MakeDecision(AutoDecisionResponse response)
		{
			try
			{
				Init();
				if (IsException())
					return false;

				FillPayPalFiguresForExplanationMail(Db, customerId, response);

				if (maxExperianConsumerScore < rejectDefaultsCreditScore &&
				    numOfDefaultAccounts >= rejectDefaultsAccountsNum)
				{
					response.AutoRejectReason = "AutoReject: Score & DefaultAccountsNum. Condition not met:" +
					                            maxExperianConsumerScore +
					                            " < " + rejectDefaultsCreditScore + " AND " + numOfDefaultAccounts + " >= " +
					                            rejectDefaultsAccountsNum;
				}
				else if (maxExperianConsumerScore < lowCreditScore)
				{
					response.AutoRejectReason = "AutoReject: Low score. Condition not met:" + maxExperianConsumerScore + " < " +
					                            lowCreditScore;
				}
				else if (
					(response.PayPalNumberOfStores == 0 ||
					 response.PayPalTotalSumOfOrders3M < lowTotalThreeMonthTurnover ||
					 response.PayPalTotalSumOfOrders1Y < lowTotalAnnualTurnover)
					&&
					(totalSumOfOrders3MTotal < lowTotalThreeMonthTurnover || totalSumOfOrders1YTotal < lowTotalAnnualTurnover)
					)
				{
					response.AutoRejectReason = "AutoReject: Totals. Condition not met: (" + response.PayPalNumberOfStores + " < 0 OR " +
					                            response.PayPalTotalSumOfOrders3M + " < " +
					                            lowTotalThreeMonthTurnover + " OR " + response.PayPalTotalSumOfOrders1Y + " < " +
					                            lowTotalAnnualTurnover + ") AND (" + totalSumOfOrders3MTotal + " < " +
					                            lowTotalThreeMonthTurnover + " OR " + totalSumOfOrders1YTotal + " < " +
					                            lowTotalAnnualTurnover + ")";
				}
				else if (marketplaceSeniorityDays < rejectMinimalSeniority && errorMPsNum == 0)
				{
					response.AutoRejectReason = "AutoReject: Seniority. Condition not met: (" + marketplaceSeniorityDays + " < " +
					                            rejectMinimalSeniority + ")";
				}
				else
				{
					return false;
				}

				response.CreditResult = enableAutomaticRejection ? "Rejected" : "WaitingForDecision";
				response.UserStatus = "Rejected";
				response.SystemDecision = "Reject";

				return true;
			}
			catch (Exception e)
			{
				log.Error("Exception during rejection:{0}", e);
				return false;
			}
		}

		public static void FillPayPalFiguresForExplanationMail(AConnection db, int customerId, AutoDecisionResponse response)
		{
			DataTable dt = db.ExecuteReader(
				"GetPayPalAggregations",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
				);

			var sr = new SafeReader(dt.Rows[0]);

			response.PayPalNumberOfStores = sr["PayPal_NumberOfStores"];
			response.PayPalTotalSumOfOrders3M = sr["PayPal_TotalSumOfOrders3M"];
			response.PayPalTotalSumOfOrders1Y = sr["PayPal_TotalSumOfOrders1Y"];
		}
	}
}

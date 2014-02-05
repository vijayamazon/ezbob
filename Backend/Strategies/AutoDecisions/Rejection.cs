namespace EzBob.Backend.Strategies.AutoDecisions
{
	using Ezbob.Database;
	using System.Data;
	using Ezbob.Logger;

	public class Rejection
	{
		private readonly int autoRejectionExceptionAnualTurnover;
		private readonly int rejectDefaultsCreditScore;
		private readonly int rejectMinimalSeniority;
		private readonly int lowCreditScore;
		private readonly int rejectDefaultsAccountsNum;
		private readonly int autoRejectionExceptionCreditScore;
		private readonly bool hasAccountingAccounts;
		private readonly int errorMPsNum;
		private readonly int loanOfferApprovalNum;
		private readonly int numOfDefaultAccounts;
		private readonly AConnection Db;
		private readonly ASafeLog log;
		private readonly double totalSumOfOrders1YTotal;
		private readonly double totalSumOfOrders3MTotal;
		private readonly double marketplaceSeniorityDays;
		private readonly bool enableAutomaticRejection;
		private readonly int lowTotalAnnualTurnover;
		private readonly int lowTotalThreeMonthTurnover;
		private readonly double initialExperianConsumerScore;
		private readonly int customerId;

		public Rejection(int customerId, double totalSumOfOrders1YTotal, double totalSumOfOrders3MTotal, double marketplaceSeniorityDays, bool enableAutomaticRejection, double initialExperianConsumerScore, AConnection oDb, ASafeLog oLog)
		{
			Db = oDb;
			log = oLog;
			this.totalSumOfOrders1YTotal = totalSumOfOrders1YTotal;
			this.totalSumOfOrders3MTotal = totalSumOfOrders3MTotal;
			this.marketplaceSeniorityDays = marketplaceSeniorityDays;
			this.enableAutomaticRejection = enableAutomaticRejection;
			this.initialExperianConsumerScore = initialExperianConsumerScore;
			this.customerId = customerId;
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

		private bool IsException()
		{
			if (loanOfferApprovalNum > 0 || totalSumOfOrders1YTotal > autoRejectionExceptionAnualTurnover ||
				initialExperianConsumerScore > autoRejectionExceptionCreditScore || errorMPsNum > 0 ||
				(decimal)initialExperianConsumerScore == 0 || hasAccountingAccounts)
			{
				return true;
			}

			return false;
		}

		public bool MakeDecision(AutoDecisionResponse response)
		{
			if (IsException())
				return false;

			DataTable dt = Db.ExecuteReader(
				"GetPayPalAggregations",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);

			var sr = new SafeReader(dt.Rows[0]);

			response.PayPalNumberOfStores = sr["PayPal_NumberOfStores"];
			response.PayPalTotalSumOfOrders3M = sr["PayPal_TotalSumOfOrders3M"];
			response.PayPalTotalSumOfOrders1Y = sr["PayPal_TotalSumOfOrders1Y"];

			if (initialExperianConsumerScore < rejectDefaultsCreditScore &&
				numOfDefaultAccounts >= rejectDefaultsAccountsNum)
			{
				response.AutoRejectReason = "AutoReject: Score & DefaultAccountsNum. Condition not met:" + initialExperianConsumerScore +
								   " < " + rejectDefaultsCreditScore + " AND " + numOfDefaultAccounts + " >= " +
								   rejectDefaultsAccountsNum;
			}
			else if (initialExperianConsumerScore < lowCreditScore)
			{
				response.AutoRejectReason = "AutoReject: Low score. Condition not met:" + initialExperianConsumerScore + " < " +
								   lowCreditScore;
			}
			else if (
				(response.PayPalNumberOfStores == 0 ||
				 response.PayPalTotalSumOfOrders3M < lowTotalThreeMonthTurnover || response.PayPalTotalSumOfOrders1Y < lowTotalAnnualTurnover)
				 &&
				(totalSumOfOrders3MTotal < lowTotalThreeMonthTurnover || totalSumOfOrders1YTotal < lowTotalAnnualTurnover)
			   )
			{
				response.AutoRejectReason = "AutoReject: Totals. Condition not met: (" + response.PayPalNumberOfStores + " < 0 OR" +
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

			response.CreditResult = enableAutomaticRejection ? "WaitingForDecision" : "Rejected";
			response.UserStatus = "Rejected";
			response.SystemDecision = "Reject";

			return true;
		}
	}
}

﻿namespace EzBob.Backend.Strategies.AutoDecisions
{
	using System;
	using ConfigManager;
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
		private int errorMPsNum;
		private int loanOfferApprovalNum;
		private int numOfDefaultAccounts;
		private int numOfDefaultAccountsForCompany;
		private readonly AConnection db;
		private readonly ASafeLog log;
		private readonly double totalSumOfOrders1YTotalForRejection;
		private readonly double totalSumOfOrders3MTotalForRejection;
		private readonly double marketplaceSeniorityDays;
		private readonly bool enableAutomaticRejection;
		private int lowTotalAnnualTurnover;
		private int lowTotalThreeMonthTurnover;
		private readonly double maxExperianConsumerScore;
		private readonly int customerId;
		private readonly int maxCompanyScore;
		private readonly bool customerStatusIsEnabled;
		private readonly bool customerStatusIsWarning;
		private readonly bool isBrokerCustomer;
		private readonly bool isLimitedCompany;

		public Rejection(int customerId, double totalSumOfOrders1YTotalForRejection, double totalSumOfOrders3MTotalForRejection, double marketplaceSeniorityDays, bool enableAutomaticRejection, double maxExperianConsumerScore, int maxCompanyScore, bool customerStatusIsEnabled,
				bool customerStatusIsWarning, bool isBrokerCustomer, bool isLimitedCompany, AConnection oDb, ASafeLog oLog)
		{
			db = oDb;
			log = oLog;
			this.totalSumOfOrders1YTotalForRejection = totalSumOfOrders1YTotalForRejection;
			this.totalSumOfOrders3MTotalForRejection = totalSumOfOrders3MTotalForRejection;
			this.marketplaceSeniorityDays = marketplaceSeniorityDays;
			this.enableAutomaticRejection = enableAutomaticRejection;
			this.maxExperianConsumerScore = maxExperianConsumerScore;
			this.customerId = customerId;
			this.maxCompanyScore = maxCompanyScore;
			this.customerStatusIsEnabled = customerStatusIsEnabled;
			this.customerStatusIsWarning = customerStatusIsWarning;
			this.isBrokerCustomer = isBrokerCustomer;
			this.isLimitedCompany = isLimitedCompany;
		}

		private bool IsException()
		{
			if (loanOfferApprovalNum > 0 && customerStatusIsEnabled && !customerStatusIsWarning)
			{
				return true;
			}
			if (totalSumOfOrders1YTotalForRejection > autoRejectionExceptionAnualTurnover)
			{
				return true;
			}
			if (maxExperianConsumerScore > autoRejectionExceptionCreditScore)
			{
				return true;
			}
			int rejectionExceptionMaxConsumerScoreForMpError = CurrentValues.Instance.RejectionExceptionMaxConsumerScoreForMpError;
			int rejectionExceptionMaxCompanyScoreForMpError = CurrentValues.Instance.RejectionExceptionMaxCompanyScoreForMpError;
			if (errorMPsNum > 0 && (maxExperianConsumerScore > rejectionExceptionMaxConsumerScoreForMpError || maxCompanyScore > rejectionExceptionMaxCompanyScoreForMpError))
			{
				return true;
			}
			if ((decimal)maxExperianConsumerScore == 0)
			{
				return true;
			}

			int rejectionExceptionMaxCompanyScore = CurrentValues.Instance.RejectionExceptionMaxCompanyScore;
			if (maxCompanyScore >= rejectionExceptionMaxCompanyScore)
			{
				return true;
			}
			if (isBrokerCustomer) // TODO: Currently rejections are disabled for broker customers - this logic is in contradiction to it
			{
				return true;
			}

			return false;
		}

		private void Init()
		{
			DataTable dt = db.ExecuteReader("GetRejectionConfigs", CommandSpecies.StoredProcedure);
			var sr = new SafeReader(dt.Rows[0]);

			autoRejectionExceptionAnualTurnover = sr["AutoRejectionException_AnualTurnover"];
			rejectDefaultsCreditScore = sr["Reject_Defaults_CreditScore"];
			rejectMinimalSeniority = sr["Reject_Minimal_Seniority"];
			lowCreditScore = sr["LowCreditScore"];
			rejectDefaultsAccountsNum = sr["Reject_Defaults_AccountsNum"];
			autoRejectionExceptionCreditScore = sr["AutoRejectionException_CreditScore"];
			int rejectDefaultsMonths = sr["Reject_Defaults_MonthsNum"];
			int rejectDefaultsAmount = sr["Reject_Defaults_Amount"];
			int rejectByCompanyDefaultsMonths = CurrentValues.Instance.RejectByCompany_Defaults_MonthsNum;
			int rejectByCompanyDefaultsAmount = CurrentValues.Instance.RejectByCompany_Defaults_Amount;
			lowTotalAnnualTurnover = sr["LowTotalAnnualTurnover"];
			lowTotalThreeMonthTurnover = sr["LowTotalThreeMonthTurnover"];

			dt = db.ExecuteReader(
				"GetCustomerRejectionData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("Reject_Defaults_Months", rejectDefaultsMonths),
				new QueryParameter("Reject_Defaults_Amount", rejectDefaultsAmount),
				new QueryParameter("RejectByCompany_Defaults_Months", rejectByCompanyDefaultsMonths),
				new QueryParameter("RejectByCompany_Defaults_Amount", rejectByCompanyDefaultsAmount)
			);

			sr = new SafeReader(dt.Rows[0]);

			errorMPsNum = sr["ErrorMPsNum"];
			loanOfferApprovalNum = sr["ApprovalNum"];
			numOfDefaultAccounts = sr["NumOfDefaultAccounts"];
			numOfDefaultAccountsForCompany = sr["NumOfDefaultAccountsForCompany"];
		}

		public bool MakeDecision(AutoDecisionResponse response)
		{
			try
			{
				Init();
				if (IsException())
					return false;

				int rejectByCompanyNumOfDefaultAccounts = CurrentValues.Instance.RejectByCompanyNumOfDefaultAccounts;
				int rejectByCompanyDefaultsScore = CurrentValues.Instance.RejectByCompanyDefaultsScore;
				int rejectionCompanyScore = CurrentValues.Instance.RejectionCompanyScore;
				FillPayPalFiguresForExplanationMail(db, customerId, response);

				if (maxExperianConsumerScore < lowCreditScore)
				{
					response.AutoRejectReason = "AutoReject: Low score. Condition not met:" + maxExperianConsumerScore + " < " +
					                            lowCreditScore;
				}
				else if (maxCompanyScore < rejectionCompanyScore)
				{
					response.AutoRejectReason = "AutoReject: Low company score. Condition not met:" + maxCompanyScore + " < " +
												rejectionCompanyScore;
				}
				else if (maxExperianConsumerScore < rejectDefaultsCreditScore &&
				    numOfDefaultAccounts >= rejectDefaultsAccountsNum)
				{
					response.AutoRejectReason = "AutoReject: Score & DefaultAccountsNum. Condition not met:" +
					                            maxExperianConsumerScore +
					                            " < " + rejectDefaultsCreditScore + " AND " + numOfDefaultAccounts + " >= " +
					                            rejectDefaultsAccountsNum;
				}
				else if (maxCompanyScore < rejectByCompanyDefaultsScore && numOfDefaultAccountsForCompany >= rejectByCompanyNumOfDefaultAccounts &&
				         isLimitedCompany)
				{
					response.AutoRejectReason = "AutoReject: Limited company defaults. Condition not met:" +
												numOfDefaultAccountsForCompany +
												" >= " + rejectByCompanyNumOfDefaultAccounts + " AND is limited company";
				}
				// TODO: Add condition:
				// 5. Late over 30 days in personal CAIS (should be configurable according to ExperianAccountStatuses) At least in 2 accounts in last 3 months

				// TODO: Add condition:
				// 6. max(Marketplace(Ecomm) or company )seniority < 11 months.

				else if (!customerStatusIsEnabled || customerStatusIsWarning)
				{
					response.AutoRejectReason = "AutoReject: Customer status. Condition not met:" +
												!customerStatusIsEnabled +
												" AND " + customerStatusIsWarning;
				
				}

				// TODO: the 2 next conditions are in the previous implementation but are not mentioned in the new story - should they be removed?
				// TODO: should this next condition be removed (was it replaced by condition #6 in story?)
				else if (marketplaceSeniorityDays < rejectMinimalSeniority)
				{
					response.AutoRejectReason = "AutoReject: Seniority. Condition not met: (" + marketplaceSeniorityDays + " < " +
												rejectMinimalSeniority + ")";
				}
				// TODO: should this next condition be removed (was it replaced by condition #1 (second paragraph) in story?)
				else if (
					(response.PayPalNumberOfStores == 0 ||
					 response.PayPalTotalSumOfOrders3M < lowTotalThreeMonthTurnover ||
					 response.PayPalTotalSumOfOrders1Y < lowTotalAnnualTurnover)
					&&
					(totalSumOfOrders3MTotalForRejection < lowTotalThreeMonthTurnover || totalSumOfOrders1YTotalForRejection < lowTotalAnnualTurnover)
					)
				{
					response.AutoRejectReason = "AutoReject: Totals. Condition not met: (" + response.PayPalNumberOfStores + " < 0 OR " +
					                            response.PayPalTotalSumOfOrders3M + " < " +
					                            lowTotalThreeMonthTurnover + " OR " + response.PayPalTotalSumOfOrders1Y + " < " +
					                            lowTotalAnnualTurnover + ") AND (" + totalSumOfOrders3MTotalForRejection + " < " +
					                            lowTotalThreeMonthTurnover + " OR " + totalSumOfOrders1YTotalForRejection + " < " +
					                            lowTotalAnnualTurnover + ")";
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

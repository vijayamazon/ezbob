﻿namespace EzBob.Backend.Strategies {
	using AutoDecisions;
	using Experian;
	using EzBob.Models;
	using Ezbob.Backend.Models;
	using MailStrategies.API;
	using ScoreCalculation;
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.Linq;
	using System.Threading;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EzBob.Backend.Strategies.MailStrategies;

	#region class MainStrategy

	public class MainStrategy : AStrategy {
		#region public

		#region constructor

		public MainStrategy(
			int customerId,
			NewCreditLineOption newCreditLine,
			int avoidAutoDecision,
			AConnection oDb,
			ASafeLog oLog
		)
			: this(customerId, newCreditLine, avoidAutoDecision, false, oDb, oLog)
		{
		} // constructor

		public MainStrategy(
			int customerId,
			NewCreditLineOption newCreditLine,
			int avoidAutoDecision,
			bool isUnderwriterForced,
			AConnection oDb,
			ASafeLog oLog
		)
			: base(oDb, oLog)
		{
			medalScoreCalculator = new MedalScoreCalculator(DB, Log);
			mailer = new StrategiesMailer(DB, Log);
			this.customerId = customerId;
			newCreditLineOption = newCreditLine;
			avoidAutomaticDecision = avoidAutoDecision;
			underwriterCheck = isUnderwriterForced;
			m_bOverrideApprovedRejected = true;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name { get { return "Main strategy"; } } // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			ReadConfigurations();

			MakeSureMpDataIsSufficient();

			GetPersonalInfo();

			SetAutoDecisionAvailability();

			if (newCreditLineOption != NewCreditLineOption.SkipEverything) {
				PerformCompanyExperianCheck();
				PerformConsumerExperianCheck();

				minExperianScore = experianConsumerScore;
				maxExperianScore = experianConsumerScore;
				initialExperianConsumerScore = experianConsumerScore;

				PerformExperianConsumerCheckForDirectors();

				GetAml();
				GetBwa();
			}
			else {
				experianConsumerScore = GetCurrentExperianScore();
				minExperianScore = experianConsumerScore;
				maxExperianScore = experianConsumerScore;
				initialExperianConsumerScore = experianConsumerScore;
			} // if

			ScoreMedalOffer scoringResult = CalculateScoreAndMedal();

			if (underwriterCheck) {
				GetZooplaData();
				SetEndTimestamp();
				return;
			} // if

			GetLastCashRequestData();

			CalcAndCapOffer();

			autoDecisionResponse = AutoDecisionMaker.MakeDecision(
				customerId,
				minExperianScore,
				maxExperianScore,
				totalSumOfOrders1YTotalForRejection,
				totalSumOfOrders3MTotalForRejection,
				offeredCreditLine,
				marketplaceSeniorityDays,
				enableAutomaticReRejection,
				enableAutomaticRejection,
				enableAutomaticReApproval,
				enableAutomaticApproval,
				loanOfferReApprovalFullAmountOld,
				loanOfferReApprovalFullAmount,
				loanOfferReApprovalRemainingAmount,
				loanOfferReApprovalRemainingAmountOld,
				DB,
				Log
			);

			if (autoDecisionResponse.IsAutoApproval) {
				modelLoanOffer = autoDecisionResponse.AutoApproveAmount;

				if (modelLoanOffer < offeredCreditLine)
					offeredCreditLine = modelLoanOffer;
			}
			else if (autoDecisionResponse.IsAutoBankBasedApproval) {
				modelLoanOffer = autoDecisionResponse.BankBasedAutoApproveAmount;

				if (modelLoanOffer < offeredCreditLine)
					offeredCreditLine = modelLoanOffer;
			}

			if (autoDecisionResponse.SystemDecision == "Reject")
				modelLoanOffer = 0;

			UpdateCustomerAndCashRequest(scoringResult.ScoreResult, scoringResult.MaxOfferPercent);

			if (autoDecisionResponse.UserStatus == "Approved") {
				if (autoDecisionResponse.IsAutoApproval) {
					UpdateApprovalData();
					SendApprovalMails(scoringResult.MaxOfferPercent);

					strategyHelper.AddApproveIntoDecisionHistory(customerId, "Auto Approval");
				}
				else if (autoDecisionResponse.IsAutoBankBasedApproval) {
					UpdateBankBasedApprovalData();
					SendBankBasedApprovalMails();

					strategyHelper.AddApproveIntoDecisionHistory(customerId, "Auto bank based approval");
				}
				else {
					UpdateReApprovalData();
					SendReApprovalMails();

					if (enableAutomaticReApproval)
						strategyHelper.AddApproveIntoDecisionHistory(customerId, "Auto Re-Approval");
				} // if
			}
			else if (autoDecisionResponse.UserStatus == "Rejected") {
				if ((autoDecisionResponse.IsReRejected && !enableAutomaticReRejection) || (!autoDecisionResponse.IsReRejected && !enableAutomaticRejection))
					SendRejectionExplanationMail(autoDecisionResponse.IsReRejected ? "Mandrill - User supposed to be re-rejected by the strategy" : "Mandrill - User supposed to be rejected by the strategy");
				else {
					SendRejectionExplanationMail("Mandrill - User is rejected by the strategy");

					new RejectUser(customerId, DB, Log).Execute();

					strategyHelper.AddRejectIntoDecisionHistory(customerId, autoDecisionResponse.AutoRejectReason);
				} // if
			}
			else
				SendWaitingForDecisionMail();

			GetZooplaData();
			SetEndTimestamp();
		} // Execute

		#endregion method Execute

		#region method SetOverrideApproved

		public virtual MainStrategy SetOverrideApprovedRejected(bool bOverrideApprovedRejected) {
			m_bOverrideApprovedRejected = bOverrideApprovedRejected;
			return this;
		} // SetOverrideApprovedRejected

		#endregion method SetOverrideApproved

		#endregion public

		#region private

		#region method CalcAndCapOffer

		private void CalcAndCapOffer() {
			Log.Info("Finalizing and capping offer");

			if (loanOfferReApprovalRemainingAmount < 1000) // TODO: make this 1000 configurable
				loanOfferReApprovalRemainingAmount = 0;

			if (loanOfferReApprovalRemainingAmountOld < 500) // TODO: make this 500 configurable
				loanOfferReApprovalRemainingAmountOld = 0;

			loanOfferReApprovalSum = new [] {
				loanOfferReApprovalFullAmount,
				loanOfferReApprovalRemainingAmount,
				loanOfferReApprovalFullAmountOld,
				loanOfferReApprovalRemainingAmountOld
			}.Max();

			offeredCreditLine = modelLoanOffer;

			if (appHomeOwner == "Home owner" && maxCapHomeOwner < loanOfferReApprovalSum)
				loanOfferReApprovalSum = maxCapHomeOwner;

			if (appHomeOwner != "Home owner" && maxCapNotHomeOwner < loanOfferReApprovalSum)
				loanOfferReApprovalSum = maxCapNotHomeOwner;

			if (appHomeOwner == "Home owner" && maxCapHomeOwner < offeredCreditLine)
				offeredCreditLine = maxCapHomeOwner;

			if (appHomeOwner != "Home owner" && maxCapNotHomeOwner < offeredCreditLine)
				offeredCreditLine = maxCapNotHomeOwner;
		} // CalcAndCapOffer

		#endregion method CalcAndCapOffer

		#region method UpdateCustomerAndCashRequest

		private void UpdateCustomerAndCashRequest(decimal scoringResult, decimal loanInterestBase) {
			DB.ExecuteNonQuery(
				"UpdateScoringResultsNew",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("CreditResult", autoDecisionResponse.CreditResult),
				new QueryParameter("SystemDecision", autoDecisionResponse.SystemDecision),
				new QueryParameter("Status", autoDecisionResponse.UserStatus),
				new QueryParameter("Medal", medalType.ToString()),
				new QueryParameter("ValidFor", autoDecisionResponse.AppValidFor),
				new QueryParameter("Now", DateTime.UtcNow),
				new QueryParameter("OverrideApprovedRejected", m_bOverrideApprovedRejected)
			);

			DB.ExecuteNonQuery(
				"UpdateCashRequestsNew",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("SystemCalculatedAmount", modelLoanOffer),
				new QueryParameter("ManagerApprovedSum", offeredCreditLine),
				new QueryParameter("SystemDecision", autoDecisionResponse.SystemDecision),
				new QueryParameter("MedalType", medalType.ToString()),
				new QueryParameter("ScorePoints", scoringResult),
				new QueryParameter("ExpirianRating", experianConsumerScore),
				new QueryParameter("AnualTurnover", totalSumOfOrders1YTotal),
				new QueryParameter("InterestRate", loanInterestBase),
				new QueryParameter("ManualSetupFeeAmount", manualSetupFeeAmount),
				new QueryParameter("ManualSetupFeePercent", manualSetupFeePercent),
				new QueryParameter("RepaymentPeriod", autoDecisionResponse.RepaymentPeriod),
				new QueryParameter("Now", DateTime.UtcNow)
			);
		} // UpdateCustomerAndCashRequest

		#endregion method UpdateCustomerAndCashRequest

		#region method SendWaitingForDecisionMail

		private void SendWaitingForDecisionMail() {
			mailer.Send("Mandrill - User is waiting for decision", new Dictionary<string, string> {
				{"RegistrationDate", appRegistrationDate.ToString(CultureInfo.InvariantCulture)},
				{"userID", customerId.ToString(CultureInfo.InvariantCulture)},
				{"Name", appEmail},
				{"FirstName", appFirstName},
				{"Surname", appSurname},
				{"MP_Counter", allMPsNum.ToString(CultureInfo.InvariantCulture)},
				{"MedalType", medalType.ToString()},
				{"SystemDecision", autoDecisionResponse.SystemDecision}
			});
		} // SendWaitingForDecisionMail

		#endregion method SendWaitingForDecisionMail

		#region method SendReApprovalMails

		private void SendReApprovalMails() {
			mailer.Send("Mandrill - User is re-approved",  new Dictionary<string, string> {
				{"ApprovedReApproved", "Re-Approved"},
				{"RegistrationDate", appRegistrationDate.ToString(CultureInfo.InvariantCulture)},
				{"userID", customerId.ToString(CultureInfo.InvariantCulture)},
				{"Name", appEmail},
				{"FirstName", appFirstName},
				{"Surname", appSurname},
				{"MP_Counter", allMPsNum.ToString(CultureInfo.InvariantCulture)},
				{"MedalType", medalType.ToString()},
				{"SystemDecision", autoDecisionResponse.SystemDecision},
				{"ApprovalAmount", loanOfferReApprovalSum.ToString(CultureInfo.InvariantCulture)},
				{"RepaymentPeriod", loanOfferRepaymentPeriod.ToString(CultureInfo.InvariantCulture)},
				{"InterestRate", loanOfferInterestRate.ToString(CultureInfo.InvariantCulture)},
				{
					"OfferValidUntil", autoDecisionResponse.AppValidFor.HasValue
						? autoDecisionResponse.AppValidFor.Value.ToString(CultureInfo.InvariantCulture)
						: string.Empty
				}
			});

			if (enableAutomaticReApproval) {
				var customerMailVariables = new Dictionary<string, string> {
					{"FirstName", appFirstName},
					{"LoanAmount", loanOfferReApprovalSum.ToString(CultureInfo.InvariantCulture)},
					{"ValidFor", autoDecisionResponse.AppValidFor.HasValue ? autoDecisionResponse.AppValidFor.Value.ToString(CultureInfo.InvariantCulture) : string.Empty}
				};

				mailer.Send("Mandrill - Approval (not 1st time)", customerMailVariables, new Addressee(appEmail));
			} // if
		} // SendReApprovalMails

		#endregion method SendReApprovalMails

		#region method UpdateReApprovalData

		private void UpdateReApprovalData() {
			DB.ExecuteNonQuery(
				"UpdateCashRequestsReApproval",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("UnderwriterDecision", autoDecisionResponse.UserStatus),
				new QueryParameter("ManagerApprovedSum", loanOfferReApprovalSum),
				new QueryParameter("APR", loanOfferApr),
				new QueryParameter("RepaymentPeriod", loanOfferRepaymentPeriod),
				new QueryParameter("InterestRate", loanOfferInterestRate),
				new QueryParameter("UseSetupFee", loanOfferUseSetupFee),
				new QueryParameter("EmailSendingBanned", autoDecisionResponse.LoanOfferEmailSendingBannedNew),
				new QueryParameter("LoanTypeId", loanOfferLoanTypeId),
				new QueryParameter("UnderwriterComment", autoDecisionResponse.LoanOfferUnderwriterComment),
				new QueryParameter("IsLoanTypeSelectionAllowed", loanOfferIsLoanTypeSelectionAllowed),
				new QueryParameter("DiscountPlanId", loanOfferDiscountPlanId),
				new QueryParameter("ExperianRating", experianConsumerScore),
				new QueryParameter("LoanSourceId", loanSourceId),
				new QueryParameter("IsCustomerRepaymentPeriodSelectionAllowed", isCustomerRepaymentPeriodSelectionAllowed),
				new QueryParameter("UseBrokerSetupFee", useBrokerSetupFee),
				new QueryParameter("Now", DateTime.UtcNow)
			);
		} // UpdateReApprovalData

		#endregion method UpdateReApprovalData

		#region method SendApprovalMails

		private void SendApprovalMails(decimal interestRate) {
			mailer.Send("Mandrill - User is approved", new Dictionary<string, string> {
				{"ApprovedReApproved", "Approved"},
				{"RegistrationDate", appRegistrationDate.ToString(CultureInfo.InvariantCulture)},
				{"userID", customerId.ToString(CultureInfo.InvariantCulture)},
				{"Name", appEmail},
				{"FirstName", appFirstName},
				{"Surname", appSurname},
				{"MP_Counter", allMPsNum.ToString(CultureInfo.InvariantCulture)},
				{"MedalType", medalType.ToString()},
				{"SystemDecision", autoDecisionResponse.SystemDecision},
				{"ApprovalAmount", autoDecisionResponse.AutoApproveAmount.ToString(CultureInfo.InvariantCulture)},
				{"RepaymentPeriod", loanOfferRepaymentPeriod.ToString(CultureInfo.InvariantCulture)},
				{"InterestRate", interestRate.ToString(CultureInfo.InvariantCulture)},
				{
					"OfferValidUntil", autoDecisionResponse.AppValidFor.HasValue
						? autoDecisionResponse.AppValidFor.Value.ToString(CultureInfo.InvariantCulture)
						: string.Empty
				}
			});

			var customerMailVariables = new Dictionary<string, string> {
				{"FirstName", appFirstName},
				{"LoanAmount", autoDecisionResponse.AutoApproveAmount.ToString(CultureInfo.InvariantCulture)},
				{"ValidFor", autoDecisionResponse.AppValidFor.HasValue ? autoDecisionResponse.AppValidFor.Value.ToString(CultureInfo.InvariantCulture) : string.Empty}
			};

			mailer.Send("Mandrill - Approval (" + (isFirstLoan ? "" : "not ") + "1st time)", customerMailVariables, new Addressee(appEmail));
		} // SendApprovalMails

		#endregion method SendApprovalMails

		#region method UpdateApprovalData

		private void UpdateApprovalData() {
			DB.ExecuteNonQuery(
				"UpdateAutoApproval",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("AutoApproveAmount", autoDecisionResponse.AutoApproveAmount)
			);
		} // UpdateApprovalData

		#endregion method UpdateApprovalData

		#region method SendBankBasedApprovalMails

		private void SendBankBasedApprovalMails() {
			mailer.Send("Mandrill - User is approved", new Dictionary<string, string> {
				{"ApprovedReApproved", "Approved"},
				{"RegistrationDate", appRegistrationDate.ToString(CultureInfo.InvariantCulture)},
				{"userID", customerId.ToString(CultureInfo.InvariantCulture)},
				{"Name", appEmail},
				{"FirstName", appFirstName},
				{"Surname", appSurname},
				{"MP_Counter", allMPsNum.ToString(CultureInfo.InvariantCulture)},
				{"MedalType", medalType.ToString()},
				{"SystemDecision", autoDecisionResponse.SystemDecision},
				{"ApprovalAmount", autoDecisionResponse.BankBasedAutoApproveAmount.ToString(CultureInfo.InvariantCulture)},
				{"RepaymentPeriod", autoDecisionResponse.RepaymentPeriod.ToString(CultureInfo.InvariantCulture)},
				{"InterestRate", loanOfferInterestRate.ToString(CultureInfo.InvariantCulture)},
				{
					"OfferValidUntil", autoDecisionResponse.AppValidFor.HasValue
						? autoDecisionResponse.AppValidFor.Value.ToString(CultureInfo.InvariantCulture)
						: string.Empty
				}
			});

			var customerMailVariables = new Dictionary<string, string> {
				{"FirstName", appFirstName},
				{"LoanAmount", autoDecisionResponse.BankBasedAutoApproveAmount.ToString(CultureInfo.InvariantCulture)},
				{"ValidFor", autoDecisionResponse.AppValidFor.HasValue ? autoDecisionResponse.AppValidFor.Value.ToString(CultureInfo.InvariantCulture) : string.Empty}
			};

			mailer.Send("Mandrill - Approval (" + (isFirstLoan ? "" : "not ") + "1st time)", customerMailVariables, new Addressee(appEmail));
		} // SendBankBasedApprovalMails

		#endregion method SendBankBasedApprovalMails

		#region method UpdateBankBasedApprovalData

		private void UpdateBankBasedApprovalData() {
			DB.ExecuteNonQuery(
				"UpdateBankBasedAutoApproval",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("AutoApproveAmount", autoDecisionResponse.BankBasedAutoApproveAmount),
				new QueryParameter("RepaymentPeriod", autoDecisionResponse.RepaymentPeriod)
			);
		} // UpdateBankBasedApprovalData

		#endregion method UpdateBankBasedApprovalData

		#region method SetEndTimestamp

		private void SetEndTimestamp() {
			DB.ExecuteNonQuery("Update_Main_Strat_Finish_Date",
				CommandSpecies.StoredProcedure,
				new QueryParameter("UserId", customerId),
				new QueryParameter("Now", DateTime.UtcNow)
			);
		} // SetEndTimestatmp

		#endregion method SetEndTimestamp

		#region method GetLastCashRequestData

		private void GetLastCashRequestData() {
			DataTable lastOfferDataTable = DB.ExecuteReader(
				"GetLastOfferForAutomatedDecision",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("Now", DateTime.UtcNow)
			);

			if (lastOfferDataTable.Rows.Count == 1) {
				var lastOfferResults = new SafeReader(lastOfferDataTable.Rows[0]);
				loanOfferReApprovalFullAmount = lastOfferResults["ReApprovalFullAmountNew"];
				loanOfferReApprovalRemainingAmount = lastOfferResults["ReApprovalRemainingAmount"];
				loanOfferReApprovalFullAmountOld = lastOfferResults["ReApprovalFullAmountOld"];
				loanOfferReApprovalRemainingAmountOld = lastOfferResults["ReApprovalRemainingAmountOld"];
				loanOfferApr = lastOfferResults["APR"];
				loanOfferRepaymentPeriod = lastOfferResults["RepaymentPeriod"];
				loanOfferInterestRate = lastOfferResults["InterestRate"];
				loanOfferUseSetupFee = lastOfferResults["UseSetupFee"];
				loanOfferLoanTypeId = lastOfferResults["LoanTypeId"];
				loanOfferIsLoanTypeSelectionAllowed = lastOfferResults["IsLoanTypeSelectionAllowed"];
				loanOfferDiscountPlanId = lastOfferResults["DiscountPlanId"];
				loanSourceId = lastOfferResults["LoanSourceID"];
				isCustomerRepaymentPeriodSelectionAllowed = lastOfferResults["IsCustomerRepaymentPeriodSelectionAllowed"];
				useBrokerSetupFee = lastOfferResults["UseBrokerSetupFee"];
				manualSetupFeeAmount = lastOfferResults["ManualSetupFeeAmount"];
				manualSetupFeePercent = lastOfferResults["ManualSetupFeePercent"];
			} // if
		} // GetLastCashRequestData

		#endregion method GetLastCashRequestData

		#region method CalculateScoreAndMedal

		private ScoreMedalOffer CalculateScoreAndMedal() {
			Log.Info("Starting to calculate score and medal");

			DataTable scoreCardDataTable = DB.ExecuteReader(
				"GetScoreCardData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("Today", DateTime.Today)
			);

			var scoreCardResults = new SafeReader(scoreCardDataTable.Rows[0]);
			string maritalStatusStr = scoreCardResults["MaritalStatus"];
			MaritalStatus maritalStatus;

			if (!Enum.TryParse(maritalStatusStr, true, out maritalStatus)) {
				Log.Warn("Cant parse marital status:{0}. Will use 'Other'", maritalStatusStr);
				maritalStatus = MaritalStatus.Other;
			} // if

			int modelMaxFeedback = scoreCardResults.IntWithDefault("MaxFeedback", defaultFeedbackValue);

			int modelMPsNumber = scoreCardResults["MPsNumber"];
			int modelEzbobSeniority = scoreCardResults["EZBOBSeniority"];
			int modelOnTimeLoans = scoreCardResults["OnTimeLoans"];
			int modelLatePayments = scoreCardResults["LatePayments"];
			int modelEarlyPayments = scoreCardResults["EarlyPayments"];

			bool firstRepaymentDatePassed = false;

			DateTime modelFirstRepaymentDate = scoreCardResults["FirstRepaymentDate"];
			if (modelFirstRepaymentDate != default(DateTime))
				firstRepaymentDatePassed = modelFirstRepaymentDate < DateTime.UtcNow;

			Log.Info("Getting turnovers and seniority");

			MpsTotals totals = strategyHelper.GetMpsTotals(customerId);
			totalSumOfOrders1YTotal = totals.TotalSumOfOrders1YTotal;
			totalSumOfOrders1YTotalForRejection = totals.TotalSumOfOrders1YTotalForRejection;
			totalSumOfOrders3MTotalForRejection = totals.TotalSumOfOrders3MTotalForRejection;
			marketplaceSeniorityDays = totals.MarketplaceSeniorityDays;
			decimal totalSumOfOrdersForLoanOffer = totals.TotalSumOfOrdersForLoanOffer;
			decimal marketplaceSeniorityYears = (decimal)totals.MarketplaceSeniorityDays / 365; // It is done this way to fit to the excel
			decimal ezbobSeniorityMonths = (decimal)modelEzbobSeniority * 12 / 365; // It is done this way to fit to the excel

			Log.Info("Calculating score & medal");

			ScoreMedalOffer scoringResult = medalScoreCalculator.CalculateMedalScore(
				totalSumOfOrdersForLoanOffer,
				minExperianScore,
				marketplaceSeniorityYears,
				modelMaxFeedback,
				maritalStatus,
				appGender == "M" ? Gender.M : Gender.F,
				modelMPsNumber,
				firstRepaymentDatePassed,
				ezbobSeniorityMonths,
				modelOnTimeLoans,
				modelLatePayments,
				modelEarlyPayments
			);

			modelLoanOffer = scoringResult.MaxOffer;

			medalType = scoringResult.Medal;

			DB.ExecuteNonQuery(
				"CustomerScoringResult_Insert",
				CommandSpecies.StoredProcedure,
				new QueryParameter("pCustomerId", customerId),
				new QueryParameter("pAC_Parameters", scoringResult.AcParameters),
				new QueryParameter("AC_Descriptors", scoringResult.AcDescriptors),
				new QueryParameter("Result_Weight", scoringResult.ResultWeigts),
				new QueryParameter("pResult_MAXPossiblePoints", scoringResult.ResultMaxPoints),
				new QueryParameter("pMedal", scoringResult.Medal.ToString()),
				new QueryParameter("pScorePoints", scoringResult.ScorePoints),
				new QueryParameter("pScoreResult", scoringResult.ScoreResult)
			);

			DB.ExecuteNonQuery(
				"CustomerAnalyticsUpdateLocalData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", customerId),
				new QueryParameter("AnalyticsDate", DateTime.UtcNow),
				new QueryParameter("AnnualTurnover", totalSumOfOrders1YTotal),
				new QueryParameter("TotalSumOfOrdersForLoanOffer", totalSumOfOrdersForLoanOffer),
				new QueryParameter("MarketplaceSeniorityYears", marketplaceSeniorityYears),
				new QueryParameter("MaxFeedback", modelMaxFeedback),
				new QueryParameter("MPsNumber", modelMPsNumber),
				new QueryParameter("FirstRepaymentDatePassed", firstRepaymentDatePassed),
				new QueryParameter("EzbobSeniorityMonths", ezbobSeniorityMonths),
				new QueryParameter("OnTimeLoans", modelOnTimeLoans),
				new QueryParameter("LatePayments", modelLatePayments),
				new QueryParameter("EarlyPayments", modelEarlyPayments)
			);
			return scoringResult;
		} // CalculateScoreAndMedal

		#endregion method CalculateScoreAndMedal

		#region method PerformExperianConsumerCheckForDirectors

		private void PerformExperianConsumerCheckForDirectors() {
			if (companyType == "Entrepreneur")
				return;

			DataTable dt = DB.ExecuteReader(
				"GetCustomerDirectorsForConsumerCheck",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);

			foreach (DataRow row in dt.Rows) {
				var sr = new SafeReader(row);
				int appDirId = sr["DirId"];
				string appDirName = sr["DirName"];
				string appDirSurname = sr["DirSurname"];

				if (string.IsNullOrEmpty(appDirName) || string.IsNullOrEmpty(appDirSurname))
					continue;

				PerformConsumerExperianCheck(appDirId);

				if (experianConsumerScore > 0 && experianConsumerScore < minExperianScore)
					minExperianScore = experianConsumerScore;

				if (experianConsumerScore > 0 && experianConsumerScore > maxExperianScore)
					maxExperianScore = experianConsumerScore;
			} // foreach
		} // PerformExperianConsumerCheckForDirectors

		#endregion method PerformExperianConsumerCheckForDirectors

		#region method PerformConsumerExperianCheck

		private int PerformConsumerExperianCheck(int directorId = 0) {
			if (wasMainStrategyExecutedBefore) {
				Log.Info("Performing experian consumer check");

				var strat = new ExperianConsumerCheck(customerId, directorId, false, DB, Log);
				strat.Execute();

				if (directorId == 0)
					experianConsumerScore = strat.Score;

				return strat.Score;
			} // if

			if (!WaitForExperianConsumerCheckToFinishUpdates(directorId)) {
				Log.Info("No data exist from experian consumer check for customer {0}{1}.", customerId, directorId == 0 ? "" : "director " + directorId);
				return 0;
			} // if

			if (directorId == 0) {
				experianConsumerScore = GetCurrentExperianScore();
				return experianConsumerScore;
			} // if

			return 0;
		} // PerformConsumerExperianCheck

		#endregion method PerformConsumerExperianCheck

		#region method GetCurrentExperianScore

		private int GetCurrentExperianScore() {
			return DB.ExecuteScalar<int>(
				"GetExperianScore",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);
		} // GetCurrentExperianScore

		#endregion method GetCurrentExperianScore

		#region method PerformCompanyExperianCheck

		private void PerformCompanyExperianCheck() {
			if (wasMainStrategyExecutedBefore) {
				Log.Info("Performing experian company check");
				var experianCompanyChecker = new ExperianCompanyCheck(customerId, false, DB, Log);
				experianCompanyChecker.Execute();
			}
			else if (!WaitForExperianCompanyCheckToFinishUpdates())
				Log.Info("No data exist from experian company check for customer:{0}.", customerId);
		} // PerformCompanyExperianCheck

		#endregion method PerformCompanyExperianCheck

		#region method MakeSureMpDataIsSufficient

		private void MakeSureMpDataIsSufficient() {
			bool shouldExpectMpData =
				newCreditLineOption != NewCreditLineOption.SkipEverything &&
				newCreditLineOption != NewCreditLineOption.UpdateEverythingExceptMp;

			if (shouldExpectMpData) {
				if (!WaitForMarketplacesToFinishUpdates()) {
					Log.Info("Waiting for marketplace data ended with error");

					mailer.Send("Mandrill - No Information about shops", new Dictionary<string, string> {
						{"UserEmail", appEmail},
						{"CustomerID", customerId.ToString(CultureInfo.InvariantCulture)},
						{"ApplicationID", appEmail}
					});
				} // if
			} // if
		} // MakeSureMpDataIsSufficient

		#endregion method MakeSureMpDataIsSufficient

		#region method SetAutoDecisionAvailability

		private void SetAutoDecisionAvailability() {
			Log.Info("Setting auto decision availability");

			if (!customerStatusIsEnabled || customerStatusIsWarning) {
				enableAutomaticReApproval = false;
				enableAutomaticApproval = false;
			} // if

			if (isOffline || isBrokerCustomer) {
				enableAutomaticApproval = false;
				enableAutomaticRejection = false;
			} // if

			if (
				newCreditLineOption == NewCreditLineOption.SkipEverything ||
				newCreditLineOption == NewCreditLineOption.UpdateEverythingExceptMp ||
				newCreditLineOption == NewCreditLineOption.UpdateEverythingAndGoToManualDecision ||
				avoidAutomaticDecision == 1
			) {
				enableAutomaticApproval = false;
				enableAutomaticReApproval = false;
				enableAutomaticRejection = false;
				enableAutomaticReRejection = false;
			} // if
		} // SetAutoDecisionAvailability

		#endregion method SetAutoDecisionAvailability

		#region method ReadConfigurations

		private void ReadConfigurations()
		{
			Log.Info("Getting configurations");
			DataTable dt = DB.ExecuteReader("MainStrategyGetConfigs", CommandSpecies.StoredProcedure);
			DataRow results = dt.Rows[0];
			var sr = new SafeReader(results);
			rejectDefaultsCreditScore = sr["Reject_Defaults_CreditScore"];
			rejectDefaultsAccountsNum = sr["Reject_Defaults_AccountsNum"];
			rejectMinimalSeniority = sr["Reject_Minimal_Seniority"];
			rejectDefaultsMonthsNum = sr["Reject_Defaults_MonthsNum"];
			rejectDefaultsAmount = sr["Reject_Defaults_Amount"];
			bwaBusinessCheck = sr["BWABusinessCheck"];
			enableAutomaticApproval = sr["EnableAutomaticApproval"];
			enableAutomaticReApproval = sr["EnableAutomaticReApproval"];
			enableAutomaticRejection = sr["EnableAutomaticRejection"];
			enableAutomaticReRejection = sr["EnableAutomaticReRejection"];
			maxCapHomeOwner = sr["MaxCapHomeOwner"];
			maxCapNotHomeOwner = sr["MaxCapNotHomeOwner"];
			lowCreditScore = sr["LowCreditScore"];
			lowTotalAnnualTurnover = sr["LowTotalAnnualTurnover"];
			lowTotalThreeMonthTurnover = sr["LowTotalThreeMonthTurnover"];
			defaultFeedbackValue = sr["DefaultFeedbackValue"];
			totalTimeToWaitForMarketplacesUpdate = sr["TotalTimeToWaitForMarketplacesUpdate"];
			intervalWaitForMarketplacesUpdate = sr["IntervalWaitForMarketplacesUpdate"];
			totalTimeToWaitForExperianCompanyCheck = sr["TotalTimeToWaitForExperianCompanyCheck"];
			intervalWaitForExperianCompanyCheck = sr["IntervalWaitForExperianCompanyCheck"];
			totalTimeToWaitForExperianConsumerCheck = sr["TotalTimeToWaitForExperianConsumerCheck"];
			intervalWaitForExperianConsumerCheck = sr["IntervalWaitForExperianConsumerCheck"];
			totalTimeToWaitForAmlCheck = sr["TotalTimeToWaitForAmlCheck"];
			intervalWaitForAmlCheck = sr["IntervalWaitForAmlCheck"];
		} // ReadConfigurations

		#endregion method ReadConfigurations

		#region method GetPersonalInfo

		private void GetPersonalInfo()
		{
			Log.Info("Getting personal info for customer:{0}", customerId);
			DataTable dt = DB.ExecuteReader("GetPersonalInfo", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", customerId));
			var results = new SafeReader(dt.Rows[0]);

			customerStatusIsEnabled = results["CustomerStatusIsEnabled"];
			customerStatusIsWarning = results["CustomerStatusIsWarning"];
			isOffline = results["IsOffline"];
			isBrokerCustomer = results["IsBrokerCustomer"];
			appEmail = results["CustomerEmail"];
			companyType = results["CompanyType"];
			experianRefNum = results["ExperianRefNum"];
			wasMainStrategyExecutedBefore = results["MainStrategyExecutedBefore"];
			appFirstName = results["FirstName"];
			appSurname = results["Surname"];
			appGender = results["Gender"];
			appHomeOwner = results["HomeOwner"];
			allMPsNum = results["NumOfMps"];
			appAccountNumber = results["AccountNumber"];
			appSortCode = results["SortCode"];
			appRegistrationDate = results["RegistrationDate"];
			appBankAccountType = results["BankAccountType"];
			initialExperianConsumerScore = results["PrevExperianConsumerScore"];
			int numOfLoans = results["NumOfLoans"];
			isFirstLoan = numOfLoans == 0;
		} // GetPersonalInfo

		#endregion method GetPersonalInfo

		#region method SendRejectionExplanationMail

		private void SendRejectionExplanationMail(string templateName)
		{
			// TODO: set inside auto decision and get here instead of calculating again
			DataTable defaultAccountsNumDataTable = DB.ExecuteReader(
				"GetNumberOfDefaultAccounts",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("Months", rejectDefaultsMonthsNum),
				new QueryParameter("Amount", rejectDefaultsAmount)
			);

			var defaultAccountsNumResults = new SafeReader(defaultAccountsNumDataTable.Rows[0]);
			numOfDefaultAccounts = defaultAccountsNumResults["NumOfDefaultAccounts"];

			mailer.Send(templateName, new Dictionary<string, string> {
				{"RegistrationDate", appRegistrationDate.ToString(CultureInfo.InvariantCulture)},
				{"userID", customerId.ToString(CultureInfo.InvariantCulture)},
				{"Name", appEmail},
				{"FirstName", appFirstName},
				{"Surname", appSurname},
				{"MP_Counter", allMPsNum.ToString(CultureInfo.InvariantCulture)},
				{"MedalType", medalType.ToString()},
				{"SystemDecision", autoDecisionResponse.SystemDecision},
				{"ExperianConsumerScore", initialExperianConsumerScore.ToString(CultureInfo.InvariantCulture)},
				{"CVExperianConsumerScore", lowCreditScore.ToString(CultureInfo.InvariantCulture)},
				{"TotalAnnualTurnover", totalSumOfOrders1YTotalForRejection.ToString(CultureInfo.InvariantCulture)},
				{"CVTotalAnnualTurnover", lowTotalAnnualTurnover.ToString(CultureInfo.InvariantCulture)},
				{"Total3MTurnover", totalSumOfOrders3MTotalForRejection.ToString(CultureInfo.InvariantCulture)},
				{"CVTotal3MTurnover", lowTotalThreeMonthTurnover.ToString(CultureInfo.InvariantCulture)},
				{"PayPalStoresNum", autoDecisionResponse.PayPalNumberOfStores.ToString(CultureInfo.InvariantCulture)},
				{"PayPalAnnualTurnover", autoDecisionResponse.PayPalTotalSumOfOrders1Y.ToString(CultureInfo.InvariantCulture)},
				{"CVPayPalAnnualTurnover", lowTotalAnnualTurnover.ToString(CultureInfo.InvariantCulture)},
				{"PayPal3MTurnover", autoDecisionResponse.PayPalTotalSumOfOrders3M.ToString(CultureInfo.InvariantCulture)},
				{"CVPayPal3MTurnover", lowTotalThreeMonthTurnover.ToString(CultureInfo.InvariantCulture)},
				{"CVExperianConsumerScoreDefAcc", rejectDefaultsCreditScore.ToString(CultureInfo.InvariantCulture)},
				{"ExperianDefAccNum", numOfDefaultAccounts.ToString(CultureInfo.InvariantCulture)},
				{"CVExperianDefAccNum", rejectDefaultsAccountsNum.ToString(CultureInfo.InvariantCulture)},
				{"Seniority", marketplaceSeniorityDays.ToString(CultureInfo.InvariantCulture)},
				{"SeniorityThreshold", rejectMinimalSeniority.ToString(CultureInfo.InvariantCulture)}
			});
		} // SendRejectionExplanationMail

		#endregion method SendRejectionExplanationMail

		#region method GetBwa

		private void GetBwa() {
			if (ShouldRunBwa()) {
				Log.Info("Getting BWA for customer: {0}", customerId);
				var bwaChecker = new BwaChecker(customerId, DB, Log);
				bwaChecker.Execute();
			} // if
		} // GetBwa

		#endregion method GetBwa

		#region method GetAml

		private void GetAml() {
			if (wasMainStrategyExecutedBefore) {
				Log.Info("Getting AML for customer: {0}", customerId);
				var amlChecker = new AmlChecker(customerId, DB, Log);
				amlChecker.Execute();
			}
			else if (!WaitForAmlToFinishUpdates())
				Log.Info("No AML data exist for customer:{0}.", customerId);
		} // GetAml

		#endregion method GetAml

		#region method ShouldRunBwa

		private bool ShouldRunBwa()
		{
			return appBankAccountType == "Personal" && bwaBusinessCheck == "1" && appSortCode != null && appAccountNumber != null;
		} // ShouldRunBwa

		#endregion method ShouldRunBwa

		#region method WaitForMarketplacesToFinishUpdates

		private bool WaitForMarketplacesToFinishUpdates() {
			Log.Info("Waiting for marketplace data");
			return WaitForUpdateToFinish(GetIsMarketPlacesUpdated, totalTimeToWaitForMarketplacesUpdate, intervalWaitForMarketplacesUpdate);
		} // WaitForMarketplacesToFinishUpdates

		#endregion method WaitForMarketplacesToFinishUpdates

		#region method WaitForExperianCompanyCheckToFinishUpdates

		private bool WaitForExperianCompanyCheckToFinishUpdates() {
			Log.Info("Waiting for experian company check");

			if (string.IsNullOrEmpty(experianRefNum))
				return true;

			return WaitForUpdateToFinish(GetIsExperianCompanyUpdated, totalTimeToWaitForExperianCompanyCheck, intervalWaitForExperianCompanyCheck);
		} // WaitForExperianCompanyCheckToFinishUpdates

		#endregion method WaitForExperianCompanyCheckToFinishUpdates

		#region method WaitForExperianConsumerCheckToFinishUpdates

		private bool WaitForExperianConsumerCheckToFinishUpdates(int directorId = 0) {
			Log.Info("Waiting for experian consumer check");
			return WaitForUpdateToFinish(() => GetIsExperianConsumerUpdated(directorId), totalTimeToWaitForExperianConsumerCheck, intervalWaitForExperianConsumerCheck);
		} // WaitForExperianConsumerCheckToFinishUpdates

		#endregion method WaitForExperianConsumerCheckToFinishUpdates

		#region method WaitForAmlToFinishUpdates

		private bool WaitForAmlToFinishUpdates() {
			Log.Info("Waiting for AML check");
			return WaitForUpdateToFinish(GetIsAmlUpdated, totalTimeToWaitForAmlCheck, intervalWaitForAmlCheck);
		} // WaitForMarketplacesToFinishUpdates

		#endregion method WaitForAmlToFinishUpdates

		#region method WaitForUpdateToFinish

		private bool WaitForUpdateToFinish(Func<bool> function, int totalSecondsToWait, int intervalBetweenCheck) {
			DateTime startWaitingTime = DateTime.UtcNow;

			for ( ; ; ) {
				if (function())
					return true;

				if ((DateTime.UtcNow - startWaitingTime).TotalSeconds > totalSecondsToWait)
					return false;

				Thread.Sleep(intervalBetweenCheck);
			} // forever
		} // WaitForUpdateToFinish

		#endregion method WaitForUpdateToFinish

		#region method GetIsExperianConsumerUpdated

		private bool GetIsExperianConsumerUpdated(int directorId) {
			return DB.ExecuteScalar<bool>(
				"GetIsConsumerDataUpdated",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("DirectorId", directorId),
				new QueryParameter("Today", DateTime.Today)
			);
		} // GetIsExperianConsumerUpdated

		#endregion method GetIsExperianConsumerUpdated

		#region method GetIsAmlUpdated

		private bool GetIsAmlUpdated() {
			return DB.ExecuteScalar<bool>("GetIsAmlUpdated",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId));
		} // GetIsAmlUpdated

		#endregion method GetIsAmlUpdated

		#region method GetIsExperianCompanyUpdated

		private bool GetIsExperianCompanyUpdated() {
			return DB.ExecuteScalar<bool>(
				"GetIsCompanyDataUpdated",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CompanyRefNumber", experianRefNum),
				new QueryParameter("Today", DateTime.Today)
			);
		} // GetIsExperianCompanyUpdated

		#endregion method GetIsExperianCompanyUpdated

		#region method GetIsMarketPlacesUpdated

		private bool GetIsMarketPlacesUpdated() {
			bool bResult = true;

			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					string lastStatus = sr["CurrentStatus"];

					if (lastStatus != "Done" && lastStatus != "Never Started" && lastStatus != "Finished" && lastStatus != "Failed" && lastStatus != "Terminated") {
						bResult = false;
						return ActionResult.SkipAll;
					} // if

					return ActionResult.Continue;
				},
				"GetAllLastMarketplaceStatuses",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);

			return bResult;
		} // GetIsMarketPlacesUpdated

		#endregion method GetIsMarketPlacesUpdated

		#region method GetZooplaData

		private void GetZooplaData() {
			Log.Info("Getting zoopla data for customer:{0}", customerId);
			strategyHelper.GetZooplaData(customerId);
		} // GetZooplaData

		#endregion method GetZooplaData

		#region fields

		// Helpers
		private readonly StrategiesMailer mailer;
		private readonly StrategyHelper strategyHelper = new StrategyHelper();
		private readonly MedalScoreCalculator medalScoreCalculator;

		// Inputs
		private readonly int customerId;
		private readonly NewCreditLineOption newCreditLineOption;
		private readonly bool underwriterCheck;
		private readonly int avoidAutomaticDecision;

		/// <summary>
		/// Default: true. However when Main strategy is executed as a part of
		/// Finish Wizard strategy and customer is already approved/rejected
		/// then customer's status should not change.
		/// </summary>
		private bool m_bOverrideApprovedRejected;

		// Configs
		private int rejectDefaultsCreditScore;
		private int rejectDefaultsAccountsNum;
		private int rejectMinimalSeniority;
		private int rejectDefaultsMonthsNum;
		private int rejectDefaultsAmount;
		private string bwaBusinessCheck;
		private bool enableAutomaticReRejection;
		private bool enableAutomaticReApproval;
		private bool enableAutomaticApproval;
		private bool enableAutomaticRejection;
		private int maxCapHomeOwner;
		private int maxCapNotHomeOwner;
		private int lowCreditScore;
		private int lowTotalAnnualTurnover;
		private int lowTotalThreeMonthTurnover;
		private int defaultFeedbackValue;
		private int totalTimeToWaitForMarketplacesUpdate;
		private int intervalWaitForMarketplacesUpdate;
		private int totalTimeToWaitForExperianCompanyCheck;
		private int intervalWaitForExperianCompanyCheck;
		private int totalTimeToWaitForExperianConsumerCheck;
		private int intervalWaitForExperianConsumerCheck;
		private int totalTimeToWaitForAmlCheck;
		private int intervalWaitForAmlCheck;

		// Loaded from DB per customer
		private bool customerStatusIsEnabled;
		private bool customerStatusIsWarning;
		private bool isOffline;
		private bool isBrokerCustomer;
		private string appEmail;
		private string companyType;
		private string experianRefNum;
		private string appFirstName;
		private string appSurname;
		private string appGender;
		private string appHomeOwner;
		private string appAccountNumber;
		private string appSortCode;
		private DateTime appRegistrationDate;
		private string appBankAccountType;
		private bool wasMainStrategyExecutedBefore;

		private int minExperianScore;
		private int maxExperianScore;
		private int experianConsumerScore;
		private int allMPsNum;
		private AutoDecisionResponse autoDecisionResponse;
		private int numOfDefaultAccounts;
		private MedalMultiplier medalType;
		private decimal loanOfferApr;
		private int loanOfferRepaymentPeriod;
		private decimal loanOfferInterestRate;
		private int loanOfferUseSetupFee;
		private int loanOfferLoanTypeId;
		private int loanOfferIsLoanTypeSelectionAllowed;
		private int loanOfferDiscountPlanId;
		private bool useBrokerSetupFee;
		private int manualSetupFeeAmount;
		private decimal manualSetupFeePercent;
		private int loanSourceId;
		private int isCustomerRepaymentPeriodSelectionAllowed;
		private decimal loanOfferReApprovalSum;
		private decimal loanOfferReApprovalFullAmount;
		private decimal loanOfferReApprovalRemainingAmount;
		private decimal loanOfferReApprovalFullAmountOld;
		private decimal loanOfferReApprovalRemainingAmountOld;
		private int offeredCreditLine;
		private double initialExperianConsumerScore;
		private double marketplaceSeniorityDays;
		private int modelLoanOffer;
		private double totalSumOfOrders1YTotal;
		private double totalSumOfOrders1YTotalForRejection;
		private double totalSumOfOrders3MTotalForRejection;
		private bool isFirstLoan;

		#endregion fields

		#endregion private
	} // class MainStrategy

	#endregion class MainStrategy
} // namespace

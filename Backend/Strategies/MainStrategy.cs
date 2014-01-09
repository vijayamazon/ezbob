namespace EzBob.Backend.Strategies
{
	using AutoDecisions;
	using Models;
	using ScoreCalculation;
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.Linq;
	using System.Threading;
	using EZBob.DatabaseLib.Model.Database;
	using ExperianLib;
	using ExperianLib.Ebusiness;
	using ExperianLib.IdIdentityHub;
	using EzBobIntegration.Web_References.Consumer;
	using Ezbob.Database;
	using Ezbob.Logger;

	#region enum NewCreditLineOption

	public enum NewCreditLineOption
	{
		SkipEverything = 1,
		UpdateEverythingExceptMp = 2,
		UpdateEverythingAndApplyAutoRules = 3,
		UpdateEverythingAndGoToManualDecision = 4
	} // enum NewCreditLineOption

	#endregion enum NewCreditLineOption

	#region class MainStrategy

	public class MainStrategy : AStrategy
	{
		#region public

		#region constructors

		#region constructor - flow #1

		public MainStrategy(
			int customerId,
			NewCreditLineOption newCreditLine,
			int avoidAutoDescison,
			AConnection oDb,
			ASafeLog oLog
		)
			: this(customerId, newCreditLine, avoidAutoDescison, false, oDb, oLog)
		{
		} // constructor

		public MainStrategy(
			int customerId,
			NewCreditLineOption newCreditLine,
			int avoidAutoDescison,
			bool isUnderwriterForced,
			AConnection oDb,
			ASafeLog oLog
		)
			: base(oDb, oLog)
		{
			medalScoreCalculator = new MedalScoreCalculator(oLog);
			mailer = new StrategiesMailer(DB, Log);
			this.customerId = customerId;
			newCreditLineOption = newCreditLine;
			avoidAutomaticDescison = avoidAutoDescison;
			underwriterCheck = isUnderwriterForced;
		} // constructor

		#endregion constructor - flow #1

		#region constructor - flow #2

		public MainStrategy(
			int customerId,
			int checkType,
			string houseNumber,
			string houseName,
			string street,
			string district,
			string town,
			string county,
			string postcode,
			string bankAccount,
			string sortCode,
			int avoidAutoDescison,
			AConnection oDb,
			ASafeLog oLog
		)
			: base(oDb, oLog)
		{
			mailer = new StrategiesMailer(DB, Log);
			this.customerId = customerId;
			useCustomIdHubAddress = checkType;
			underwriterCheck = true;
			idhubHouseNumber = houseNumber;
			idhubHouseName = houseName;
			idhubStreet = street;
			idhubDistrict = district;
			idhubTown = town;
			idhubCounty = county;
			idhubPostCode = postcode;
			idhubAccountNumber = bankAccount;
			idhubBranchCode = sortCode;
			avoidAutomaticDescison = avoidAutoDescison;
		} // constructor

		#endregion constructor - flow #2

		#endregion constructors

		#region property Name

		public override string Name { get { return "Main strategy"; } } // Name

		#endregion property Name

		#region method Execute

		public override void Execute()
		{
			ReadConfigurations();
			GetPersonalInfo();
			strategyHelper.GetZooplaData(customerId);

			if (!customerStatusIsEnabled || customerStatusIsWarning)
			{
				enableAutomaticReApproval = false;
				enableAutomaticApproval = false;
			} // if

			if (isOffline)
			{
				enableAutomaticReApproval = false;
				enableAutomaticApproval = false;
				enableAutomaticReRejection = false;
				enableAutomaticRejection = false;
			} // if

			if (newCreditLineOption != NewCreditLineOption.SkipEverything && newCreditLineOption != NewCreditLineOption.UpdateEverythingExceptMp)
			{
				if (!WaitForMarketplacesToFinishUpdates())
				{
					var variables = new Dictionary<string, string> {
						{"UserEmail", appEmail},
						{"CustomerID", customerId.ToString(CultureInfo.InvariantCulture)},
						{"ApplicationID", appEmail}
					};

					mailer.SendToEzbob(variables, "Mandrill - No Information about shops", "No information about customer marketplace");

					return;
				} // if
			} // if

			if (newCreditLineOption != NewCreditLineOption.SkipEverything)
			{
				UpdateCompanyScore();
				GetAddresses();

				string experianConsumerError;
				string experianConsumerErrorPrev = null;

				GetConsumerInfo(appFirstName, appSurname, appGender, appDateOfBirth, 0, appLine1, appLine2, appLine3, appLine4, appLine5, appLine6, out experianConsumerError);

				if (!string.IsNullOrEmpty(experianConsumerError) && appTimeAtAddress == 1 && !string.IsNullOrEmpty(appLine6Prev))
					GetConsumerInfo(appFirstName, appSurname, appGender, appDateOfBirth, 0, appLine1Prev, appLine2Prev, appLine3Prev, appLine4Prev, appLine5Prev, appLine6Prev, out experianConsumerErrorPrev);

				if (experianBirthDate.Year == 1900 && experianBirthDate.Month == 1 && experianBirthDate.Day == 1)
					experianBirthDate = appDateOfBirth;

				minExperianScore = experianConsumerScore;
				inintialExperianConsumerScore = experianConsumerScore;

				UpdateExperianConsumer(appFirstName, appSurname, appLine6, experianConsumerError, experianConsumerScore, 0);
				UpdateExperianConsumer(appFirstName, appSurname, appLine6Prev, experianConsumerErrorPrev, experianConsumerScore, 0);

				if (companyType != "Entrepreneur")
				{
					DataTable dt = DB.ExecuteReader(
						"GetDirectorsAddresses",
						CommandSpecies.StoredProcedure,
						new QueryParameter("CustomerId", customerId)
					);

					foreach (DataRow row in dt.Rows)
					{
						var sr = new SafeReader(row);
						int appDirId = sr["DirId"];
						string dirLine1 = sr["DirLine1"];
						string dirLine2 = sr["DirLine2"];
						string dirLine3 = sr["DirLine3"];
						string dirLine4 = sr["DirLine4"];
						string dirLine5 = sr["DirLine5"];
						string dirLine6 = sr["DirLine6"];
						string appDirName = sr["DirName"];
						string appDirSurname = sr["DirSurname"];
						DateTime dirBirthdate = sr["DirDateOfBirth"];
						string dirGender = sr["DirGender"];

						if (string.IsNullOrEmpty(appDirName) || string.IsNullOrEmpty(appDirSurname))
							continue;

						string experianDirectorError;
						GetConsumerInfo(appDirName, appDirSurname, dirGender, dirBirthdate, appDirId, dirLine1, dirLine2, dirLine3, dirLine4, dirLine5, dirLine6, out experianDirectorError);

						if (experianConsumerScore > 0 && experianConsumerScore < minExperianScore)
							minExperianScore = experianConsumerScore;

						UpdateExperianConsumer(appDirName, appDirSurname, dirLine6, experianDirectorError, experianConsumerScore, appDirId);
					} // foreach
				} // if

				AmlAndBwa();

				DB.ExecuteNonQuery(
					"UpdateExperianBWA_AML",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", customerId),
					new QueryParameter("BWAResult", experianBwaResult),
					new QueryParameter("AMLResult", experianAmlResult)
				);
			} // if

			DataTable scoreCardDataTable = DB.ExecuteReader(
				"GetScoreCardData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);
			
			var scoreCardResults = new SafeReader(scoreCardDataTable.Rows[0]);
			string maritalStatusStr = scoreCardResults["MaritalStatus"];
			MaritalStatus maritalStatus;
			if (!Enum.TryParse(maritalStatusStr, true, out maritalStatus))
			{
				Log.Warn("Cant parse marital status:{0}. Will use 'Other'", maritalStatusStr);
				maritalStatus = MaritalStatus.Other;
			}
			int modelMaxFeedback = scoreCardResults.IntWithDefault("MaxFeedback", defaultFeedbackValue);
			
			int modelMPsNumber = scoreCardResults["MPsNumber"];
			int modelEzbobSeniority = scoreCardResults["EZBOBSeniority"];
			int modelOnTimeLoans = scoreCardResults["OnTimeLoans"];
			int modelLatePayments = scoreCardResults["LatePayments"];
			int modelEarlyPayments = scoreCardResults["EarlyPayments"];

			bool firstRepaymentDatePassed = false;

			DateTime modelFirstRepaymentDate = scoreCardResults["FirstRepaymentDate"];
			if (modelFirstRepaymentDate != default(DateTime))
			{
				firstRepaymentDatePassed = modelFirstRepaymentDate < DateTime.UtcNow;
			}

			totalSumOfOrders1YTotal = strategyHelper.GetAnualTurnOverByCustomer(customerId);
			totalSumOfOrders3MTotal = strategyHelper.GetTotalSumOfOrders3M(customerId);
			marketplaceSeniorityDays = strategyHelper.MarketplaceSeniority(customerId);
			decimal totalSumOfOrdersForLoanOffer = (decimal)strategyHelper.GetTotalSumOfOrdersForLoanOffer(customerId);

			ScoreMedalOffer scoringResult = medalScoreCalculator.CalculateMedalScore(totalSumOfOrdersForLoanOffer, minExperianScore, (decimal)marketplaceSeniorityDays / 365, modelMaxFeedback, maritalStatus, appGender == "M" ? Gender.M : Gender.F, modelMPsNumber, firstRepaymentDatePassed, modelEzbobSeniority, modelOnTimeLoans, modelLatePayments, modelEarlyPayments);
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
				new QueryParameter("pMedal", scoringResult.Medal),
				new QueryParameter("pScorePoints", scoringResult.ScorePoints),
				new QueryParameter("pScoreResult", scoringResult.ScoreResult)
			);

			if (newCreditLineOption == NewCreditLineOption.SkipEverything ||
				newCreditLineOption == NewCreditLineOption.UpdateEverythingExceptMp ||
				newCreditLineOption == NewCreditLineOption.UpdateEverythingAndGoToManualDecision ||
				avoidAutomaticDescison == 1)
				{
					enableAutomaticApproval = false;
					enableAutomaticReApproval = false;
					enableAutomaticRejection = false;
					enableAutomaticReRejection = false;
				}

			DataTable defaultAccountsNumDataTable = DB.ExecuteReader(
				"GetNumberOfDefaultAccounts",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("Months", rejectDefaultsMonthsNum),
				new QueryParameter("Amount", rejectDefaultsAmount)
			);

			var defaultAccountsNumResults = new SafeReader(defaultAccountsNumDataTable.Rows[0]);
			numOfDefaultAccounts = defaultAccountsNumResults["NumOfDefaultAccounts"];

			DataTable lastOfferDataTable = DB.ExecuteReader(
				"GetLastOfferForAutomatedDecision",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);

			if (lastOfferDataTable.Rows.Count == 1)
			{
				var lastOfferResults = new SafeReader(lastOfferDataTable.Rows[0]);
				loanOfferReApprovalFullAmount = lastOfferResults["ReApprovalFullAmountNew"];
				loanOfferReApprovalRemainingAmount = lastOfferResults["ReApprovalRemainingAmount"];
				loanOfferReApprovalFullAmountOld = lastOfferResults["ReApprovalFullAmountOld"];
				loanOfferReApprovalRemainingAmountOld = lastOfferResults["ReApprovalRemainingAmountOld"];
				loanOfferApr = lastOfferResults["APR"];
				loanOfferRepaymentPeriod = lastOfferResults["RepaymentPeriod"];
				loanOfferExpirianRating = lastOfferResults["ExpirianRating"];
				loanOfferInterestRate = lastOfferResults["InterestRate"];
				loanOfferUseSetupFee = lastOfferResults["UseSetupFee"];
				loanOfferLoanTypeId = lastOfferResults["LoanTypeId"];
				loanOfferIsLoanTypeSelectionAllowed = lastOfferResults["IsLoanTypeSelectionAllowed"];
				loanOfferDiscountPlanId = lastOfferResults["DiscountPlanId"];
				loanSourceId = lastOfferResults["LoanSourceID"];
				isCustomerRepaymentPeriodSelectionAllowed = lastOfferResults["IsCustomerRepaymentPeriodSelectionAllowed"];
				useBrokerSetupFee = lastOfferResults["UseBrokerSetupFee"];
			}

			DataTable basicInterestRateDataTable = DB.ExecuteReader(
				"GetBasicInterestRate",
				CommandSpecies.StoredProcedure,
				new QueryParameter("Score", inintialExperianConsumerScore)
			);

			var basicInterestRateRow = new SafeReader(basicInterestRateDataTable.Rows[0]);
			loanIntrestBase = basicInterestRateRow["LoanIntrestBase"];

			if (loanOfferReApprovalRemainingAmount < 1000) // TODO: make this 1000 configurable
				loanOfferReApprovalRemainingAmount = 0;

			if (loanOfferReApprovalRemainingAmountOld < 500) // TODO: make this 500 configurable
				loanOfferReApprovalRemainingAmountOld = 0;

			loanOfferReApprovalSum = new decimal[] {
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

			autoDecisionResponse = AutoDecisionMaker.MakeDecision(CreateAutoDecisionRequest(), DB, Log);
			modelLoanOffer = autoDecisionResponse.ModelLoanOffer;

			if (underwriterCheck)
			{
				DB.ExecuteNonQuery(
					"Update_Main_Strat_Finish_Date",
					CommandSpecies.StoredProcedure,
					new QueryParameter("UserId", customerId)
				);

				return;
			} // if

			DB.ExecuteNonQuery(
				"UpdateScoringResultsNew",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("CreditResult", autoDecisionResponse.CreditResult),
				new QueryParameter("SystemDecision", autoDecisionResponse.SystemDecision),
				new QueryParameter("Status", autoDecisionResponse.UserStatus),
				new QueryParameter("Medal", medalType),
				new QueryParameter("ApplyForLoan", autoDecisionResponse.AppApplyForLoan),
				new QueryParameter("ValidFor", autoDecisionResponse.AppValidFor)
			);

			DB.ExecuteNonQuery(
				"UpdateCashRequests",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("SystemCalculatedAmount", modelLoanOffer),
				new QueryParameter("ManagerApprovedSum", offeredCreditLine),
				new QueryParameter("SystemDecision", autoDecisionResponse.SystemDecision),
				new QueryParameter("MedalType", medalType),
				new QueryParameter("ScorePoints", scoringResult.ScoreResult),
				new QueryParameter("ExpirianRating", experianConsumerScore),
				new QueryParameter("AnualTurnover", totalSumOfOrders1YTotal),
				new QueryParameter("InterestRate", loanIntrestBase)
			);

			if (autoDecisionResponse.UserStatus == "Approved")
			{
				if (autoDecisionResponse.IsAutoApproval)
				{
					DB.ExecuteNonQuery(
						"UpdateAutoApproval",
						CommandSpecies.StoredProcedure,
						new QueryParameter("CustomerId", customerId),
						new QueryParameter("AutoApproveAmount", autoDecisionResponse.AutoApproveAmount)
					);

					var variables = new Dictionary<string, string> {
						{"ApprovedReApproved", "Approved"},
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
						{"OfferValidUntil", autoDecisionResponse.AppValidFor.HasValue ? autoDecisionResponse.AppValidFor.Value.ToString(CultureInfo.InvariantCulture) : string.Empty}
					};

					mailer.SendToEzbob(variables, "Mandrill - User is approved or re-approved", "User was automatically approved");

					if (isFirstLoan)
					{
						var customerMailVariables = new Dictionary<string, string> {
							{"FirstName", appFirstName},
							{"LoanAmount", autoDecisionResponse.AutoApproveAmount.ToString(CultureInfo.InvariantCulture)}
						};

						mailer.SendToCustomerAndEzbob(
							customerMailVariables,
							appEmail,
							"Mandrill - Approval (1st time)",
							"Congratulations " + appFirstName + ", £" + autoDecisionResponse.AutoApproveAmount + " is available to fund your business today"
						);

						strategyHelper.AddApproveIntoDecisionHistory(customerId, "Auto Approval");
						DB.ExecuteNonQuery(
							"Update_Main_Strat_Finish_Date",
							CommandSpecies.StoredProcedure,
							new QueryParameter("UserId", customerId)
						);
					}
					else
					{
						var customerMailVariables = new Dictionary<string, string> {
							{"FirstName", appFirstName},
							{"LoanAmount", autoDecisionResponse.AutoApproveAmount.ToString(CultureInfo.InvariantCulture)}
						};

						mailer.SendToCustomerAndEzbob(
							customerMailVariables,
							appEmail,
							"Mandrill - Approval (not 1st time)",
							"Congratulations " + appFirstName + ", £" + autoDecisionResponse.AutoApproveAmount + " is available to fund your business today"
						);

						strategyHelper.AddApproveIntoDecisionHistory(customerId, "AutoApproval");
						DB.ExecuteNonQuery(
							"Update_Main_Strat_Finish_Date",
							CommandSpecies.StoredProcedure,
							new QueryParameter("UserId", customerId)
						);
					} // if
				}
				else
				{
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
						new QueryParameter("OfferValidDays", autoDecisionResponse.LoanOfferOfferValidDays),
						new QueryParameter("EmailSendingBanned", autoDecisionResponse.LoanOfferEmailSendingBannedNew),
						new QueryParameter("LoanTypeId", loanOfferLoanTypeId),
						new QueryParameter("UnderwriterComment", autoDecisionResponse.LoanOfferUnderwriterComment),
						new QueryParameter("IsLoanTypeSelectionAllowed", loanOfferIsLoanTypeSelectionAllowed),
						new QueryParameter("DiscountPlanId", loanOfferDiscountPlanId),
						new QueryParameter("ExperianRating", loanOfferExpirianRating),
						new QueryParameter("LoanSourceId", loanSourceId),
						new QueryParameter("IsCustomerRepaymentPeriodSelectionAllowed", isCustomerRepaymentPeriodSelectionAllowed),
						new QueryParameter("UseBrokerSetupFee", useBrokerSetupFee)
					);

					var variables = new Dictionary<string, string> {
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
						{"OfferValidUntil", autoDecisionResponse.AppValidFor.HasValue ? autoDecisionResponse.AppValidFor.Value.ToString(CultureInfo.InvariantCulture) : string.Empty}
					};

					mailer.SendToEzbob(variables, "Mandrill - User is approved or re-approved", "User was automatically Re-Approved");

					if (!enableAutomaticReApproval)
					{
						DB.ExecuteNonQuery(
							"Update_Main_Strat_Finish_Date",
							CommandSpecies.StoredProcedure,
							new QueryParameter("UserId", customerId)
						);
					}
					else
					{
						var customerMailVariables = new Dictionary<string, string> {
							{"FirstName", appFirstName},
							{"LoanAmount", loanOfferReApprovalSum.ToString(CultureInfo.InvariantCulture)}
						};

						mailer.SendToCustomerAndEzbob(
							customerMailVariables,
							appEmail,
							"Mandrill - Approval (not 1st time)", "Congratulations " + appFirstName + ", £" + loanOfferReApprovalSum + " is available to fund your business today"
						);

						strategyHelper.AddApproveIntoDecisionHistory(customerId, "Auto Re-Approval");

						DB.ExecuteNonQuery(
							"Update_Main_Strat_Finish_Date",
							CommandSpecies.StoredProcedure,
							new QueryParameter("UserId", customerId)
						);
					} // if
				} // if
			}
			else if (autoDecisionResponse.UserStatus == "Rejected")
			{
				if ((autoDecisionResponse.IsReRejected && !enableAutomaticReRejection) || (!autoDecisionResponse.IsReRejected && !enableAutomaticRejection))
					SendRejectionExplanationMail(autoDecisionResponse.IsReRejected ? "User was automatically Re-Rejected" : "User was automatically Rejected");
				else
				{
					const string rejectionSubject = "Sorry, ezbob cannot make you a loan offer at this time";
					SendRejectionExplanationMail(rejectionSubject);

					var variables = new Dictionary<string, string> {
						{"FirstName", appFirstName},
						{"EzbobAccount", "https://app.ezbob.com/Customer/Profile"}
					};

					mailer.SendToCustomerAndEzbob(variables, appEmail, "Mandrill - Rejection email", rejectionSubject);
					strategyHelper.AddRejectIntoDecisionHistory(customerId, autoDecisionResponse.AutoRejectReason);
				} // if

				DB.ExecuteNonQuery(
					"Update_Main_Strat_Finish_Date",
					CommandSpecies.StoredProcedure,
					new QueryParameter("UserId", customerId)
				);
			}
			else
			{
				var variables = new Dictionary<string, string> {
					{"RegistrationDate", appRegistrationDate.ToString(CultureInfo.InvariantCulture)},
					{"userID", customerId.ToString(CultureInfo.InvariantCulture)},
					{"Name", appEmail},
					{"FirstName", appFirstName},
					{"Surname", appSurname},
					{"MP_Counter", allMPsNum.ToString(CultureInfo.InvariantCulture)},
					{"MedalType", medalType.ToString()},
					{"SystemDecision", autoDecisionResponse.SystemDecision}
				};

				mailer.SendToEzbob(variables, "Mandrill - User is waiting for decision", "User is now waiting for decision");

				DB.ExecuteNonQuery(
					"Update_Main_Strat_Finish_Date",
					CommandSpecies.StoredProcedure,
					new QueryParameter("UserId", customerId)
				);
			} // if
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		#region properties

		// Helpers
		private readonly StrategiesMailer mailer;
		private readonly StrategyHelper strategyHelper = new StrategyHelper();
		private readonly IdHubService idHubService = new IdHubService();
		private readonly MedalScoreCalculator medalScoreCalculator;

		// Consts
		private const string CpExperianActionsAmlMortality = "The underwriter will need to clarify that the applicant is actually alive (can be a tricky discussion!) and get copies of proof of identity";
		private const string CpExperianActionsAmlAccommodationAddress = "If this is a personal address then reject";
		private const string CpExperianActionsAmlRedirection = "Contact client and confirm reason for redirection. Why is this person’s mail being redirected? Might be a legitimate way of doing e-business";
		private const string CpExperianActionsAmlInconsistencies = "The underwriter will need to ask the applicant for copies of proof of Identity";
		private const string CpExperianActionsAmlpep = "The underwriter needs to confirm that the applicant is not a politician or relative of a politician with the same surname";
		private const string CpExperianActionsAmlAuthentication = "The underwriter will need to ask the applicant for copies of proof of Identity";
		private const string CpExperianActionsBwaNameError = "Underwriter to confirm the account details by asking for copy of statement";
		private const string CpExperianActionsBwaAccountStatus = "Underwriter to confirm the account details by asking for copy of statement";
		private const string CpExperianActionsBwaAddressError = "Underwriter to confirm the account details by asking for copy of statement";

		// Inputs
		private readonly int customerId;
		private readonly NewCreditLineOption newCreditLineOption;
		private readonly bool underwriterCheck;
		private readonly int avoidAutomaticDescison;
		private readonly int useCustomIdHubAddress;
		private readonly string idhubHouseNumber;
		private readonly string idhubHouseName;
		private readonly string idhubStreet;
		private readonly string idhubDistrict;
		private readonly string idhubTown;
		private readonly string idhubCounty;
		private readonly string idhubPostCode;
		private readonly string idhubBranchCode;
		private readonly string idhubAccountNumber;

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

		// Loaded from DB per customer
		private bool customerStatusIsEnabled;
		private bool customerStatusIsWarning;
		private bool isOffline;
		private string appEmail;
		private string companyType;
		private string appLimitedRefNum;
		private string appNonLimitedRefNum;
		private string appFirstName;
		private string appSurname;
		private DateTime appDateOfBirth;
		private string appGender;
		private string appHomeOwner;
		private int appTimeAtAddress;
		private string appAccountNumber;
		private string appSortCode;
		private DateTime appRegistrationDate;
		private string appBankAccountType;

		// Validated as used
		private string appLine1;
		private string appLine2;
		private string appLine3;
		private string appLine4;
		private string appLine5;
		private string appLine6;
		private string appLine1Prev;
		private string appLine2Prev;
		private string appLine3Prev;
		private string appLine4Prev;
		private string appLine5Prev;
		private string appLine6Prev;
		private int minExperianScore;
		private int experianConsumerScore;
		private DateTime experianBirthDate = new DateTime(1900, 1, 1);
		private int allMPsNum;
		private AutoDecisionResponse autoDecisionResponse;
		private int numOfDefaultAccounts;
		private MedalMultiplier medalType;
		private decimal loanOfferApr;
		private int loanOfferRepaymentPeriod;
		private int loanOfferExpirianRating;
		private decimal loanOfferInterestRate;
		private int loanOfferUseSetupFee;
		private int loanOfferLoanTypeId;
		private int loanOfferIsLoanTypeSelectionAllowed;
		private int loanOfferDiscountPlanId;
		private bool useBrokerSetupFee;
		private int loanSourceId;
		private int isCustomerRepaymentPeriodSelectionAllowed;
		private decimal loanIntrestBase;
		private decimal loanOfferReApprovalSum;
		private decimal loanOfferReApprovalFullAmount;
		private decimal loanOfferReApprovalRemainingAmount;
		private decimal loanOfferReApprovalFullAmountOld;
		private decimal loanOfferReApprovalRemainingAmountOld;
		private int offeredCreditLine;
		private double inintialExperianConsumerScore;
		private double marketplaceSeniorityDays;
		private double totalSumOfOrders3MTotal;
		private int modelLoanOffer;
		private double totalSumOfOrders1YTotal;
		private bool isFirstLoan;

		// AML & BWA
		private decimal experianAmlAuthentication;
		private string experianAmlResult;
		private string experianAmlWarning;
		private string experianAmlReject;
		private string experianBwaResult;
		private string experianBwaWarning;
		private string experianBwaPassed;
		private string experianBwaAccountStatus;
		private decimal experianBwaNameScore;
		private decimal experianBwaAddressScore;
		private string experianBwaError;
		private string experianAmlPassed;
		private string experianAmlError;

		#endregion properties

		#region method ReadConfigurations

		private void ReadConfigurations()
		{
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
		} // ReadConfigurations

		#endregion method ReadConfigurations

		#region method GetPersonalInfo

		private void GetPersonalInfo()
		{
			DataTable dt = DB.ExecuteReader("MainStrategyGetPersonalInfo", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", customerId));
			var results = new SafeReader(dt.Rows[0]);

			customerStatusIsEnabled = results["CustomerStatusIsEnabled"];
			customerStatusIsWarning = results["CustomerStatusIsWarning"];
			isOffline = results["IsOffline"];
			appEmail = results["CustomerEmail"];
			companyType = results["CompanyType"];
			appLimitedRefNum = results["LimitedRefNum"];
			appNonLimitedRefNum = results["NonLimitedRefNum"];
			appFirstName = results["FirstName"];
			appSurname = results["Surname"];
			appGender = results["Gender"];
			appDateOfBirth = results["DateOfBirth"];
			appHomeOwner = results["HomeOwner"];
			allMPsNum = results["NumOfMps"];
			appTimeAtAddress = results["TimeAtAddress"];
			appAccountNumber = results["AccountNumber"];
			appSortCode = results["SortCode"];
			appRegistrationDate = results["RegistrationDate"];
			appBankAccountType = results["BankAccountType"];
			int numOfLoans = results["NumOfLoans"];
			isFirstLoan = numOfLoans == 0;
		} // GetPersonalInfo

		#endregion method GetPersonalInfo

		#region method UpdateExperianConsumer

		private void UpdateExperianConsumer(string firstName, string surname, string postCode, string error, int score, int directorId)
		{
			DB.ExecuteNonQuery(
				"UpdateExperianConsumer",
				CommandSpecies.StoredProcedure,
				new QueryParameter("Name", firstName),
				new QueryParameter("Surname", surname),
				new QueryParameter("PostCode", postCode),
				new QueryParameter("ExperianError", error),
				new QueryParameter("ExperianScore", score),
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("DirectorId", directorId)
			);
		} // UpdateExperianConsumer

		#endregion method UpdateExperianConsumer

		#region method GetConsumerInfo

		private void GetConsumerInfo(string firstName, string surname, string gender, DateTime birthDate, int directorId, string line1, string line2, string line3, string line4, string line5, string line6, out string error)
		{
			var consumerService = new ConsumerService();

			var location = new InputLocationDetailsMultiLineLocation
			{
				LocationLine1 = line1,
				LocationLine2 = line2,
				LocationLine3 = line3,
				LocationLine4 = line4,
				LocationLine5 = line5,
				LocationLine6 = line6
			};

			ConsumerServiceResult result = consumerService.GetConsumerInfo(firstName, surname, gender, birthDate, null, location, "PL", customerId, directorId);

			if (result.IsError)
				error = result.Error;
			else
			{
				experianConsumerScore = (int)result.BureauScore;
				experianBirthDate = result.BirthDate;
				error = null;
			}
		} // GetConsumerInfo

		#endregion method GetConsumerInfo

		#region GetAddresses

		private void GetAddresses()
		{
			DataTable dt = DB.ExecuteReader("GetCustomerAddresses", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", customerId));
			var addressesResults = new SafeReader(dt.Rows[0]);
			appLine1 = addressesResults["Line1"];
			appLine2 = addressesResults["Line2"];
			appLine3 = addressesResults["Line3"];
			appLine4 = addressesResults["Line4"];
			appLine5 = addressesResults["Line5"];
			appLine6 = addressesResults["Line6"];
			appLine1Prev = addressesResults["Line1Prev"];
			appLine2Prev = addressesResults["Line2Prev"];
			appLine3Prev = addressesResults["Line3Prev"];
			appLine4Prev = addressesResults["Line4Prev"];
			appLine5Prev = addressesResults["Line5Prev"];
			appLine6Prev = addressesResults["Line6Prev"];
		} // GetAddresses

		#endregion GetAddresses

		#region method UpdateCompanyScore

		private void UpdateCompanyScore()
		{
			if (companyType == "Limited" || companyType == "LLP")
			{
				string experianLimitedError = null;
				decimal experianBureauScoreLimited = 0;

				if (string.IsNullOrEmpty(appLimitedRefNum))
					experianLimitedError = "RefNumber is empty";
				else
				{
					var service = new EBusinessService();
					LimitedResults limitedData = service.GetLimitedBusinessData(appLimitedRefNum, customerId);

					if (!limitedData.IsError)
						experianBureauScoreLimited = limitedData.BureauScore;
					else
						experianLimitedError = limitedData.Error;
				} // if

				DB.ExecuteNonQuery(
					"UpdateExperianBusiness",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CompanyRefNumber", appLimitedRefNum),
					new QueryParameter("ExperianError", experianLimitedError),
					new QueryParameter("ExperianScore", experianBureauScoreLimited),
					new QueryParameter("CustomerId", customerId)
				);
			}
			else if (companyType == "PShip3P" || companyType == "PShip" || companyType == "SoleTrader")
			{
				string experianNonLimitedError = null;
				decimal experianBureauScoreNonLimited = 0;

				if (string.IsNullOrEmpty(appNonLimitedRefNum))
					experianNonLimitedError = "RefNumber is empty";
				else
				{
					var service = new EBusinessService();
					var nonlimitedData = service.GetNotLimitedBusinessData(appNonLimitedRefNum, customerId);

					if (!nonlimitedData.IsError)
						experianBureauScoreNonLimited = nonlimitedData.BureauScore;
					else
						experianNonLimitedError = nonlimitedData.Error;
				} // if

				DB.ExecuteNonQuery(
					"UpdateExperianBusiness",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CompanyRefNumber", appNonLimitedRefNum),
					new QueryParameter("ExperianError", experianNonLimitedError),
					new QueryParameter("ExperianScore", experianBureauScoreNonLimited),
					new QueryParameter("CustomerId", customerId)
				);
			} // if
		} // UpdateCompanyScore

		#endregion method UpdateCompanyScore

		#region method SendRejectionExplanationMail

		private void SendRejectionExplanationMail(string subject)
		{
			var variables = new Dictionary<string, string> {
				{"RegistrationDate", appRegistrationDate.ToString(CultureInfo.InvariantCulture)},
				{"userID", customerId.ToString(CultureInfo.InvariantCulture)},
				{"Name", appEmail},
				{"FirstName", appFirstName},
				{"Surname", appSurname},
				{"MP_Counter", allMPsNum.ToString(CultureInfo.InvariantCulture)},
				{"MedalType", medalType.ToString()},
				{"SystemDecision", autoDecisionResponse.SystemDecision},
				{"ExperianConsumerScore", inintialExperianConsumerScore.ToString(CultureInfo.InvariantCulture)},
				{"CVExperianConsumerScore", lowCreditScore.ToString(CultureInfo.InvariantCulture)},
				{"TotalAnnualTurnover", totalSumOfOrders1YTotal.ToString(CultureInfo.InvariantCulture)},
				{"CVTotalAnnualTurnover", lowTotalAnnualTurnover.ToString(CultureInfo.InvariantCulture)},
				{"Total3MTurnover", totalSumOfOrders3MTotal.ToString(CultureInfo.InvariantCulture)},
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
			};

			mailer.SendToEzbob(variables, "Mandrill - User is rejected by the strategy", subject);
		} // SendRejectionExplanationMail

		#endregion method SendRejectionExplanationMail

		#region method CreateAutoDecisionRequest

		private AutoDecisionRequest CreateAutoDecisionRequest()
		{
			return new AutoDecisionRequest
			{
				CustomerId = customerId,
				EnableAutomaticApproval = enableAutomaticApproval,
				EnableAutomaticReApproval = enableAutomaticReApproval,
				EnableAutomaticRejection = enableAutomaticRejection,
				EnableAutomaticReRejection = enableAutomaticReRejection,
				InitialExperianConsumerScore = inintialExperianConsumerScore,
				ModelLoanOffer = modelLoanOffer,
				IsReRejected = false,
				LoanOfferReApprovalFullAmountOld = loanOfferReApprovalFullAmountOld,
				LoanOfferReApprovalFullAmount = loanOfferReApprovalFullAmount,
				LoanOfferReApprovalRemainingAmount = loanOfferReApprovalRemainingAmount,
				LoanOfferReApprovalRemainingAmountOld = loanOfferReApprovalRemainingAmountOld,
				LowTotalAnnualTurnover = lowTotalAnnualTurnover,
				LowTotalThreeMonthTurnover = lowTotalThreeMonthTurnover,
				MarketplaceSeniorityDays = marketplaceSeniorityDays,
				MinExperianScore = minExperianScore,
				OfferedCreditLine = offeredCreditLine
			};
		} // CreateAutoDecisionRequest

		#endregion method CreateAutoDecisionRequest

		#region method AmlAndBwa

		private void AmlAndBwa()
		{
			AccountVerificationResults accountVerificationResults;
			AuthenticationResults authenticationResults;

			if (useCustomIdHubAddress != 0)
			{
				if (useCustomIdHubAddress != 2)
				{
					authenticationResults = idHubService.AuthenticateForcedWithCustomAddress(appFirstName, null, appSurname, appGender, appDateOfBirth, idhubHouseNumber, idhubHouseName, idhubStreet, idhubDistrict, idhubTown, idhubCounty, idhubPostCode, customerId);
					CreateAmlResultFromAuthenticationReuslts(authenticationResults);

					if (useCustomIdHubAddress != 1)
					{
						if (ShouldRunBwa())
						{
							accountVerificationResults = idHubService.AccountVerificationForcedWithCustomAddress(
								appFirstName, null, appSurname, appGender,
								appDateOfBirth, idhubHouseNumber, idhubHouseName,
								idhubStreet, idhubDistrict, idhubTown, idhubCounty,
								idhubPostCode, idhubBranchCode, idhubAccountNumber,
								customerId
							);

							CreateBwaResultFromAccountVerificationResults(accountVerificationResults);
						} // if
					} // if
				}
				else
				{
					accountVerificationResults = idHubService.AccountVerificationForcedWithCustomAddress(
						appFirstName, null, appSurname, appGender,
						appDateOfBirth, idhubHouseNumber, idhubHouseName,
						idhubStreet, idhubDistrict, idhubTown, idhubCounty,
						idhubPostCode, idhubBranchCode, idhubAccountNumber,
						customerId
					);

					CreateBwaResultFromAccountVerificationResults(accountVerificationResults);
				} // if
			}
			else
			{
				authenticationResults = idHubService.Authenticate(
					appFirstName, null, appSurname,
					appGender, appDateOfBirth, appLine1,
					appLine2, appLine3, appLine4, null,
					appLine6, customerId
				);

				CreateAmlResultFromAuthenticationReuslts(authenticationResults);

				accountVerificationResults = idHubService.AccountVerification(appFirstName, null, appSurname, appGender, appDateOfBirth, appLine1, appLine2, appLine3, appLine4, null, appLine6, appSortCode, appAccountNumber, customerId);

				CreateBwaResultFromAccountVerificationResults(accountVerificationResults);

				if (experianAmlError != "" && appTimeAtAddress == 1 && appLine6Prev != null)
				{
					authenticationResults = idHubService.Authenticate(
						appFirstName, null, appSurname, appGender, appDateOfBirth,
						appLine1Prev, appLine2Prev, appLine3Prev, appLine4Prev, null,
						appLine6Prev, customerId
					);

					CreateAmlResultFromAuthenticationReuslts(authenticationResults);
				}

				if (experianBwaError != "" && appTimeAtAddress == 1 && appLine6Prev != null)
				{
					accountVerificationResults = idHubService.AccountVerification(appFirstName, null, appSurname, appGender, appDateOfBirth, appLine1Prev, appLine2Prev, appLine3Prev, appLine4Prev, null, appLine6Prev, appSortCode, appAccountNumber, customerId);
					CreateBwaResultFromAccountVerificationResults(accountVerificationResults);
				}
			} // if

			if (experianAmlError != "")
				experianAmlResult = "Warning";
			else
			{
				if (experianAmlAuthentication < 40 && experianAmlResult == "Rejected")
				{
					experianAmlWarning = experianAmlWarning +
						"#1,Authentication < 40 (" +
						experianAmlAuthentication +
						")||" +
						CpExperianActionsAmlAuthentication +
						";";
				}
				else
				{
					experianAmlPassed = experianAmlPassed +
						"#1,Authentication >= 40 (" +
						experianAmlAuthentication +
						"];";
				} // if

				if (experianAmlAuthentication < 40 && experianAmlResult != "Rejected")
				{
					experianAmlWarning = experianAmlWarning +
						"#1,Authentication < 40 (" +
						experianAmlAuthentication + ")||" +
						CpExperianActionsAmlAuthentication +
						";";

					experianAmlResult = "Warning";
				}
				else
					experianAmlPassed = experianAmlPassed + "#1,Authentication >= 40 (" + experianAmlAuthentication + "];";
			} // if

			if (useCustomIdHubAddress == 1)
			{
				DataTable dt = DB.ExecuteReader("GetPrevBwaResult", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", customerId));
				var sr = new SafeReader(dt.Rows[0]);
				experianBwaResult = sr["BWAResult"];
			}
			else
			{
				if (appSortCode == null && appAccountNumber == null)
					experianBwaResult = "Not performed";
				else
				{
					if (experianBwaError != "")
						experianBwaResult = "Warning";
					else
					{
						if (appBankAccountType == "Business")
							experianBwaResult = "Not performed";
						else
						{
							experianBwaResult = "Passed";

							if (experianBwaAccountStatus == "No Match")
							{
								experianBwaWarning = experianBwaWarning + "#1, Account Status = No Match||" + CpExperianActionsBwaAccountStatus + ";";
								experianBwaResult = "Warning";
							}
							else
								experianBwaPassed = experianBwaPassed + "#1, Account Status != No Match;";

							if (experianBwaAccountStatus == "Unable to check")
							{
								experianBwaWarning = experianBwaWarning + "#1, Account Status = Unable to check||" + CpExperianActionsBwaAccountStatus + ";";
								experianBwaResult = "Warning";
							}
							else
								experianBwaPassed = experianBwaPassed + "#1, Account Status != Unable to check;";

							if (experianBwaNameScore == 1)
							{
								experianBwaWarning = experianBwaWarning + "#2, Name error = 1||" + CpExperianActionsBwaNameError + ";";
								experianBwaResult = "Warning";
							}
							else
								experianBwaPassed = experianBwaPassed + "#2, Name error != 1;";

							if (experianBwaNameScore == 2)
							{
								experianBwaWarning = experianBwaWarning + "#2, Name error = 2||" + CpExperianActionsBwaNameError + ";";
								experianBwaResult = "Warning";
							}
							else
								experianBwaPassed = experianBwaPassed + "#2, Name error != 2;";

							if (experianBwaNameScore == 3)
							{
								experianBwaWarning = experianBwaWarning + "#2, Name error = 3||" + CpExperianActionsBwaNameError + ";";
								experianBwaResult = "Warning";
							}
							else
								experianBwaPassed = experianBwaPassed + "#2, Name error != 3;";

							if (experianBwaNameScore == 4)
							{
								experianBwaWarning = experianBwaWarning + "#2, Name error = 4||" + CpExperianActionsBwaNameError + ";";
								experianBwaResult = "Warning";
							}
							else
								experianBwaPassed = experianBwaPassed + "#2, Name error != 4;";

							if (experianBwaAddressScore == 1)
							{
								experianBwaWarning = experianBwaWarning + "#3, Address error = 1||" + CpExperianActionsBwaAddressError + ";";
								experianBwaResult = "Warning";
							}
							else
								experianBwaPassed = experianBwaPassed + "#3, Address error != 1;";

							if (experianBwaAddressScore == 2)
							{
								experianBwaWarning = experianBwaWarning + "#3, Address error = 2||" + CpExperianActionsBwaAddressError + ";";
								experianBwaResult = "Warning";
							}
							else
								experianBwaPassed = experianBwaPassed + "#3, Address error != 2;";

							if (experianBwaAddressScore == 3)
							{
								experianBwaWarning = experianBwaWarning + "#3, Address error = 3||" + CpExperianActionsBwaAddressError + ";";
								experianBwaResult = "Warning";
							}
							else
								experianBwaPassed = experianBwaPassed + "#3, Address error != 3;";

							if (experianBwaAddressScore == 4)
							{
								experianBwaWarning = experianBwaWarning + "#3, Address error = 4||" + CpExperianActionsBwaAddressError + ";";
								experianBwaResult = "Warning";
							}
							else
								experianBwaPassed = experianBwaPassed + "#3, Address error != 4;";
						} // if
					} // if
				} // if
			} // if
		} // AmlAndBwa

		#endregion method AmlAndBwa

		#region method ShouldRunBwa

		private bool ShouldRunBwa()
		{
			return appBankAccountType == "Personal" && bwaBusinessCheck == "1" && appSortCode != null && appAccountNumber != null;
		} // ShouldRunBwa

		#endregion method ShouldRunBwa

		#region method CreateBwaResultFromAccountVerificationResults

		private void CreateBwaResultFromAccountVerificationResults(AccountVerificationResults results)
		{
			if (!results.HasError)
			{
				Log.Info("account status: {0}, name score: {1}, address score: {2}", results.AccountStatus, results.NameScore, results.AddressScore);
				experianBwaAccountStatus = results.AccountStatus;
				experianBwaNameScore = results.NameScore;
				experianBwaAddressScore = results.AddressScore;
			}
			else
				experianBwaError = results.Error;
		} // CreateBwaResultFromAccountVerificationResults

		#endregion method CreateBwaResultFromAccountVerificationResults

		#region method CreateAmlResultFromAuthenticationReuslts

		private void CreateAmlResultFromAuthenticationReuslts(AuthenticationResults results)
		{
			if (!results.HasError)
			{
				experianAmlAuthentication = results.AuthenticationIndexType;
				experianAmlResult = "Passed";

				foreach (var returnedHrp in results.ReturnedHRP)
				{
					if (returnedHrp.HighRiskPolRuleID == "U001")
					{
						experianAmlWarning += "#2, Mortality||" + CpExperianActionsAmlMortality + ";";
						experianAmlResult = "Warning";
					}
					else if (returnedHrp.HighRiskPolRuleID == "U004")
					{
						experianAmlWarning += "#3, Accommodation address||" + CpExperianActionsAmlAccommodationAddress + ";";
						experianAmlResult = "Warning";
					}
					else if (returnedHrp.HighRiskPolRuleID == "U007")
						experianAmlReject += "#4, Developed Identity;";
					else if (returnedHrp.HighRiskPolRuleID == "U013")
					{
						experianAmlWarning += "#5, Redirection||" + CpExperianActionsAmlRedirection + ";";
						experianAmlResult = "Warning";
					}
					else if (returnedHrp.HighRiskPolRuleID == "U015" || returnedHrp.HighRiskPolRuleID == "U131" ||
							 returnedHrp.HighRiskPolRuleID == "U133" || returnedHrp.HighRiskPolRuleID == "U135")
					{
						experianAmlReject += "#6, Sanctions;";
						experianAmlResult = "Warning";
					}
					else if (returnedHrp.HighRiskPolRuleID == "U018")
					{
						experianAmlWarning += "#7, Inconsistencies||" + CpExperianActionsAmlInconsistencies + ";";
						experianAmlResult = "Warning";
					}
					else if (returnedHrp.HighRiskPolRuleID == "U0132" || returnedHrp.HighRiskPolRuleID == "U0134")
					{
						experianAmlWarning += "#8, PEP||" + CpExperianActionsAmlpep + ";";
						experianAmlResult = "Warning";
					}
					else if (returnedHrp.HighRiskPolRuleID == "U007")
					{
						experianAmlReject += "#4, Developed Identity;";
						experianAmlResult = "Rejected";
					}
					else if (returnedHrp.HighRiskPolRuleID != "U001")
						experianAmlPassed += "#2, NO Mortality;";
					else if (returnedHrp.HighRiskPolRuleID != "U004")
						experianAmlPassed += "#3, NO Accommodation address;";
					else if (returnedHrp.HighRiskPolRuleID != "U007")
						experianAmlPassed += "#4, NO Developed Identity;";
					else if (returnedHrp.HighRiskPolRuleID != "U013")
						experianAmlPassed += "#5, NO Redirection;";
					else if (returnedHrp.HighRiskPolRuleID != "U015" || returnedHrp.HighRiskPolRuleID == "U131" ||
							 returnedHrp.HighRiskPolRuleID == "U133" || returnedHrp.HighRiskPolRuleID == "U135")
						experianAmlPassed += "#6, NO Sanctions;";
					else if (returnedHrp.HighRiskPolRuleID != "U018")
						experianAmlPassed += "#7, NO Inconsistencies;";
					else if (returnedHrp.HighRiskPolRuleID != "U0132" || returnedHrp.HighRiskPolRuleID == "U0134")
						experianAmlPassed += "#8, NO PEP;";
					else if (returnedHrp.HighRiskPolRuleID != "U007")
						experianAmlPassed += "#4, NO Developed Identity;";
				} // foreach
			} // if
			else
				experianAmlError = results.Error;
		} // CreateAmlResultFromAuthenticationReuslts

		#endregion method CreateAmlResultFromAuthenticationReuslts

		#region method WaitForMarketplacesToFinishUpdates

		private bool WaitForMarketplacesToFinishUpdates()
		{
			DataTable dt = DB.ExecuteReader(
				"MP_CustomerMarketplacesIsUpdated",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);

			var sr = new SafeReader(dt.Rows[0]);
			bool isUpdated = sr["IsUpdated"];

			DateTime startWaitingTime = DateTime.UtcNow;

			while (!isUpdated)
			{
				if ((DateTime.UtcNow - startWaitingTime).TotalSeconds > totalTimeToWaitForMarketplacesUpdate)
					return false;

				Thread.Sleep(intervalWaitForMarketplacesUpdate);

				dt = DB.ExecuteReader(
					"MP_CustomerMarketplacesIsUpdated",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", customerId)
				);

				sr = new SafeReader(dt.Rows[0]);
				isUpdated = sr["IsUpdated"];
			} // while

			return true;
		} // WaitForMarketplacesToFinishUpdates

		#endregion method WaitForMarketplacesToFinishUpdates

		#endregion private
	} // class MainStrategy

	#endregion class MainStrategy
} // namespace

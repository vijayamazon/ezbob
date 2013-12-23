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

namespace EzBob.Backend.Strategies
{
	using AutoDecisions;
	using Models;
	using ScoreCalculation;

	#region enum NewCreditLineOption

	public enum NewCreditLineOption {
		SkipEverything = 1,
		UpdateEverythingExceptMp = 2,
		UpdateEverythingAndApplyAutoRules = 3,
		UpdateEverythingAndGoToManualDecision = 4
	} // enum NewCreditLineOption

	#endregion enum NewCreditLineOption

	#region class MainStrategy

	public class MainStrategy : AStrategy {
		#region public

		#region constructors

		#region constructor - flow #1

		public MainStrategy(
			int customerId,
			NewCreditLineOption newCreditLine,
			int avoidAutoDescison,
			AConnection oDB,
			ASafeLog oLog
		) : this(customerId, newCreditLine, avoidAutoDescison, false, oDB, oLog)
		{
		} // constructor

		public MainStrategy(
			int customerId,
			NewCreditLineOption newCreditLine,
			int avoidAutoDescison,
			bool isUnderwriterForced,
			AConnection oDB,
			ASafeLog oLog
		) : base(oDB, oLog)
		{
			mailer = new StrategiesMailer(DB, Log);
			CustomerId = customerId;
			newCreditLineOption = newCreditLine;
			avoidAutomaticDescison = avoidAutoDescison;
			Underwriter_Check = isUnderwriterForced;
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
			AConnection oDB,
			ASafeLog oLog
		) : base(oDB, oLog)
		{
			mailer = new StrategiesMailer(DB, Log);
			CustomerId = customerId;
			UseCustomIdHubAddress = checkType;
			Underwriter_Check = true;
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

		public override void Execute() {
			ReadConfigurations();
			GerPersonalInfo();
			strategyHelper.GetZooplaData(CustomerId);

			if (!CustomerStatusIsEnabled || CustomerStatusIsWarning) {
				EnableAutomaticReApproval = false;
				EnableAutomaticApproval = false;
			} // if

			if (IsOffline) {
				EnableAutomaticReApproval = false;
				EnableAutomaticApproval = false;
				EnableAutomaticReRejection = false;
				EnableAutomaticRejection = false;
			} // if

			if (newCreditLineOption != NewCreditLineOption.SkipEverything && newCreditLineOption != NewCreditLineOption.UpdateEverythingExceptMp) {
				if (!WaitForMarketplacesToFinishUpdates()) {
					var variables = new Dictionary<string, string> {
						{"UserEmail", App_email},
						{"CustomerID", CustomerId.ToString(CultureInfo.InvariantCulture)},
						{"ApplicationID", App_email}
					};

					mailer.SendToEzbob(variables, "Mandrill - No Information about shops", "No information about customer marketplace");

					return;
				} // if
			} // if

			if (newCreditLineOption != NewCreditLineOption.SkipEverything) {
				UpdateCompanyScore();
				GetAddresses();

				string ExperianConsumerError;
				string ExperianConsumerErrorPrev = null;

				GetConsumerInfo(App_FirstName, App_Surname, App_Gender, App_DateOfBirth, 0, App_Line1, App_Line2, App_Line3, App_Line4, App_Line5, App_Line6, out ExperianConsumerError);

				if (!string.IsNullOrEmpty(ExperianConsumerError) && App_TimeAtAddress == 1 && !string.IsNullOrEmpty(App_Line6Prev))
					GetConsumerInfo(App_FirstName, App_Surname, App_Gender, App_DateOfBirth, 0, App_Line1Prev, App_Line2Prev, App_Line3Prev, App_Line4Prev, App_Line5Prev, App_Line6Prev, out ExperianConsumerErrorPrev);

				if (ExperianBirthDate.Year == 1900 && ExperianBirthDate.Month == 1 && ExperianBirthDate.Day == 1)
					ExperianBirthDate = App_DateOfBirth;

				MinExperianScore = ExperianConsumerScore;
				Inintial_ExperianConsumerScore = ExperianConsumerScore;

				UpdateExperianConsumer(App_FirstName, App_Surname, App_Line6, ExperianConsumerError, ExperianConsumerScore, CustomerId, 0);
				UpdateExperianConsumer(App_FirstName, App_Surname, App_Line6Prev, ExperianConsumerErrorPrev, ExperianConsumerScore, CustomerId, 0);
				
				if (CompanyType != "Entrepreneur") {
					DataTable dt = DB.ExecuteReader(
						"GetDirectorsAddresses",
						CommandSpecies.StoredProcedure,
						new QueryParameter("CustomerId", CustomerId)
					);

					foreach (DataRow row in dt.Rows) {
						int App_DirId = int.Parse(row["DirId"].ToString());
						string dirLine1 = row["DirLine1"].ToString();
						string dirLine2 = row["DirLine2"].ToString();
						string dirLine3 = row["DirLine3"].ToString();
						string dirLine4 = row["DirLine4"].ToString();
						string dirLine5 = row["DirLine5"].ToString();
						string dirLine6 = row["DirLine6"].ToString();
						string App_DirName = row["DirName"].ToString();
						string App_DirSurname = row["DirSurname"].ToString();
						DateTime dirBirthdate = DateTime.Parse(row["DirDateOfBirth"].ToString());
						string dirGender = row["DirGender"].ToString();

						if (string.IsNullOrEmpty(App_DirName) || string.IsNullOrEmpty(App_DirSurname))
							continue;

						string ExperianDirectorError;
						GetConsumerInfo(App_DirName, App_DirSurname, dirGender, dirBirthdate, App_DirId, dirLine1, dirLine2, dirLine3, dirLine4, dirLine5, dirLine6, out ExperianDirectorError);

						if (ExperianConsumerScore > 0 && ExperianConsumerScore < MinExperianScore)
							MinExperianScore = ExperianConsumerScore;

						UpdateExperianConsumer(App_DirName, App_DirSurname, dirLine6, ExperianDirectorError, ExperianConsumerScore, CustomerId, App_DirId);
					} // foreach
				} // if

				AmlAndBwa(CustomerId);

				DB.ExecuteReader(
					"UpdateExperianBWA_AML",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", CustomerId),
					new QueryParameter("BWAResult", ExperianBwaResult),
					new QueryParameter("AMLResult", ExperianAmlResult)
				);
			} // if

			DataTable scoreCardDataTable = DB.ExecuteReader(
				"GetDirectorsAddresses",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", CustomerId)
			);

			DataRow scoreCardResults = scoreCardDataTable.Rows[0];
			var maritalStatus = (MaritalStatus) int.Parse(scoreCardResults["MaritalStatus"].ToString());
			string maxFeedbackRaw = scoreCardResults["MaxFeedback"].ToString();
			int Model_MaxFeedback;
			
			if (string.IsNullOrEmpty(maxFeedbackRaw)) {
				Log.Info("No feedback information exists. Will use {0}.", defaultFeedbackValue);
				Model_MaxFeedback = defaultFeedbackValue;
			}
			else
				Model_MaxFeedback = int.Parse(maxFeedbackRaw);

			int Model_MPsNumber = int.Parse(scoreCardResults["MPsNumber"].ToString());
			int Model_EZBOBSeniority = int.Parse(scoreCardResults["EZBOBSeniority"].ToString());
			int Model_OnTimeLoans = int.Parse(scoreCardResults["OnTimeLoans"].ToString());
			int Model_LatePayments = int.Parse(scoreCardResults["LatePayments"].ToString());
			int Model_EarlyPayments = int.Parse(scoreCardResults["EarlyPayments"].ToString());
			DateTime Model_FirstRepaymentDate = DateTime.Parse(scoreCardResults["FirstRepaymentDate"].ToString());

			TotalSumOfOrders1YTotal = strategyHelper.GetAnualTurnOverByCustomer(CustomerId);
			TotalSumOfOrders3MTotal = strategyHelper.GetTotalSumOfOrders3M(CustomerId);
			MarketplaceSeniorityDays = strategyHelper.MarketplaceSeniority(CustomerId);
			decimal TotalSumOfOrdersForLoanOffer = (decimal)strategyHelper.GetTotalSumOfOrdersForLoanOffer(CustomerId);

			ScoreMedalOffer scoringResult = medalScoreCalculator.CalculateMedalScore(TotalSumOfOrdersForLoanOffer, MinExperianScore, (decimal)MarketplaceSeniorityDays / 365, Model_MaxFeedback, maritalStatus, App_Gender == "M" ? Gender.M : Gender.F, Model_MPsNumber, Model_FirstRepaymentDate < DateTime.UtcNow, Model_EZBOBSeniority, Model_OnTimeLoans, Model_LatePayments, Model_EarlyPayments);
			ModelLoanOffer = scoringResult.MaxOffer;
			
			MedalType = scoringResult.Medal;

			DB.ExecuteNonQuery(
				"CustomerScoringResult_Insert",
				CommandSpecies.StoredProcedure,
				new QueryParameter("pCustomerId", CustomerId),
				new QueryParameter("pAC_Parameters", scoringResult.AcParameters),
				new QueryParameter("AC_Descriptors", scoringResult.AcDescriptors),
				new QueryParameter("Result_Weight", scoringResult.ResultWeigts),
				new QueryParameter("pResult_MAXPossiblePoints", scoringResult.ResultMaxPoints),
				new QueryParameter("pMedal", scoringResult.Medal),
				new QueryParameter("pScorePoints", scoringResult.ScorePoints),
				new QueryParameter("pScoreResult", scoringResult.ScoreResult)
			);
			
			if (
				newCreditLineOption == NewCreditLineOption.SkipEverything ||
				newCreditLineOption == NewCreditLineOption.UpdateEverythingExceptMp ||
				newCreditLineOption == NewCreditLineOption.UpdateEverythingAndGoToManualDecision ||
				avoidAutomaticDescison == 1
			) {
				EnableAutomaticApproval = false;
				EnableAutomaticReApproval = false;
				EnableAutomaticRejection = false;
				EnableAutomaticReRejection = false;
			}
			
			DataTable defaultAccountsNumDataTable = DB.ExecuteReader(
				"GetNumberOfDefaultAccounts",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", CustomerId),
				new QueryParameter("Months", Reject_Defaults_MonthsNum),
				new QueryParameter("Amount", Reject_Defaults_Amount)
			);

			DataRow defaultAccountsNumResults = defaultAccountsNumDataTable.Rows[0];
			NumOfDefaultAccounts = int.Parse(defaultAccountsNumResults["NumOfDefaultAccounts"].ToString());

			DataTable lastOfferDataTable = DB.ExecuteReader(
				"GetLastOfferForAutomatedDecision",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", CustomerId)
			);

			DataRow lastOfferResults = lastOfferDataTable.Rows[0];
			LoanOffer_ReApprovalFullAmount = int.Parse(lastOfferResults["ReApprovalFullAmountNew"].ToString());
			LoanOffer_ReApprovalRemainingAmount = int.Parse(lastOfferResults["ReApprovalRemainingAmount"].ToString());
			LoanOffer_ReApprovalFullAmountOld = int.Parse(lastOfferResults["ReApprovalFullAmountOld"].ToString());
			LoanOffer_ReApprovalRemainingAmountOld = int.Parse(lastOfferResults["ReApprovalRemainingAmountOld"].ToString());
			LoanOffer_APR = int.Parse(lastOfferResults["APR"].ToString());
			LoanOffer_RepaymentPeriod = int.Parse(lastOfferResults["RepaymentPeriod"].ToString());
			LoanOffer_ExpirianRating = int.Parse(lastOfferResults["ExpirianRating"].ToString());
			LoanOffer_InterestRate = int.Parse(lastOfferResults["InterestRate"].ToString());
			LoanOffer_UseSetupFee = int.Parse(lastOfferResults["UseSetupFee"].ToString());
			LoanOffer_LoanTypeId = int.Parse(lastOfferResults["LoanTypeId"].ToString());
			LoanOffer_IsLoanTypeSelectionAllowed = int.Parse(lastOfferResults["IsLoanTypeSelectionAllowed"].ToString());
			LoanOffer_DiscountPlanId = int.Parse(lastOfferResults["DiscountPlanId"].ToString());
			LoanSourceId = int.Parse(lastOfferResults["LoanSourceID"].ToString());
			IsCustomerRepaymentPeriodSelectionAllowed = int.Parse(lastOfferResults["IsCustomerRepaymentPeriodSelectionAllowed"].ToString());

			DataTable basicInterestRateDataTable = DB.ExecuteReader(
				"GetBasicInterestRate",
				CommandSpecies.StoredProcedure,
				new QueryParameter("Score", Inintial_ExperianConsumerScore)
			);

			DataRow basicInterestRateRow = basicInterestRateDataTable.Rows[0];
			LoanIntrestBase = decimal.Parse(basicInterestRateRow["LoanIntrestBase"].ToString());
			
			if (LoanOffer_ReApprovalRemainingAmount < 1000) // TODO: make this 1000 configurable
				LoanOffer_ReApprovalRemainingAmount = 0;

			if (LoanOffer_ReApprovalRemainingAmountOld < 500) // TODO: make this 500 configurable
				LoanOffer_ReApprovalRemainingAmountOld = 0;

			LoanOffer_ReApprovalSum = new int[] {
				LoanOffer_ReApprovalFullAmount,
				LoanOffer_ReApprovalRemainingAmount,
				LoanOffer_ReApprovalFullAmountOld,
				LoanOffer_ReApprovalRemainingAmountOld
			}.Max();

			OfferedCreditLine = ModelLoanOffer;

			if (App_HomeOwner == "Home owner" && MaxCapHomeOwner < LoanOffer_ReApprovalSum)
				LoanOffer_ReApprovalSum = MaxCapHomeOwner;

			if (App_HomeOwner != "Home owner" && MaxCapNotHomeOwner < LoanOffer_ReApprovalSum)
				LoanOffer_ReApprovalSum = MaxCapNotHomeOwner;

			if (App_HomeOwner == "Home owner" && MaxCapHomeOwner < OfferedCreditLine)
				OfferedCreditLine = MaxCapHomeOwner;

			if (App_HomeOwner != "Home owner" && MaxCapNotHomeOwner < OfferedCreditLine)
				OfferedCreditLine = MaxCapNotHomeOwner;

			autoDecisionResponse = AutoDecisionMaker.MakeDecision(CreateAutoDecisionRequest(), DB);
			ModelLoanOffer = autoDecisionResponse.ModelLoanOffer;

			if (Underwriter_Check) {
				DB.ExecuteNonQuery(
					"Update_Main_Strat_Finish_Date", 
					CommandSpecies.StoredProcedure,
					new QueryParameter("UserId", CustomerId)
				);

				return;
			} // if

			DB.ExecuteNonQuery(
				"UpdateScoringResultsNew",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CreditResult", autoDecisionResponse.CreditResult),
				new QueryParameter("SystemDecision", autoDecisionResponse.SystemDecision),
				new QueryParameter("Status", autoDecisionResponse.UserStatus),
				new QueryParameter("Medal", MedalType),
				new QueryParameter("ApplyForLoan", autoDecisionResponse.App_ApplyForLoan),
				new QueryParameter("ValidFor", autoDecisionResponse.App_ValidFor)
			);

			DB.ExecuteNonQuery(
				"UpdateCashRequests",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", CustomerId),
				new QueryParameter("SystemCalculatedAmount", ModelLoanOffer),
				new QueryParameter("ManagerApprovedSum", OfferedCreditLine),
				new QueryParameter("SystemDecision", autoDecisionResponse.SystemDecision),
				new QueryParameter("MedalType", MedalType),
				new QueryParameter("ScorePoints", scoringResult.ScoreResult),
				new QueryParameter("ExpirianRating", ExperianConsumerScore),
				new QueryParameter("AnualTurnover", TotalSumOfOrders1YTotal),
				new QueryParameter("InterestRate", LoanIntrestBase)
			);

			if (autoDecisionResponse.UserStatus == "Approved") {
				if (autoDecisionResponse.IsAutoApproval) {
					DB.ExecuteNonQuery(
						"UpdateAutoApproval",
						CommandSpecies.StoredProcedure,
						new QueryParameter("CustomerId", CustomerId),
						new QueryParameter("AutoApproveAmount", autoDecisionResponse.AutoApproveAmount)
					);

					var variables = new Dictionary<string, string> {
						{"ApprovedReApproved", "Approved"},
						{"RegistrationDate", App_RegistrationDate.ToString(CultureInfo.InvariantCulture)},
						{"userID", CustomerId.ToString(CultureInfo.InvariantCulture)},
						{"Name", App_email},
						{"FirstName", App_FirstName},
						{"Surname", App_Surname},
						{"MP_Counter", AllMPsNum.ToString(CultureInfo.InvariantCulture)},
						{"MedalType", MedalType.ToString()},
						{"SystemDecision", autoDecisionResponse.SystemDecision},
						{"ApprovalAmount", LoanOffer_ReApprovalSum.ToString(CultureInfo.InvariantCulture)},
						{"RepaymentPeriod", LoanOffer_RepaymentPeriod.ToString(CultureInfo.InvariantCulture)},
						{"InterestRate", LoanOffer_InterestRate.ToString(CultureInfo.InvariantCulture)},
						{"OfferValidUntil", autoDecisionResponse.App_ValidFor.ToString(CultureInfo.InvariantCulture)}
					};

					mailer.SendToEzbob(variables, "Mandrill - User is approved or re-approved", "User was automatically approved");

					if (isFirstLoan) {
						var customerMailVariables = new Dictionary<string, string> {
							{"FirstName", App_FirstName},
							{"LoanAmount", autoDecisionResponse.AutoApproveAmount.ToString(CultureInfo.InvariantCulture)}
						};

						mailer.SendToCustomerAndEzbob(
							customerMailVariables,
							App_email,
							"Mandrill - Approval (1st time)",
							"Congratulations " + App_FirstName + ", £" + autoDecisionResponse.AutoApproveAmount + " is available to fund your business today"
						);

						strategyHelper.AddApproveIntoDecisionHistory(CustomerId, "Auto Approval");
						DB.ExecuteNonQuery(
							"Update_Main_Strat_Finish_Date",
							CommandSpecies.StoredProcedure,
							new QueryParameter("UserId", CustomerId)
						);
					}
					else {
						var customerMailVariables = new Dictionary<string, string> {
							{"FirstName", App_FirstName},
							{"LoanAmount", autoDecisionResponse.AutoApproveAmount.ToString(CultureInfo.InvariantCulture)}
						};

						mailer.SendToCustomerAndEzbob(
							customerMailVariables,
							App_email,
							"Mandrill - Approval (not 1st time)",
							"Congratulations " + App_FirstName + ", £" + autoDecisionResponse.AutoApproveAmount + " is available to fund your business today"
						);

						strategyHelper.AddApproveIntoDecisionHistory(CustomerId, "AutoApproval");
						DB.ExecuteNonQuery(
							"Update_Main_Strat_Finish_Date",
							CommandSpecies.StoredProcedure,
							new QueryParameter("UserId", CustomerId)
						);
					} // if
				}
				else {
					DB.ExecuteNonQuery(
						"UpdateCashRequestsReApproval",
						CommandSpecies.StoredProcedure,
						new QueryParameter("CustomerId", CustomerId),
						new QueryParameter("UnderwriterDecision", autoDecisionResponse.UserStatus),
						new QueryParameter("ManagerApprovedSum", LoanOffer_ReApprovalSum),
						new QueryParameter("APR", LoanOffer_APR),
						new QueryParameter("RepaymentPeriod", LoanOffer_RepaymentPeriod),
						new QueryParameter("InterestRate", LoanOffer_InterestRate),
						new QueryParameter("UseSetupFee", LoanOffer_UseSetupFee),
						new QueryParameter("OfferValidDays", autoDecisionResponse.LoanOffer_OfferValidDays),
						new QueryParameter("EmailSendingBanned", autoDecisionResponse.LoanOffer_EmailSendingBanned_new),
						new QueryParameter("LoanTypeId", LoanOffer_LoanTypeId),
						new QueryParameter("UnderwriterComment", autoDecisionResponse.LoanOffer_UnderwriterComment),
						new QueryParameter("IsLoanTypeSelectionAllowed", LoanOffer_IsLoanTypeSelectionAllowed),
						new QueryParameter("DiscountPlanId", LoanOffer_DiscountPlanId),
						new QueryParameter("ExperianRating", LoanOffer_ExpirianRating),
						new QueryParameter("LoanSourceId", LoanSourceId),
						new QueryParameter("IsCustomerRepaymentPeriodSelectionAllowed", IsCustomerRepaymentPeriodSelectionAllowed)
					);

					var variables = new Dictionary<string, string> {
						{"ApprovedReApproved", "Re-Approved"},
						{"RegistrationDate", App_RegistrationDate.ToString(CultureInfo.InvariantCulture)},
						{"userID", CustomerId.ToString(CultureInfo.InvariantCulture)},
						{"Name", App_email},
						{"FirstName", App_FirstName},
						{"Surname", App_Surname},
						{"MP_Counter", AllMPsNum.ToString(CultureInfo.InvariantCulture)},
						{"MedalType", MedalType.ToString()},
						{"SystemDecision", autoDecisionResponse.SystemDecision},
						{"ApprovalAmount", LoanOffer_ReApprovalSum.ToString(CultureInfo.InvariantCulture)},
						{"RepaymentPeriod", LoanOffer_RepaymentPeriod.ToString(CultureInfo.InvariantCulture)},
						{"InterestRate", LoanOffer_InterestRate.ToString(CultureInfo.InvariantCulture)},
						{"OfferValidUntil", autoDecisionResponse.App_ValidFor.ToString(CultureInfo.InvariantCulture)}
					};

					mailer.SendToEzbob(variables, "Mandrill - User is approved or re-approved", "User was automatically Re-Approved");

					if (!EnableAutomaticReApproval) {
						DB.ExecuteNonQuery(
							"Update_Main_Strat_Finish_Date",
							CommandSpecies.StoredProcedure,
							new QueryParameter("UserId", CustomerId)
						);
					}
					else {
						var customerMailVariables = new Dictionary<string, string> {
							{"FirstName", App_FirstName},
							{"LoanAmount", LoanOffer_ReApprovalSum.ToString(CultureInfo.InvariantCulture)}
						};

						mailer.SendToCustomerAndEzbob(
							customerMailVariables,
							App_email,
							"Mandrill - Approval (not 1st time)", "Congratulations " + App_FirstName + ", £" + LoanOffer_ReApprovalSum + " is available to fund your business today"
						);

						strategyHelper.AddApproveIntoDecisionHistory(CustomerId, "Auto Re-Approval");

						DB.ExecuteNonQuery(
							"Update_Main_Strat_Finish_Date",
							CommandSpecies.StoredProcedure,
							new QueryParameter("UserId", CustomerId)
						);
					} // if
				} // if
			}
			else if (autoDecisionResponse.UserStatus == "Rejected") {
				if ((autoDecisionResponse.IsReRejected && !EnableAutomaticReRejection) || (!autoDecisionResponse.IsReRejected && !EnableAutomaticRejection))
					SendRejectionExplanationMail(autoDecisionResponse.IsReRejected ? "User was automatically Re-Rejected" : "User was automatically Rejected");
				else {
					const string rejectionSubject = "Sorry, ezbob cannot make you a loan offer at this time";
					SendRejectionExplanationMail(rejectionSubject);

					var variables = new Dictionary<string, string> {
						{"FirstName", App_FirstName},
						{"EzbobAccount", "https://app.ezbob.com/Customer/Profile"}
					};

					mailer.SendToCustomerAndEzbob(variables, App_email, "Mandrill - Rejection email", rejectionSubject);
					strategyHelper.AddRejectIntoDecisionHistory(CustomerId, autoDecisionResponse.AutoRejectReason);
				} // if

				DB.ExecuteNonQuery(
					"Update_Main_Strat_Finish_Date",
					CommandSpecies.StoredProcedure,
					new QueryParameter("UserId", CustomerId)
				);
			}
			else {
				var variables = new Dictionary<string, string> {
					{"RegistrationDate", App_RegistrationDate.ToString(CultureInfo.InvariantCulture)},
					{"userID", CustomerId.ToString(CultureInfo.InvariantCulture)},
					{"Name", App_email},
					{"FirstName", App_FirstName},
					{"Surname", App_Surname},
					{"MP_Counter", AllMPsNum.ToString(CultureInfo.InvariantCulture)},
					{"MedalType", MedalType.ToString()},
					{"SystemDecision", autoDecisionResponse.SystemDecision}
				};

				mailer.SendToEzbob(variables, "Mandrill - User is waiting for decision", "User is now waiting for decision");

				DB.ExecuteNonQuery(
					"Update_Main_Strat_Finish_Date",
					CommandSpecies.StoredProcedure,
					new QueryParameter("UserId", CustomerId)
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
		private readonly MedalScoreCalculator medalScoreCalculator = new MedalScoreCalculator();

		// Consts
		private const string CP_Experian_Actions_AMLMortality = "The underwriter will need to clarify that the applicant is actually alive (can be a tricky discussion!) and get copies of proof of identity";
		private const string CP_Experian_Actions_AMLAccommodationAddress = "If this is a personal address then reject";
		private const string CP_Experian_Actions_AMLRedirection = "Contact client and confirm reason for redirection. Why is this person’s mail being redirected? Might be a legitimate way of doing e-business";
		private const string CP_Experian_Actions_AMLInconsistencies = "The underwriter will need to ask the applicant for copies of proof of Identity";
		private const string CP_Experian_Actions_AMLPEP = "The underwriter needs to confirm that the applicant is not a politician or relative of a politician with the same surname";
		private const string CP_Experian_Actions_AMLAuthentication = "The underwriter will need to ask the applicant for copies of proof of Identity";
		private const string CP_Experian_Actions_BWANameError = "Underwriter to confirm the account details by asking for copy of statement";
		private const string CP_Experian_Actions_BWAAccountStatus = "Underwriter to confirm the account details by asking for copy of statement";
		private const string CP_Experian_Actions_BWAAddressError = "Underwriter to confirm the account details by asking for copy of statement";
		
		// Inputs
		private int CustomerId;
		private NewCreditLineOption newCreditLineOption;
		private bool Underwriter_Check;
		private int avoidAutomaticDescison;
		private int UseCustomIdHubAddress;
		private string idhubHouseNumber;
		private string idhubHouseName;
		private string idhubStreet;
		private string idhubDistrict;
		private string idhubTown;
		private string idhubCounty;
		private string idhubPostCode;
		private string idhubBranchCode;
		private string idhubAccountNumber;

		// Configs
		private int Reject_Defaults_CreditScore;
		private int Reject_Defaults_AccountsNum;
		private int Reject_Minimal_Seniority;
		private int Reject_Defaults_MonthsNum;
		private int Reject_Defaults_Amount;
		private string BWABusinessCheck;
		private bool EnableAutomaticReRejection;
		private bool EnableAutomaticReApproval;
		private bool EnableAutomaticApproval;
		private bool EnableAutomaticRejection;
		private int MaxCapHomeOwner;
		private int MaxCapNotHomeOwner;
		private int LowCreditScore;
		private int LowTotalAnnualTurnover;
		private int LowTotalThreeMonthTurnover;
		private int defaultFeedbackValue;
		private int totalTimeToWaitForMarketplacesUpdate;
		private int intervalWaitForMarketplacesUpdate;

		// Loaded from DB per customer
		private bool CustomerStatusIsEnabled;
		private bool CustomerStatusIsWarning;
		private bool IsOffline;
		private string App_email;
		private string CompanyType;
		private string App_LimitedRefNum;
		private string App_NonLimitedRefNum;
		private string App_FirstName;
		private string App_Surname;
		private DateTime App_DateOfBirth;
		private string App_Gender;
		private string App_HomeOwner;
		private int App_TimeAtAddress;
		private string App_AccountNumber;
		private string App_SortCode;
		private DateTime App_RegistrationDate;
		private string App_BankAccountType;
		
		// Validated as used
		private string App_Line1;
		private string App_Line2;
		private string App_Line3;
		private string App_Line4;
		private string App_Line5;
		private string App_Line6;
		private string App_Line1Prev;
		private string App_Line2Prev;
		private string App_Line3Prev;
		private string App_Line4Prev;
		private string App_Line5Prev;
		private string App_Line6Prev;
		private int MinExperianScore;
		private int ExperianConsumerScore;
		private DateTime ExperianBirthDate = new DateTime(1900, 1, 1);
		private int AllMPsNum;
		private AutoDecisionResponse autoDecisionResponse;
		private int NumOfDefaultAccounts;
		private MedalMultiplier MedalType;
		private decimal LoanOffer_APR;
		private int LoanOffer_RepaymentPeriod;
		private int LoanOffer_ExpirianRating;
		private decimal LoanOffer_InterestRate;
		private int LoanOffer_UseSetupFee;
		private int LoanOffer_LoanTypeId;
		private int LoanOffer_IsLoanTypeSelectionAllowed;
		private int LoanOffer_DiscountPlanId;
		private int LoanSourceId;
		private int IsCustomerRepaymentPeriodSelectionAllowed;
		private decimal LoanIntrestBase;
		private int LoanOffer_ReApprovalSum;
		private int LoanOffer_ReApprovalFullAmount;
		private int LoanOffer_ReApprovalRemainingAmount;
		private int LoanOffer_ReApprovalFullAmountOld;
		private int LoanOffer_ReApprovalRemainingAmountOld;
		private int OfferedCreditLine;
		private double Inintial_ExperianConsumerScore;
		private double MarketplaceSeniorityDays;
		private double TotalSumOfOrders3MTotal;
		private int ModelLoanOffer;
		private double TotalSumOfOrders1YTotal;
		private bool isFirstLoan;

		// AML & BWA
		private decimal ExperianAMLAuthentication;
		private string ExperianAmlResult;
		private string ExperianAMLWarning;
		private string ExperianAMLReject;
		private string ExperianBwaResult;
		private string ExperianBWAWarning;
		private string ExperianBWAPassed;
		private string ExperianBWAAccountStatus;
		private decimal ExperianBWANameScore;
		private decimal ExperianBWAAddressScore;
		private string ExperianBWAError;
		private string ExperianAMLPassed;
		private string ExperianAMLError;

		#endregion properties

		#region method ReadConfigurations

		private void ReadConfigurations() {
			DataTable dt = DB.ExecuteReader("MainStrategyGetConfigs", CommandSpecies.StoredProcedure);
			DataRow results = dt.Rows[0];
			
			Reject_Defaults_CreditScore = int.Parse(results["Reject_Defaults_CreditScore"].ToString());
			Reject_Defaults_AccountsNum = int.Parse(results["Reject_Defaults_AccountsNum"].ToString());
			Reject_Minimal_Seniority = int.Parse(results["Reject_Minimal_Seniority"].ToString());
			Reject_Defaults_MonthsNum = int.Parse(results["Reject_Defaults_MonthsNum"].ToString());
			Reject_Defaults_Amount = int.Parse(results["Reject_Defaults_Amount"].ToString());
			BWABusinessCheck = results["BWABusinessCheck"].ToString();
			EnableAutomaticApproval = bool.Parse(results["EnableAutomaticApproval"].ToString());
			EnableAutomaticReApproval = bool.Parse(results["EnableAutomaticReApproval"].ToString());
			EnableAutomaticRejection = bool.Parse(results["EnableAutomaticRejection"].ToString());
			EnableAutomaticReRejection = bool.Parse(results["EnableAutomaticReRejection"].ToString());
			MaxCapHomeOwner = int.Parse(results["MaxCapHomeOwner"].ToString());
			MaxCapNotHomeOwner = int.Parse(results["MaxCapNotHomeOwner"].ToString());
			LowCreditScore = int.Parse(results["LowCreditScore"].ToString());
			LowTotalAnnualTurnover = int.Parse(results["LowTotalAnnualTurnover"].ToString());
			LowTotalThreeMonthTurnover = int.Parse(results["LowTotalThreeMonthTurnover"].ToString());
			defaultFeedbackValue = int.Parse(results["DefaultFeedbackValue"].ToString());
			totalTimeToWaitForMarketplacesUpdate = int.Parse(results["TotalTimeToWaitForMarketplacesUpdate"].ToString());
			intervalWaitForMarketplacesUpdate = int.Parse(results["IntervalWaitForMarketplacesUpdate"].ToString());
		} // ReadConfigurations

		#endregion method ReadConfigurations

		#region method GerPersonalInfo

		private void GerPersonalInfo() {
			DataTable dt = DB.ExecuteReader("MainStrategyGetPersonalInfo", CommandSpecies.StoredProcedure);
			DataRow results = dt.Rows[0];

			CustomerStatusIsEnabled = bool.Parse(results["CustomerStatusIsEnabled"].ToString());
			CustomerStatusIsWarning = bool.Parse(results["CustomerStatusIsWarning"].ToString());
			IsOffline = bool.Parse(results["IsOffline"].ToString());
			App_email = results["CustomerEmail"].ToString();
			CompanyType = results["CompanyType"].ToString();
			App_LimitedRefNum = results["LimitedRefNum"].ToString();
			App_NonLimitedRefNum = results["NonLimitedRefNum"].ToString();
			App_FirstName = results["FirstName"].ToString();
			App_Surname = results["Surname"].ToString();
			App_Gender = results["Gender"].ToString();
			App_DateOfBirth = DateTime.Parse(results["DateOfBirth"].ToString());
			App_HomeOwner = results["HomeOwner"].ToString();
			AllMPsNum = int.Parse(results["NumOfMps"].ToString());
			App_TimeAtAddress = int.Parse(results["TimeAtAddress"].ToString());
			App_AccountNumber = results["AccountNumber"].ToString();
			App_SortCode = results["SortCode"].ToString();
			App_RegistrationDate = DateTime.Parse(results["RegistrationDate"].ToString());
			App_BankAccountType = results["BankAccountType"].ToString();
			int numOfLoans = int.Parse(results["NumOfLoans"].ToString());
			isFirstLoan = numOfLoans == 0;
		} // GerPersonalInfo

		#endregion method GerPersonalInfo

		#region method UpdateExperianConsumer

		private void UpdateExperianConsumer(string firstName, string surname, string postCode, string error, int score, int customerId, int directorId) {
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

		private void GetConsumerInfo(string firstName, string surname, string gender, DateTime birthDate, int directorId, string line1, string line2, string line3, string line4, string line5, string line6, out string error) {
			var consumerService = new ConsumerService();

			var location = new InputLocationDetailsMultiLineLocation {
				LocationLine1 = line1,
				LocationLine2 = line2,
				LocationLine3 = line3,
				LocationLine4 = line4,
				LocationLine5 = line5,
				LocationLine6 = line6
			};

			ConsumerServiceResult result = consumerService.GetConsumerInfo(firstName, surname, gender, birthDate, null, location, "PL", CustomerId, directorId);

			if (result.IsError)
				error = result.Error;
			else {
				ExperianConsumerScore = (int) result.BureauScore;
				ExperianBirthDate = result.BirthDate;
				error = null;
			}
		} // GetConsumerInfo

		#endregion method GetConsumerInfo

		#region GetAddresses

		private void GetAddresses() {
			DataTable dt = DB.ExecuteReader("GetCustomerAddresses", CommandSpecies.StoredProcedure);
			DataRow addressesResults = dt.Rows[0];
			App_Line1 = addressesResults["Line1"].ToString();
			App_Line2 = addressesResults["Line2"].ToString();
			App_Line3 = addressesResults["Line3"].ToString();
			App_Line4 = addressesResults["Line4"].ToString();
			App_Line5 = addressesResults["Line5"].ToString();
			App_Line6 = addressesResults["Line6"].ToString();
			App_Line1Prev = addressesResults["Line1Prev"].ToString();
			App_Line2Prev = addressesResults["Line2Prev"].ToString();
			App_Line3Prev = addressesResults["Line3Prev"].ToString();
			App_Line4Prev = addressesResults["Line4Prev"].ToString();
			App_Line5Prev = addressesResults["Line5Prev"].ToString();
			App_Line6Prev = addressesResults["Line6Prev"].ToString();
		} // GetAddresses

		#endregion GetAddresses

		#region method UpdateCompanyScore

		private void UpdateCompanyScore() {
			if (CompanyType == "Limited" || CompanyType == "LLP") {
				string ExperianLimitedError = null;
				decimal ExperianBureauScoreLimited = 0;

				if (string.IsNullOrEmpty(App_LimitedRefNum))
					ExperianLimitedError = "RefNumber is empty";
				else {
					var service = new EBusinessService();
					LimitedResults limitedData = service.GetLimitedBusinessData(App_LimitedRefNum, CustomerId);

					if (!limitedData.IsError)
						ExperianBureauScoreLimited = limitedData.BureauScore;
					else
						ExperianLimitedError = limitedData.Error;
				} // if

				DB.ExecuteNonQuery(
					"UpdateExperianBusiness",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CompanyRefNumber", App_LimitedRefNum),
					new QueryParameter("ExperianError", ExperianLimitedError),
					new QueryParameter("ExperianScore", ExperianBureauScoreLimited),
					new QueryParameter("CustomerId", CustomerId)
				);
			}
			else if (CompanyType == "PShip3P" || CompanyType == "PShip" || CompanyType == "SoleTrader") {
				string ExperianNonLimitedError = null;
				decimal ExperianBureauScoreNonLimited = 0;

				if (string.IsNullOrEmpty(App_NonLimitedRefNum))
					ExperianNonLimitedError = "RefNumber is empty";
				else {
					var service = new EBusinessService();
					var nonlimitedData = service.GetNotLimitedBusinessData(App_NonLimitedRefNum, CustomerId);

					if (!nonlimitedData.IsError)
						ExperianBureauScoreNonLimited = nonlimitedData.BureauScore;
					else
						ExperianNonLimitedError = nonlimitedData.Error;
				} // if

				DB.ExecuteNonQuery(
					"UpdateExperianBusiness",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CompanyRefNumber", App_NonLimitedRefNum),
					new QueryParameter("ExperianError", ExperianNonLimitedError),
					new QueryParameter("ExperianScore", ExperianBureauScoreNonLimited),
					new QueryParameter("CustomerId", CustomerId)
				);
			} // if
		} // UpdateCompanyScore

		#endregion method UpdateCompanyScore

		#region method SendRejectionExplanationMail

		private void SendRejectionExplanationMail(string subject) {
			var variables = new Dictionary<string, string> {
				{"RegistrationDate", App_RegistrationDate.ToString(CultureInfo.InvariantCulture)},
				{"userID", CustomerId.ToString(CultureInfo.InvariantCulture)},
				{"Name", App_email},
				{"FirstName", App_FirstName},
				{"Surname", App_Surname},
				{"MP_Counter", AllMPsNum.ToString(CultureInfo.InvariantCulture)},
				{"MedalType", MedalType.ToString()},
				{"SystemDecision", autoDecisionResponse.SystemDecision},
				{"ExperianConsumerScore", Inintial_ExperianConsumerScore.ToString(CultureInfo.InvariantCulture)},
				{"CVExperianConsumerScore", LowCreditScore.ToString(CultureInfo.InvariantCulture)},
				{"TotalAnnualTurnover", TotalSumOfOrders1YTotal.ToString(CultureInfo.InvariantCulture)},
				{"CVTotalAnnualTurnover", LowTotalAnnualTurnover.ToString(CultureInfo.InvariantCulture)},
				{"Total3MTurnover", TotalSumOfOrders3MTotal.ToString(CultureInfo.InvariantCulture)},
				{"CVTotal3MTurnover", LowTotalThreeMonthTurnover.ToString(CultureInfo.InvariantCulture)},
				{"PayPalStoresNum", autoDecisionResponse.PayPal_NumberOfStores.ToString(CultureInfo.InvariantCulture)},
				{"PayPalAnnualTurnover", autoDecisionResponse.PayPal_TotalSumOfOrders1Y.ToString(CultureInfo.InvariantCulture)},
				{"CVPayPalAnnualTurnover", LowTotalAnnualTurnover.ToString(CultureInfo.InvariantCulture)},
				{"PayPal3MTurnover", autoDecisionResponse.PayPal_TotalSumOfOrders3M.ToString(CultureInfo.InvariantCulture)},
				{"CVPayPal3MTurnover", LowTotalThreeMonthTurnover.ToString(CultureInfo.InvariantCulture)},
				{"CVExperianConsumerScoreDefAcc", Reject_Defaults_CreditScore.ToString(CultureInfo.InvariantCulture)},
				{"ExperianDefAccNum", NumOfDefaultAccounts.ToString(CultureInfo.InvariantCulture)},
				{"CVExperianDefAccNum", Reject_Defaults_AccountsNum.ToString(CultureInfo.InvariantCulture)},
				{"Seniority", MarketplaceSeniorityDays.ToString(CultureInfo.InvariantCulture)},
				{"SeniorityThreshold", Reject_Minimal_Seniority.ToString(CultureInfo.InvariantCulture)}
			};

			mailer.SendToEzbob(variables, "Mandrill - User is rejected by the strategy", subject);
		} // SendRejectionExplanationMail

		#endregion method SendRejectionExplanationMail

		#region method CreateAutoDecisionRequest

		private AutoDecisionRequest CreateAutoDecisionRequest() {
			return new AutoDecisionRequest {
				CustomerId = CustomerId,
				EnableAutomaticApproval = EnableAutomaticApproval,
				EnableAutomaticReApproval = EnableAutomaticReApproval,
				EnableAutomaticRejection = EnableAutomaticRejection,
				EnableAutomaticReRejection = EnableAutomaticReRejection,
				Inintial_ExperianConsumerScore = Inintial_ExperianConsumerScore,
				ModelLoanOffer = ModelLoanOffer,
				IsReRejected = false,
				LoanOffer_ReApprovalFullAmountOld = LoanOffer_ReApprovalFullAmountOld,
				LoanOffer_ReApprovalFullAmount = LoanOffer_ReApprovalFullAmount,
				LoanOffer_ReApprovalRemainingAmount = LoanOffer_ReApprovalRemainingAmount,
				LoanOffer_ReApprovalRemainingAmountOld = LoanOffer_ReApprovalRemainingAmountOld,
				LowTotalAnnualTurnover = LowTotalAnnualTurnover,
				LowTotalThreeMonthTurnover = LowTotalThreeMonthTurnover,
				MarketplaceSeniorityDays = MarketplaceSeniorityDays,
				MinExperianScore = MinExperianScore,
				OfferedCreditLine = OfferedCreditLine
			};
		} // CreateAutoDecisionRequest

		#endregion method CreateAutoDecisionRequest

		#region method AmlAndBwa

		private void AmlAndBwa(int CustomerId) {
			AccountVerificationResults accountVerificationResults;
			AuthenticationResults authenticationResults;

			if (UseCustomIdHubAddress != 0) {
				if (UseCustomIdHubAddress != 2) {
					authenticationResults = idHubService.AuthenticateForcedWithCustomAddress(App_FirstName, null, App_Surname, App_Gender, App_DateOfBirth, idhubHouseNumber, idhubHouseName, idhubStreet, idhubDistrict, idhubTown, idhubCounty, idhubPostCode, CustomerId);
					CreateAmlResultFromAuthenticationReuslts(authenticationResults);

					if (UseCustomIdHubAddress != 1) {
						if (ShouldRunBwa(App_BankAccountType, BWABusinessCheck, App_SortCode, App_AccountNumber)) {
							accountVerificationResults = idHubService.AccountVerificationForcedWithCustomAddress(
								App_FirstName, null, App_Surname, App_Gender,
								App_DateOfBirth, idhubHouseNumber, idhubHouseName,
								idhubStreet, idhubDistrict, idhubTown, idhubCounty,
								idhubPostCode, idhubBranchCode, idhubAccountNumber,
								CustomerId
							);

							CreateBwaResultFromAccountVerificationResults(accountVerificationResults);
						} // if
					} // if
				}
				else {
					accountVerificationResults = idHubService.AccountVerificationForcedWithCustomAddress(
						App_FirstName, null, App_Surname, App_Gender,
						App_DateOfBirth, idhubHouseNumber, idhubHouseName,
						idhubStreet, idhubDistrict, idhubTown, idhubCounty,
						idhubPostCode, idhubBranchCode, idhubAccountNumber,
						CustomerId
					);

					CreateBwaResultFromAccountVerificationResults(accountVerificationResults);
				} // if
			}
			else {
				authenticationResults = idHubService.Authenticate(
					App_FirstName, null, App_Surname,
					App_Gender, App_DateOfBirth, App_Line1,
					App_Line2, App_Line3, App_Line4, null,
					App_Line6, CustomerId
				);

				CreateAmlResultFromAuthenticationReuslts(authenticationResults);

				accountVerificationResults = idHubService.AccountVerification(App_FirstName, null, App_Surname, App_Gender, App_DateOfBirth, App_Line1, App_Line2, App_Line3, App_Line4, null, App_Line6, App_SortCode, App_AccountNumber, CustomerId);

				CreateBwaResultFromAccountVerificationResults(accountVerificationResults);

				if (ExperianAMLError != "" && App_TimeAtAddress == 1 && App_Line6Prev != null) {
					authenticationResults = idHubService.Authenticate(
						App_FirstName, null, App_Surname, App_Gender, App_DateOfBirth,
						App_Line1Prev, App_Line2Prev, App_Line3Prev, App_Line4Prev, null,
						App_Line6Prev, CustomerId
					);

					CreateAmlResultFromAuthenticationReuslts(authenticationResults);
				}

				if (ExperianBWAError != "" && App_TimeAtAddress == 1 && App_Line6Prev != null) {
					accountVerificationResults = idHubService.AccountVerification(App_FirstName, null, App_Surname, App_Gender, App_DateOfBirth, App_Line1Prev, App_Line2Prev, App_Line3Prev, App_Line4Prev, null, App_Line6Prev, App_SortCode, App_AccountNumber, CustomerId);
					CreateBwaResultFromAccountVerificationResults(accountVerificationResults);
				}
			} // if

			if (ExperianAMLError != "")
				ExperianAmlResult = "Warning";
			else {
				if (ExperianAMLAuthentication < 40 && ExperianAmlResult == "Rejected") {
					ExperianAMLWarning = ExperianAMLWarning +
						"#1,Authentication < 40 (" +
						ExperianAMLAuthentication +
						")||" +
						CP_Experian_Actions_AMLAuthentication +
						";";
				}
				else {
					ExperianAMLPassed = ExperianAMLPassed +
						"#1,Authentication >= 40 (" +
						ExperianAMLAuthentication +
						");";
				} // if

				if (ExperianAMLAuthentication < 40 && ExperianAmlResult != "Rejected") {
					ExperianAMLWarning = ExperianAMLWarning +
						"#1,Authentication < 40 (" +
						ExperianAMLAuthentication + ")||" +
						CP_Experian_Actions_AMLAuthentication +
						";";

					ExperianAmlResult = "Warning";
				}
				else
					ExperianAMLPassed = ExperianAMLPassed + "#1,Authentication >= 40 (" + ExperianAMLAuthentication + ");";
			} // if

			if (UseCustomIdHubAddress == 1) {
				DataTable dt = DB.ExecuteReader("GetPrevBwaResult", CommandSpecies.StoredProcedure);
				ExperianBwaResult = dt.Rows[0]["BWAResult"].ToString();
			}
			else {
				if (App_SortCode == null && App_AccountNumber == null)
					ExperianBwaResult = "Not performed";
				else {
					if (ExperianBWAError != "")
						ExperianBwaResult = "Warning";
					else {
						if (App_BankAccountType == "Business")
							ExperianBwaResult = "Not performed";
						else {
							ExperianBwaResult = "Passed";

							if (ExperianBWAAccountStatus == "No Match") {
								ExperianBWAWarning = ExperianBWAWarning + "#1, Account Status = No Match||" + CP_Experian_Actions_BWAAccountStatus + ";";
								ExperianBwaResult = "Warning";
							}
							else
								ExperianBWAPassed = ExperianBWAPassed + "#1, Account Status != No Match;";

							if (ExperianBWAAccountStatus == "Unable to check") {
								ExperianBWAWarning = ExperianBWAWarning + "#1, Account Status = Unable to check||" + CP_Experian_Actions_BWAAccountStatus + ";";
								ExperianBwaResult = "Warning";
							}
							else
								ExperianBWAPassed = ExperianBWAPassed + "#1, Account Status != Unable to check;";

							if (ExperianBWANameScore == 1) {
								ExperianBWAWarning = ExperianBWAWarning + "#2, Name error = 1||" + CP_Experian_Actions_BWANameError + ";";
								ExperianBwaResult = "Warning";
							}
							else
								ExperianBWAPassed = ExperianBWAPassed + "#2, Name error != 1;";

							if (ExperianBWANameScore == 2) {
								ExperianBWAWarning = ExperianBWAWarning + "#2, Name error = 2||" + CP_Experian_Actions_BWANameError + ";";
								ExperianBwaResult = "Warning";
							}
							else
								ExperianBWAPassed = ExperianBWAPassed + "#2, Name error != 2;";

							if (ExperianBWANameScore == 3) {
								ExperianBWAWarning = ExperianBWAWarning + "#2, Name error = 3||" + CP_Experian_Actions_BWANameError + ";";
								ExperianBwaResult = "Warning";
							}
							else
								ExperianBWAPassed = ExperianBWAPassed + "#2, Name error != 3;";

							if (ExperianBWANameScore == 4) {
								ExperianBWAWarning = ExperianBWAWarning + "#2, Name error = 4||" + CP_Experian_Actions_BWANameError + ";";
								ExperianBwaResult = "Warning";
							}
							else
								ExperianBWAPassed = ExperianBWAPassed + "#2, Name error != 4;";

							if (ExperianBWAAddressScore == 1) {
								ExperianBWAWarning = ExperianBWAWarning + "#3, Address error = 1||" + CP_Experian_Actions_BWAAddressError + ";";
								ExperianBwaResult = "Warning";
							}
							else
								ExperianBWAPassed = ExperianBWAPassed + "#3, Address error != 1;";

							if (ExperianBWAAddressScore == 2) {
								ExperianBWAWarning = ExperianBWAWarning + "#3, Address error = 2||" + CP_Experian_Actions_BWAAddressError + ";";
								ExperianBwaResult = "Warning";
							}
							else
								ExperianBWAPassed = ExperianBWAPassed + "#3, Address error != 2;";

							if (ExperianBWAAddressScore == 3) {
								ExperianBWAWarning = ExperianBWAWarning + "#3, Address error = 3||" + CP_Experian_Actions_BWAAddressError + ";";
								ExperianBwaResult = "Warning";
							}
							else
								ExperianBWAPassed = ExperianBWAPassed + "#3, Address error != 3;";

							if (ExperianBWAAddressScore == 4) {
								ExperianBWAWarning = ExperianBWAWarning + "#3, Address error = 4||" + CP_Experian_Actions_BWAAddressError + ";";
								ExperianBwaResult = "Warning";
							}
							else
								ExperianBWAPassed = ExperianBWAPassed + "#3, Address error != 4;";
						} // if
					} // if
				} // if
			} // if
		} // AmlAndBwa

		#endregion method AmlAndBwa

		#region method ShouldRunBwa

		private bool ShouldRunBwa(string App_BankAccountType, string BWABusinessCheck, string App_SortCode, string App_AccountNumber) {
			return App_BankAccountType == "Personal" && BWABusinessCheck == "1" && App_SortCode != null && App_AccountNumber != null;
		} // ShouldRunBwa

		#endregion method ShouldRunBwa

		#region method CreateBwaResultFromAccountVerificationResults

		private void CreateBwaResultFromAccountVerificationResults(AccountVerificationResults results) {
			if (!results.HasError) {
				Log.Info("account status: {0}, name score: {1}, address score: {2}", results.AccountStatus, results.NameScore, results.AddressScore);
				ExperianBWAAccountStatus = results.AccountStatus;
				ExperianBWANameScore = results.NameScore;
				ExperianBWAAddressScore = results.AddressScore;
			}
			else
				ExperianBWAError = results.Error;
		} // CreateBwaResultFromAccountVerificationResults

		#endregion method CreateBwaResultFromAccountVerificationResults

		#region method CreateAmlResultFromAuthenticationReuslts

		private void CreateAmlResultFromAuthenticationReuslts(AuthenticationResults results) {
			if (!results.HasError) {
				ExperianAMLAuthentication = results.AuthenticationIndexType;
				ExperianAmlResult = "Passed";

				foreach (var returnedHrp in results.ReturnedHRP) {
					if (returnedHrp.HighRiskPolRuleID == "U001") {
						ExperianAMLWarning += "#2, Mortality||" + CP_Experian_Actions_AMLMortality + ";";
						ExperianAmlResult = "Warning";
					}
					else if (returnedHrp.HighRiskPolRuleID == "U004") {
						ExperianAMLWarning += "#3, Accommodation address||" + CP_Experian_Actions_AMLAccommodationAddress + ";";
						ExperianAmlResult = "Warning";
					}
					else if (returnedHrp.HighRiskPolRuleID == "U007")
						ExperianAMLReject += "#4, Developed Identity;";
					else if (returnedHrp.HighRiskPolRuleID == "U013") {
						ExperianAMLWarning += "#5, Redirection||" + CP_Experian_Actions_AMLRedirection + ";";
						ExperianAmlResult = "Warning";
					}
					else if (returnedHrp.HighRiskPolRuleID == "U015" || returnedHrp.HighRiskPolRuleID == "U131" ||
							 returnedHrp.HighRiskPolRuleID == "U133" || returnedHrp.HighRiskPolRuleID == "U135") {
						ExperianAMLReject += "#6, Sanctions;";
						ExperianAmlResult = "Warning";
					}
					else if (returnedHrp.HighRiskPolRuleID == "U018") {
						ExperianAMLWarning += "#7, Inconsistencies||" + CP_Experian_Actions_AMLInconsistencies + ";";
						ExperianAmlResult = "Warning";
					}
					else if (returnedHrp.HighRiskPolRuleID == "U0132" || returnedHrp.HighRiskPolRuleID == "U0134") {
						ExperianAMLWarning += "#8, PEP||" + CP_Experian_Actions_AMLPEP + ";";
						ExperianAmlResult = "Warning";
					}
					else if (returnedHrp.HighRiskPolRuleID == "U007") {
						ExperianAMLReject += "#4, Developed Identity;";
						ExperianAmlResult = "Rejected";
					}
					else if (returnedHrp.HighRiskPolRuleID != "U001")
						ExperianAMLPassed += "#2, NO Mortality;";
					else if (returnedHrp.HighRiskPolRuleID != "U004")
						ExperianAMLPassed += "#3, NO Accommodation address;";
					else if (returnedHrp.HighRiskPolRuleID != "U007")
						ExperianAMLPassed += "#4, NO Developed Identity;";
					else if (returnedHrp.HighRiskPolRuleID != "U013")
						ExperianAMLPassed += "#5, NO Redirection;";
					else if (returnedHrp.HighRiskPolRuleID != "U015" || returnedHrp.HighRiskPolRuleID == "U131" ||
							 returnedHrp.HighRiskPolRuleID == "U133" || returnedHrp.HighRiskPolRuleID == "U135")
						ExperianAMLPassed += "#6, NO Sanctions;";
					else if (returnedHrp.HighRiskPolRuleID != "U018")
						ExperianAMLPassed += "#7, NO Inconsistencies;";
					else if (returnedHrp.HighRiskPolRuleID != "U0132" || returnedHrp.HighRiskPolRuleID == "U0134")
						ExperianAMLPassed += "#8, NO PEP;";
					else if (returnedHrp.HighRiskPolRuleID != "U007")
						ExperianAMLPassed += "#4, NO Developed Identity;";
				} // foreach
			} // if
			else
				ExperianAMLError = results.Error;
		} // CreateAmlResultFromAuthenticationReuslts

		#endregion method CreateAmlResultFromAuthenticationReuslts

		#region method WaitForMarketplacesToFinishUpdates

		private bool WaitForMarketplacesToFinishUpdates() {
			DataTable dt = DB.ExecuteReader(
				"MP_CustomerMarketplacesIsUpdated",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", CustomerId)
			);

			DataRow result = dt.Rows[0];

			bool isUpdated = bool.Parse(result["IsUpdated"].ToString());
			
			DateTime startWaitingTime = DateTime.UtcNow;

			while (!isUpdated) {
				if ((DateTime.UtcNow - startWaitingTime).TotalSeconds > totalTimeToWaitForMarketplacesUpdate)
					return false;

				Thread.Sleep(intervalWaitForMarketplacesUpdate);

				dt = DB.ExecuteReader(
					"MP_CustomerMarketplacesIsUpdated",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", CustomerId)
				);

				result = dt.Rows[0];
				isUpdated = bool.Parse(result["IsUpdated"].ToString());
			} // while

			return true;
		} // WaitForMarketplacesToFinishUpdates

		#endregion method WaitForMarketplacesToFinishUpdates

		#endregion private
	} // class MainStrategy

	#endregion class MainStrategy
} // namespace

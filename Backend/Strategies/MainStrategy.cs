namespace EzBob.Backend.Strategies
{
	using System;
	using System.Globalization;
	using System.Threading;
	using ExperianLib;
	using ExperianLib.Ebusiness;
	using ExperianLib.IdIdentityHub;
	using EzBob.Models;
	using EzBobIntegration.Web_References.Consumer;
	using global::Strategies.AutoDecisions;
	using log4net;
	using System.Collections.Generic;

	public enum NewCreditLineOption
	{
		SkipEverything = 1,
		UpdateEverythingExceptMp = 2,
		UpdateEverythingAndApplyAutoRules = 3,
		UpdateEverythingAndGoToManualDecision = 4
	}

	public class MainStrategy
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(MainStrategy));
		private readonly StrategiesMailer mailer = new StrategiesMailer();
		private StrategyHelper strategyHelper = new StrategyHelper();
		private AutoDecisionMaker autoDecisionMaker = new AutoDecisionMaker();

		private const string CP_Experian_Actions_AMLMortality = "The underwriter will need to clarify that the applicant is actually alive (can be a tricky discussion!) and get copies of proof of identity";
		private const string CP_Experian_Actions_AMLAccommodationAddress = "If this is a personal address then reject";
		private const string CP_Experian_Actions_AMLRedirection = "Contact client and confirm reason for redirection. Why is this person’s mail being redirected? Might be a legitimate way of doing e-business";
		private const string CP_Experian_Actions_AMLInconsistencies = "The underwriter will need to ask the applicant for copies of proof of Identity";
		private const string CP_Experian_Actions_AMLPEP = "The underwriter needs to confirm that the applicant is not a politician or relative of a politician with the same surname";
		private const string CP_Experian_Actions_AMLAuthentication = "The underwriter will need to ask the applicant for copies of proof of Identity";
		private const string CP_Experian_Actions_BWANameError = "Underwriter to confirm the account details by asking for copy of statement";
		private const string CP_Experian_Actions_BWAAccountStatus = "Underwriter to confirm the account details by asking for copy of statement";
		private const string CP_Experian_Actions_BWAAddressError = "Underwriter to confirm the account details by asking for copy of statement";

		private const string FAQPage = "https://www.ezbob.com/Customer/HowItWorks#Faq";

		// These parameter are from the other flow:
		string idhubHouseNumber = null;
		string idhubHouseName = null;
		string idhubStreet = null;
		string idhubDistrict = null;
		string idhubTown = null;
		string idhubCounty = null;
		string idhubPostCode = null;
		string idhubBranchCode = null;
		string idhubAccountNumber = null;
		
		// TODO: Read from ConfigurationVariables (ConfigurationVariables_MainStrat)
		string BWABusinessCheck = string.Empty;
		bool EnableAutomaticRejection = false;
		int MaxCapHomeOwner = 1;
		int MaxCapNotHomeOwner = 1;
		int Reject_Defaults_Amount = 1;
		int Reject_Defaults_MonthsNum = 1;





		string ExperianBwaResult = null;
		string ExperianBWAWarning = null;
		string ExperianBWAPassed = null;


		// TODO: get personal info (GetPersonalInfo)
		string App_email = string.Empty;
		string App_FirstName = string.Empty;
		string App_Surname = string.Empty;
		DateTime App_DateOfBirth = DateTime.Now;
		string App_HomeOwner = string.Empty;
		string App_MaritalStatus = string.Empty;
		int App_TimeAtAddress = 1;
		string App_Gender = string.Empty;
		string CompanyType = string.Empty;
		string App_AccountNumber = string.Empty;
		string App_SortCode = string.Empty;
		string App_LimitedRefNum = string.Empty;
		string App_NonLimitedRefNum = string.Empty;
		decimal App_OverallTurnOver = 1;
		decimal App_WebSiteTurnOver = 1;
		DateTime App_RegistrationDate = DateTime.Now;
		string App_RefNumber = string.Empty;
		string App_BankAccountType = string.Empty;
		int Prev_ExperianConsumerScore = 1;
		bool CustomerStatusIsEnabled = false;
		bool CustomerStatusIsWarning = false;
		bool IsOffline = false;
		string ScortoInternalErrorMessage = string.Empty;



		string ExperianLimitedError = null;
		string ExperianNonLimitedError = null;
		decimal ExperianBureauScoreLimited = 0;
		decimal ExperianExistingBusinessLoans = 0;
		decimal ExperianBureauScoreNonLimited = 0;
		bool ExperianCompanyNotFoundOnBureau = false;
		string ExperianConsumerError = string.Empty;
		string ExperianConsumerErrorPrev = string.Empty;
		string ExperianDirectorError = string.Empty;
		int ExperianConsumerScore = 0;
		double ExperianScoreConsumer = 0;
		DateTime ExperianBirthDate = DateTime.Now;
		

		IdHubService idHubService = new IdHubService();
		AuthenticationResults authenticationResults = null;
		AccountVerificationResults accountVerificationResults = null;
		int UseCustomIdHubAddress = 2; // TODO: should be input parameter from one flow



		string App_Line1 = null;
		string App_Line2 = null;
		string App_Line3 = null;
		string App_Line4 = null;
		string App_Line5 = null;
		string App_Line6 = null;
		string App_Line1Prev = null;
		string App_Line2Prev = null;
		string App_Line3Prev = null;
		string App_Line4Prev = null;
		string App_Line5Prev = null;
		string App_Line6Prev = null;


		private string Bwa;
		private string ExperianBWAAccountStatus;
		private decimal ExperianBWANameScore;
		private decimal ExperianBWAAddressScore;
		private string ExperianBWAError;
		private decimal ExperianAMLAuthentication;
		private string ExperianAMLResult;
		private string ExperianAMLWarning;
		private string ExperianAMLReject;
		private string ExperianAMLPassed;
		private string ExperianAMLError;


		private string Model_MaritalStatus;
		private int Model_MaxFeedback;
		private int Model_MPsNumber;
		private int Model_EZBOBSeniority;
		private int Model_OnTimeLoans;
		private int Model_LatePayments;
		private int Model_EarlyPayments;
		private DateTime Model_FirstRepaymentDate;
		private string Model_ScortoInternalErrorMessage;


		private double TotalSumOfOrdersForLoanOffer;



		private string ModelMedal;
		private string MedalType;
		private int ModelScoreResult;
		private int ModelScorePoints;
		private string Model_AC_Parameters;
		private string Model_AC_Descriptors;
		private string Model_Result_Weights;
		private string Model_Result_MAXPossiblePoints;






		int AllMPsNum;
		int LoanOffer_EKMStoresNum;
		string LoanOffer_SystemDecision;
		int LoanOffer_ManagerApprovedSum;
		string LoanOffer_MedalType;
		decimal LoanOffer_APR;
		int LoanOffer_RepaymentPeriod;
		int LoanOffer_ScorePoints;
		int LoanOffer_ExpirianRating;
		decimal LoanOffer_AnualTurnover;
		decimal LoanOffer_InterestRate;
		int LoanOffer_UseSetupFee;
		int LoanOffer_LoanTypeId;
		int LoanOffer_IsLoanTypeSelectionAllowed;
		int LoanOffer_DiscountPlanId;
		int LoanSourceId;
		int IsCustomerRepaymentPeriodSelectionAllowed;
		decimal LoanIntrestBase;


		private int LoanOffer_ReApprovalSum;


		private int Underwriter_Check;





		/* AutoDecisionMaker related properties */
		public bool EnableAutomaticReRejection { get; private set; }
		public int CustomerId { get; private set; }
		public int LoanOffer_ReApprovalFullAmount { get; private set; }
		public int LoanOffer_ReApprovalRemainingAmount { get; private set; }
		public int LoanOffer_ReApprovalFullAmountOld { get; private set; }
		public int LoanOffer_ReApprovalRemainingAmountOld { get; private set; }
		public bool EnableAutomaticReApproval { get; private set; }
		public int MinExperianScore { get; private set; }
		public int OfferedCreditLine { get; private set; }
		public bool EnableAutomaticApproval { get; private set; }
		public double Inintial_ExperianConsumerScore { get; private set; }
		public double MarketplaceSeniorityDays { get; private set; }
		public double TotalSumOfOrders3MTotal { get; private set; }

		// Being set inside the decision maker, should extract to DecisionResult class
		public bool IsReRejected { get; set; }
		public string AutoRejectReason { get; set; }
		public string CreditResult { get; set; }
		public string UserStatus { get; set; }
		public string SystemDecision { get; set; }
		public int ModelLoanOffer { get; set; }
		public int NumOfOutstandingLoans { get; set; }
		public string LoanOffer_UnderwriterComment { get; set; }
		public double LoanOffer_OfferValidDays { get; set; }
		public DateTime? App_ApplyForLoan { get; set; }
		public DateTime App_ValidFor { get; set; }
		public bool LoanOffer_EmailSendingBanned_new { get; set; }
		public int AutoApproveAmount { get; set; }
		public int AvailableFunds { get; set; }
		public bool IsAutoApproval { get; set; }
		public double TotalSumOfOrders1YTotal { get; private set; }
		public int LowTotalAnnualTurnover { get; private set; }
		public int LowTotalThreeMonthTurnover { get; private set; }
		public decimal PayPal_TotalSumOfOrders3M { get; private set; }
		public decimal PayPal_TotalSumOfOrders1Y { get; private set; }
		public int PayPal_NumberOfStores { get; private set; }

		// main strategy - flow 1
		public void Evaluate(int customerId, NewCreditLineOption newCreditLineOption, int avoidAutomaticDescison,
		                     bool isUnderwriterForced = false)
		{
			CustomerId = customerId;
			// TODO: remove column Customer.LastStartedMainStrategy

			strategyHelper.GetZooplaData(CustomerId);

			if (!CustomerStatusIsEnabled || CustomerStatusIsWarning)
			{
				EnableAutomaticReApproval = false;
				EnableAutomaticApproval = false;
			}

			if (IsOffline)
			{
				EnableAutomaticReApproval = false;
				EnableAutomaticApproval = false;
				EnableAutomaticReRejection = false;
				EnableAutomaticRejection = false;
			}

			if (newCreditLineOption != NewCreditLineOption.SkipEverything &&
			    newCreditLineOption != NewCreditLineOption.UpdateEverythingExceptMp)
			{
				if (!WaitForMarketplacesToFinishUpdates(CustomerId))
				{
					string customerEmail = null;

					// TODO: get customerEmail from CustomerId

					var variables = new Dictionary<string, string>
						{
							{"UserEmail", customerEmail},
							{"CustomerID", CustomerId.ToString(CultureInfo.InvariantCulture)},
							{"ApplicationID", customerEmail}
						};
					mailer.SendToEzbob(variables, "Mandrill - No Information about shops", "No information about customer marketplace");
					return;
				}
			}

			if (newCreditLineOption != NewCreditLineOption.SkipEverything)
			{
				if (CompanyType == "Limited" || CompanyType == "LLP")
				{
					if (string.IsNullOrEmpty(App_LimitedRefNum))
					{
						ExperianLimitedError = "RefNumber is empty";
					}
					else
					{
						var service = new EBusinessService();
						LimitedResults limitedData = service.GetLimitedBusinessData(App_LimitedRefNum, CustomerId);

						if (!limitedData.IsError)
						{
							ExperianBureauScoreLimited = limitedData.BureauScore;
							ExperianExistingBusinessLoans = limitedData.ExistingBusinessLoans;
						}
						else
						{
							ExperianLimitedError = limitedData.Error;
						}
					}

					// TODO: call UpdateExperianBusiness (App_LimitedRefNum,ExperianLimitedError,ExperianBureauScoreLimited)
				}
				else if (CompanyType == "PShip3P" || CompanyType == "PShip" || CompanyType == "SoleTrader")
				{
					if (string.IsNullOrEmpty(App_NonLimitedRefNum))
					{
						ExperianNonLimitedError = "RefNumber is empty";
					}
					else
					{
						var service = new EBusinessService();
						var nonlimitedData = service.GetNotLimitedBusinessData(App_NonLimitedRefNum, CustomerId);
						if (!nonlimitedData.IsError)
						{
							ExperianBureauScoreNonLimited = nonlimitedData.BureauScore;
							ExperianCompanyNotFoundOnBureau = nonlimitedData.CompanyNotFoundOnBureau;
						}
						else
						{
							ExperianNonLimitedError = nonlimitedData.Error;
						}
					}

					// TODO: call UpdateExperianBusiness (App_NonLimitedRefNum,ExperianNonLimitedError,ExperianBureauScoreNonLimited)
				}

				// TODO: call Get_Customer_Address_and_Previous_Address




				var consumerService = new ConsumerService();
				var location = new InputLocationDetailsMultiLineLocation();
				location.LocationLine1 = App_Line1;
				location.LocationLine2 = App_Line2;
				location.LocationLine3 = App_Line3;
				location.LocationLine4 = App_Line4;
				location.LocationLine5 = App_Line5;
				location.LocationLine6 = App_Line6;
				var result = consumerService.GetConsumerInfo(App_FirstName, App_Surname, App_Gender, App_DateOfBirth, null, location,
				                                             "PL", CustomerId, 0); // TODO: verify the null in 5th parameter is ok

				if (result.IsError)
				{
					ExperianConsumerError = result.Error;
				}
				else
				{
					ExperianConsumerScore = (int)result.BureauScore;
					ExperianBirthDate = result.BirthDate;
				}

				if (!string.IsNullOrEmpty(ExperianConsumerError) && App_TimeAtAddress == 1 && !string.IsNullOrEmpty(App_Line6Prev))
				{
					var consumerServiceForPrev = new ConsumerService();
					var prevLocation = new InputLocationDetailsMultiLineLocation();
					prevLocation.LocationLine1 = App_Line1Prev;
					prevLocation.LocationLine2 = App_Line2Prev;
					prevLocation.LocationLine3 = App_Line3Prev;
					prevLocation.LocationLine4 = App_Line4Prev;
					prevLocation.LocationLine5 = App_Line5Prev;
					prevLocation.LocationLine6 = App_Line6Prev;

					result = consumerServiceForPrev.GetConsumerInfo(App_FirstName, App_Surname, App_Gender, App_DateOfBirth, null,
					                                                location, "PL", CustomerId, 0);
						// TODO: verify the null in 5th parameter is ok

					if (result.IsError)
					{
						ExperianConsumerErrorPrev = result.Error;
					}
					else
					{
						ExperianConsumerScore = (int)result.BureauScore;
						ExperianBirthDate = result.BirthDate;
					}
				}

				if (ExperianBirthDate.Year == 1900 && ExperianBirthDate.Month == 1 && ExperianBirthDate.Day == 1)
				{
					ExperianBirthDate = App_DateOfBirth;
				}

				MinExperianScore = ExperianConsumerScore;
				Inintial_ExperianConsumerScore = ExperianConsumerScore;
				ExperianScoreConsumer = ExperianConsumerScore;

				// TODO: 2 calls to UpdateExperianConsumer (current and prev)

				if (CompanyType != "Entrepreneur")
				{
					IEnumerable<string> results = null; // TODO: get results from Get_Other_Directors_Address

					foreach (var director in results)
					{
						string App_DirSurnameScalar = null;
						string App_DirNameScalar = null;
						App_Line6 = null;
						App_Line5 = null;
						App_Line4 = null;
						App_Line3 = null;
						App_Line2 = null;
						App_Line1 = null;
						int App_DirIdScalar = 0;
						string App_DirGenderScalar = null;
						DateTime? App_DirDateOfBirthScalar = null;

						if (string.IsNullOrEmpty(App_DirNameScalar) || string.IsNullOrEmpty(App_DirSurnameScalar))
						{
							continue;
						}

						var consumerServiceForDir = new ConsumerService();
						var dirLocation = new InputLocationDetailsMultiLineLocation();
						dirLocation.LocationLine1 = App_Line1;
						dirLocation.LocationLine2 = App_Line2;
						dirLocation.LocationLine3 = App_Line3;
						dirLocation.LocationLine4 = App_Line4;
						dirLocation.LocationLine5 = App_Line5;
						dirLocation.LocationLine6 = App_Line6;
						var dirResult = consumerServiceForDir.GetConsumerInfo(App_DirNameScalar, App_DirSurnameScalar, App_DirGenderScalar,
						                                                      App_DirDateOfBirthScalar, null, location, "PL", CustomerId,
						                                                      App_DirIdScalar);
							// TODO: verify the null in 5th parameter is ok

						if (dirResult.IsError)
						{
							ExperianDirectorError = dirResult.Error;
						}
						else
						{
							ExperianConsumerScore = (int)dirResult.BureauScore;
							ExperianBirthDate = dirResult.BirthDate;
						}

						if (ExperianConsumerScore > 0 && ExperianConsumerScore < MinExperianScore)
						{
							MinExperianScore = ExperianConsumerScore;
						}

						// TODO: call to UpdateExperianConsumer
					}
				}

				// TODO: call Get_Customer_Address_and_Previous_Address

				AmlAndBwa(CustomerId);

				// TODO:UpdateExperianBWA_AML
			}

			// TODO: with MP_GetScoreCardData...

			TotalSumOfOrders1YTotal = strategyHelper.GetAnualTurnOverByCustomer(CustomerId);
			TotalSumOfOrders3MTotal = strategyHelper.GetTotalSumOfOrders3M(CustomerId);
			MarketplaceSeniorityDays = strategyHelper.MarketplaceSeniority(CustomerId);
			TotalSumOfOrdersForLoanOffer = strategyHelper.GetTotalSumOfOrdersForLoanOffer(CustomerId);

			if (Model_MaxFeedback == null)
			{
				log.InfoFormat("No feedback information exists. Will use 20000.");
				Model_MaxFeedback = 20000; // average value will not influence scorecard calculations
			}

			ScoringStrategyStub();

			MedalType = ModelMedal;

			// TODO: call CustomerScoringResult_Insert

			if (newCreditLineOption == NewCreditLineOption.SkipEverything ||
			    newCreditLineOption == NewCreditLineOption.UpdateEverythingExceptMp ||
			    newCreditLineOption == NewCreditLineOption.UpdateEverythingAndGoToManualDecision || avoidAutomaticDescison == 1)
			{
				EnableAutomaticApproval = false;
				EnableAutomaticReApproval = false;
				EnableAutomaticRejection = false;
				EnableAutomaticReRejection = false;
			}

			
			// TODO: make the following calls:
			// Get_EKM_Shops_Number
			// Get_MPs_Error_Num
			// GetExperianDefaultsAccounts
			// GetLastOfferForAutomtedDecision
			// MP_Get_PayPal_Aggregates
			// GetBaseLoanInterest
			// Get_Automatic_ReRejects

			if (LoanOffer_ReApprovalRemainingAmount < 1000) // TODO: make this 1000 configurable
			{
				LoanOffer_ReApprovalRemainingAmount = 0;
			}

			if (LoanOffer_ReApprovalRemainingAmountOld < 500) // TODO: make this 500 configurable
			{
				LoanOffer_ReApprovalRemainingAmountOld = 0;
			}

			LoanOffer_ReApprovalSum = Math.Max(Math.Max(Math.Max(LoanOffer_ReApprovalFullAmount, LoanOffer_ReApprovalRemainingAmount),
			                                   LoanOffer_ReApprovalFullAmountOld), LoanOffer_ReApprovalRemainingAmountOld);

			OfferedCreditLine = ModelLoanOffer;

			int MaxCapHomeOwnerNum = 0;
			int MaxCapNotHomeOwnerNum = 0;
			if (App_HomeOwner == "Home owner" && MaxCapHomeOwnerNum < LoanOffer_ReApprovalSum)
			{
				LoanOffer_ReApprovalSum = MaxCapHomeOwnerNum;
			}

			if (App_HomeOwner != "Home owner" && MaxCapNotHomeOwnerNum < LoanOffer_ReApprovalSum)
			{
				LoanOffer_ReApprovalSum = MaxCapNotHomeOwnerNum;
			}

			if (App_HomeOwner == "Home owner" && MaxCapHomeOwnerNum < OfferedCreditLine)
			{
				OfferedCreditLine = MaxCapHomeOwnerNum;
			}

			if (App_HomeOwner != "Home owner" && MaxCapNotHomeOwnerNum < OfferedCreditLine)
			{
				OfferedCreditLine = MaxCapNotHomeOwnerNum;
			}

			AutoDecisionMaker.MakeDecision(this);

			if (Underwriter_Check == 1)
			{
				
			}
		}

		private void ScoringStrategyStub()
		{
			// implemented elsewhere by Stas
			// Fill these:
			ModelScoreResult = 1;
			ModelScorePoints = 1;
			ModelLoanOffer = 1;
			ModelMedal = "";
			Model_AC_Parameters = "";
			Model_AC_Descriptors = "";
			Model_Result_Weights = "";
			Model_Result_MAXPossiblePoints = "";
		}

		private void AmlAndBwa(int CustomerId)
		{
			if (UseCustomIdHubAddress != 0)
			{
				if (UseCustomIdHubAddress != 2)
				{
					authenticationResults = idHubService.AuthenticateForcedWithCustomAddress(App_FirstName, null, App_Surname, App_Gender, App_DateOfBirth, idhubHouseNumber, idhubHouseName, idhubStreet, idhubDistrict, idhubTown, idhubCounty, idhubPostCode, CustomerId);
					CreateAmlResultFromAuthenticationReuslts(authenticationResults);

					if (UseCustomIdHubAddress != 1)
					{
						if (ShouldRunBwa(App_BankAccountType, BWABusinessCheck, App_SortCode, App_AccountNumber))
						{
							accountVerificationResults = idHubService.AccountVerificationForcedWithCustomAddress(App_FirstName, null, App_Surname, App_Gender,
																						 App_DateOfBirth, idhubHouseNumber, idhubHouseName,
																						 idhubStreet, idhubDistrict, idhubTown, idhubCounty,
																						 idhubPostCode, idhubBranchCode, idhubAccountNumber,
																						 CustomerId);
							CreateBwaResultFromAccountVerificationResults(accountVerificationResults);
						}
					}
				}
				else
				{
					accountVerificationResults = idHubService.AccountVerificationForcedWithCustomAddress(App_FirstName, null, App_Surname, App_Gender,
																						 App_DateOfBirth, idhubHouseNumber, idhubHouseName,
																						 idhubStreet, idhubDistrict, idhubTown, idhubCounty,
																						 idhubPostCode, idhubBranchCode, idhubAccountNumber,
																						 CustomerId);
					CreateBwaResultFromAccountVerificationResults(accountVerificationResults);
				}
			}
			else
			{
				authenticationResults = idHubService.Authenticate(App_FirstName, null, App_Surname,
																						App_Gender, App_DateOfBirth, App_Line1,
																						App_Line2, App_Line3, App_Line4, null,
																						App_Line6, CustomerId);
				CreateAmlResultFromAuthenticationReuslts(authenticationResults);


				accountVerificationResults = idHubService.AccountVerification(App_FirstName, null, App_Surname, App_Gender, App_DateOfBirth, App_Line1, App_Line2, App_Line3, App_Line4, null, App_Line6, App_SortCode, App_AccountNumber, CustomerId);
				CreateBwaResultFromAccountVerificationResults(accountVerificationResults);

				if (ExperianAMLError != "" && App_TimeAtAddress == 1 && App_Line6Prev != null)
				{
					authenticationResults = idHubService.Authenticate(App_FirstName, null, App_Surname, App_Gender, App_DateOfBirth,
																	  App_Line1Prev, App_Line2Prev, App_Line3Prev, App_Line4Prev, null,
																	  App_Line6Prev, CustomerId);
					CreateAmlResultFromAuthenticationReuslts(authenticationResults);
				}
				if (ExperianBWAError != "" && App_TimeAtAddress == 1 && App_Line6Prev != null)
				{
					accountVerificationResults = idHubService.AccountVerification(App_FirstName, null, App_Surname, App_Gender, App_DateOfBirth, App_Line1Prev, App_Line2Prev, App_Line3Prev, App_Line4Prev, null, App_Line6Prev, App_SortCode, App_AccountNumber, CustomerId);
					CreateBwaResultFromAccountVerificationResults(accountVerificationResults);
				}
			}

			if (ExperianAMLError != "")
			{
				ExperianAMLResult = "Warning";
			}
			else
			{
				if (ExperianAMLAuthentication < 40 && ExperianAMLResult == "Rejected")
				{
					ExperianAMLWarning = ExperianAMLWarning + "#1,Authentication < 40 (" +
													ExperianAMLAuthentication + ")||" +
													CP_Experian_Actions_AMLAuthentication + ";";
				}
				else
				{
					ExperianAMLPassed = ExperianAMLPassed + "#1,Authentication >= 40 (" +
												   ExperianAMLAuthentication + ");";
				}

				if (ExperianAMLAuthentication < 40 && ExperianAMLResult != "Rejected")
				{
					ExperianAMLWarning = ExperianAMLWarning + "#1,Authentication < 40 (" +
													ExperianAMLAuthentication + ")||" +
													CP_Experian_Actions_AMLAuthentication + ";";
					ExperianAMLResult = "Warning";
				}
				else
				{
					ExperianAMLPassed = ExperianAMLPassed + "#1,Authentication >= 40 (" +
												   ExperianAMLAuthentication + ");";
				}
			}

			if (UseCustomIdHubAddress == 1)
			{
				// TODO: DB - GetPrevBwaResult
			}
			else
			{
				if (App_SortCode == null && App_AccountNumber == null)
				{
					ExperianBwaResult = "Not performed";
				}
				else
				{
					if (ExperianBWAError != "")
					{
						ExperianBwaResult = "Warning";
					}
					else
					{
						if (App_BankAccountType == "Business")
						{
							ExperianBwaResult = "Not performed";
						}
						else
						{
							ExperianBwaResult = "Passed";
							if (ExperianBWAAccountStatus == "No Match")
							{
								ExperianBWAWarning = ExperianBWAWarning + "#1, Account Status = No Match||" + CP_Experian_Actions_BWAAccountStatus + ";";
								ExperianBwaResult = "Warning";
							}
							else
							{
								ExperianBWAPassed = ExperianBWAPassed + "#1, Account Status != No Match;";
							}

							if (ExperianBWAAccountStatus == "Unable to check")
							{
								ExperianBWAWarning = ExperianBWAWarning + "#1, Account Status = Unable to check||" + CP_Experian_Actions_BWAAccountStatus + ";";
								ExperianBwaResult = "Warning";
							}
							else
							{
								ExperianBWAPassed = ExperianBWAPassed + "#1, Account Status != Unable to check;";
							}

							if (ExperianBWANameScore == 1)
							{
								ExperianBWAWarning = ExperianBWAWarning + "#2, Name error = 1||" + CP_Experian_Actions_BWANameError + ";";
								ExperianBwaResult = "Warning";
							}
							else
							{
								ExperianBWAPassed = ExperianBWAPassed + "#2, Name error != 1;";
							}

							if (ExperianBWANameScore == 2)
							{
								ExperianBWAWarning = ExperianBWAWarning + "#2, Name error = 2||" + CP_Experian_Actions_BWANameError + ";";
								ExperianBwaResult = "Warning";
							}
							else
							{
								ExperianBWAPassed = ExperianBWAPassed + "#2, Name error != 2;";
							}

							if (ExperianBWANameScore == 3)
							{
								ExperianBWAWarning = ExperianBWAWarning + "#2, Name error = 3||" + CP_Experian_Actions_BWANameError + ";";
								ExperianBwaResult = "Warning";
							}
							else
							{
								ExperianBWAPassed = ExperianBWAPassed + "#2, Name error != 3;";
							}

							if (ExperianBWANameScore == 4)
							{
								ExperianBWAWarning = ExperianBWAWarning + "#2, Name error = 4||" + CP_Experian_Actions_BWANameError + ";";
								ExperianBwaResult = "Warning";
							}
							else
							{
								ExperianBWAPassed = ExperianBWAPassed + "#2, Name error != 4;";
							}

							if (ExperianBWAAddressScore == 1)
							{
								ExperianBWAWarning = ExperianBWAWarning + "#3, Address error = 1||" + CP_Experian_Actions_BWAAddressError + ";";
								ExperianBwaResult = "Warning";
							}
							else
							{
								ExperianBWAPassed = ExperianBWAPassed + "#3, Address error != 1;";
							}

							if (ExperianBWAAddressScore == 2)
							{
								ExperianBWAWarning = ExperianBWAWarning + "#3, Address error = 2||" + CP_Experian_Actions_BWAAddressError + ";";
								ExperianBwaResult = "Warning";
							}
							else
							{
								ExperianBWAPassed = ExperianBWAPassed + "#3, Address error != 2;";
							}

							if (ExperianBWAAddressScore == 3)
							{
								ExperianBWAWarning = ExperianBWAWarning + "#3, Address error = 3||" + CP_Experian_Actions_BWAAddressError + ";";
								ExperianBwaResult = "Warning";
							}
							else
							{
								ExperianBWAPassed = ExperianBWAPassed + "#3, Address error != 3;";
							}

							if (ExperianBWAAddressScore == 4)
							{
								ExperianBWAWarning = ExperianBWAWarning + "#3, Address error = 4||" + CP_Experian_Actions_BWAAddressError + ";";
								ExperianBwaResult = "Warning";
							}
							else
							{
								ExperianBWAPassed = ExperianBWAPassed + "#3, Address error != 4;";
							}
						}
					}
				}
			}
		}

		private bool ShouldRunBwa(string App_BankAccountType, string BWABusinessCheck, string App_SortCode, string App_AccountNumber)
		{
			return App_BankAccountType == "Personal" && BWABusinessCheck == "1" && App_SortCode != null && App_AccountNumber != null;
		}

		private void CreateBwaResultFromAccountVerificationResults(AccountVerificationResults results)
		{
			if (!results.HasError)
			{
				Bwa = string.Format("account status: {0}, name score: {1}, address score: {2}", results.AccountStatus,
				                    results.NameScore, results.AddressScore);
				ExperianBWAAccountStatus = results.AccountStatus;
				ExperianBWANameScore = results.NameScore;
				ExperianBWAAddressScore = results.AddressScore;
			}
			else
			{
				ExperianBWAError = results.Error;
			}
		}

		private void CreateAmlResultFromAuthenticationReuslts(AuthenticationResults results)
		{
			if (!results.HasError)
			{
				ExperianAMLAuthentication = results.AuthenticationIndexType;
				ExperianAMLResult = "Passed";
				foreach (var returnedHrp in results.ReturnedHRP)
				{
					if (returnedHrp.HighRiskPolRuleID == "U001")
					{
						ExperianAMLWarning += "#2, Mortality||" + CP_Experian_Actions_AMLMortality + ";";
						ExperianAMLResult = "Warning";
					}
					else if (returnedHrp.HighRiskPolRuleID == "U004")
					{
						ExperianAMLWarning += "#3, Accommodation address||" + CP_Experian_Actions_AMLAccommodationAddress + ";";
						ExperianAMLResult = "Warning";
					}
					else if (returnedHrp.HighRiskPolRuleID == "U007")
					{
						ExperianAMLReject += "#4, Developed Identity;";
					}
					else if (returnedHrp.HighRiskPolRuleID == "U013")
					{
						ExperianAMLWarning += "#5, Redirection||" + CP_Experian_Actions_AMLRedirection + ";";
						ExperianAMLResult = "Warning";
					}
					else if (returnedHrp.HighRiskPolRuleID == "U015" || returnedHrp.HighRiskPolRuleID == "U131" ||
							 returnedHrp.HighRiskPolRuleID == "U133" || returnedHrp.HighRiskPolRuleID == "U135")
					{
						ExperianAMLReject += "#6, Sanctions;";
						ExperianAMLResult = "Warning";
					}
					else if (returnedHrp.HighRiskPolRuleID == "U018")
					{
						ExperianAMLWarning += "#7, Inconsistencies||" + CP_Experian_Actions_AMLInconsistencies + ";";
						ExperianAMLResult = "Warning";
					}
					else if (returnedHrp.HighRiskPolRuleID == "U0132" || returnedHrp.HighRiskPolRuleID == "U0134")
					{
						ExperianAMLWarning += "#8, PEP||" + CP_Experian_Actions_AMLPEP + ";";
						ExperianAMLResult = "Warning";
					}
					else if (returnedHrp.HighRiskPolRuleID == "U007")
					{
						ExperianAMLReject += "#4, Developed Identity;";
						ExperianAMLResult = "Rejected";
					}
					else if (returnedHrp.HighRiskPolRuleID != "U001")
					{
						ExperianAMLPassed += "#2, NO Mortality;";
					}
					else if (returnedHrp.HighRiskPolRuleID != "U004")
					{
						ExperianAMLPassed += "#3, NO Accommodation address;";
					}
					else if (returnedHrp.HighRiskPolRuleID != "U007")
					{
						ExperianAMLPassed += "#4, NO Developed Identity;";
					}
					else if (returnedHrp.HighRiskPolRuleID != "U013")
					{
						ExperianAMLPassed += "#5, NO Redirection;";
					}
					else if (returnedHrp.HighRiskPolRuleID != "U015" || returnedHrp.HighRiskPolRuleID == "U131" ||
							 returnedHrp.HighRiskPolRuleID == "U133" || returnedHrp.HighRiskPolRuleID == "U135")
					{
						ExperianAMLPassed += "#6, NO Sanctions;";
					}
					else if (returnedHrp.HighRiskPolRuleID != "U018")
					{
						ExperianAMLPassed += "#7, NO Inconsistencies;";
					}
					else if (returnedHrp.HighRiskPolRuleID != "U0132" || returnedHrp.HighRiskPolRuleID == "U0134")
					{
						ExperianAMLPassed += "#8, NO PEP;";
					}
					else if (returnedHrp.HighRiskPolRuleID != "U007")
					{
						ExperianAMLPassed += "#4, NO Developed Identity;";
					}
				}
			}
			else
			{
				ExperianAMLError = results.Error;
			}
		}

		private bool WaitForMarketplacesToFinishUpdates(int CustomerId)
		{
			bool isUpdated = false; // TODO: get from MP_IsCustomerMarketPlacesUpdated
			int totalTimeToWaitInSeconds = 43200; //TODO put in ConfigurationVariables
			int intervalInMilliseconds = 300000; //TODO put in ConfigurationVariables
			DateTime startWaitingTime = DateTime.UtcNow;

			while (!isUpdated)
			{
				if ((DateTime.UtcNow - startWaitingTime).TotalSeconds > totalTimeToWaitInSeconds)
				{
					return false;
				}

				Thread.Sleep(intervalInMilliseconds);
				isUpdated = false; // TODO: get from MP_IsCustomerMarketPlacesUpdated
			}

			return true;
		}
	}
}

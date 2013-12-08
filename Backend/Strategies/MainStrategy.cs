﻿namespace EzBob.Backend.Strategies
{
	using System;
	using System.Data;
	using System.Globalization;
	using System.Threading;
	using AutoDecisions;
	using ExperianLib;
	using ExperianLib.Ebusiness;
	using ExperianLib.IdIdentityHub;
	using Models;
	using EzBobIntegration.Web_References.Consumer;
	using log4net;
	using System.Collections.Generic;
	using DbConnection;

	public enum NewCreditLineOption
	{
		SkipEverything = 1,
		UpdateEverythingExceptMp = 2,
		UpdateEverythingAndApplyAutoRules = 3,
		UpdateEverythingAndGoToManualDecision = 4
	}

	public class MainStrategy
	{
		// Helpers
		private static readonly ILog log = LogManager.GetLogger(typeof(MainStrategy));
		private readonly StrategiesMailer mailer = new StrategiesMailer();
		private readonly StrategyHelper strategyHelper = new StrategyHelper();
		private readonly IdHubService idHubService = new IdHubService();

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

		// For auto-decisions
		AuthenticationResults authenticationResults;
		AccountVerificationResults accountVerificationResults;

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
		private string BWABusinessCheck;
		private bool EnableAutomaticReRejection;
		private bool EnableAutomaticReApproval;
		private bool EnableAutomaticApproval;
		private bool EnableAutomaticRejection;
		private int MaxCapHomeOwner;
		private int MaxCapNotHomeOwner;

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

		DateTime ExperianBirthDate = new DateTime(1900, 1, 1); // Could be deleted?


		// Below this line variables should be converted to locals


		// ?


		string App_HomeOwner = string.Empty;
		string App_MaritalStatus = string.Empty;
		int App_TimeAtAddress = 1;
		string App_AccountNumber = string.Empty;
		string App_SortCode = string.Empty;
		decimal App_OverallTurnOver = 1;
		decimal App_WebSiteTurnOver = 1;
		DateTime App_RegistrationDate = DateTime.Now;
		string App_RefNumber = string.Empty;
		string App_BankAccountType = string.Empty;
		int Prev_ExperianConsumerScore = 1;
		string ScortoInternalErrorMessage = string.Empty;
		private bool isFirstLoan;
		public decimal PayPal_TotalSumOfOrders3M { get; private set; }
		public decimal PayPal_TotalSumOfOrders1Y { get; private set; }
		public int PayPal_NumberOfStores { get; private set; }


		// ???

		

		int LowCreditScore = 0;



		// fill it
		string ExperianBwaResult = null;
		string ExperianBWAWarning = null;
		string ExperianBWAPassed = null;


		



		string ExperianNonLimitedError = null;
		bool ExperianCompanyNotFoundOnBureau = false;
		string ExperianDirectorError = string.Empty;
		





		int NumOfDefaultAccounts = 0;

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




		/* AutoDecisionMaker related properties */
		public int LoanOffer_ReApprovalFullAmount { get; private set; }
		public int LoanOffer_ReApprovalRemainingAmount { get; private set; }
		public int LoanOffer_ReApprovalFullAmountOld { get; private set; }
		public int LoanOffer_ReApprovalRemainingAmountOld { get; private set; }
		public int OfferedCreditLine { get; private set; }
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
		public string LoanOffer_UnderwriterComment { get; set; }
		public double LoanOffer_OfferValidDays { get; set; }
		public DateTime? App_ApplyForLoan { get; set; }
		public DateTime App_ValidFor { get; set; }
		public bool LoanOffer_EmailSendingBanned_new { get; set; }
		public bool IsAutoApproval { get; set; }
		public double TotalSumOfOrders1YTotal { get; private set; }
		public int LowTotalAnnualTurnover { get; private set; }
		public int LowTotalThreeMonthTurnover { get; private set; }
		
		private void ReadConfigurations()
		{
			DataTable dt = DbConnection.ExecuteSpReader("MainStrategyGetConfigs");
			DataRow results = dt.Rows[0];
			
			Reject_Defaults_CreditScore = int.Parse(results["Reject_Defaults_CreditScore"].ToString());
			Reject_Defaults_AccountsNum = int.Parse(results["Reject_Defaults_AccountsNum"].ToString());
			Reject_Minimal_Seniority = int.Parse(results["Reject_Minimal_Seniority"].ToString());
			BWABusinessCheck = results["BWABusinessCheck"].ToString();
			EnableAutomaticApproval = bool.Parse(results["EnableAutomaticApproval"].ToString());
			EnableAutomaticReApproval = bool.Parse(results["EnableAutomaticReApproval"].ToString());
			EnableAutomaticRejection = bool.Parse(results["EnableAutomaticRejection"].ToString());
			EnableAutomaticReRejection = bool.Parse(results["EnableAutomaticReRejection"].ToString());
			MaxCapHomeOwner = int.Parse(results["MaxCapHomeOwner"].ToString());
			MaxCapNotHomeOwner = int.Parse(results["MaxCapNotHomeOwner"].ToString());


		}

		private void GerPersonalInfo()
		{
			DataTable dt = DbConnection.ExecuteSpReader("MainStrategyGetPersonalInfo");
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
		}

		public void Execute()
		{
			ReadConfigurations();
			GerPersonalInfo();
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
				if (!WaitForMarketplacesToFinishUpdates())
				{
					var variables = new Dictionary<string, string>
						{
							{"UserEmail", App_email},
							{"CustomerID", CustomerId.ToString(CultureInfo.InvariantCulture)},
							{"ApplicationID", App_email}
						};
					mailer.SendToEzbob(variables, "Mandrill - No Information about shops", "No information about customer marketplace");
					return;
				}
			}

			if (newCreditLineOption != NewCreditLineOption.SkipEverything)
			{
				UpdateCompanyScore();
				GetAddresses();

				string ExperianConsumerError;
				string ExperianConsumerErrorPrev = null;

				GetConsumerInfo(App_Line1, App_Line2, App_Line3, App_Line4, App_Line5, App_Line6, out ExperianConsumerError);

				if (!string.IsNullOrEmpty(ExperianConsumerError) && App_TimeAtAddress == 1 && !string.IsNullOrEmpty(App_Line6Prev))
				{
					GetConsumerInfo(App_Line1Prev, App_Line2Prev, App_Line3Prev, App_Line4Prev, App_Line5Prev, App_Line6Prev, out ExperianConsumerErrorPrev);
				}

				if (ExperianBirthDate.Year == 1900 && ExperianBirthDate.Month == 1 && ExperianBirthDate.Day == 1)
				{
					ExperianBirthDate = App_DateOfBirth;
				}

				MinExperianScore = ExperianConsumerScore;
				Inintial_ExperianConsumerScore = ExperianConsumerScore;
				//ExperianScoreConsumer = ExperianConsumerScore; // in 3 updates
				
				DbConnection.ExecuteSpNonQuery("UpdateExperianConsumer",
				                               DbConnection.CreateParam("Name", App_FirstName),
				                               DbConnection.CreateParam("Surname", App_Surname),
				                               DbConnection.CreateParam("PostCode", App_Line6),
				                               DbConnection.CreateParam("ExperianError", ExperianConsumerError),
				                               DbConnection.CreateParam("ExperianScore", ExperianConsumerScore),
				                               DbConnection.CreateParam("CustomerId", CustomerId),
				                               DbConnection.CreateParam("DirectorId", 0));


				DbConnection.ExecuteSpNonQuery("UpdateExperianConsumer",
											   DbConnection.CreateParam("Name", App_FirstName),
											   DbConnection.CreateParam("Surname", App_Surname),
											   DbConnection.CreateParam("PostCode", App_Line6Prev),
											   DbConnection.CreateParam("ExperianError", ExperianConsumerErrorPrev),
											   DbConnection.CreateParam("ExperianScore", ExperianConsumerScore),
											   DbConnection.CreateParam("CustomerId", CustomerId),
											   DbConnection.CreateParam("DirectorId", 0));
				
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
						                                                      App_DirDateOfBirthScalar, null, dirLocation, "PL", CustomerId,
						                                                      App_DirIdScalar);

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
			// GetExperianDefaultsAccounts and fill NumOfDefaultAccounts
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

			if (App_HomeOwner == "Home owner" && MaxCapHomeOwner < LoanOffer_ReApprovalSum)
			{
				LoanOffer_ReApprovalSum = MaxCapHomeOwner;
			}

			if (App_HomeOwner != "Home owner" && MaxCapNotHomeOwner < LoanOffer_ReApprovalSum)
			{
				LoanOffer_ReApprovalSum = MaxCapNotHomeOwner;
			}

			if (App_HomeOwner == "Home owner" && MaxCapHomeOwner < OfferedCreditLine)
			{
				OfferedCreditLine = MaxCapHomeOwner;
			}

			if (App_HomeOwner != "Home owner" && MaxCapNotHomeOwner < OfferedCreditLine)
			{
				OfferedCreditLine = MaxCapNotHomeOwner;
			}




			AutoDecisionResponse autoDecisionResponse = AutoDecisionMaker.MakeDecision(CreateAutoDecisionRequest());
			IsReRejected = autoDecisionResponse.IsReRejected;
			AutoRejectReason = autoDecisionResponse.AutoRejectReason;
			CreditResult = autoDecisionResponse.CreditResult;
			UserStatus = autoDecisionResponse.UserStatus;
			SystemDecision = autoDecisionResponse.SystemDecision;
			ModelLoanOffer = autoDecisionResponse.ModelLoanOffer;
			LoanOffer_UnderwriterComment = autoDecisionResponse.LoanOffer_UnderwriterComment;
			LoanOffer_OfferValidDays = autoDecisionResponse.LoanOffer_OfferValidDays;
			App_ApplyForLoan = autoDecisionResponse.App_ApplyForLoan;
			App_ValidFor = autoDecisionResponse.App_ValidFor;
			LoanOffer_EmailSendingBanned_new = autoDecisionResponse.LoanOffer_EmailSendingBanned_new;
			IsAutoApproval = autoDecisionResponse.IsAutoApproval;

			if (Underwriter_Check)
			{
				DbConnection.ExecuteSpNonQuery("Update_Main_Strat_Finish_Date", DbConnection.CreateParam("UserId", CustomerId));
				return;
			}

			// Update scoring result

			// UpdateCashRequests

			if (UserStatus == "Approved")
			{
				if (IsAutoApproval)
				{
					// UpdateAutoApproval


					var variables = new Dictionary<string, string>
						{
							{"ApprovedReApproved", "Approved"},
							{"RegistrationDate", App_RegistrationDate.ToString(CultureInfo.InvariantCulture)},
							{"userID", CustomerId.ToString(CultureInfo.InvariantCulture)},
							{"Name", App_email},
							{"FirstName", App_FirstName},
							{"Surname", App_Surname},
							{"MP_Counter", AllMPsNum.ToString(CultureInfo.InvariantCulture)},
							{"MedalType", MedalType},
							{"SystemDecision", SystemDecision},
							{"ApprovalAmount", LoanOffer_ReApprovalSum.ToString(CultureInfo.InvariantCulture)},
							{"RepaymentPeriod", LoanOffer_RepaymentPeriod.ToString(CultureInfo.InvariantCulture)},
							{"InterestRate", LoanOffer_InterestRate.ToString(CultureInfo.InvariantCulture)},
							{"OfferValidUntil", App_ValidFor.ToString(CultureInfo.InvariantCulture)}
						};
					mailer.SendToEzbob(variables, "Mandrill - User is approved or re-approved", "User was automatically approved");

					double LoanOffer_OfferValidHours = Math.Round(LoanOffer_OfferValidDays * 24, 0);
					

					if (isFirstLoan)
					{
						var variables3 = new Dictionary<string, string>
							{
								{"FirstName", App_FirstName},
								{"LoanAmount", autoDecisionResponse.AutoApproveAmount.ToString(CultureInfo.InvariantCulture)}
							};

						mailer.SendToCustomerAndEzbob(variables3, App_email, "Mandrill - Approval (1st time)",
													  "Congratulations " + App_FirstName + ", £" + autoDecisionResponse.AutoApproveAmount +
						                              " is available to fund your business today");

						strategyHelper.AddApproveIntoDecisionHistory(CustomerId, "Auto Approval");
						DbConnection.ExecuteSpNonQuery("Update_Main_Strat_Finish_Date", DbConnection.CreateParam("UserId", CustomerId));
					}
					else
					{
						var variables4 = new Dictionary<string, string>
						{
							{"FirstName", App_FirstName},
							{"LoanAmount", autoDecisionResponse.AutoApproveAmount.ToString(CultureInfo.InvariantCulture)}
						};

						mailer.SendToCustomerAndEzbob(variables4, App_email, "Mandrill - Approval (not 1st time)", "Congratulations " + App_FirstName + ", £" + autoDecisionResponse.AutoApproveAmount +
								   " is available to fund your business today");

						strategyHelper.AddApproveIntoDecisionHistory(CustomerId, "AutoApproval");
						DbConnection.ExecuteSpNonQuery("Update_Main_Strat_Finish_Date", DbConnection.CreateParam("UserId", CustomerId));
					}
					return;
				}
				else
				{
					// UpdateCashRequestsReApproval

					var variables = new Dictionary<string, string>
						{
							{"ApprovedReApproved", "Re-Approved"},
							{"RegistrationDate", App_RegistrationDate.ToString(CultureInfo.InvariantCulture)},
							{"userID", CustomerId.ToString(CultureInfo.InvariantCulture)},
							{"Name", App_email},
							{"FirstName", App_FirstName},
							{"Surname", App_Surname},
							{"MP_Counter", AllMPsNum.ToString(CultureInfo.InvariantCulture)},
							{"MedalType", MedalType},
							{"SystemDecision", SystemDecision},
							{"ApprovalAmount", LoanOffer_ReApprovalSum.ToString(CultureInfo.InvariantCulture)},
							{"RepaymentPeriod", LoanOffer_RepaymentPeriod.ToString(CultureInfo.InvariantCulture)},
							{"InterestRate", LoanOffer_InterestRate.ToString(CultureInfo.InvariantCulture)},
							{"OfferValidUntil", App_ValidFor.ToString(CultureInfo.InvariantCulture)}
						};
					mailer.SendToEzbob(variables, "Mandrill - User is approved or re-approved", "User was automatically Re-Approved");

					if (!EnableAutomaticReApproval)
					{
						DbConnection.ExecuteSpNonQuery("Update_Main_Strat_Finish_Date", DbConnection.CreateParam("UserId", CustomerId));
					}
					else
					{
						double LoanOffer_OfferValidHours = Math.Round(LoanOffer_OfferValidDays * 24, 0);

						var variables2 = new Dictionary<string, string>
						{
							{"FirstName", App_FirstName},
							{"LoanAmount", LoanOffer_ReApprovalSum.ToString(CultureInfo.InvariantCulture)}
						};

						mailer.SendToCustomerAndEzbob(variables2, App_email, "Mandrill - Approval (not 1st time)", "Congratulations " + App_FirstName + ", £" + LoanOffer_ReApprovalSum +
								   " is available to fund your business today");

						strategyHelper.AddApproveIntoDecisionHistory(CustomerId, "Auto Re-Approval");
						DbConnection.ExecuteSpNonQuery("Update_Main_Strat_Finish_Date", DbConnection.CreateParam("UserId", CustomerId));
					}

					return;
				}
			}
			else if (UserStatus == "Rejected")
			{
				if ((IsReRejected && !EnableAutomaticReRejection) || (!IsReRejected && !EnableAutomaticRejection))
				{
					SendRejectionExplanationMail(IsReRejected ? "User was automatically Re-Rejected" : "User was automatically Rejected");
				}
				else
				{
					const string rejectionSubject = "Sorry, EZBOB cannot make you a loan offer at this time";
					SendRejectionExplanationMail(rejectionSubject);

					var variables = new Dictionary<string, string>
						{
							{"FirstName", App_FirstName},
							{"EzbobAccount", "https://app.ezbob.com/Customer/Profile"}
						};

					mailer.SendToCustomerAndEzbob(variables, App_email, "Mandrill - Rejection email", rejectionSubject);
					strategyHelper.AddRejectIntoDecisionHistory(CustomerId, AutoRejectReason);
				}
				DbConnection.ExecuteSpNonQuery("Update_Main_Strat_Finish_Date", DbConnection.CreateParam("UserId", CustomerId));
			}
			else
			{
				var variables = new Dictionary<string, string>
						{
							{"RegistrationDate", App_RegistrationDate.ToString(CultureInfo.InvariantCulture)},
							{"userID", CustomerId.ToString(CultureInfo.InvariantCulture)},
							{"Name", App_email},
							{"FirstName", App_FirstName},
							{"Surname", App_Surname},
							{"MP_Counter", AllMPsNum.ToString(CultureInfo.InvariantCulture)},
							{"MedalType", MedalType},
							{"SystemDecision", SystemDecision}
						};

				mailer.SendToEzbob(variables, "Mandrill - User is waiting for decision", "User is now waiting for decision");
				DbConnection.ExecuteSpNonQuery("Update_Main_Strat_Finish_Date", DbConnection.CreateParam("UserId", CustomerId));
			}
		}

		private void GetConsumerInfo(string line1, string line2, string line3, string line4, string line5, string line6, out string error)
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

			ConsumerServiceResult result = consumerService.GetConsumerInfo(App_FirstName, App_Surname, App_Gender, App_DateOfBirth, null, location, "PL", CustomerId, 0);

			if (result.IsError)
			{
				error = result.Error;
			}
			else
			{
				ExperianConsumerScore = (int) result.BureauScore;
				ExperianBirthDate = result.BirthDate;
				error = null;
			}
		}

		private void GetAddresses()
		{
			DataTable dt = DbConnection.ExecuteSpReader("GetCustomerAddresses");
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
		}

		private void UpdateCompanyScore()
		{
			if (CompanyType == "Limited" || CompanyType == "LLP")
			{
				string ExperianLimitedError = null;
				decimal ExperianBureauScoreLimited = 0;
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
					}
					else
					{
						ExperianLimitedError = limitedData.Error;
					}
				}

				DbConnection.ExecuteSpNonQuery("UpdateExperianBusiness",
				                               DbConnection.CreateParam("CompanyRefNumber", App_LimitedRefNum),
				                               DbConnection.CreateParam("ExperianError", ExperianLimitedError),
				                               DbConnection.CreateParam("ExperianScore", ExperianBureauScoreLimited),
				                               DbConnection.CreateParam("CustomerId", CustomerId));
			}
			else if (CompanyType == "PShip3P" || CompanyType == "PShip" || CompanyType == "SoleTrader")
			{
				string ExperianNonLimitedError = null;
				decimal ExperianBureauScoreNonLimited = 0;
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
					}
					else
					{
						ExperianNonLimitedError = nonlimitedData.Error;
					}
				}

				DbConnection.ExecuteSpNonQuery("UpdateExperianBusiness",
				                               DbConnection.CreateParam("CompanyRefNumber", App_NonLimitedRefNum),
				                               DbConnection.CreateParam("ExperianError", ExperianNonLimitedError),
				                               DbConnection.CreateParam("ExperianScore", ExperianBureauScoreNonLimited),
				                               DbConnection.CreateParam("CustomerId", CustomerId));
			}
		}

		private void SendRejectionExplanationMail(string subject)
		{
			var variables = new Dictionary<string, string>
						{
							{"RegistrationDate", App_RegistrationDate.ToString(CultureInfo.InvariantCulture)},
							{"userID", CustomerId.ToString(CultureInfo.InvariantCulture)},
							{"Name", App_email},
							{"FirstName", App_FirstName},
							{"Surname", App_Surname},
							{"MP_Counter", AllMPsNum.ToString(CultureInfo.InvariantCulture)},
							{"MedalType", MedalType},
							{"SystemDecision", SystemDecision},
							{"ExperianConsumerScore", Inintial_ExperianConsumerScore.ToString(CultureInfo.InvariantCulture)},
							{"CVExperianConsumerScore", LowCreditScore.ToString(CultureInfo.InvariantCulture)},
							{"TotalAnnualTurnover", TotalSumOfOrders1YTotal.ToString(CultureInfo.InvariantCulture)},
							{"CVTotalAnnualTurnover", LowTotalAnnualTurnover.ToString(CultureInfo.InvariantCulture)},
							{"Total3MTurnover", TotalSumOfOrders3MTotal.ToString(CultureInfo.InvariantCulture)},
							{"CVTotal3MTurnover", LowTotalThreeMonthTurnover.ToString(CultureInfo.InvariantCulture)},
							{"PayPalStoresNum", PayPal_NumberOfStores.ToString(CultureInfo.InvariantCulture)},
							{"PayPalAnnualTurnover", PayPal_TotalSumOfOrders1Y.ToString(CultureInfo.InvariantCulture)},
							{"CVPayPalAnnualTurnover", LowTotalAnnualTurnover.ToString(CultureInfo.InvariantCulture)},
							{"PayPal3MTurnover", PayPal_TotalSumOfOrders3M.ToString(CultureInfo.InvariantCulture)},
							{"CVPayPal3MTurnover", LowTotalThreeMonthTurnover.ToString(CultureInfo.InvariantCulture)},
							{"CVExperianConsumerScoreDefAcc", Reject_Defaults_CreditScore.ToString(CultureInfo.InvariantCulture)},
							{"ExperianDefAccNum", NumOfDefaultAccounts.ToString(CultureInfo.InvariantCulture)},
							{"CVExperianDefAccNum", Reject_Defaults_AccountsNum.ToString(CultureInfo.InvariantCulture)},
							{"Seniority", MarketplaceSeniorityDays.ToString(CultureInfo.InvariantCulture)},
							{"SeniorityThreshold", Reject_Minimal_Seniority.ToString(CultureInfo.InvariantCulture)}
						};

			mailer.SendToEzbob(variables, "Mandrill - User is rejected by the strategy", subject);
		}

		private AutoDecisionRequest CreateAutoDecisionRequest()
		{
			return new AutoDecisionRequest
			{
				App_ApplyForLoan = App_ApplyForLoan,
				App_ValidFor = App_ValidFor,
				AutoRejectReason = AutoRejectReason,
				CreditResult = CreditResult,
				CustomerId = CustomerId,
				EnableAutomaticApproval = EnableAutomaticApproval,
				EnableAutomaticReApproval = EnableAutomaticReApproval,
				EnableAutomaticRejection = EnableAutomaticRejection,
				EnableAutomaticReRejection = EnableAutomaticReRejection,
				Inintial_ExperianConsumerScore = Inintial_ExperianConsumerScore,
				IsAutoApproval = IsAutoApproval,
				SystemDecision = SystemDecision,
				UserStatus = UserStatus,
				LoanOffer_UnderwriterComment = LoanOffer_UnderwriterComment,
				ModelLoanOffer = ModelLoanOffer,
				IsReRejected = IsReRejected,
				LoanOffer_EmailSendingBanned_new = LoanOffer_EmailSendingBanned_new,
				LoanOffer_ReApprovalFullAmountOld = LoanOffer_ReApprovalFullAmountOld,
				LoanOffer_OfferValidDays = LoanOffer_OfferValidDays,
				LoanOffer_ReApprovalFullAmount = LoanOffer_ReApprovalFullAmount,
				LoanOffer_ReApprovalRemainingAmount = LoanOffer_ReApprovalRemainingAmount,
				LoanOffer_ReApprovalRemainingAmountOld = LoanOffer_ReApprovalRemainingAmountOld,
				LowTotalAnnualTurnover = LowTotalAnnualTurnover,
				LowTotalThreeMonthTurnover = LowTotalThreeMonthTurnover,
				MarketplaceSeniorityDays = MarketplaceSeniorityDays,
				MinExperianScore = MinExperianScore,
				OfferedCreditLine = OfferedCreditLine,
				PayPal_NumberOfStores = PayPal_NumberOfStores,
				PayPal_TotalSumOfOrders1Y = PayPal_TotalSumOfOrders1Y,
				PayPal_TotalSumOfOrders3M = PayPal_TotalSumOfOrders3M
			};
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

		private bool WaitForMarketplacesToFinishUpdates()
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

		// These are the activation methods:
		// main strategy - flow 1
		public void Evaluate(int customerId, NewCreditLineOption newCreditLine, int avoidAutoDescison,
							 bool isUnderwriterForced = false)
		{
			CustomerId = customerId;
			newCreditLineOption = newCreditLine;
			avoidAutomaticDescison = avoidAutoDescison;
			Underwriter_Check = isUnderwriterForced;
			Execute();
		}

		// main strategy - flow 2
		public void EvaluateWithIdHubCustomAddress(int customerId, int checkType, string houseNumber, string houseName,
												   string street,
												   string district, string town, string county, string postcode,
												   string bankAccount, string sortCode, int avoidAutoDescison)
		{
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
			Execute();
		}
	}
}

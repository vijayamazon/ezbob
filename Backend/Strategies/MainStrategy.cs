namespace EzBob.Backend.Strategies
{
	using AutoDecisions;
	using EzBob.Models;
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
		
		public override void Execute()
		{
			ReadConfigurations();
			GetPersonalInfo();
			strategyHelper.GetZooplaData(customerId);

			SetAutoDecisionAvailability();

			if (!MakeSureMpDataIsSufficient())
			{
				return;
			}

			if (newCreditLineOption != NewCreditLineOption.SkipEverything)
			{
				PerformCompanyExperianCheck();
				PerformConsumerExperianCheck();

				minExperianScore = experianConsumerScore;
				initialExperianConsumerScore = experianConsumerScore;

				PerformExperianConsumerCheckForDirectors();

				GetAml();
				GetBwa();
			} // if

			decimal scoringResult = CalculateScoreAndMedal();
			
			if (underwriterCheck)
			{
				SetEndTimestamp();
				return;
			} // if

			decimal loanInterestBase = GetBaseInterest();

			GetLastCashRequestData();

			CalcAndCapOffer();

			autoDecisionResponse = AutoDecisionMaker.MakeDecision(CreateAutoDecisionRequest(), DB, Log);
			if (autoDecisionResponse.SystemDecision == "Reject")
			{
				modelLoanOffer = 0;
			}

			UpdateCustomerAndCashRequest(scoringResult, loanInterestBase);

			if (autoDecisionResponse.UserStatus == "Approved")
			{
				if (autoDecisionResponse.IsAutoApproval)
				{
					UpdateApprovalData();
					SendApprovalMails();
					strategyHelper.AddApproveIntoDecisionHistory(customerId, "Auto Approval");
				}
				else
				{
					UpdateReApprovalData();
					SendReApprovalMails();
					if (enableAutomaticReApproval)
					{
						strategyHelper.AddApproveIntoDecisionHistory(customerId, "Auto Re-Approval");
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
			}
			else
			{
				SendWaitingForDecisionMail();
			} // if

			SetEndTimestamp();
		}

		private void CalcAndCapOffer()
		{
			if (loanOfferReApprovalRemainingAmount < 1000) // TODO: make this 1000 configurable
				loanOfferReApprovalRemainingAmount = 0;

			if (loanOfferReApprovalRemainingAmountOld < 500) // TODO: make this 500 configurable
				loanOfferReApprovalRemainingAmountOld = 0;
			loanOfferReApprovalSum = new decimal[]
				{
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
		}

		private void UpdateCustomerAndCashRequest(decimal scoringResult, decimal loanInterestBase)
		{
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
				"UpdateCashRequestsNew",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("SystemCalculatedAmount", modelLoanOffer),
				new QueryParameter("ManagerApprovedSum", offeredCreditLine),
				new QueryParameter("SystemDecision", autoDecisionResponse.SystemDecision),
				new QueryParameter("MedalType", medalType),
				new QueryParameter("ScorePoints", scoringResult),
				new QueryParameter("ExpirianRating", experianConsumerScore),
				new QueryParameter("AnualTurnover", totalSumOfOrders1YTotal),
				new QueryParameter("InterestRate", loanInterestBase),
				new QueryParameter("ManualSetupFeeAmount", manualSetupFeeAmount),
				new QueryParameter("ManualSetupFeePercent", manualSetupFeePercent)
				);
		}

		private decimal GetBaseInterest()
		{
			DataTable basicInterestRateDataTable = DB.ExecuteReader(
				"GetBasicInterestRate",
				CommandSpecies.StoredProcedure,
				new QueryParameter("Score", initialExperianConsumerScore)
			);
			var basicInterestRateRow = new SafeReader(basicInterestRateDataTable.Rows[0]);
			return basicInterestRateRow["LoanIntrestBase"];
		}

		private void SendWaitingForDecisionMail()
		{
			var variables = new Dictionary<string, string>
				{
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
		}

		private void SendReApprovalMails()
		{
			var variables = new Dictionary<string, string>
				{
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
						"OfferValidUntil",
						autoDecisionResponse.AppValidFor.HasValue
							? autoDecisionResponse.AppValidFor.Value.ToString(CultureInfo.InvariantCulture)
							: string.Empty
					}
				};

			mailer.SendToEzbob(variables, "Mandrill - User is approved or re-approved", "User was automatically Re-Approved");

			if (enableAutomaticReApproval)
			{
				var customerMailVariables = new Dictionary<string, string>
					{
						{"FirstName", appFirstName},
						{"LoanAmount", loanOfferReApprovalSum.ToString(CultureInfo.InvariantCulture)}
					};

				mailer.SendToCustomerAndEzbob(
					customerMailVariables,
					appEmail,
					"Mandrill - Approval (not 1st time)",
					"Congratulations " + appFirstName + ", £" + loanOfferReApprovalSum + " is available to fund your business today"
					);
			}
		}

		private void UpdateReApprovalData()
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
		}

		private void SendApprovalMails()
		{
			var variables = new Dictionary<string, string>
				{
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
					{
						"OfferValidUntil",
						autoDecisionResponse.AppValidFor.HasValue
							? autoDecisionResponse.AppValidFor.Value.ToString(CultureInfo.InvariantCulture)
							: string.Empty
					}
				};

			mailer.SendToEzbob(variables, "Mandrill - User is approved or re-approved", "User was automatically approved");

			var customerMailVariables = new Dictionary<string, string>
				{
					{"FirstName", appFirstName},
					{"LoanAmount", autoDecisionResponse.AutoApproveAmount.ToString(CultureInfo.InvariantCulture)}
				};

			mailer.SendToCustomerAndEzbob(
				customerMailVariables,
				appEmail,
				isFirstLoan ? "Mandrill - Approval (1st time)" : "Mandrill - Approval (not 1st time)",
				"Congratulations " + appFirstName + ", £" + autoDecisionResponse.AutoApproveAmount +
				" is available to fund your business today"
				);
		}

		private void UpdateApprovalData()
		{
			DB.ExecuteNonQuery(
				"UpdateAutoApproval",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("AutoApproveAmount", autoDecisionResponse.AutoApproveAmount)
				);
		}

		private void SetEndTimestamp()
		{
			DB.ExecuteNonQuery("Update_Main_Strat_Finish_Date", CommandSpecies.StoredProcedure, new QueryParameter("UserId", customerId));
		}

		private void GetLastCashRequestData()
		{
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
				manualSetupFeeAmount = lastOfferResults["ManualSetupFeeAmount"];
				manualSetupFeePercent = lastOfferResults["ManualSetupFeePercent"];
			}
		}

		private decimal CalculateScoreAndMedal()
		{
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

			return scoringResult.ScoreResult;
		}

		private void PerformExperianConsumerCheckForDirectors()
		{
			if (companyType != "Entrepreneur")
			{
				DataTable dt = DB.ExecuteReader(
					"GetCustomerDirectorsForConsumerCheck",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", customerId)
					);

				foreach (DataRow row in dt.Rows)
				{
					var sr = new SafeReader(row);
					int appDirId = sr["DirId"];
					string appDirName = sr["DirName"];
					string appDirSurname = sr["DirSurname"];

					if (string.IsNullOrEmpty(appDirName) || string.IsNullOrEmpty(appDirSurname))
						continue;

					PerformConsumerExperianCheck(appDirId);

					if (experianConsumerScore > 0 && experianConsumerScore < minExperianScore)
						minExperianScore = experianConsumerScore;
				} // foreach
			} // if
		}

		private void PerformConsumerExperianCheck(int directorId = 0)
		{
			if (wasMainStrategyExecutedBefore)
			{
				var strat = new ExperianConsumerCheck(customerId, directorId, DB, Log);
				strat.Execute();
				experianConsumerScore = strat.Score;
			}
			else if (!WaitForExperianConsumerCheckToFinishUpdates(directorId))
			{
				if (directorId == 0)
				{
					Log.Info("No data exist from experian consumer check for customer:{0}.", customerId);
				}
				else
				{
					Log.Info("No data exist from experian consumer check for director:{0}.", directorId);
				}
			}
		}

		private void PerformCompanyExperianCheck()
		{
			if (wasMainStrategyExecutedBefore)
			{
				var experianCompanyChecker = new ExperianCompanyCheck(customerId, DB, Log);
				experianCompanyChecker.Execute();
			}
			else if (!WaitForExperianCompanyCheckToFinishUpdates())
			{
				Log.Info("No data exist from experian company check for customer:{0}.", customerId);
			}
		}

		private bool MakeSureMpDataIsSufficient()
		{
			bool shouldExpectMpDta = newCreditLineOption != NewCreditLineOption.SkipEverything &&
			                         newCreditLineOption != NewCreditLineOption.UpdateEverythingExceptMp;
			if (shouldExpectMpDta)
			{
				if (!WaitForMarketplacesToFinishUpdates())
				{
					var variables = new Dictionary<string, string> {
						{"UserEmail", appEmail},
						{"CustomerID", customerId.ToString(CultureInfo.InvariantCulture)},
						{"ApplicationID", appEmail}
					};

					mailer.SendToEzbob(variables, "Mandrill - No Information about shops", "No information about customer marketplace");

					return false;
				} // if
			} // if

			return true;
		}

		private void SetAutoDecisionAvailability()
		{
			if (!customerStatusIsEnabled || customerStatusIsWarning)
			{
				enableAutomaticReApproval = false;
				enableAutomaticApproval = false;
			} // if
			
			if (isOffline || 
				newCreditLineOption == NewCreditLineOption.SkipEverything ||
				newCreditLineOption == NewCreditLineOption.UpdateEverythingExceptMp ||
				newCreditLineOption == NewCreditLineOption.UpdateEverythingAndGoToManualDecision ||
				avoidAutomaticDescison == 1)
			{
				enableAutomaticApproval = false;
				enableAutomaticReApproval = false;
				enableAutomaticRejection = false;
				enableAutomaticReRejection = false;
			}
		}
		
		#endregion public

		#region private

		#region properties

		// Helpers
		private readonly StrategiesMailer mailer;
		private readonly StrategyHelper strategyHelper = new StrategyHelper();
		private readonly MedalScoreCalculator medalScoreCalculator;
		
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
		private int experianConsumerScore;
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
		private double totalSumOfOrders3MTotal;
		private int modelLoanOffer;
		private double totalSumOfOrders1YTotal;
		private bool isFirstLoan;
		
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
			DataTable dt = DB.ExecuteReader("GetPersonalInfo", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", customerId));
			var results = new SafeReader(dt.Rows[0]);

			customerStatusIsEnabled = results["CustomerStatusIsEnabled"];
			customerStatusIsWarning = results["CustomerStatusIsWarning"];
			isOffline = results["IsOffline"];
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

		private void SendRejectionExplanationMail(string subject)
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

			totalSumOfOrders3MTotal = strategyHelper.GetTotalSumOfOrders3M(customerId);

			var variables = new Dictionary<string, string> {
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
				InitialExperianConsumerScore = initialExperianConsumerScore,
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
		
		private void GetBwa()
		{
			if (useCustomIdHubAddress == 0)
			{
				var bwaChecker = new BwaChecker(customerId, DB, Log);
				bwaChecker.Execute();
			}
			
			if (useCustomIdHubAddress == 2 || (useCustomIdHubAddress != 1 && ShouldRunBwa()))
			{
				var bwaChecker = new BwaChecker(customerId, idhubHouseNumber, idhubHouseName, idhubStreet, idhubDistrict, idhubTown, 
					idhubCounty, idhubPostCode, idhubBranchCode, idhubAccountNumber, DB, Log);
				bwaChecker.Execute();
			}
		}
		
		private void GetAml()
		{
			if (wasMainStrategyExecutedBefore)
			{
				if (useCustomIdHubAddress == 0)
				{
					var amlChecker = new AmlChecker(customerId, DB, Log);
					amlChecker.Execute();
				}

				if (useCustomIdHubAddress != 2)
				{
					var amlChecker = new AmlChecker(customerId, idhubHouseNumber, idhubHouseName, idhubStreet, idhubDistrict, idhubTown,
					                                idhubCounty, idhubPostCode, DB, Log);
					amlChecker.Execute();
				}
			}
			else if (!WaitForAmlToFinishUpdates())
			{
				Log.Info("No AML data exist for customer:{0}.", customerId);
			}
		}

		#region method ShouldRunBwa

		private bool ShouldRunBwa()
		{
			return appBankAccountType == "Personal" && bwaBusinessCheck == "1" && appSortCode != null && appAccountNumber != null;
		} // ShouldRunBwa

		#endregion method ShouldRunBwa
		
		private bool WaitForMarketplacesToFinishUpdates()
		{
			return WaitForUpdateToFinish(GetIsMarketPlacesUpdated, totalTimeToWaitForMarketplacesUpdate, intervalWaitForMarketplacesUpdate);
		} // WaitForMarketplacesToFinishUpdates
		
		private bool WaitForExperianCompanyCheckToFinishUpdates()
		{
			if (string.IsNullOrEmpty(experianRefNum))
			{
				return true;
			}

			return WaitForUpdateToFinish(GetIsExperianCompanyUpdated, totalTimeToWaitForExperianCompanyCheck, intervalWaitForExperianCompanyCheck);
		} // WaitForExperianCompanyCheckToFinishUpdates

		private bool WaitForExperianConsumerCheckToFinishUpdates(int directorId = 0)
		{
			return WaitForUpdateToFinish(() => GetIsExperianConsumerUpdated(directorId), totalTimeToWaitForExperianConsumerCheck, intervalWaitForExperianConsumerCheck);
		} // WaitForExperianConsumerCheckToFinishUpdates

		private bool WaitForAmlToFinishUpdates()
		{
			return WaitForUpdateToFinish(GetIsAmlUpdated, totalTimeToWaitForAmlCheck, intervalWaitForAmlCheck);
		} // WaitForMarketplacesToFinishUpdates

		private bool WaitForUpdateToFinish(Func<bool> function, int totalSecondsToWait, int intervalBetweenCheck)
		{
			DateTime startWaitingTime = DateTime.UtcNow;
			bool isUpdated = false;
			while (!isUpdated)
			{
				isUpdated = function();

				if (isUpdated)
					return true;

				if ((DateTime.UtcNow - startWaitingTime).TotalSeconds > totalSecondsToWait)
					return false;

				Thread.Sleep(intervalBetweenCheck);
			} // while

			return true;
		}

		private bool GetIsExperianConsumerUpdated(int directorId)
		{
			DataTable dt = DB.ExecuteReader("GetIsConsumerDataUpdated", CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", customerId),
					new QueryParameter("DirectorId", directorId));

			var sr = new SafeReader(dt.Rows[0]);
			return sr["IsUpdated"];
		}

		private bool GetIsAmlUpdated()
		{
			DataTable dt = DB.ExecuteReader(
				"GetIsAmlUpdated",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);

			var sr = new SafeReader(dt.Rows[0]);
			return sr["IsUpdated"];
		}
		private bool GetIsExperianCompanyUpdated()
		{
			DataTable dt = DB.ExecuteReader(
				"GetIsCompanyDataUpdated",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CompanyRefNumber", experianRefNum)
			);

			var sr = new SafeReader(dt.Rows[0]);
			return sr["IsUpdated"];
		}

		private bool GetIsMarketPlacesUpdated()
		{
			DataTable dt = DB.ExecuteReader(
				"MP_CustomerMarketplacesIsUpdated",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);

			var sr = new SafeReader(dt.Rows[0]);
			return sr["IsUpdated"];
		}

		#endregion private
	} // class MainStrategy

	#endregion class MainStrategy
} // namespace

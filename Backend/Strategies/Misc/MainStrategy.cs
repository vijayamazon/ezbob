namespace EzBob.Backend.Strategies.Misc
{
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
	using MailStrategies;

	public class MainStrategy : AStrategy
	{
		// Helpers
		private readonly StrategiesMailer mailer;
		private readonly StrategyHelper strategyHelper = new StrategyHelper();
		private readonly MedalScoreCalculator medalScoreCalculator;
		private readonly NewMedalScoreCalculator offlineMedalCalculator;
		private readonly AutoDecisionMaker autoDecisionMaker;

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
		private bool overrideApprovedRejected;

		// Configs
		private int rejectDefaultsCreditScore;
		private int rejectDefaultsAccountsNum;
		private int rejectMinimalSeniority;
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
		private string customerStatusName;
		private bool isOffline;
		private bool isBrokerCustomer;
		private string appEmail;
		private string companyType;
		private string experianRefNum;
		private string appFirstName;
		private string appSurname;
		private string appGender;
		private bool isHomeOwner;
		private string propertyStatusDescription;
		private string appAccountNumber;
		private string appSortCode;
		private DateTime appRegistrationDate;
		private string appBankAccountType;
		private bool wasMainStrategyExecutedBefore;
		private string typeOfBusiness;

		private int minExperianScore;
		private int maxExperianScore;
		private int maxCompanyScore;
		private int companySeniorityDays;
		private int experianConsumerScore;
		private int allMPsNum;
		private AutoDecisionResponse autoDecisionResponse;
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
		private double yodlee1YForRejection;
		private double yodlee3MForRejection;
		private bool isFirstLoan;

		private readonly List<string> consumerCaisDetailWorstStatuses = new List<string>();
		
		public override string Name { get { return "Main strategy"; } }

		public MainStrategy(
			int customerId,
			NewCreditLineOption newCreditLine,
			int avoidAutoDecision,
			AConnection oDb,
			ASafeLog oLog
		)
			: this(customerId, newCreditLine, avoidAutoDecision, false, oDb, oLog)
		{
		}

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
			offlineMedalCalculator = new NewMedalScoreCalculator(oDb, oLog);
			mailer = new StrategiesMailer(DB, Log);
			this.customerId = customerId;
			newCreditLineOption = newCreditLine;
			avoidAutomaticDecision = avoidAutoDecision;
			underwriterCheck = isUnderwriterForced;
			overrideApprovedRejected = true;
			autoDecisionMaker = new AutoDecisionMaker(DB, Log);
		}

		public override void Execute()
		{
			ReadConfigurations();

			MakeSureMpDataIsSufficient();

			GetPersonalInfo();

			SetAutoDecisionAvailability();

			GetExperianData();

			GetZooplaData();

			ScoreMedalOffer scoringResult = CalculateScoreAndMedal();

			if (underwriterCheck)
			{
				SetEndTimestamp();
				return;
			}

			GetLastCashRequestData();

			AutoDecisionRejectionResponse autoDecisionRejectionResponse = ProcessRejections();

			if (autoDecisionRejectionResponse.DecidedToReject)
			{
				modelLoanOffer = 0;

				if ((autoDecisionRejectionResponse.IsReRejected && !enableAutomaticReRejection) ||
					(!autoDecisionRejectionResponse.IsReRejected && !enableAutomaticRejection))
				{
					SendRejectionExplanationMail(autoDecisionRejectionResponse.IsReRejected
													 ? "Mandrill - User supposed to be re-rejected by the strategy"
													 : "Mandrill - User supposed to be rejected by the strategy",
												 autoDecisionRejectionResponse.RejectionModel);
				}
				else
				{
					SendRejectionExplanationMail("Mandrill - User is rejected by the strategy", autoDecisionRejectionResponse.RejectionModel);

					new RejectUser(customerId, true, DB, Log).Execute();

					strategyHelper.AddRejectIntoDecisionHistory(customerId, autoDecisionRejectionResponse.AutoRejectReason);
				}
			}

			GetLandRegistryDataIfNotRejected(autoDecisionRejectionResponse);

			CalcAndCapOffer();

			if (isOffline)
			{
				CalculateAndSaveOfflineMedal();
			}
			
			ProcessApprovals(autoDecisionRejectionResponse);

			autoDecisionMaker.LogDecision(customerId, autoDecisionRejectionResponse, autoDecisionResponse);

			if (autoDecisionResponse != null && autoDecisionResponse.IsAutoApproval)
			{
				modelLoanOffer = autoDecisionResponse.AutoApproveAmount;

				if (modelLoanOffer < offeredCreditLine)
				{
					offeredCreditLine = modelLoanOffer;
				}
			}
			else if (autoDecisionResponse != null && autoDecisionResponse.IsAutoBankBasedApproval)
			{
				modelLoanOffer = autoDecisionResponse.BankBasedAutoApproveAmount;

				if (modelLoanOffer < offeredCreditLine)
				{
					offeredCreditLine = modelLoanOffer;
				}
			}

			if (autoDecisionResponse == null)
			{
				UpdateCustomerAndCashRequest(scoringResult.ScoreResult, scoringResult.MaxOfferPercent, autoDecisionRejectionResponse.CreditResult,
					autoDecisionRejectionResponse.SystemDecision, autoDecisionRejectionResponse.UserStatus, null, 0);
			}
			else
			{
				UpdateCustomerAndCashRequest(scoringResult.ScoreResult, scoringResult.MaxOfferPercent, autoDecisionResponse.CreditResult, autoDecisionResponse.SystemDecision,
					autoDecisionResponse.UserStatus, autoDecisionResponse.AppValidFor, autoDecisionResponse.RepaymentPeriod);
			}

			if (autoDecisionResponse != null) // Means that wasn't rejected
			{
				if (autoDecisionResponse.UserStatus == "Approved")
				{
					if (autoDecisionResponse.IsAutoApproval)
					{
						UpdateApprovalData();
						SendApprovalMails(scoringResult.MaxOfferPercent);

						strategyHelper.AddApproveIntoDecisionHistory(customerId, "Auto Approval");
					}
					else if (autoDecisionResponse.IsAutoBankBasedApproval)
					{
						UpdateBankBasedApprovalData();
						SendBankBasedApprovalMails();

						strategyHelper.AddApproveIntoDecisionHistory(customerId, "Auto bank based approval");
					}
					else
					{
						UpdateReApprovalData();
						SendReApprovalMails();

						if (enableAutomaticReApproval)
						{
							strategyHelper.AddApproveIntoDecisionHistory(customerId, "Auto Re-Approval");
						}
					}
				}
				else
				{
					SendWaitingForDecisionMail();
				}
			}

			SetEndTimestamp();
		}

		private void ProcessApprovals(AutoDecisionRejectionResponse autoDecisionRejectionResponse)
		{
			if (!autoDecisionRejectionResponse.DecidedToReject)
			{
				autoDecisionResponse = autoDecisionMaker.MakeDecision(
					customerId,
					minExperianScore,
					maxExperianScore,
					maxCompanyScore,
					totalSumOfOrders1YTotalForRejection,
					totalSumOfOrders3MTotalForRejection,
					yodlee1YForRejection,
					yodlee3MForRejection,
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
					customerStatusIsEnabled,
					customerStatusIsWarning,
					isBrokerCustomer,
					typeOfBusiness == "Limited" || typeOfBusiness == "LLP",
					companySeniorityDays,
					isOffline,
					customerStatusName,
					consumerCaisDetailWorstStatuses,
					DB,
					Log
					);
			}
		}

		private void GetLandRegistryDataIfNotRejected(AutoDecisionRejectionResponse autoDecisionRejectionResponse)
		{
			if (autoDecisionRejectionResponse.CreditResult != "Rejected" && !autoDecisionRejectionResponse.DecidedToReject && isHomeOwner)
			{
				Log.Debug("Retrieving LandRegistry system decision: {0} residential status: {1}", autoDecisionRejectionResponse.SystemDecision, propertyStatusDescription);
				GetLandRegistry();
			}
			else
			{
				Log.Info("Not retrieving LandRegistry system decision: {0} residential status: {1}", autoDecisionRejectionResponse.SystemDecision, propertyStatusDescription);
			}
		}

		private AutoDecisionRejectionResponse ProcessRejections()
		{
			AutoDecisionRejectionResponse autoDecisionRejectionResponse = autoDecisionMaker.MakeRejectionDecision(
				customerId,
				maxExperianScore,
				maxCompanyScore,
				totalSumOfOrders1YTotalForRejection,
				totalSumOfOrders3MTotalForRejection,
				yodlee1YForRejection,
				yodlee3MForRejection,
				marketplaceSeniorityDays,
				enableAutomaticReRejection,
				enableAutomaticRejection,
				customerStatusIsEnabled,
				customerStatusIsWarning,
				isBrokerCustomer,
				typeOfBusiness == "Limited" || typeOfBusiness == "LLP",
				companySeniorityDays,
				isOffline,
				customerStatusName
				);
			return autoDecisionRejectionResponse;
		}

		private void GetExperianData()
		{
			if (newCreditLineOption != NewCreditLineOption.SkipEverything)
			{
				PerformCompanyExperianCheck();
				PerformConsumerExperianCheck();

				minExperianScore = experianConsumerScore;
				maxExperianScore = experianConsumerScore;
				initialExperianConsumerScore = experianConsumerScore;

				PerformExperianConsumerCheckForDirectors();

				GetAml();
				GetBwa();
			}
			else
			{
				experianConsumerScore = GetCurrentExperianScore();
				GetMaxCompanyExperianScore();
				minExperianScore = experianConsumerScore;
				maxExperianScore = experianConsumerScore;
				initialExperianConsumerScore = experianConsumerScore;
			}
		}

		private void GetLandRegistry()
		{
			var customerAddressesHelper = new CustomerAddressHelper(customerId, DB, Log);
			customerAddressesHelper.Execute();
			try
			{
				strategyHelper.GetLandRegistryData(customerId, customerAddressesHelper.OwnedAddresses);
			}
			catch (Exception e)
			{
				Log.Error("Error while getting land registry data: {0}", e);
			}
		}

		public virtual MainStrategy SetOverrideApprovedRejected(bool bOverrideApprovedRejected)
		{
			overrideApprovedRejected = bOverrideApprovedRejected;
			return this;
		}

		private void CalcAndCapOffer()
		{
			Log.Info("Finalizing and capping offer");

			if (loanOfferReApprovalRemainingAmount < 1000) // TODO: make this 1000 configurable
			{
				loanOfferReApprovalRemainingAmount = 0;
			}

			if (loanOfferReApprovalRemainingAmountOld < 500) // TODO: make this 500 configurable
			{
				loanOfferReApprovalRemainingAmountOld = 0;
			}

			loanOfferReApprovalSum = new[] {
				loanOfferReApprovalFullAmount,
				loanOfferReApprovalRemainingAmount,
				loanOfferReApprovalFullAmountOld,
				loanOfferReApprovalRemainingAmountOld
			}.Max();

			offeredCreditLine = modelLoanOffer;

			bool isHomeOwnerAccordingToLandRegistry = false;
			DataTable dt = DB.ExecuteReader(
				"GetIsCustomerHomeOwnerAccordingToLandRegistry",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);

			if (dt.Rows.Count == 1)
			{
				var sr = new SafeReader(dt.Rows[0]);
				isHomeOwnerAccordingToLandRegistry = sr["IsOwner"];
			}

			if (isHomeOwnerAccordingToLandRegistry && maxCapHomeOwner < loanOfferReApprovalSum)
			{
				loanOfferReApprovalSum = maxCapHomeOwner;
			}

			if (!isHomeOwnerAccordingToLandRegistry && maxCapNotHomeOwner < loanOfferReApprovalSum)
			{
				loanOfferReApprovalSum = maxCapNotHomeOwner;
			}

			if (isHomeOwnerAccordingToLandRegistry && maxCapHomeOwner < offeredCreditLine)
			{
				offeredCreditLine = maxCapHomeOwner;
			}

			if (!isHomeOwnerAccordingToLandRegistry && maxCapNotHomeOwner < offeredCreditLine)
			{
				offeredCreditLine = maxCapNotHomeOwner;
			}
		}
		
		private void UpdateCustomerAndCashRequest(decimal scoringResult, decimal loanInterestBase, string creditResult, string systemDecision, string userStatus, DateTime? appValidFor, int repaymentPeriod)
		{
			DB.ExecuteNonQuery(
				"UpdateScoringResultsNew",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("CreditResult", creditResult),
				new QueryParameter("SystemDecision", systemDecision),
				new QueryParameter("Status", userStatus),
				new QueryParameter("Medal", medalType.ToString()),
				new QueryParameter("ValidFor", appValidFor),
				new QueryParameter("Now", DateTime.UtcNow),
				new QueryParameter("OverrideApprovedRejected", overrideApprovedRejected)
			);

			decimal interestAccordingToPast = -1;
			DB.ForEachRowSafe(
				(sr, bRowsetStart) =>
				{
					interestAccordingToPast = sr["InterestRate"];
					return ActionResult.SkipAll;
				},
				"GetLatestInterestRate",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("Today", DateTime.UtcNow.Date)
			);

			DB.ExecuteNonQuery(
				"UpdateCashRequestsNew",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("SystemCalculatedAmount", modelLoanOffer),
				new QueryParameter("ManagerApprovedSum", offeredCreditLine),
				new QueryParameter("SystemDecision", systemDecision),
				new QueryParameter("MedalType", medalType.ToString()),
				new QueryParameter("ScorePoints", scoringResult),
				new QueryParameter("ExpirianRating", experianConsumerScore),
				new QueryParameter("AnualTurnover", totalSumOfOrders1YTotal),
				new QueryParameter("InterestRate", interestAccordingToPast == -1 ? loanInterestBase : interestAccordingToPast),
				new QueryParameter("ManualSetupFeeAmount", manualSetupFeeAmount),
				new QueryParameter("ManualSetupFeePercent", manualSetupFeePercent),
				new QueryParameter("RepaymentPeriod", repaymentPeriod),
				new QueryParameter("Now", DateTime.UtcNow)
			);
		}

		private void SendWaitingForDecisionMail()
		{
			mailer.Send("Mandrill - User is waiting for decision", new Dictionary<string, string> {
				{"RegistrationDate", appRegistrationDate.ToString(CultureInfo.InvariantCulture)},
				{"userID", customerId.ToString(CultureInfo.InvariantCulture)},
				{"Name", appEmail},
				{"FirstName", appFirstName},
				{"Surname", appSurname},
				{"MP_Counter", allMPsNum.ToString(CultureInfo.InvariantCulture)},
				{"MedalType", medalType.ToString()},
				{"SystemDecision", "WaitingForDecision"}
			});
		}

		private void SendReApprovalMails()
		{
			mailer.Send("Mandrill - User is re-approved", new Dictionary<string, string> {
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

			if (enableAutomaticReApproval)
			{
				var customerMailVariables = new Dictionary<string, string> {
					{"FirstName", appFirstName},
					{"LoanAmount", loanOfferReApprovalSum.ToString(CultureInfo.InvariantCulture)},
					{"ValidFor", autoDecisionResponse.AppValidFor.HasValue ? autoDecisionResponse.AppValidFor.Value.ToString(CultureInfo.InvariantCulture) : string.Empty}
				};

				mailer.Send("Mandrill - Approval (not 1st time)", customerMailVariables, new Addressee(appEmail));
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
		}

		private void SendApprovalMails(decimal interestRate)
		{
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

		private void SendBankBasedApprovalMails()
		{
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
		}

		private void UpdateBankBasedApprovalData()
		{
			DB.ExecuteNonQuery(
				"UpdateBankBasedAutoApproval",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("AutoApproveAmount", autoDecisionResponse.BankBasedAutoApproveAmount),
				new QueryParameter("RepaymentPeriod", autoDecisionResponse.RepaymentPeriod)
			);
		}

		private void SetEndTimestamp()
		{
			DB.ExecuteNonQuery("Update_Main_Strat_Finish_Date",
				CommandSpecies.StoredProcedure,
				new QueryParameter("UserId", customerId),
				new QueryParameter("Now", DateTime.UtcNow)
			);
		}

		private void GetLastCashRequestData()
		{
			DataTable lastOfferDataTable = DB.ExecuteReader(
				"GetLastOfferForAutomatedDecision",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("Now", DateTime.UtcNow)
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

		private ScoreMedalOffer CalculateScoreAndMedal()
		{
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

			if (!Enum.TryParse(maritalStatusStr, true, out maritalStatus))
			{
				Log.Warn("Cant parse marital status:{0}. Will use 'Other'", maritalStatusStr);
				maritalStatus = MaritalStatus.Other;
			}

			int modelMaxFeedback = scoreCardResults["MaxFeedback", defaultFeedbackValue];

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

			Log.Info("Getting turnovers and seniority");

			// TODO: Should be out of this method
			MpsTotals totals = strategyHelper.GetMpsTotals(customerId);
			totalSumOfOrders1YTotal = totals.TotalSumOfOrders1YTotal;
			totalSumOfOrders1YTotalForRejection = totals.TotalSumOfOrders1YTotalForRejection;
			totalSumOfOrders3MTotalForRejection = totals.TotalSumOfOrders3MTotalForRejection;
			yodlee1YForRejection = totals.Yodlee1YForRejection;
			yodlee3MForRejection = totals.Yodlee3MForRejection;
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
		}

		private void CalculateAndSaveOfflineMedal()
		{
			try
			{
				ScoreResult result = offlineMedalCalculator.CalculateMedalScore(customerId);
				DB.ExecuteNonQuery("StoreNewMedal", CommandSpecies.StoredProcedure,
								   new QueryParameter("CustomerId", customerId),
								   new QueryParameter("BusinessScore", result.BusinessScore),
								   new QueryParameter("BusinessScoreWeight", result.BusinessScoreWeight),
								   new QueryParameter("BusinessScoreGrade", result.BusinessScoreGrade),
								   new QueryParameter("BusinessScoreScore", result.BusinessScoreScore),
								   new QueryParameter("FreeCashFlow", result.FreeCashFlow),
								   new QueryParameter("FreeCashFlowWeight", result.FreeCashFlowWeight),
								   new QueryParameter("FreeCashFlowGrade", result.FreeCashFlowGrade),
								   new QueryParameter("FreeCashFlowScore", result.FreeCashFlowScore),
								   new QueryParameter("AnnualTurnover", result.AnnualTurnover),
								   new QueryParameter("AnnualTurnoverWeight", result.AnnualTurnoverWeight),
								   new QueryParameter("AnnualTurnoverGrade", result.AnnualTurnoverGrade),
								   new QueryParameter("AnnualTurnoverScore", result.AnnualTurnoverScore),
								   new QueryParameter("TangibleEquity", result.TangibleEquity),
								   new QueryParameter("TangibleEquityWeight", result.TangibleEquityWeight),
								   new QueryParameter("TangibleEquityGrade", result.TangibleEquityGrade),
								   new QueryParameter("TangibleEquityScore", result.TangibleEquityScore),
								   new QueryParameter("BusinessSeniority", result.BusinessSeniority.HasValue && result.BusinessSeniority.Value.Year > 1800 ? result.BusinessSeniority : null),
								   new QueryParameter("BusinessSeniorityWeight", result.BusinessSeniorityWeight),
								   new QueryParameter("BusinessSeniorityGrade", result.BusinessSeniorityGrade),
								   new QueryParameter("BusinessSeniorityScore", result.BusinessSeniorityScore),
								   new QueryParameter("ConsumerScore", result.ConsumerScore),
								   new QueryParameter("ConsumerScoreWeight", result.ConsumerScoreWeight),
								   new QueryParameter("ConsumerScoreGrade", result.ConsumerScoreGrade),
								   new QueryParameter("ConsumerScoreScore", result.ConsumerScoreScore),
								   new QueryParameter("NetWorth", result.NetWorth),
								   new QueryParameter("NetWorthWeight", result.NetWorthWeight),
								   new QueryParameter("NetWorthGrade", result.NetWorthGrade),
								   new QueryParameter("NetWorthScore", result.NetWorthScore),
								   new QueryParameter("MaritalStatus", result.MaritalStatus.ToString()),
								   new QueryParameter("MaritalStatusWeight", result.MaritalStatusWeight),
								   new QueryParameter("MaritalStatusGrade", result.MaritalStatusGrade),
								   new QueryParameter("MaritalStatusScore", result.MaritalStatusScore),
								   new QueryParameter("EzbobSeniority", result.EzbobSeniority),
								   new QueryParameter("EzbobSeniorityWeight", result.EzbobSeniorityWeight),
								   new QueryParameter("EzbobSeniorityGrade", result.EzbobSeniorityGrade),
								   new QueryParameter("EzbobSeniorityScore", result.EzbobSeniorityScore),
								   new QueryParameter("NumOfLoans", result.NumOfLoans),
								   new QueryParameter("NumOfLoansWeight", result.NumOfLoansWeight),
								   new QueryParameter("NumOfLoansGrade", result.NumOfLoansGrade),
								   new QueryParameter("NumOfLoansScore", result.NumOfLoansScore),
								   new QueryParameter("NumOfLateRepayments", result.NumOfLateRepayments),
								   new QueryParameter("NumOfLateRepaymentsWeight", result.NumOfLateRepaymentsWeight),
								   new QueryParameter("NumOfLateRepaymentsGrade", result.NumOfLateRepaymentsGrade),
								   new QueryParameter("NumOfLateRepaymentsScore", result.NumOfLateRepaymentsScore),
								   new QueryParameter("NumOfEarlyRepayments", result.NumOfEarlyRepayments),
								   new QueryParameter("NumOfEarlyRepaymentsWeight", result.NumOfEarlyRepaymentsWeight),
								   new QueryParameter("NumOfEarlyRepaymentsGrade", result.NumOfEarlyRepaymentsGrade),
								   new QueryParameter("NumOfEarlyRepaymentsScore", result.NumOfEarlyRepaymentsScore),
								   new QueryParameter("TotalScore", result.TotalScore),
								   new QueryParameter("TotalScoreNormalized", result.TotalScoreNormalized),
								   new QueryParameter("Medal", result.Medal.ToString()));
			}
			catch (Exception e)
			{
				Log.Warn("Offline medal calculation for customer {0} failed with exception:{1}", customerId, e);
			}
		}

		private void PerformExperianConsumerCheckForDirectors()
		{
			if (companyType == "Entrepreneur")
			{
				return;
			}

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
				{
					continue;
				}

				PerformConsumerExperianCheck(appDirId);

				if (experianConsumerScore > 0 && experianConsumerScore < minExperianScore)
				{
					minExperianScore = experianConsumerScore;
				}

				if (experianConsumerScore > 0 && experianConsumerScore > maxExperianScore)
				{
					maxExperianScore = experianConsumerScore;
				}
			}
		}

		private void PerformConsumerExperianCheck(int? directorId = null)
		{
			if (wasMainStrategyExecutedBefore)
			{
				Log.Info("Performing experian consumer check");

				var strat = new ExperianConsumerCheck(customerId, directorId, false, DB, Log);
				strat.Execute();

				foreach (var caisDetails in strat.Result.Cais)
				{
					consumerCaisDetailWorstStatuses.Add(caisDetails.WorstStatus);
				}

				if (directorId == null)
				{
					experianConsumerScore = strat.Score;
				}

				return;
			}

			if (!WaitForExperianConsumerCheckToFinishUpdates(directorId))
			{
				Log.Info("No data exist from experian consumer check for customer {0}{1}.", customerId, directorId == null ? "" : "director " + directorId);
				return;
			}

			if (directorId == null)
			{
				var strat = new ExperianConsumerCheck(customerId, null, false, DB, Log);
				strat.Execute();

				foreach (var caisDetails in strat.Result.Cais)
				{
					consumerCaisDetailWorstStatuses.Add(caisDetails.WorstStatus);
				}

				experianConsumerScore = GetCurrentExperianScore();
			}
		}

		private int GetCurrentExperianScore()
		{
			var scoreStrat = new GetExperianConsumerScore(customerId, DB, Log);
			scoreStrat.Execute();
			return scoreStrat.Score;
		}

		private void GetMaxCompanyExperianScore()
		{
			maxCompanyScore = DB.ExecuteScalar<int>(
				"GetCompanyScore",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);
		}

		private void GetCompanySeniorityDays()
		{
			var seniority = new GetCompanySeniority(customerId, typeOfBusiness == "Limited" || typeOfBusiness == "LLP", DB, Log);
			seniority.Execute();
			companySeniorityDays = seniority.CompanyIncorporationDate.HasValue
				                   ? (DateTime.UtcNow - seniority.CompanyIncorporationDate.Value).Days
				                   : 0;
		}

		private void PerformCompanyExperianCheck()
		{
			if (wasMainStrategyExecutedBefore)
			{
				Log.Info("Performing experian company check");
				var experianCompanyChecker = new ExperianCompanyCheck(customerId, false, DB, Log);
				experianCompanyChecker.Execute();

				maxCompanyScore = Math.Max((int)experianCompanyChecker.MaxScore, (int)experianCompanyChecker.Score);
			}
			else if (!WaitForExperianCompanyCheckToFinishUpdates())
			{
				Log.Info("No data exist from experian company check for customer:{0}.", customerId);
			}
			else
			{
				GetMaxCompanyExperianScore();
			}
		}

		private void MakeSureMpDataIsSufficient()
		{
			bool shouldExpectMpData =
				newCreditLineOption != NewCreditLineOption.SkipEverything &&
				newCreditLineOption != NewCreditLineOption.UpdateEverythingExceptMp;

			if (shouldExpectMpData)
			{
				if (!WaitForMarketplacesToFinishUpdates())
				{
					Log.Info("Waiting for marketplace data ended with error");

					mailer.Send("Mandrill - No Information about shops", new Dictionary<string, string> {
						{"UserEmail", appEmail},
						{"CustomerID", customerId.ToString(CultureInfo.InvariantCulture)},
						{"ApplicationID", appEmail}
					});
				}
			}
		}

		private void SetAutoDecisionAvailability()
		{
			Log.Info("Setting auto decision availability");

			if (!customerStatusIsEnabled || customerStatusIsWarning)
			{
				enableAutomaticReApproval = false;
				enableAutomaticApproval = false;
			}

			if (isOffline)
			{
				enableAutomaticApproval = false;
			}

			if (isBrokerCustomer)
			{
				enableAutomaticApproval = false;
				enableAutomaticRejection = false;
			}

			if (
				newCreditLineOption == NewCreditLineOption.SkipEverything ||
				newCreditLineOption == NewCreditLineOption.UpdateEverythingExceptMp ||
				newCreditLineOption == NewCreditLineOption.UpdateEverythingAndGoToManualDecision ||
				avoidAutomaticDecision == 1
			)
			{
				enableAutomaticApproval = false;
				enableAutomaticReApproval = false;
				enableAutomaticRejection = false;
				enableAutomaticReRejection = false;
			}
		}

		private void ReadConfigurations()
		{
			Log.Info("Getting configurations");
			DataTable dt = DB.ExecuteReader("MainStrategyGetConfigs", CommandSpecies.StoredProcedure);
			DataRow results = dt.Rows[0];
			var sr = new SafeReader(results);
			rejectDefaultsCreditScore = sr["Reject_Defaults_CreditScore"];
			rejectDefaultsAccountsNum = sr["Reject_Defaults_AccountsNum"];
			rejectMinimalSeniority = sr["Reject_Minimal_Seniority"];
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
		}

		private void GetPersonalInfo()
		{
			Log.Info("Getting personal info for customer:{0}", customerId);
			DataTable dt = DB.ExecuteReader("GetPersonalInfo", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", customerId));
			var results = new SafeReader(dt.Rows[0]);

			customerStatusIsEnabled = results["CustomerStatusIsEnabled"];
			customerStatusIsWarning = results["CustomerStatusIsWarning"];
			customerStatusName = results["CustomerStatusName"];
			isOffline = results["IsOffline"];
			isBrokerCustomer = results["IsBrokerCustomer"];
			appEmail = results["CustomerEmail"];
			companyType = results["CompanyType"];
			experianRefNum = results["ExperianRefNum"];
			wasMainStrategyExecutedBefore = results["MainStrategyExecutedBefore"];
			appFirstName = results["FirstName"];
			appSurname = results["Surname"];
			appGender = results["Gender"];
			bool isOwnerOfMainAddress = results["IsOwnerOfMainAddress"];
			bool isOwnerOfOtherProperties = results["IsOwnerOfOtherProperties"];
			isHomeOwner = isOwnerOfMainAddress || isOwnerOfOtherProperties;
			propertyStatusDescription = results["PropertyStatusDescription"];
			allMPsNum = results["NumOfMps"];
			appAccountNumber = results["AccountNumber"];
			appSortCode = results["SortCode"];
			appRegistrationDate = results["RegistrationDate"];
			appBankAccountType = results["BankAccountType"];
			int numOfLoans = results["NumOfLoans"];
			isFirstLoan = numOfLoans == 0;
			typeOfBusiness = results["TypeOfBusiness"];

			GetCompanySeniorityDays();
		}

		private void SendRejectionExplanationMail(string templateName, RejectionModel rejection)
		{
			string additionalValues = string.Empty;
			if (rejection == null)
			{
				rejection = new RejectionModel();
			}
			else
			{
				additionalValues =
					string.Format(
						"\n is offline: {8} \n company score: {0} \n company seniority: {9} \n {1} \n {2} \n {3} \n {4} \n {5} \n {6} \n num of late CAIS accounts: {7}",
						rejection.CompanyScore,
						rejection.HasHmrc ? "Hmrc Annual and Quarter: " : "",
						rejection.HasHmrc ? rejection.Hmrc1Y.ToString("C2") : "",
						rejection.HasHmrc ? rejection.Hmrc3M.ToString("C2") : "",
						rejection.HasYodlee ? "Yodlee Annual and Quarter: " : "",
						rejection.HasHmrc ? rejection.Yodlee1Y.ToString("C2") : "",
						rejection.HasHmrc ? rejection.Yodlee3M.ToString("C2") : "",
						rejection.LateAccounts,
						isOffline,
						companySeniorityDays
					);
			}
			
			mailer.Send(templateName, new Dictionary<string, string> {
				{"RegistrationDate", appRegistrationDate.ToString(CultureInfo.InvariantCulture)},
				{"userID", customerId.ToString(CultureInfo.InvariantCulture)},
				{"Name", appEmail},
				{"FirstName", appFirstName},
				{"Surname", appSurname},
				{"MP_Counter", allMPsNum.ToString(CultureInfo.InvariantCulture)},
				{"MedalType", medalType.ToString()},
				{"SystemDecision", "Reject"},
				{"ExperianConsumerScore", initialExperianConsumerScore.ToString(CultureInfo.InvariantCulture)},
				{"CVExperianConsumerScore", lowCreditScore.ToString(CultureInfo.InvariantCulture)},
				{"TotalAnnualTurnover", totalSumOfOrders1YTotalForRejection.ToString("C2")},
				{"CVTotalAnnualTurnover", lowTotalAnnualTurnover.ToString("C2")},
				{"Total3MTurnover", totalSumOfOrders3MTotalForRejection.ToString("C2")},
				{"CVTotal3MTurnover", lowTotalThreeMonthTurnover.ToString("C2")},
				{"PayPalStoresNum", rejection.PayPalNumberOfStores.ToString(CultureInfo.InvariantCulture)},
				{"PayPalAnnualTurnover", rejection.PayPalTotalSumOfOrders1Y.ToString("C2")},
				{"CVPayPalAnnualTurnover", lowTotalAnnualTurnover.ToString("C2")},
				{"PayPal3MTurnover", rejection.PayPalTotalSumOfOrders3M.ToString("C2")},
				{"CVPayPal3MTurnover", lowTotalThreeMonthTurnover.ToString("C2")},
				{"CVExperianConsumerScoreDefAcc", rejectDefaultsCreditScore.ToString(CultureInfo.InvariantCulture)},
				{"ExperianDefAccNum", rejection.NumOfDefaultAccounts.ToString(CultureInfo.InvariantCulture)},
				{"CVExperianDefAccNum", rejectDefaultsAccountsNum.ToString(CultureInfo.InvariantCulture)},
				{"Seniority", marketplaceSeniorityDays.ToString(CultureInfo.InvariantCulture)},
				{"SeniorityThreshold", rejectMinimalSeniority.ToString(CultureInfo.InvariantCulture) + additionalValues}
			});
		}

		private void GetBwa()
		{
			if (ShouldRunBwa())
			{
				Log.Info("Getting BWA for customer: {0}", customerId);
				var bwaChecker = new BwaChecker(customerId, DB, Log);
				bwaChecker.Execute();
			}
		}

		private void GetAml()
		{
			if (wasMainStrategyExecutedBefore)
			{
				Log.Info("Getting AML for customer: {0}", customerId);
				var amlChecker = new AmlChecker(customerId, DB, Log);
				amlChecker.Execute();
			}
			else if (!WaitForAmlToFinishUpdates())
			{
				Log.Info("No AML data exist for customer:{0}.", customerId);
			}
		}

		private bool ShouldRunBwa()
		{
			return appBankAccountType == "Personal" && bwaBusinessCheck == "1" && appSortCode != null && appAccountNumber != null;
		}

		private bool WaitForMarketplacesToFinishUpdates()
		{
			Log.Info("Waiting for marketplace data");
			return WaitForUpdateToFinish(GetIsMarketPlacesUpdated, totalTimeToWaitForMarketplacesUpdate, intervalWaitForMarketplacesUpdate);
		}

		private bool WaitForExperianCompanyCheckToFinishUpdates()
		{
			Log.Info("Waiting for experian company check");

			if (string.IsNullOrEmpty(experianRefNum))
			{
				return true;
			}

			return WaitForUpdateToFinish(GetIsExperianCompanyUpdated, totalTimeToWaitForExperianCompanyCheck, intervalWaitForExperianCompanyCheck);
		}

		private bool WaitForExperianConsumerCheckToFinishUpdates(int? directorId = null)
		{
			Log.Info("Waiting for experian consumer check");
			return WaitForUpdateToFinish(() => GetIsExperianConsumerUpdated(directorId), totalTimeToWaitForExperianConsumerCheck, intervalWaitForExperianConsumerCheck);
		}

		private bool WaitForAmlToFinishUpdates()
		{
			Log.Info("Waiting for AML check");
			return WaitForUpdateToFinish(GetIsAmlUpdated, totalTimeToWaitForAmlCheck, intervalWaitForAmlCheck);
		}

		private bool WaitForUpdateToFinish(Func<bool> function, int totalSecondsToWait, int intervalBetweenCheck)
		{
			DateTime startWaitingTime = DateTime.UtcNow;

			for (; ; )
			{
				if (function())
				{
					return true;
				}

				if ((DateTime.UtcNow - startWaitingTime).TotalSeconds > totalSecondsToWait)
				{
					return false;
				}

				Thread.Sleep(intervalBetweenCheck);
			}
		}

		private bool GetIsExperianConsumerUpdated(int? directorId)
		{
			return DB.ExecuteScalar<bool>(
				"GetIsConsumerDataUpdated",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("DirectorId", directorId),
				new QueryParameter("Today", DateTime.Today)
			);
		}

		private bool GetIsAmlUpdated()
		{
			return DB.ExecuteScalar<bool>("GetIsAmlUpdated",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId));
		}

		private bool GetIsExperianCompanyUpdated()
		{
			return DB.ExecuteScalar<bool>(
				"GetIsCompanyDataUpdated",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("Today", DateTime.Today)
			);
		}

		private bool GetIsMarketPlacesUpdated()
		{
			bool result = true;

			DB.ForEachRowSafe(
				(sr, rowsetStart) =>
				{
					string lastStatus = sr["CurrentStatus"];

					if (lastStatus != "Done" && lastStatus != "Never Started" && lastStatus != "Finished" && lastStatus != "Failed" && lastStatus != "Terminated")
					{
						result = false;
						return ActionResult.SkipAll;
					}

					return ActionResult.Continue;
				},
				"GetAllLastMarketplaceStatuses",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);

			return result;
		}

		private void GetZooplaData()
		{
			Log.Info("Getting zoopla data for customer:{0}", customerId);
			strategyHelper.GetZooplaData(customerId);
		}
	}
}

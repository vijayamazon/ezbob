namespace EzBob.Backend.Strategies.MainStrategy
{
	using AutoDecisions;
	using Experian;
	using EzBob.Models;
	using Ezbob.Backend.Models;
	using MailStrategies.API;
	using Misc;
	using ScoreCalculation;
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Database;
	using Ezbob.Logger;
	using MailStrategies;

	public class MainStrategy : AStrategy
	{
		// Helpers
		private readonly StrategiesMailer mailer;
		private readonly Staller staller;
		private readonly DataGatherer dataGatherer;
		private readonly StrategyHelper strategyHelper = new StrategyHelper();
		private readonly MedalScoreCalculator medalScoreCalculator;
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

		// Automation availability
		private bool enableAutomaticReRejection;
		private bool enableAutomaticReApproval;
		private bool enableAutomaticApproval;
		private bool enableAutomaticRejection;

		// Calculated based on raw data
		private bool isHomeOwner;
		private bool isFirstLoan;
		private bool wasMainStrategyExecutedBefore;
		private bool isViaBroker;
		private int companySeniorityDays;


		private int minExperianScore;
		private int maxExperianScore;
		private int maxCompanyScore;
		private int experianConsumerScore;
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

			mailer = new StrategiesMailer(DB, Log);
			this.customerId = customerId;
			newCreditLineOption = newCreditLine;
			avoidAutomaticDecision = avoidAutoDecision;
			underwriterCheck = isUnderwriterForced;
			overrideApprovedRejected = true;
			autoDecisionMaker = new AutoDecisionMaker(DB, Log);
			staller = new Staller(customerId, newCreditLineOption, mailer, DB, Log);
			dataGatherer = new DataGatherer(customerId, DB, Log);
		}

		public override void Execute()
		{
			// Wait for data to be filled by other strategies
			staller.Stall();

			// Gather Raw Data
			dataGatherer.Gather();

			// Automation availability
			enableAutomaticReRejection = dataGatherer.EnableAutomaticReRejection;
			enableAutomaticReApproval = dataGatherer.EnableAutomaticReApproval;
			enableAutomaticApproval = dataGatherer.EnableAutomaticApproval;
			enableAutomaticRejection = dataGatherer.EnableAutomaticRejection;

			// Processing logic
			isHomeOwner = dataGatherer.IsOwnerOfMainAddress || dataGatherer.IsOwnerOfOtherProperties;
			isFirstLoan = dataGatherer.NumOfLoans == 0;
			wasMainStrategyExecutedBefore = dataGatherer.LastStartedMainStrategyEndTime.HasValue;
			isViaBroker = dataGatherer.BrokerId.HasValue;
			companySeniorityDays = dataGatherer.CompanyIncorporationDate.HasValue ? (DateTime.UtcNow - dataGatherer.CompanyIncorporationDate.Value).Days : 0;

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

			if (dataGatherer.NumOfHmrcMps < 2 && (dataGatherer.TypeOfBusiness == "LLP" || dataGatherer.TypeOfBusiness == "Limited"))
			{
				var instance = new CalculateLimitedMedal(DB, Log, customerId);
				instance.Execute();

				int offerAccordingToThisMedal = 0;
				if (instance.Result != null && string.IsNullOrEmpty(instance.Result.Error))
				{
					SafeReader sr = DB.GetFirst(
						"GetMedalCoefficients",
						CommandSpecies.StoredProcedure,
						new QueryParameter("MedalFlow", "Limited"),
						new QueryParameter("Medal", instance.Result.Medal.ToString())
					);

					if (!sr.IsEmpty)
					{
						decimal annualTurnoverMedalFactor = sr["AnnualTurnover"];
						decimal offerAccordingToAnnualTurnover = instance.Result.AnnualTurnover * annualTurnoverMedalFactor;

						if (instance.Result.BasedOnHmrcValues)
						{
							decimal freeCashFlowMedalFactor = sr["FreeCashFlow"];
							decimal valueAddedMedalFactor = sr["ValueAdded"];
							decimal offerAccordingToFreeCashFlow = instance.Result.FreeCashFlowValue * freeCashFlowMedalFactor;
							decimal offerAccordingToValueAdded = instance.Result.ValueAdded * valueAddedMedalFactor;

							// Get min that is over threshold
							if ((offerAccordingToFreeCashFlow <= offerAccordingToValueAdded ||
								 offerAccordingToValueAdded <= dataGatherer.LimitedMedalMinOffer) &&
							    (offerAccordingToFreeCashFlow <= offerAccordingToAnnualTurnover ||
								 offerAccordingToAnnualTurnover <= dataGatherer.LimitedMedalMinOffer) &&
								offerAccordingToFreeCashFlow >= dataGatherer.LimitedMedalMinOffer)
							{
								offerAccordingToThisMedal = (int) offerAccordingToFreeCashFlow;
								Log.Info("Calculated offer for customer: {0} according to free cash flow ({1})", customerId, offerAccordingToFreeCashFlow);
							}
							else if ((offerAccordingToValueAdded <= offerAccordingToFreeCashFlow ||
									  offerAccordingToFreeCashFlow <= dataGatherer.LimitedMedalMinOffer) &&
							         (offerAccordingToValueAdded <= offerAccordingToAnnualTurnover ||
									  offerAccordingToAnnualTurnover <= dataGatherer.LimitedMedalMinOffer) &&
									 offerAccordingToValueAdded >= dataGatherer.LimitedMedalMinOffer)
							{
								offerAccordingToThisMedal = (int) offerAccordingToValueAdded;
								Log.Info("Calculated offer for customer: {0} according to value added ({1})", customerId, offerAccordingToValueAdded);
							}
							else if ((offerAccordingToAnnualTurnover <= offerAccordingToFreeCashFlow ||
									  offerAccordingToFreeCashFlow <= dataGatherer.LimitedMedalMinOffer) &&
							         (offerAccordingToAnnualTurnover <= offerAccordingToValueAdded ||
									  offerAccordingToValueAdded <= dataGatherer.LimitedMedalMinOffer) &&
									 offerAccordingToAnnualTurnover >= dataGatherer.LimitedMedalMinOffer)
							{
								offerAccordingToThisMedal = (int) offerAccordingToAnnualTurnover;
								Log.Info("Calculated offer for customer: {0} according to annual turnover ({1})", customerId, offerAccordingToAnnualTurnover);
							}
						}
						else if (offerAccordingToAnnualTurnover >= dataGatherer.LimitedMedalMinOffer)
						{
							offerAccordingToThisMedal = (int) offerAccordingToAnnualTurnover;
							Log.Info("Calculated offer for customer: {0} according to annual turnover ({1})", customerId, offerAccordingToAnnualTurnover);
						}
					}
				}

				CalcAndCapOffer(offerAccordingToThisMedal);
			}
			else
			{
				CalcAndCapOffer(modelLoanOffer);
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
					dataGatherer.CustomerStatusIsEnabled,
					dataGatherer.CustomerStatusIsWarning,
					isViaBroker,
					dataGatherer.TypeOfBusiness == "Limited" || dataGatherer.TypeOfBusiness == "LLP",
					companySeniorityDays,
					dataGatherer.IsOffline,
					dataGatherer.CustomerStatusName,
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
				Log.Debug("Retrieving LandRegistry system decision: {0} residential status: {1}", autoDecisionRejectionResponse.SystemDecision, dataGatherer.PropertyStatusDescription);
				GetLandRegistry();
			}
			else
			{
				Log.Info("Not retrieving LandRegistry system decision: {0} residential status: {1}", autoDecisionRejectionResponse.SystemDecision, dataGatherer.PropertyStatusDescription);
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
				dataGatherer.CustomerStatusIsEnabled,
				dataGatherer.CustomerStatusIsWarning,
				isViaBroker,
				dataGatherer.TypeOfBusiness == "Limited" || dataGatherer.TypeOfBusiness == "LLP",
				companySeniorityDays,
				dataGatherer.IsOffline,
				dataGatherer.CustomerStatusName
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

		private void CalcAndCapOffer(int loanAmount)
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

			offeredCreditLine = loanAmount;

			bool isHomeOwnerAccordingToLandRegistry = false;
			SafeReader sr = DB.GetFirst(
				"GetIsCustomerHomeOwnerAccordingToLandRegistry",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);

			if (!sr.IsEmpty)
			{
				isHomeOwnerAccordingToLandRegistry = sr["IsOwner"];
			}

			if (isHomeOwnerAccordingToLandRegistry && dataGatherer.MaxCapHomeOwner < loanOfferReApprovalSum)
			{
				loanOfferReApprovalSum = dataGatherer.MaxCapHomeOwner;
			}

			if (!isHomeOwnerAccordingToLandRegistry && dataGatherer.MaxCapNotHomeOwner < loanOfferReApprovalSum)
			{
				loanOfferReApprovalSum = dataGatherer.MaxCapNotHomeOwner;
			}

			if (isHomeOwnerAccordingToLandRegistry && dataGatherer.MaxCapHomeOwner < offeredCreditLine)
			{
				offeredCreditLine = dataGatherer.MaxCapHomeOwner;
			}

			if (!isHomeOwnerAccordingToLandRegistry && dataGatherer.MaxCapNotHomeOwner < offeredCreditLine)
			{
				offeredCreditLine = dataGatherer.MaxCapNotHomeOwner;
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
				{"RegistrationDate", dataGatherer.AppRegistrationDate.ToString(CultureInfo.InvariantCulture)},
				{"userID", customerId.ToString(CultureInfo.InvariantCulture)},
				{"Name", dataGatherer.AppEmail},
				{"FirstName", dataGatherer.AppFirstName},
				{"Surname", dataGatherer.AppSurname},
				{"MP_Counter", dataGatherer.AllMPsNum.ToString(CultureInfo.InvariantCulture)},
				{"MedalType", medalType.ToString()},
				{"SystemDecision", "WaitingForDecision"}
			});
		}

		private void SendReApprovalMails()
		{
			mailer.Send("Mandrill - User is re-approved", new Dictionary<string, string> {
				{"ApprovedReApproved", "Re-Approved"},
				{"RegistrationDate", dataGatherer.AppRegistrationDate.ToString(CultureInfo.InvariantCulture)},
				{"userID", customerId.ToString(CultureInfo.InvariantCulture)},
				{"Name", dataGatherer.AppEmail},
				{"FirstName", dataGatherer.AppFirstName},
				{"Surname", dataGatherer.AppSurname},
				{"MP_Counter", dataGatherer.AllMPsNum.ToString(CultureInfo.InvariantCulture)},
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
					{"FirstName", dataGatherer.AppFirstName},
					{"LoanAmount", loanOfferReApprovalSum.ToString(CultureInfo.InvariantCulture)},
					{"ValidFor", autoDecisionResponse.AppValidFor.HasValue ? autoDecisionResponse.AppValidFor.Value.ToString(CultureInfo.InvariantCulture) : string.Empty}
				};

				mailer.Send(dataGatherer.IsAlibaba ? "Mandrill - Alibaba - Approval" : "Mandrill - Approval (not 1st time)", customerMailVariables, new Addressee(dataGatherer.AppEmail));
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
				{"RegistrationDate", dataGatherer.AppRegistrationDate.ToString(CultureInfo.InvariantCulture)},
				{"userID", customerId.ToString(CultureInfo.InvariantCulture)},
				{"Name", dataGatherer.AppEmail},
				{"FirstName", dataGatherer.AppFirstName},
				{"Surname", dataGatherer.AppSurname},
				{"MP_Counter", dataGatherer.AllMPsNum.ToString(CultureInfo.InvariantCulture)},
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
				{"FirstName", dataGatherer.AppFirstName},
				{"LoanAmount", autoDecisionResponse.AutoApproveAmount.ToString(CultureInfo.InvariantCulture)},
				{"ValidFor", autoDecisionResponse.AppValidFor.HasValue ? autoDecisionResponse.AppValidFor.Value.ToString(CultureInfo.InvariantCulture) : string.Empty}
			};

			mailer.Send("Mandrill - Approval (" + (isFirstLoan ? "" : "not ") + "1st time)", customerMailVariables, new Addressee(dataGatherer.AppEmail));
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
				{"RegistrationDate", dataGatherer.AppRegistrationDate.ToString(CultureInfo.InvariantCulture)},
				{"userID", customerId.ToString(CultureInfo.InvariantCulture)},
				{"Name", dataGatherer.AppEmail},
				{"FirstName", dataGatherer.AppFirstName},
				{"Surname", dataGatherer.AppSurname},
				{"MP_Counter", dataGatherer.AllMPsNum.ToString(CultureInfo.InvariantCulture)},
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
				{"FirstName", dataGatherer.AppFirstName},
				{"LoanAmount", autoDecisionResponse.BankBasedAutoApproveAmount.ToString(CultureInfo.InvariantCulture)},
				{"ValidFor", autoDecisionResponse.AppValidFor.HasValue ? autoDecisionResponse.AppValidFor.Value.ToString(CultureInfo.InvariantCulture) : string.Empty}
			};

			mailer.Send("Mandrill - Approval (" + (isFirstLoan ? "" : "not ") + "1st time)", customerMailVariables, new Addressee(dataGatherer.AppEmail));
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
			var lastOfferResults = DB.GetFirst(
				"GetLastOfferForAutomatedDecision",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("Now", DateTime.UtcNow)
			);

			if (!lastOfferResults.IsEmpty)
			{
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

			var scoreCardResults = DB.GetFirst(
				"GetScoreCardData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("Today", DateTime.Today)
			);

			string maritalStatusStr = scoreCardResults["MaritalStatus"];
			MaritalStatus maritalStatus;

			if (!Enum.TryParse(maritalStatusStr, true, out maritalStatus))
			{
				Log.Warn("Cant parse marital status:{0}. Will use 'Other'", maritalStatusStr);
				maritalStatus = MaritalStatus.Other;
			}

			int modelMaxFeedback = scoreCardResults["MaxFeedback", dataGatherer.DefaultFeedbackValue];

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
				dataGatherer.AppGender == "M" ? Gender.M : Gender.F,
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

		private void PerformExperianConsumerCheckForDirectors()
		{
			if (dataGatherer.CompanyType == "Entrepreneur")
				return;

			DB.ForEachRowSafe((sr, bRowsetStart) => {
				int appDirId = sr["DirId"];
				string appDirName = sr["DirName"];
				string appDirSurname = sr["DirSurname"];

				if (string.IsNullOrEmpty(appDirName) || string.IsNullOrEmpty(appDirSurname))
					return ActionResult.Continue;

				PerformConsumerExperianCheck(appDirId);
				
				return ActionResult.Continue;
			},
				"GetCustomerDirectorsForConsumerCheck",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);
		}

		private void PerformConsumerExperianCheck(int? directorId = null)
		{
			if (wasMainStrategyExecutedBefore)
			{
				Log.Info("Performing experian consumer check");

				var strat = new ExperianConsumerCheck(customerId, directorId, false, DB, Log);
				strat.Execute();

				if (strat.Result != null && strat.Result.Cais != null)
				{
					foreach (var caisDetails in strat.Result.Cais)
					{
						consumerCaisDetailWorstStatuses.Add(caisDetails.WorstStatus);
					}
				}

				if (directorId == null)
				{
					experianConsumerScore = strat.Score;
				}
				else
				{
					if (experianConsumerScore > 0 && strat.Score < minExperianScore)
						minExperianScore = strat.Score;

					if (experianConsumerScore > 0 && strat.Score > maxExperianScore)
						maxExperianScore = strat.Score;
				}

				return;
			}

			if (directorId == null)
			{
				var strat = new ExperianConsumerCheck(customerId, null, false, DB, Log);
				strat.Execute();

				if (strat.Result != null && strat.Result.Cais != null)
				{
					foreach (var caisDetails in strat.Result.Cais)
					{
						consumerCaisDetailWorstStatuses.Add(caisDetails.WorstStatus);
					}
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

		private void PerformCompanyExperianCheck()
		{
			if (wasMainStrategyExecutedBefore)
			{
				Log.Info("Performing experian company check");
				var experianCompanyChecker = new ExperianCompanyCheck(customerId, false, DB, Log);
				experianCompanyChecker.Execute();

				maxCompanyScore = Math.Max((int)experianCompanyChecker.MaxScore, (int)experianCompanyChecker.Score);
			}
			else
			{
				GetMaxCompanyExperianScore();
			}
		}

		private void SetAutoDecisionAvailability()
		{
			Log.Info("Setting auto decision availability");

			if (!dataGatherer.CustomerStatusIsEnabled || dataGatherer.CustomerStatusIsWarning)
			{
				enableAutomaticReApproval = false;
				enableAutomaticApproval = false;
			}

			if (dataGatherer.IsOffline)
			{
				enableAutomaticApproval = false;
			}

			if (isViaBroker)
			{
				enableAutomaticApproval = false;
				enableAutomaticRejection = false;
			}

			if (dataGatherer.IsAlibaba)
			{
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
						dataGatherer.IsOffline,
						companySeniorityDays
					);
			}
			
			mailer.Send(templateName, new Dictionary<string, string> {
				{"RegistrationDate", dataGatherer.AppRegistrationDate.ToString(CultureInfo.InvariantCulture)},
				{"userID", customerId.ToString(CultureInfo.InvariantCulture)},
				{"Name", dataGatherer.AppEmail},
				{"FirstName", dataGatherer.AppFirstName},
				{"Surname", dataGatherer.AppSurname},
				{"MP_Counter", dataGatherer.AllMPsNum.ToString(CultureInfo.InvariantCulture)},
				{"MedalType", medalType.ToString()},
				{"SystemDecision", "Reject"},
				{"ExperianConsumerScore", initialExperianConsumerScore.ToString(CultureInfo.InvariantCulture)},
				{"CVExperianConsumerScore", dataGatherer.LowCreditScore.ToString(CultureInfo.InvariantCulture)},
				{"TotalAnnualTurnover", totalSumOfOrders1YTotalForRejection.ToString("C2")},
				{"CVTotalAnnualTurnover", dataGatherer.LowTotalAnnualTurnover.ToString("C2")},
				{"Total3MTurnover", totalSumOfOrders3MTotalForRejection.ToString("C2")},
				{"CVTotal3MTurnover", dataGatherer.LowTotalThreeMonthTurnover.ToString("C2")},
				{"PayPalStoresNum", rejection.PayPalNumberOfStores.ToString(CultureInfo.InvariantCulture)},
				{"PayPalAnnualTurnover", rejection.PayPalTotalSumOfOrders1Y.ToString("C2")},
				{"CVPayPalAnnualTurnover", dataGatherer.LowTotalAnnualTurnover.ToString("C2")},
				{"PayPal3MTurnover", rejection.PayPalTotalSumOfOrders3M.ToString("C2")},
				{"CVPayPal3MTurnover", dataGatherer.LowTotalThreeMonthTurnover.ToString("C2")},
				{"CVExperianConsumerScoreDefAcc", dataGatherer.RejectDefaultsCreditScore.ToString(CultureInfo.InvariantCulture)},
				{"ExperianDefAccNum", rejection.NumOfDefaultAccounts.ToString(CultureInfo.InvariantCulture)},
				{"CVExperianDefAccNum", dataGatherer.RejectDefaultsAccountsNum.ToString(CultureInfo.InvariantCulture)},
				{"Seniority", marketplaceSeniorityDays.ToString(CultureInfo.InvariantCulture)},
				{"SeniorityThreshold", dataGatherer.RejectMinimalSeniority.ToString(CultureInfo.InvariantCulture) + additionalValues}
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
		}

		private bool ShouldRunBwa()
		{
			return dataGatherer.AppBankAccountType == "Personal" && dataGatherer.BwaBusinessCheck == "1" && dataGatherer.AppSortCode != null && dataGatherer.AppAccountNumber != null;
		}

		private void GetZooplaData()
		{
			Log.Info("Getting zoopla data for customer:{0}", customerId);
			strategyHelper.GetZooplaData(customerId);
		}
	}
}

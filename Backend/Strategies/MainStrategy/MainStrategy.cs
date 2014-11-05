namespace EzBob.Backend.Strategies.MainStrategy
{
	using System.Linq;
	using AutoDecisions;
	using ConfigManager;
	using EzBob.Models;
	using Ezbob.Backend.Models;
	using MailStrategies.API;
	using MedalCalculations;
	using Misc;
	using ScoreCalculation;
	using System;
	using System.Collections.Generic;
	using System.Globalization;
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

		// Inputs
		private readonly int customerId;
		private readonly NewCreditLineOption newCreditLineOption;
		private readonly int avoidAutomaticDecision;

		/// <summary>
		/// Default: true. However when Main strategy is executed as a part of
		/// Finish Wizard strategy and customer is already approved/rejected
		/// then customer's status should not change.
		/// </summary>
		private bool overrideApprovedRejected;

		// Calculated based on raw data
		private bool isHomeOwner;
		private bool isFirstLoan;
		private bool wasMainStrategyExecutedBefore;
		private bool isViaBroker;
		private int companySeniorityDays;
		private decimal loanOfferReApprovalRemainingAmount;
		private decimal loanOfferReApprovalRemainingAmountOld;

		private AutoDecisionResponse autoDecisionResponse;
		private MedalMultiplier medalType;
		private decimal loanOfferReApprovalSum;
		private int offeredCreditLine;
		private int modelLoanOffer;

		private List<string> consumerCaisDetailWorstStatuses;
		
		public override string Name { get { return "Main strategy"; } }
		
		public MainStrategy(
			int customerId,
			NewCreditLineOption newCreditLine,
			int avoidAutoDecision,
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
			overrideApprovedRejected = true;
			staller = new Staller(customerId, newCreditLineOption, mailer, DB, Log);
			dataGatherer = new DataGatherer(customerId, DB, Log);
		}

		public override void Execute()
		{
			if (newCreditLineOption == NewCreditLineOption.SkipEverything)
			{
				Log.Alert("MainStrategy was activated in SkipEverything mode. Nothing is done. Avoid such calls!");
				return;
			}

			// Wait for data to be filled by other strategies
			staller.Stall();

			// Gather preliminary data that is required by AdditionalStrategiesCaller
			dataGatherer.GatherPreliminaryData();
			wasMainStrategyExecutedBefore = dataGatherer.LastStartedMainStrategyEndTime.HasValue;

			// Trigger other strategies
			var additionalStrategiesCaller = new AdditionalStrategiesCaller(customerId, wasMainStrategyExecutedBefore,
				dataGatherer.TypeOfBusiness, dataGatherer.BwaBusinessCheck, dataGatherer.AppBankAccountType,
			    dataGatherer.AppAccountNumber, dataGatherer.AppSortCode, DB, Log);
			consumerCaisDetailWorstStatuses = additionalStrategiesCaller.Call();

			// Gather Raw Data - most data is gathered here
			dataGatherer.Gather();

			// Processing logic
			isHomeOwner = dataGatherer.IsOwnerOfMainAddress || dataGatherer.IsOwnerOfOtherProperties;
			isFirstLoan = dataGatherer.NumOfLoans == 0;
			isViaBroker = dataGatherer.BrokerId.HasValue;
			companySeniorityDays = dataGatherer.CompanyIncorporationDate.HasValue ? (DateTime.UtcNow - dataGatherer.CompanyIncorporationDate.Value).Days : 0;
			loanOfferReApprovalRemainingAmount = dataGatherer.LoanOfferReApprovalRemainingAmount;
			loanOfferReApprovalRemainingAmountOld = dataGatherer.LoanOfferReApprovalRemainingAmountOld;

			// Calculate old medal
			ScoreMedalOffer scoringResult = CalculateScoreAndMedal();
			modelLoanOffer = scoringResult.MaxOffer;

			// Make rejection decisions
			AutoDecisionRejectionResponse autoDecisionRejectionResponse = ProcessRejections();

			// Gather LR data - must be done after rejection decisions
			GetLandRegistryDataIfNotRejected(autoDecisionRejectionResponse);

			// Calculate new medal
			CalculateNewMedal();

			// Cap offer
			CapOffer();

			// Make approve decisions
			ProcessApprovals(autoDecisionRejectionResponse);

			// Log decision
			LogDecision(autoDecisionRejectionResponse);

			// process the decision - DB + mails
			ProcessDecision(scoringResult, autoDecisionRejectionResponse);

			SetEndTimestamp();
		}

		private void CalculateNewMedal()
		{
			bool medalCalculated = false;
			if (Utils.IsLimitedCompany(dataGatherer.TypeOfBusiness))
			{
				if (dataGatherer.NumOfEbayAmazonPayPalMps > 0)
				{
					modelLoanOffer = CalculateOnlineLimitedMedal();
					medalCalculated = true;
				}
				else if (dataGatherer.NumOfHmrcMps < 2)
				{
					modelLoanOffer = CalculateLimitedMedal();
					medalCalculated = true;
				}
			}
			else if (dataGatherer.MaxCompanyScore > 0 && dataGatherer.NumOfHmrcMps < 2)
			{
				modelLoanOffer = CalculateNonLimitedMedal();
				medalCalculated = true;
			}

			if (!medalCalculated)
			{
				if (dataGatherer.MaxExperianConsumerScore > 0 && (dataGatherer.NumOfHmrcMps > 0 || dataGatherer.NumOfYodleeMps > 0) && dataGatherer.NumOfHmrcMps < 2)
				{
					modelLoanOffer = CalculateSoleTraderMedal();
				}
				else
				{
					Log.Warn("No new medal was calculated for customer:{0}", customerId);
				}
			}
		}

		private void ProcessDecision(ScoreMedalOffer scoringResult, AutoDecisionRejectionResponse autoDecisionRejectionResponse)
		{
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

			if (autoDecisionRejectionResponse.DecidedToReject)
			{
				modelLoanOffer = 0;
				SendRejectionExplanationMail("Mandrill - User is rejected by the strategy", autoDecisionRejectionResponse.RejectionModel);

				new RejectUser(customerId, true, DB, Log).Execute();

				strategyHelper.AddRejectIntoDecisionHistory(customerId, autoDecisionRejectionResponse.AutoRejectReason);
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

						strategyHelper.AddApproveIntoDecisionHistory(customerId, "Auto Re-Approval");
					}
				}
				else
				{
					SendWaitingForDecisionMail();
				}
			}
		}

		private void LogDecision(AutoDecisionRejectionResponse autoDecisionRejectionResponse)
		{
			string decisionName = autoDecisionRejectionResponse.DecidedToReject ? autoDecisionRejectionResponse.DecisionName : autoDecisionResponse.DecisionName;

			int decisionId = DB.ExecuteScalar<int>(
				"AutoDecisionRecord",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("DecisionName", decisionName),
				new QueryParameter("Date", DateTime.UtcNow)
			);

			foreach (AutoDecisionCondition condition in autoDecisionRejectionResponse.RejectionConditions)
			{
				DB.ExecuteNonQuery(
					"AutoDecisionConditionRecord",
					CommandSpecies.StoredProcedure,
					new QueryParameter("DecisionId", decisionId),
					new QueryParameter("DecisionName", condition.DecisionName),
					new QueryParameter("Satisfied", condition.Satisfied),
					new QueryParameter("Description", condition.Description)
				);
			}
		}

		private void ProcessApprovals(AutoDecisionRejectionResponse autoDecisionRejectionResponse)
		{
			if (!autoDecisionRejectionResponse.DecidedToReject &&
				newCreditLineOption != NewCreditLineOption.UpdateEverythingAndGoToManualDecision &&
				avoidAutomaticDecision != 1 &&
				dataGatherer.CustomerStatusIsEnabled && 
				!dataGatherer.CustomerStatusIsWarning)
			{
				autoDecisionResponse = new AutoDecisionResponse { DecisionName = "Manual" };

				if (dataGatherer.EnableAutomaticReApproval)
				{
					new ReApproval(customerId, dataGatherer.LoanOfferReApprovalFullAmount, loanOfferReApprovalRemainingAmount,
					               dataGatherer.LoanOfferReApprovalFullAmountOld,
					               loanOfferReApprovalRemainingAmountOld, DB, Log).MakeDecision(autoDecisionResponse);
				}

				if (string.IsNullOrEmpty(autoDecisionResponse.SystemDecision) &&
					dataGatherer.EnableAutomaticApproval &&
					!dataGatherer.IsOffline &&
					!isViaBroker
					)
				{
					new Approval(customerId, dataGatherer.MinExperianConsumerScore, dataGatherer.MinCompanyScore, offeredCreditLine,
					             consumerCaisDetailWorstStatuses, DB, Log).MakeDecision(autoDecisionResponse);
				}

				if (string.IsNullOrEmpty(autoDecisionResponse.SystemDecision) && CurrentValues.Instance.BankBasedApprovalIsEnabled)
				{
					new BankBasedApproval(customerId, DB, Log).MakeDecision(autoDecisionResponse);
				}

				if (string.IsNullOrEmpty(autoDecisionResponse.SystemDecision)) // No decision is made so far
				{
					autoDecisionResponse.CreditResult = "WaitingForDecision";
					autoDecisionResponse.UserStatus = "Manual";
					autoDecisionResponse.SystemDecision = "Manual";
				}
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

		private int CalculateLimitedMedal()
		{
			var instance = new CalculateLimitedMedal(DB, Log, customerId);
			instance.Execute();

			return CalculateOfferAccordingToMedal(instance.Result);
		}

		private int CalculateSoleTraderMedal()
		{
			var instance = new CalculateSoleTraderMedal(DB, Log, customerId);
			instance.Execute();

			return CalculateOfferAccordingToMedal(instance.Result);
		}

		private int CalculateNonLimitedMedal()
		{
			var instance = new CalculateNonLimitedMedal(DB, Log, customerId);
			instance.Execute();

			return CalculateOfferAccordingToMedal(instance.Result);
		}

		private int CalculateOnlineLimitedMedal()
		{
			var instance = new CalculateOnlineLimitedMedal(DB, Log, customerId);
			instance.Execute();

			return CalculateOfferAccordingToMedal(instance.Result);
		}

		private int CalculateOfferAccordingToMedal(ScoreResult scoreResult)
		{
			if (scoreResult != null && string.IsNullOrEmpty(scoreResult.Error))
			{
				SafeReader sr = DB.GetFirst(
					"GetMedalCoefficients",
					CommandSpecies.StoredProcedure,
					new QueryParameter("MedalFlow", "Limited"),
					new QueryParameter("Medal", scoreResult.Medal.ToString())
					);

				if (!sr.IsEmpty)
				{
					decimal annualTurnoverMedalFactor = sr["AnnualTurnover"];
					decimal offerAccordingToAnnualTurnover = scoreResult.AnnualTurnover * annualTurnoverMedalFactor;

					if (scoreResult.InnerFlowName == "HMRC")
					{
						decimal freeCashFlowMedalFactor = sr["FreeCashFlow"];
						decimal valueAddedMedalFactor = sr["ValueAdded"];
						decimal offerAccordingToFreeCashFlow = scoreResult.FreeCashFlowValue * freeCashFlowMedalFactor;
						decimal offerAccordingToValueAdded = scoreResult.ValueAdded * valueAddedMedalFactor;

						// Get min that is over threshold
						if ((offerAccordingToFreeCashFlow <= offerAccordingToValueAdded ||
						     offerAccordingToValueAdded <= dataGatherer.LimitedMedalMinOffer) &&
						    (offerAccordingToFreeCashFlow <= offerAccordingToAnnualTurnover ||
						     offerAccordingToAnnualTurnover <= dataGatherer.LimitedMedalMinOffer) &&
						    offerAccordingToFreeCashFlow >= dataGatherer.LimitedMedalMinOffer)
						{
							Log.Info("Calculated offer for customer: {0} according to free cash flow ({1})", customerId,
							         offerAccordingToFreeCashFlow);
							return (int) offerAccordingToFreeCashFlow;
						}

						if ((offerAccordingToValueAdded <= offerAccordingToFreeCashFlow ||
						     offerAccordingToFreeCashFlow <= dataGatherer.LimitedMedalMinOffer) &&
						    (offerAccordingToValueAdded <= offerAccordingToAnnualTurnover ||
						     offerAccordingToAnnualTurnover <= dataGatherer.LimitedMedalMinOffer) &&
						    offerAccordingToValueAdded >= dataGatherer.LimitedMedalMinOffer)
						{
							Log.Info("Calculated offer for customer: {0} according to value added ({1})", customerId,
							         offerAccordingToValueAdded);
							return (int) offerAccordingToValueAdded;
						}

						if ((offerAccordingToAnnualTurnover <= offerAccordingToFreeCashFlow ||
						     offerAccordingToFreeCashFlow <= dataGatherer.LimitedMedalMinOffer) &&
						    (offerAccordingToAnnualTurnover <= offerAccordingToValueAdded ||
						     offerAccordingToValueAdded <= dataGatherer.LimitedMedalMinOffer) &&
						    offerAccordingToAnnualTurnover >= dataGatherer.LimitedMedalMinOffer)
						{
							Log.Info("Calculated offer for customer: {0} according to annual turnover ({1})", customerId,
							         offerAccordingToAnnualTurnover);
							return (int) offerAccordingToAnnualTurnover;
						}
					}
					else if (offerAccordingToAnnualTurnover >= dataGatherer.LimitedMedalMinOffer)
					{
						Log.Info("Calculated offer for customer: {0} according to annual turnover ({1})", customerId,
						         offerAccordingToAnnualTurnover);
						return (int) offerAccordingToAnnualTurnover;
					}
				}
			}

			return 0;
		}

		private AutoDecisionRejectionResponse ProcessRejections()
		{
			var autoDecisionRejectionResponse = new AutoDecisionRejectionResponse();

			if (newCreditLineOption != NewCreditLineOption.UpdateEverythingAndGoToManualDecision &&
			    avoidAutomaticDecision != 1)
			{
				if (dataGatherer.EnableAutomaticReRejection)
				{
					new ReRejection(customerId, DB, Log).MakeDecision(autoDecisionRejectionResponse);
				}

				if (!autoDecisionRejectionResponse.IsReRejected &&
				    dataGatherer.EnableAutomaticRejection &&
				    !isViaBroker &&
				    !dataGatherer.IsAlibaba)
				{
					var rejection = new Rejection(customerId, dataGatherer.TotalSumOfOrders1YTotalForRejection,
					                              dataGatherer.TotalSumOfOrders3MTotalForRejection,
					                              dataGatherer.Yodlee1YForRejection,
					                              dataGatherer.Yodlee3MForRejection, dataGatherer.MarketplaceSeniorityDays,
					                              dataGatherer.MaxExperianConsumerScore, dataGatherer.MaxCompanyScore,
					                              dataGatherer.CustomerStatusIsEnabled,
					                              dataGatherer.CustomerStatusIsWarning, isViaBroker,
					                              Utils.IsLimitedCompany(dataGatherer.TypeOfBusiness), companySeniorityDays,
					                              dataGatherer.IsOffline, dataGatherer.CustomerStatusName, DB, Log).MakeDecision(
						                              autoDecisionRejectionResponse);
					Log.Debug(rejection.ToString());
				}
			}
			return autoDecisionRejectionResponse;
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

		private void CapOffer()
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
				dataGatherer.LoanOfferReApprovalFullAmount,
				loanOfferReApprovalRemainingAmount,
				dataGatherer.LoanOfferReApprovalFullAmountOld,
				loanOfferReApprovalRemainingAmountOld
			}.Max();

			offeredCreditLine = modelLoanOffer;

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
				new QueryParameter("ExpirianRating", dataGatherer.ExperianConsumerScore),
				new QueryParameter("AnualTurnover", dataGatherer.TotalSumOfOrders1YTotal),
				new QueryParameter("InterestRate", interestAccordingToPast == -1 ? loanInterestBase : interestAccordingToPast),
				new QueryParameter("ManualSetupFeeAmount", dataGatherer.ManualSetupFeeAmount),
				new QueryParameter("ManualSetupFeePercent", dataGatherer.ManualSetupFeePercent),
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
				{"RepaymentPeriod", dataGatherer.LoanOfferRepaymentPeriod.ToString(CultureInfo.InvariantCulture)},
				{"InterestRate", dataGatherer.LoanOfferInterestRate.ToString(CultureInfo.InvariantCulture)},
				{
					"OfferValidUntil", autoDecisionResponse.AppValidFor.HasValue
						? autoDecisionResponse.AppValidFor.Value.ToString(CultureInfo.InvariantCulture)
						: string.Empty
				}
			});

			var customerMailVariables = new Dictionary<string, string> {
				{"FirstName", dataGatherer.AppFirstName},
				{"LoanAmount", loanOfferReApprovalSum.ToString(CultureInfo.InvariantCulture)},
				{"ValidFor", autoDecisionResponse.AppValidFor.HasValue ? autoDecisionResponse.AppValidFor.Value.ToString(CultureInfo.InvariantCulture) : string.Empty}
			};

			mailer.Send(dataGatherer.IsAlibaba ? "Mandrill - Alibaba - Approval" : "Mandrill - Approval (not 1st time)", customerMailVariables, new Addressee(dataGatherer.AppEmail));
		}

		private void UpdateReApprovalData()
		{
			DB.ExecuteNonQuery(
				"UpdateCashRequestsReApproval",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("UnderwriterDecision", autoDecisionResponse.UserStatus),
				new QueryParameter("ManagerApprovedSum", loanOfferReApprovalSum),
				new QueryParameter("APR", dataGatherer.LoanOfferApr),
				new QueryParameter("RepaymentPeriod", dataGatherer.LoanOfferRepaymentPeriod),
				new QueryParameter("InterestRate", dataGatherer.LoanOfferInterestRate),
				new QueryParameter("UseSetupFee", dataGatherer.LoanOfferUseSetupFee),
				new QueryParameter("EmailSendingBanned", autoDecisionResponse.LoanOfferEmailSendingBannedNew),
				new QueryParameter("LoanTypeId", dataGatherer.LoanOfferLoanTypeId),
				new QueryParameter("UnderwriterComment", autoDecisionResponse.LoanOfferUnderwriterComment),
				new QueryParameter("IsLoanTypeSelectionAllowed", dataGatherer.LoanOfferIsLoanTypeSelectionAllowed),
				new QueryParameter("DiscountPlanId", dataGatherer.LoanOfferDiscountPlanId),
				new QueryParameter("ExperianRating", dataGatherer.ExperianConsumerScore),
				new QueryParameter("LoanSourceId", dataGatherer.LoanSourceId),
				new QueryParameter("IsCustomerRepaymentPeriodSelectionAllowed", dataGatherer.IsCustomerRepaymentPeriodSelectionAllowed),
				new QueryParameter("UseBrokerSetupFee", dataGatherer.UseBrokerSetupFee),
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
				{"RepaymentPeriod", dataGatherer.LoanOfferRepaymentPeriod.ToString(CultureInfo.InvariantCulture)},
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
				{"InterestRate", dataGatherer.LoanOfferInterestRate.ToString(CultureInfo.InvariantCulture)},
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

		private ScoreMedalOffer CalculateScoreAndMedal()
		{
			Log.Info("Calculating score & medal");

			ScoreMedalOffer scoringResult = medalScoreCalculator.CalculateMedalScore(
				dataGatherer.TotalSumOfOrdersForLoanOffer,
				dataGatherer.MinExperianConsumerScore,
				dataGatherer.MarketplaceSeniorityYears,
				dataGatherer.ModelMaxFeedback,
				dataGatherer.MaritalStatus,
				dataGatherer.AppGender == "M" ? Gender.M : Gender.F,
				dataGatherer.NumOfEbayAmazonPayPalMps,
				dataGatherer.FirstRepaymentDatePassed,
				dataGatherer.EzbobSeniorityMonths,
				dataGatherer.ModelOnTimeLoans,
				dataGatherer.ModelLatePayments,
				dataGatherer.ModelEarlyPayments
			);

			medalType = scoringResult.Medal;

			// Save online medal
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

			// Update CustomerAnalyticsLocalData
			DB.ExecuteNonQuery(
				"CustomerAnalyticsUpdateLocalData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", customerId),
				new QueryParameter("AnalyticsDate", DateTime.UtcNow),
				new QueryParameter("AnnualTurnover", dataGatherer.TotalSumOfOrders1YTotal),
				new QueryParameter("TotalSumOfOrdersForLoanOffer", dataGatherer.TotalSumOfOrdersForLoanOffer),
				new QueryParameter("MarketplaceSeniorityYears", dataGatherer.MarketplaceSeniorityYears),
				new QueryParameter("MaxFeedback", dataGatherer.ModelMaxFeedback),
				new QueryParameter("MPsNumber", dataGatherer.NumOfEbayAmazonPayPalMps),
				new QueryParameter("FirstRepaymentDatePassed", dataGatherer.FirstRepaymentDatePassed),
				new QueryParameter("EzbobSeniorityMonths", dataGatherer.EzbobSeniorityMonths),
				new QueryParameter("OnTimeLoans", dataGatherer.ModelOnTimeLoans),
				new QueryParameter("LatePayments", dataGatherer.ModelLatePayments),
				new QueryParameter("EarlyPayments", dataGatherer.ModelEarlyPayments)
			);
			return scoringResult;
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
				{"ExperianConsumerScore", dataGatherer.ExperianConsumerScore.ToString(CultureInfo.InvariantCulture)},
				{"CVExperianConsumerScore", dataGatherer.LowCreditScore.ToString(CultureInfo.InvariantCulture)},
				{"TotalAnnualTurnover", dataGatherer.TotalSumOfOrders1YTotalForRejection.ToString("C2")},
				{"CVTotalAnnualTurnover", dataGatherer.LowTotalAnnualTurnover.ToString("C2")},
				{"Total3MTurnover", dataGatherer.TotalSumOfOrders3MTotalForRejection.ToString("C2")},
				{"CVTotal3MTurnover", dataGatherer.LowTotalThreeMonthTurnover.ToString("C2")},
				{"PayPalStoresNum", rejection.PayPalNumberOfStores.ToString(CultureInfo.InvariantCulture)},
				{"PayPalAnnualTurnover", rejection.PayPalTotalSumOfOrders1Y.ToString("C2")},
				{"CVPayPalAnnualTurnover", dataGatherer.LowTotalAnnualTurnover.ToString("C2")},
				{"PayPal3MTurnover", rejection.PayPalTotalSumOfOrders3M.ToString("C2")},
				{"CVPayPal3MTurnover", dataGatherer.LowTotalThreeMonthTurnover.ToString("C2")},
				{"CVExperianConsumerScoreDefAcc", dataGatherer.RejectDefaultsCreditScore.ToString(CultureInfo.InvariantCulture)},
				{"ExperianDefAccNum", rejection.NumOfDefaultAccounts.ToString(CultureInfo.InvariantCulture)},
				{"CVExperianDefAccNum", dataGatherer.RejectDefaultsAccountsNum.ToString(CultureInfo.InvariantCulture)},
				{"Seniority", dataGatherer.MarketplaceSeniorityDays.ToString(CultureInfo.InvariantCulture)},
				{"SeniorityThreshold", dataGatherer.RejectMinimalSeniority.ToString(CultureInfo.InvariantCulture) + additionalValues}
			});
		}
	}
}

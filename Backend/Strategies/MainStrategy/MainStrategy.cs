namespace EzBob.Backend.Strategies.MainStrategy {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using EZBob.DatabaseLib.Model.Loans;
	using MailStrategies;
	using MailStrategies.API;
	using AutoDecisions;
	using MedalCalculations;
	using Misc;
	using NHibernate;
	using ScoreCalculation;
	using Ezbob.Backend.Models;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EzBob.Models;
	using StructureMap;

	public class MainStrategy : AStrategy {
		public override string Name { get { return "Main strategy"; } }

		public MainStrategy(
			int customerId,
			NewCreditLineOption newCreditLine,
			int avoidAutoDecision,
			AConnection oDb,
			ASafeLog oLog
		) : base(oDb, oLog) {

			_session = ObjectFactory.GetInstance<ISession>();
			_customers = ObjectFactory.GetInstance<CustomerRepository>();
			_decisionHistory = ObjectFactory.GetInstance<DecisionHistoryRepository>();
			_loanSourceRepository = ObjectFactory.GetInstance<LoanSourceRepository>();
			_loanTypeRepository = ObjectFactory.GetInstance<LoanTypeRepository>();
			_discountPlanRepository = ObjectFactory.GetInstance<DiscountPlanRepository>();

			medalScoreCalculator = new MedalScoreCalculator(DB, Log);

			mailer = new StrategiesMailer(DB, Log);
			this.customerId = customerId;
			newCreditLineOption = newCreditLine;
			avoidAutomaticDecision = avoidAutoDecision;
			overrideApprovedRejected = true;
			staller = new Staller(customerId, newCreditLineOption, mailer, DB, Log);
			dataGatherer = new DataGatherer(customerId, DB, Log);
		}

		public virtual MainStrategy SetOverrideApprovedRejected(bool bOverrideApprovedRejected) {
			overrideApprovedRejected = bOverrideApprovedRejected;
			return this;
		}

		public override void Execute() {
			autoDecisionResponse = new AutoDecisionResponse { DecisionName = "Manual" };

			if (newCreditLineOption == NewCreditLineOption.SkipEverything) {
				Log.Alert("MainStrategy was activated in SkipEverything mode. Nothing is done. Avoid such calls!");
				return;
			} // if

			// Wait for data to be filled by other strategies
			if (newCreditLineOption != NewCreditLineOption.SkipEverythingAndApplyAutoRules)
				staller.Stall();

			// Gather preliminary data that is required by AdditionalStrategiesCaller
			dataGatherer.GatherPreliminaryData();
			wasMainStrategyExecutedBefore = dataGatherer.LastStartedMainStrategyEndTime.HasValue;

			// Trigger other strategies
			if (newCreditLineOption != NewCreditLineOption.SkipEverythingAndApplyAutoRules) {
				new AdditionalStrategiesCaller(
					customerId,
					wasMainStrategyExecutedBefore,
					dataGatherer.TypeOfBusiness,
					dataGatherer.BwaBusinessCheck,
					dataGatherer.AppBankAccountType,
					dataGatherer.AppAccountNumber,
					dataGatherer.AppSortCode,
					DB,
					Log
				).Call();
			} // if

			// Gather Raw Data - most data is gathered here
			dataGatherer.Gather();

			// Processing logic
			isHomeOwner = dataGatherer.IsOwnerOfMainAddress || dataGatherer.IsOwnerOfOtherProperties;
			isFirstLoan = dataGatherer.NumOfLoans == 0;

			// Calculate old medal
			ScoreMedalOffer scoringResult = CalculateScoreAndMedal();
			modelLoanOffer = scoringResult.MaxOffer;

			// Make rejection decisions
			ProcessRejections();

			// Gather LR data - must be done after rejection decisions
			bool bSkip =
				newCreditLineOption == NewCreditLineOption.SkipEverything ||
				newCreditLineOption == NewCreditLineOption.SkipEverythingAndApplyAutoRules;

			if (!bSkip)
				GetLandRegistryDataIfNotRejected();

			// Calculate new medal
			CalculateNewMedal();

			// Cap offer
			CapOffer();

			// Make approve decisions
			ProcessApprovals();

			AdjustOfferredCreditLine();

			UpdateCustomerAndCashRequest();

			SendEmails();
		} // Execute

		private void CalculateNewMedal() {
			var instance = new CalculateMedal(DB, Log, customerId, dataGatherer.TypeOfBusiness, dataGatherer.MaxExperianConsumerScore, dataGatherer.MaxCompanyScore, dataGatherer.NumOfHmrcMps,
				dataGatherer.NumOfYodleeMps, dataGatherer.NumOfEbayAmazonPayPalMps, dataGatherer.EarliestHmrcLastUpdateDate, dataGatherer.EarliestYodleeLastUpdateDate);
			instance.Execute();

			medalClassification = instance.Result.MedalClassification;
			medalScore = instance.Result.TotalScoreNormalized;

			modelLoanOffer = RoundOfferedAmount(instance.Result.OfferedLoanAmount);
		}

		private int RoundOfferedAmount(decimal amount) {
			return (int)Math.Truncate(amount / CurrentValues.Instance.GetCashSliderStep) * CurrentValues.Instance.GetCashSliderStep;
		}

		private void AdjustOfferredCreditLine() {
			if (autoDecisionResponse.IsAutoReApproval || autoDecisionResponse.IsAutoApproval) {
				offeredCreditLine = RoundOfferedAmount(autoDecisionResponse.AutoApproveAmount);
			}
			else if (autoDecisionResponse.IsAutoBankBasedApproval) {
				offeredCreditLine = RoundOfferedAmount(autoDecisionResponse.BankBasedAutoApproveAmount);
			}
			else if (autoDecisionResponse.DecidedToReject) {
				offeredCreditLine = 0;
			}
		}

		private void SendEmails() {
			if (autoDecisionResponse.DecidedToReject) {
				new RejectUser(customerId, true, DB, Log).Execute();
			}
			else if (autoDecisionResponse.IsAutoApproval) {
				SendApprovalMails();
			}
			else if (autoDecisionResponse.IsAutoBankBasedApproval) {
				SendBankBasedApprovalMails();
			}
			else if (autoDecisionResponse.IsAutoReApproval) {
				SendReApprovalMails();
			}
			else if (!autoDecisionResponse.HasAutomaticDecision)
				SendWaitingForDecisionMail();
		}

		private void ProcessApprovals() {
			if (autoDecisionResponse.DecidedToReject) {
				Log.Debug("Not processing approvals: reject decision has been made.");
				return;
			} // if

			if (newCreditLineOption == NewCreditLineOption.UpdateEverythingAndGoToManualDecision) {
				Log.Debug("Not processing approvals: {0} option selected.", newCreditLineOption);
				return;
			} // if

			if (avoidAutomaticDecision == 1) {
				Log.Debug("Not processing approvals: automatic decisions should be avoided.");
				return;
			} // if

			if (!dataGatherer.CustomerStatusIsEnabled) {
				Log.Debug("Not processing approvals: customer status is not enabled.");
				return;
			} // if

			if (dataGatherer.CustomerStatusIsWarning) {
				Log.Debug("Not processing approvals: customer status is 'warning'.");
				return;
			} // if

			bool bContinue = true;

			// ReSharper disable ConditionIsAlwaysTrueOrFalse
			if (dataGatherer.EnableAutomaticReApproval && bContinue) {
				// ReSharper restore ConditionIsAlwaysTrueOrFalse
				new AutoDecisions.ReApproval.Agent(
					customerId,
					DB,
					Log
				).Init().MakeDecision(autoDecisionResponse);

				bContinue = !autoDecisionResponse.SystemDecision.HasValue;

				if (!bContinue)
					Log.Debug("Auto re-approval has reached decision: {0}.", autoDecisionResponse.SystemDecision);
			}
			else
				Log.Debug("Not processed auto re-approval: it is currently disabled in configuration.");

			if (dataGatherer.EnableAutomaticApproval && bContinue) {
				new Approval(
					customerId,
					offeredCreditLine,
					medalClassification,
					DB,
					Log
				).Init().MakeDecision(autoDecisionResponse);

				bContinue = !autoDecisionResponse.SystemDecision.HasValue;

				if (!bContinue)
					Log.Debug("Auto approval has reached decision: {0}.", autoDecisionResponse.SystemDecision);
			} // if
			else
				Log.Debug("Not processed auto approval: it is currently disabled in configuration or decision has already been made earlier.");

			if (CurrentValues.Instance.BankBasedApprovalIsEnabled && bContinue) {
				new BankBasedApproval(customerId, DB, Log).MakeDecision(autoDecisionResponse);

				bContinue = !autoDecisionResponse.SystemDecision.HasValue;

				if (!bContinue)
					Log.Debug("Bank based approval has reached decision: {0}.", autoDecisionResponse.SystemDecision);
			}
			else
				Log.Debug("Not processed bank based approval: it is currently disabled in configuration or decision has already been made earlier.");

			if (bContinue) { // No decision is made so far
				autoDecisionResponse.CreditResult = CreditResultStatus.WaitingForDecision;
				autoDecisionResponse.UserStatus = Status.Manual;
				autoDecisionResponse.SystemDecision = SystemDecision.Manual;

				Log.Debug("Not approval has reached decision: setting it to be 'waiting for decision'.");
			} // if
		} // ProcessApprovals

		private void GetLandRegistryDataIfNotRejected() {
			if (!autoDecisionResponse.DecidedToReject && isHomeOwner) {
				Log.Debug("Retrieving LandRegistry system decision: {0} residential status: {1}", autoDecisionResponse.DecisionName, dataGatherer.PropertyStatusDescription);
				GetLandRegistry();
			}
			else {
				Log.Info("Not retrieving LandRegistry system decision: {0} residential status: {1}", autoDecisionResponse.DecisionName, dataGatherer.PropertyStatusDescription);
			}
		}

		private void ProcessRejections() {
			if (newCreditLineOption == NewCreditLineOption.UpdateEverythingAndGoToManualDecision)
				return;

			if (avoidAutomaticDecision == 1)
				return;

			if (dataGatherer.EnableAutomaticReRejection)
				new ReRejection(customerId, DB, Log).MakeDecision(autoDecisionResponse);

			if (autoDecisionResponse.IsReRejected)
				return;

			if (!dataGatherer.EnableAutomaticRejection)
				return;

			if (dataGatherer.IsAlibaba)
				return;

			new AutoDecisions.Reject.Agent(
				customerId, DB, Log
			).Init().MakeDecision(autoDecisionResponse);
		} // ProcessRejections

		private void GetLandRegistry() {
			var customerAddressesHelper = new CustomerAddressHelper(customerId, DB, Log);
			customerAddressesHelper.Execute();
			try {
				strategyHelper.GetLandRegistryData(customerId, customerAddressesHelper.OwnedAddresses);
			}
			catch (Exception e) {
				Log.Error("Error while getting land registry data: {0}", e);
			}
		}

		private void CapOffer() {
			Log.Info("Finalizing and capping offer");

			offeredCreditLine = modelLoanOffer;

			bool isHomeOwnerAccordingToLandRegistry = DB.ExecuteScalar<bool>(
				"GetIsCustomerHomeOwnerAccordingToLandRegistry",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);

			if (isHomeOwnerAccordingToLandRegistry) {
				Log.Info("Capped for home owner according to land registry");
				offeredCreditLine = Math.Min(offeredCreditLine, dataGatherer.MaxCapHomeOwner);
			}
			else {
				Log.Info("Capped for not home owner");
				offeredCreditLine = Math.Min(offeredCreditLine, dataGatherer.MaxCapNotHomeOwner);
			} // if
		} // CappOffer
		
		private void UpdateCustomerAndCashRequest() {
			var now = DateTime.UtcNow;

			decimal interestRateToUse;
			decimal setupFeePercentToUse, setupFeeAmountToUse;
			int repaymentPeriodToUse;
			LoanType loanTypeIdToUse;
			bool isEuToUse = false;

			if (autoDecisionResponse.IsAutoApproval) {
				interestRateToUse = autoDecisionResponse.InterestRate;
				setupFeePercentToUse = autoDecisionResponse.SetupFee;
				setupFeeAmountToUse = setupFeePercentToUse * offeredCreditLine;
				repaymentPeriodToUse = autoDecisionResponse.RepaymentPeriod;
				isEuToUse = autoDecisionResponse.IsEu;
				loanTypeIdToUse = _loanTypeRepository.Get(autoDecisionResponse.LoanTypeId) ?? _loanTypeRepository.GetDefault();
			}
			else {
				interestRateToUse = dataGatherer.LoanOfferInterestRate;
				setupFeePercentToUse = dataGatherer.ManualSetupFeePercent;
				setupFeeAmountToUse = dataGatherer.ManualSetupFeeAmount;
				repaymentPeriodToUse = autoDecisionResponse.RepaymentPeriod;
				loanTypeIdToUse = _loanTypeRepository.GetDefault();
			}

			var customer = _customers.Get(customerId);

			if (customer == null)
				return;

			if (overrideApprovedRejected) {
				customer.CreditResult = autoDecisionResponse.CreditResult;
				customer.Status = autoDecisionResponse.UserStatus;
			}

			customer.OfferStart = now;
			customer.OfferValidUntil = now.AddHours(CurrentValues.Instance.OfferValidForHours);
			customer.SystemDecision = autoDecisionResponse.SystemDecision;
			customer.Medal = medalClassification;
			customer.CreditSum = offeredCreditLine;
			customer.LastStatus = autoDecisionResponse.CreditResult.HasValue ? autoDecisionResponse.CreditResult.ToString() : null ;
			customer.SystemCalculatedSum = modelLoanOffer;

			if (autoDecisionResponse.DecidedToReject) {
				customer.DateRejected = now;
				customer.RejectedReason = autoDecisionResponse.DecisionName;
				customer.NumRejects++;
			}

			if (autoDecisionResponse.DecidedToApprove) {
				customer.DateApproved = now;
				customer.ApprovedReason = autoDecisionResponse.DecisionName;
				customer.NumApproves++;
				customer.IsLoanTypeSelectionAllowed = 1;
			}

			var cr = customer.LastCashRequest;

			if (cr != null) {
				cr.OfferStart = customer.OfferStart;
				cr.OfferValidUntil = customer.OfferValidUntil;

				cr.SystemDecision = autoDecisionResponse.SystemDecision;
				cr.SystemCalculatedSum = modelLoanOffer;
				cr.SystemDecisionDate = now;
				cr.ManagerApprovedSum = offeredCreditLine;
				cr.UnderwriterDecision = autoDecisionResponse.CreditResult;
				cr.UnderwriterDecisionDate = now;
				cr.UnderwriterComment = autoDecisionResponse.DecisionName;
				cr.AutoDecisionID = autoDecisionResponse.DecisionCode;
				cr.MedalType = medalClassification;
				cr.ScorePoints = (double)medalScore;
				cr.ExpirianRating = dataGatherer.ExperianConsumerScore;
				cr.AnnualTurnover = (int)dataGatherer.TotalSumOfOrders1YTotal;
				cr.LoanType = loanTypeIdToUse;
				cr.LoanSource = isEuToUse ? _loanSourceRepository.GetByName("EU") : _loanSourceRepository.GetDefault();

				if (autoDecisionResponse.DecidedToApprove)
					cr.InterestRate = interestRateToUse;

				if (repaymentPeriodToUse != 0) {
					cr.ApprovedRepaymentPeriod = repaymentPeriodToUse;
					cr.RepaymentPeriod = repaymentPeriodToUse;
				}

				cr.ManualSetupFeeAmount = (int)setupFeeAmountToUse;
				cr.ManualSetupFeePercent = setupFeePercentToUse;
				cr.UseSetupFee = setupFeeAmountToUse > 0 || setupFeePercentToUse > 0;
				cr.APR = dataGatherer.LoanOfferApr;

				if (autoDecisionResponse.IsAutoReApproval) {
					cr.UseSetupFee = dataGatherer.LoanOfferUseSetupFee != 0;
					cr.EmailSendingBanned = autoDecisionResponse.LoanOfferEmailSendingBannedNew;
					cr.IsCustomerRepaymentPeriodSelectionAllowed = dataGatherer.IsCustomerRepaymentPeriodSelectionAllowed != 0;
					cr.DiscountPlan = _discountPlanRepository.Get(dataGatherer.LoanOfferDiscountPlanId);
					cr.UseBrokerSetupFee = dataGatherer.UseBrokerSetupFee;
				}
			}

			_customers.SaveOrUpdate(customer);

			if (autoDecisionResponse.Decision.HasValue) {
				_decisionHistory.LogAction(
					autoDecisionResponse.Decision.Value, autoDecisionResponse.DecisionName, _session.Get<User>(1), customer
				);
			}
		}

		private void SendWaitingForDecisionMail() {
			mailer.Send("Mandrill - User is waiting for decision", new Dictionary<string, string> {
				{"RegistrationDate", dataGatherer.AppRegistrationDate.ToString(CultureInfo.InvariantCulture)},
				{"userID", customerId.ToString(CultureInfo.InvariantCulture)},
				{"Name", dataGatherer.AppEmail},
				{"FirstName", dataGatherer.AppFirstName},
				{"Surname", dataGatherer.AppSurname},
				{"MP_Counter", dataGatherer.AllMPsNum.ToString(CultureInfo.InvariantCulture)},
				{"MedalType", medalClassification.ToString()},
				{"SystemDecision", "WaitingForDecision"}
			});
		}

		private void SendReApprovalMails() {
			mailer.Send("Mandrill - User is re-approved", new Dictionary<string, string> {
				{"ApprovedReApproved", "Re-Approved"},
				{"RegistrationDate", dataGatherer.AppRegistrationDate.ToString(CultureInfo.InvariantCulture)},
				{"userID", customerId.ToString(CultureInfo.InvariantCulture)},
				{"Name", dataGatherer.AppEmail},
				{"FirstName", dataGatherer.AppFirstName},
				{"Surname", dataGatherer.AppSurname},
				{"MP_Counter", dataGatherer.AllMPsNum.ToString(CultureInfo.InvariantCulture)},
				{"MedalType", medalClassification.ToString()},
				{"SystemDecision", autoDecisionResponse.SystemDecision.ToString()},
				{"ApprovalAmount", offeredCreditLine.ToString(CultureInfo.InvariantCulture)},
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
				{"LoanAmount", autoDecisionResponse.AutoApproveAmount.ToString(CultureInfo.InvariantCulture)},
				{"ValidFor", autoDecisionResponse.AppValidFor.HasValue ? autoDecisionResponse.AppValidFor.Value.ToString(CultureInfo.InvariantCulture) : string.Empty}
			};

			mailer.Send(dataGatherer.IsAlibaba ? "Mandrill - Alibaba - Approval" : "Mandrill - Approval (not 1st time)", customerMailVariables, new Addressee(dataGatherer.AppEmail));
		}
		
		private void SendApprovalMails() {
			mailer.Send("Mandrill - User is approved", new Dictionary<string, string> {
				{"ApprovedReApproved", "Approved"},
				{"RegistrationDate", dataGatherer.AppRegistrationDate.ToString(CultureInfo.InvariantCulture)},
				{"userID", customerId.ToString(CultureInfo.InvariantCulture)},
				{"Name", dataGatherer.AppEmail},
				{"FirstName", dataGatherer.AppFirstName},
				{"Surname", dataGatherer.AppSurname},
				{"MP_Counter", dataGatherer.AllMPsNum.ToString(CultureInfo.InvariantCulture)},
				{"MedalType", medalClassification.ToString()},
				{"SystemDecision", autoDecisionResponse.SystemDecision.ToString()},
				{"ApprovalAmount", autoDecisionResponse.AutoApproveAmount.ToString(CultureInfo.InvariantCulture)},
				{"RepaymentPeriod", dataGatherer.LoanOfferRepaymentPeriod.ToString(CultureInfo.InvariantCulture)},
				{"InterestRate", autoDecisionResponse.InterestRate.ToString(CultureInfo.InvariantCulture)},
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

		private void SendBankBasedApprovalMails() {
			mailer.Send("Mandrill - User is approved", new Dictionary<string, string> {
				{"ApprovedReApproved", "Approved"},
				{"RegistrationDate", dataGatherer.AppRegistrationDate.ToString(CultureInfo.InvariantCulture)},
				{"userID", customerId.ToString(CultureInfo.InvariantCulture)},
				{"Name", dataGatherer.AppEmail},
				{"FirstName", dataGatherer.AppFirstName},
				{"Surname", dataGatherer.AppSurname},
				{"MP_Counter", dataGatherer.AllMPsNum.ToString(CultureInfo.InvariantCulture)},
				{"MedalType", medalClassification.ToString()},
				{"SystemDecision", autoDecisionResponse.SystemDecision.ToString()},
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

		private ScoreMedalOffer CalculateScoreAndMedal() {
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

		private AutoDecisionResponse autoDecisionResponse;
		private int offeredCreditLine;
		private int modelLoanOffer;
		private Medal medalClassification;
		private decimal medalScore;

		private readonly CustomerRepository _customers;
		private readonly DecisionHistoryRepository _decisionHistory;
		private readonly ISession _session;
		private readonly LoanTypeRepository _loanTypeRepository;
		private readonly LoanSourceRepository _loanSourceRepository;
		private readonly DiscountPlanRepository _discountPlanRepository;
	} // class MainStrategy
} // namespace

namespace Ezbob.Backend.Strategies.MainStrategy {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using ConfigManager;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.MailStrategies;
	using Ezbob.Backend.Strategies.MailStrategies.API;
	using Ezbob.Backend.Strategies.MainStrategy.AutoDecisions;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Backend.Strategies.ScoreCalculation;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using EZBob.DatabaseLib.Model.Loans;
	using NHibernate;
	using StructureMap;

	public class MainStrategy : AStrategy {
		public override string Name {
			get {
				return "Main strategy";
			}
		}

		public MainStrategy(
			int customerId,
			NewCreditLineOption newCreditLine,
			int avoidAutoDecision,
			FinishWizardArgs fwa
			) {
			this.finishWizardArgs = fwa;

			this._session = ObjectFactory.GetInstance<ISession>();
			this._customers = ObjectFactory.GetInstance<CustomerRepository>();
			this._decisionHistory = ObjectFactory.GetInstance<DecisionHistoryRepository>();
			this._loanSourceRepository = ObjectFactory.GetInstance<LoanSourceRepository>();
			this._loanTypeRepository = ObjectFactory.GetInstance<LoanTypeRepository>();
			this._discountPlanRepository = ObjectFactory.GetInstance<DiscountPlanRepository>();

			this.medalScoreCalculator = new MedalScoreCalculator(DB, Log);

			this.mailer = new StrategiesMailer();
			this.customerId = customerId;
			this.newCreditLineOption = newCreditLine;
			this.avoidAutomaticDecision = avoidAutoDecision;
			this.overrideApprovedRejected = true;
			this.staller = new Staller(customerId, this.newCreditLineOption, this.mailer, DB, Log);
			this.dataGatherer = new DataGatherer(customerId, DB, Log);
		}

		public override void Execute() {
			if (this.finishWizardArgs != null)
				FinishWizard();

			this.autoDecisionResponse = new AutoDecisionResponse {
				DecisionName = "Manual"
			};

			if (this.newCreditLineOption == NewCreditLineOption.SkipEverything) {
				Log.Alert("MainStrategy was activated in SkipEverything mode. Nothing is done. Avoid such calls!");
				return;
			} // if

			// Wait for data to be filled by other strategies
			if (this.newCreditLineOption != NewCreditLineOption.SkipEverythingAndApplyAutoRules)
				this.staller.Stall();

			// Gather preliminary data that is required by AdditionalStrategiesCaller
			this.dataGatherer.GatherPreliminaryData();
			this.wasMainStrategyExecutedBefore = this.dataGatherer.LastStartedMainStrategyEndTime.HasValue;

			// Trigger other strategies
			if (this.newCreditLineOption != NewCreditLineOption.SkipEverythingAndApplyAutoRules) {
				new AdditionalStrategiesCaller(this.customerId, this.wasMainStrategyExecutedBefore, this.dataGatherer.TypeOfBusiness, this.dataGatherer.BwaBusinessCheck, this.dataGatherer.AppBankAccountType, this.dataGatherer.AppAccountNumber, this.dataGatherer.AppSortCode,
					DB,
					Log
					).Call();
			} // if

			// Gather Raw Data - most data is gathered here
			this.dataGatherer.Gather();

			//check for fraud
			if (!this.dataGatherer.IsTest) {
				var fraudChecker = new FraudChecker(this.customerId, FraudMode.FullCheck);
				fraudChecker.Execute();
			}

			// Processing logic
			this.isHomeOwner = this.dataGatherer.IsOwnerOfMainAddress || this.dataGatherer.IsOwnerOfOtherProperties;
			this.isFirstLoan = this.dataGatherer.NumOfLoans == 0;

			// Calculate old medal
			ScoreMedalOffer scoringResult = CalculateScoreAndMedal();
			this.modelLoanOffer = scoringResult.MaxOffer;

			// Make rejection decisions
			ProcessRejections();

			// Gather LR data - must be done after rejection decisions
			bool bSkip = this.newCreditLineOption == NewCreditLineOption.SkipEverything || this.newCreditLineOption == NewCreditLineOption.SkipEverythingAndApplyAutoRules;

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
		}

		public virtual MainStrategy SetOverrideApprovedRejected(bool bOverrideApprovedRejected) {
			this.overrideApprovedRejected = bOverrideApprovedRejected;
			return this;
		}

		// Execute

		private void AdjustOfferredCreditLine() {
			if (this.autoDecisionResponse.IsAutoReApproval || this.autoDecisionResponse.IsAutoApproval)
				this.offeredCreditLine = RoundOfferedAmount(this.autoDecisionResponse.AutoApproveAmount);
			else if (this.autoDecisionResponse.IsAutoBankBasedApproval)
				this.offeredCreditLine = RoundOfferedAmount(this.autoDecisionResponse.BankBasedAutoApproveAmount);
			else if (this.autoDecisionResponse.DecidedToReject)
				this.offeredCreditLine = 0;
		}

		private void CalculateNewMedal() {
			var instance = new CalculateMedal(this.customerId, this.dataGatherer.TypeOfBusiness, this.dataGatherer.MaxExperianConsumerScore, this.dataGatherer.MaxCompanyScore, this.dataGatherer.NumOfHmrcMps, this.dataGatherer.NumOfYodleeMps, this.dataGatherer.NumOfEbayAmazonPayPalMps, this.dataGatherer.EarliestHmrcLastUpdateDate, this.dataGatherer.EarliestYodleeLastUpdateDate);
			instance.Execute();

			this.medalClassification = instance.Result.MedalClassification;
			this.medalScore = instance.Result.TotalScoreNormalized;
			this.medalType = instance.Result.MedalType;
			this.turnoverType = instance.Result.TurnoverType;

			this.modelLoanOffer = RoundOfferedAmount(instance.Result.OfferedLoanAmount);
		}

		private ScoreMedalOffer CalculateScoreAndMedal() {
			Log.Info("Calculating score & medal");

			ScoreMedalOffer scoringResult = this.medalScoreCalculator.CalculateMedalScore(this.dataGatherer.TotalSumOfOrdersForLoanOffer, this.dataGatherer.MinExperianConsumerScore, this.dataGatherer.MarketplaceSeniorityYears, this.dataGatherer.ModelMaxFeedback, this.dataGatherer.MaritalStatus, this.dataGatherer.AppGender == "M" ? Gender.M : Gender.F, this.dataGatherer.NumOfEbayAmazonPayPalMps, this.dataGatherer.FirstRepaymentDatePassed, this.dataGatherer.EzbobSeniorityMonths, this.dataGatherer.ModelOnTimeLoans, this.dataGatherer.ModelLatePayments, this.dataGatherer.ModelEarlyPayments
				);

			// Save online medal
			DB.ExecuteNonQuery(
				"CustomerScoringResult_Insert",
				CommandSpecies.StoredProcedure,
				new QueryParameter("pCustomerId", this.customerId),
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
				new QueryParameter("CustomerID", this.customerId),
				new QueryParameter("AnalyticsDate", DateTime.UtcNow),
				new QueryParameter("AnnualTurnover", this.dataGatherer.TotalSumOfOrders1YTotal),
				new QueryParameter("TotalSumOfOrdersForLoanOffer", this.dataGatherer.TotalSumOfOrdersForLoanOffer),
				new QueryParameter("MarketplaceSeniorityYears", this.dataGatherer.MarketplaceSeniorityYears),
				new QueryParameter("MaxFeedback", this.dataGatherer.ModelMaxFeedback),
				new QueryParameter("MPsNumber", this.dataGatherer.NumOfEbayAmazonPayPalMps),
				new QueryParameter("FirstRepaymentDatePassed", this.dataGatherer.FirstRepaymentDatePassed),
				new QueryParameter("EzbobSeniorityMonths", this.dataGatherer.EzbobSeniorityMonths),
				new QueryParameter("OnTimeLoans", this.dataGatherer.ModelOnTimeLoans),
				new QueryParameter("LatePayments", this.dataGatherer.ModelLatePayments),
				new QueryParameter("EarlyPayments", this.dataGatherer.ModelEarlyPayments)
				);
			return scoringResult;
		}

		private void CapOffer() {
			Log.Info("Finalizing and capping offer");

			this.offeredCreditLine = this.modelLoanOffer;

			bool isHomeOwnerAccordingToLandRegistry = DB.ExecuteScalar<bool>(
				"GetIsCustomerHomeOwnerAccordingToLandRegistry",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerId)
				);

			if (isHomeOwnerAccordingToLandRegistry) {
				Log.Info("Capped for home owner according to land registry");
				this.offeredCreditLine = Math.Min(this.offeredCreditLine, this.dataGatherer.MaxCapHomeOwner);
			} else {
				Log.Info("Capped for not home owner");
				this.offeredCreditLine = Math.Min(this.offeredCreditLine, this.dataGatherer.MaxCapNotHomeOwner);
			} // if
		}

		private void FinishWizard() {
			if (this.finishWizardArgs == null)
				return;

			this.finishWizardArgs.DoMain = false;

			new FinishWizard(this.finishWizardArgs).Execute();
		} // FinishWizard

		private void GetLandRegistry() {
			var customerAddressesHelper = new CustomerAddressHelper(this.customerId);
			customerAddressesHelper.Execute();
			try {
				this.strategyHelper.GetLandRegistryData(this.customerId, customerAddressesHelper.OwnedAddresses);
			} catch (Exception e) {
				Log.Error("Error while getting land registry data: {0}", e);
			}
		}

		private void GetLandRegistryDataIfNotRejected() {
			if (!this.autoDecisionResponse.DecidedToReject && this.isHomeOwner) {
				Log.Debug("Retrieving LandRegistry system decision: {0} residential status: {1}", this.autoDecisionResponse.DecisionName, this.dataGatherer.PropertyStatusDescription);
				GetLandRegistry();
			} else
				Log.Info("Not retrieving LandRegistry system decision: {0} residential status: {1}", this.autoDecisionResponse.DecisionName, this.dataGatherer.PropertyStatusDescription);
		}

		private void ProcessApprovals() {
			if (this.autoDecisionResponse.DecidedToReject) {
				Log.Debug("Not processing approvals: reject decision has been made.");
				return;
			} // if

			if (this.newCreditLineOption == NewCreditLineOption.UpdateEverythingAndGoToManualDecision) {
				Log.Debug("Not processing approvals: {0} option selected.", this.newCreditLineOption);
				return;
			} // if

			if (this.avoidAutomaticDecision == 1) {
				Log.Debug("Not processing approvals: automatic decisions should be avoided.");
				return;
			} // if

			if (!this.dataGatherer.CustomerStatusIsEnabled) {
				Log.Debug("Not processing approvals: customer status is not enabled.");
				return;
			} // if

			if (this.dataGatherer.CustomerStatusIsWarning) {
				Log.Debug("Not processing approvals: customer status is 'warning'.");
				return;
			} // if

			bool bContinue = true;

			// ReSharper disable ConditionIsAlwaysTrueOrFalse
			if (this.dataGatherer.EnableAutomaticReApproval && bContinue) {
				// ReSharper restore ConditionIsAlwaysTrueOrFalse
				new AutoDecisions.ReApproval.Agent(this.customerId,
					DB,
					Log
					).Init()
					.MakeDecision(this.autoDecisionResponse);

				bContinue = !this.autoDecisionResponse.SystemDecision.HasValue;

				if (!bContinue)
					Log.Debug("Auto re-approval has reached decision: {0}.", this.autoDecisionResponse.SystemDecision);
			} else
				Log.Debug("Not processed auto re-approval: it is currently disabled in configuration.");

			if (this.dataGatherer.EnableAutomaticApproval && bContinue) {
				new Approval(this.customerId, this.offeredCreditLine, this.medalClassification,
					(AutomationCalculator.Common.MedalType)this.medalType,
					(AutomationCalculator.Common.TurnoverType?)this.turnoverType,
					DB,
					Log
					).Init()
					.MakeDecision(this.autoDecisionResponse);

				bContinue = !this.autoDecisionResponse.SystemDecision.HasValue;

				if (!bContinue)
					Log.Debug("Auto approval has reached decision: {0}.", this.autoDecisionResponse.SystemDecision);
			} // if
			else
				Log.Debug("Not processed auto approval: it is currently disabled in configuration or decision has already been made earlier.");

			if (CurrentValues.Instance.BankBasedApprovalIsEnabled && bContinue) {
				new BankBasedApproval(this.customerId).MakeDecision(this.autoDecisionResponse);

				bContinue = !this.autoDecisionResponse.SystemDecision.HasValue;

				if (!bContinue)
					Log.Debug("Bank based approval has reached decision: {0}.", this.autoDecisionResponse.SystemDecision);
			} else
				Log.Debug("Not processed bank based approval: it is currently disabled in configuration or decision has already been made earlier.");

			if (bContinue) { // No decision is made so far
				this.autoDecisionResponse.CreditResult = CreditResultStatus.WaitingForDecision;
				this.autoDecisionResponse.UserStatus = Status.Manual;
				this.autoDecisionResponse.SystemDecision = SystemDecision.Manual;

				Log.Debug("Not approval has reached decision: setting it to be 'waiting for decision'.");
			} // if
		}

		// ProcessApprovals
		private void ProcessRejections() {
			if (this.newCreditLineOption == NewCreditLineOption.UpdateEverythingAndGoToManualDecision)
				return;

			if (this.avoidAutomaticDecision == 1)
				return;

			if (this.dataGatherer.EnableAutomaticReRejection)
				new ReRejection(this.customerId, DB, Log).MakeDecision(this.autoDecisionResponse);

			if (this.autoDecisionResponse.IsReRejected)
				return;

			if (!this.dataGatherer.EnableAutomaticRejection)
				return;

			if (this.dataGatherer.IsAlibaba)
				return;

			new AutoDecisions.Reject.Agent(this.customerId, DB, Log
				).Init()
				.MakeDecision(this.autoDecisionResponse);
		}

		private int RoundOfferedAmount(decimal amount) {
			return (int)Math.Truncate(amount / CurrentValues.Instance.GetCashSliderStep) * CurrentValues.Instance.GetCashSliderStep;
		}

		private void SendApprovalMails() {
			this.mailer.Send("Mandrill - User is approved", new Dictionary<string, string> {
				{
					"ApprovedReApproved", "Approved"
				}, {
					"RegistrationDate", this.dataGatherer.AppRegistrationDate.ToString(CultureInfo.InvariantCulture)
				}, {
					"userID", this.customerId.ToString(CultureInfo.InvariantCulture)
				}, {
					"Name", this.dataGatherer.AppEmail
				}, {
					"FirstName", this.dataGatherer.AppFirstName
				}, {
					"Surname", this.dataGatherer.AppSurname
				}, {
					"MP_Counter", this.dataGatherer.AllMPsNum.ToString(CultureInfo.InvariantCulture)
				}, {
					"MedalType", this.medalClassification.ToString()
				}, {
					"SystemDecision", this.autoDecisionResponse.SystemDecision.ToString()
				}, {
					"ApprovalAmount", this.autoDecisionResponse.AutoApproveAmount.ToString(CultureInfo.InvariantCulture)
				}, {
					"RepaymentPeriod", this.dataGatherer.LoanOfferRepaymentPeriod.ToString(CultureInfo.InvariantCulture)
				}, {
					"InterestRate", this.autoDecisionResponse.InterestRate.ToString(CultureInfo.InvariantCulture)
				}, {
					"OfferValidUntil", this.autoDecisionResponse.AppValidFor.HasValue
						? this.autoDecisionResponse.AppValidFor.Value.ToString(CultureInfo.InvariantCulture)
						: string.Empty
				}
			});

			var customerMailVariables = new Dictionary<string, string> {
				{
					"FirstName", this.dataGatherer.AppFirstName
				}, {
					"LoanAmount", this.autoDecisionResponse.AutoApproveAmount.ToString(CultureInfo.InvariantCulture)
				}, {
					"ValidFor", this.autoDecisionResponse.AppValidFor.HasValue ? this.autoDecisionResponse.AppValidFor.Value.ToString(CultureInfo.InvariantCulture) : string.Empty
				}
			};

			this.mailer.Send("Mandrill - Approval (" + (this.isFirstLoan ? "" : "not ") + "1st time)", customerMailVariables, new Addressee(this.dataGatherer.AppEmail));
		}

		private void SendBankBasedApprovalMails() {
			this.mailer.Send("Mandrill - User is approved", new Dictionary<string, string> {
				{
					"ApprovedReApproved", "Approved"
				}, {
					"RegistrationDate", this.dataGatherer.AppRegistrationDate.ToString(CultureInfo.InvariantCulture)
				}, {
					"userID", this.customerId.ToString(CultureInfo.InvariantCulture)
				}, {
					"Name", this.dataGatherer.AppEmail
				}, {
					"FirstName", this.dataGatherer.AppFirstName
				}, {
					"Surname", this.dataGatherer.AppSurname
				}, {
					"MP_Counter", this.dataGatherer.AllMPsNum.ToString(CultureInfo.InvariantCulture)
				}, {
					"MedalType", this.medalClassification.ToString()
				}, {
					"SystemDecision", this.autoDecisionResponse.SystemDecision.ToString()
				}, {
					"ApprovalAmount", this.autoDecisionResponse.BankBasedAutoApproveAmount.ToString(CultureInfo.InvariantCulture)
				}, {
					"RepaymentPeriod", this.autoDecisionResponse.RepaymentPeriod.ToString(CultureInfo.InvariantCulture)
				}, {
					"InterestRate", this.dataGatherer.LoanOfferInterestRate.ToString(CultureInfo.InvariantCulture)
				}, {
					"OfferValidUntil", this.autoDecisionResponse.AppValidFor.HasValue
						? this.autoDecisionResponse.AppValidFor.Value.ToString(CultureInfo.InvariantCulture)
						: string.Empty
				}
			});

			var customerMailVariables = new Dictionary<string, string> {
				{
					"FirstName", this.dataGatherer.AppFirstName
				}, {
					"LoanAmount", this.autoDecisionResponse.BankBasedAutoApproveAmount.ToString(CultureInfo.InvariantCulture)
				}, {
					"ValidFor", this.autoDecisionResponse.AppValidFor.HasValue ? this.autoDecisionResponse.AppValidFor.Value.ToString(CultureInfo.InvariantCulture) : string.Empty
				}
			};

			this.mailer.Send("Mandrill - Approval (" + (this.isFirstLoan ? "" : "not ") + "1st time)", customerMailVariables, new Addressee(this.dataGatherer.AppEmail));
		}

		private void SendEmails() {
			if (this.autoDecisionResponse.DecidedToReject)
				new RejectUser(this.customerId, true).Execute();
			else if (this.autoDecisionResponse.IsAutoApproval)
				SendApprovalMails();
			else if (this.autoDecisionResponse.IsAutoBankBasedApproval)
				SendBankBasedApprovalMails();
			else if (this.autoDecisionResponse.IsAutoReApproval)
				SendReApprovalMails();
			else if (!this.autoDecisionResponse.HasAutomaticDecision)
				SendWaitingForDecisionMail();
		}

		// ProcessRejections
		// CappOffer

		private void SendReApprovalMails() {
			this.mailer.Send("Mandrill - User is re-approved", new Dictionary<string, string> {
				{
					"ApprovedReApproved", "Re-Approved"
				}, {
					"RegistrationDate", this.dataGatherer.AppRegistrationDate.ToString(CultureInfo.InvariantCulture)
				}, {
					"userID", this.customerId.ToString(CultureInfo.InvariantCulture)
				}, {
					"Name", this.dataGatherer.AppEmail
				}, {
					"FirstName", this.dataGatherer.AppFirstName
				}, {
					"Surname", this.dataGatherer.AppSurname
				}, {
					"MP_Counter", this.dataGatherer.AllMPsNum.ToString(CultureInfo.InvariantCulture)
				}, {
					"MedalType", this.medalClassification.ToString()
				}, {
					"SystemDecision", this.autoDecisionResponse.SystemDecision.ToString()
				}, {
					"ApprovalAmount", this.offeredCreditLine.ToString(CultureInfo.InvariantCulture)
				}, {
					"RepaymentPeriod", this.dataGatherer.LoanOfferRepaymentPeriod.ToString(CultureInfo.InvariantCulture)
				}, {
					"InterestRate", this.dataGatherer.LoanOfferInterestRate.ToString(CultureInfo.InvariantCulture)
				}, {
					"OfferValidUntil", this.autoDecisionResponse.AppValidFor.HasValue
						? this.autoDecisionResponse.AppValidFor.Value.ToString(CultureInfo.InvariantCulture)
						: string.Empty
				}
			});

			var customerMailVariables = new Dictionary<string, string> {
				{
					"FirstName", this.dataGatherer.AppFirstName
				}, {
					"LoanAmount", this.autoDecisionResponse.AutoApproveAmount.ToString(CultureInfo.InvariantCulture)
				}, {
					"ValidFor", this.autoDecisionResponse.AppValidFor.HasValue ? this.autoDecisionResponse.AppValidFor.Value.ToString(CultureInfo.InvariantCulture) : string.Empty
				}
			};

			this.mailer.Send(this.dataGatherer.IsAlibaba ? "Mandrill - Alibaba - Approval" : "Mandrill - Approval (not 1st time)", customerMailVariables, new Addressee(this.dataGatherer.AppEmail));
		}

		private void SendWaitingForDecisionMail() {
			this.mailer.Send("Mandrill - User is waiting for decision", new Dictionary<string, string> {
				{
					"RegistrationDate", this.dataGatherer.AppRegistrationDate.ToString(CultureInfo.InvariantCulture)
				}, {
					"userID", this.customerId.ToString(CultureInfo.InvariantCulture)
				}, {
					"Name", this.dataGatherer.AppEmail
				}, {
					"FirstName", this.dataGatherer.AppFirstName
				}, {
					"Surname", this.dataGatherer.AppSurname
				}, {
					"MP_Counter", this.dataGatherer.AllMPsNum.ToString(CultureInfo.InvariantCulture)
				}, {
					"MedalType", this.medalClassification.ToString()
				}, {
					"SystemDecision", "WaitingForDecision"
				}
			});
		}

		private void UpdateCustomerAndCashRequest() {
			var now = DateTime.UtcNow;

			decimal interestRateToUse;
			decimal setupFeePercentToUse, setupFeeAmountToUse;
			int repaymentPeriodToUse;
			LoanType loanTypeIdToUse;
			bool isEuToUse = false;

			if (this.autoDecisionResponse.IsAutoApproval) {
				interestRateToUse = this.autoDecisionResponse.InterestRate;
				setupFeePercentToUse = this.autoDecisionResponse.SetupFee;
				setupFeeAmountToUse = setupFeePercentToUse * this.offeredCreditLine;
				repaymentPeriodToUse = this.autoDecisionResponse.RepaymentPeriod;
				isEuToUse = this.autoDecisionResponse.IsEu;
				loanTypeIdToUse = this._loanTypeRepository.Get(this.autoDecisionResponse.LoanTypeId) ?? this._loanTypeRepository.GetDefault();
			} else {
				interestRateToUse = this.dataGatherer.LoanOfferInterestRate;
				setupFeePercentToUse = this.dataGatherer.ManualSetupFeePercent;
				setupFeeAmountToUse = this.dataGatherer.ManualSetupFeeAmount;
				repaymentPeriodToUse = this.autoDecisionResponse.RepaymentPeriod;
				loanTypeIdToUse = this._loanTypeRepository.GetDefault();
			}

			var customer = this._customers.Get(this.customerId);

			if (customer == null)
				return;

			if (this.overrideApprovedRejected) {
				customer.CreditResult = this.autoDecisionResponse.CreditResult;
				customer.Status = this.autoDecisionResponse.UserStatus;
			}

			customer.OfferStart = now;
			customer.OfferValidUntil = now.AddHours(CurrentValues.Instance.OfferValidForHours);
			customer.SystemDecision = this.autoDecisionResponse.SystemDecision;
			customer.Medal = this.medalClassification;
			customer.CreditSum = this.offeredCreditLine;
			customer.LastStatus = autoDecisionResponse.CreditResult.HasValue ? autoDecisionResponse.CreditResult.ToString() : "N/A";
			customer.SystemCalculatedSum = this.modelLoanOffer;

			if (this.autoDecisionResponse.DecidedToReject) {
				customer.DateRejected = now;
				customer.RejectedReason = this.autoDecisionResponse.DecisionName;
				customer.NumRejects++;
			}

			if (this.autoDecisionResponse.DecidedToApprove) {
				customer.DateApproved = now;
				customer.ApprovedReason = this.autoDecisionResponse.DecisionName;
				customer.NumApproves++;
				customer.IsLoanTypeSelectionAllowed = 1;
			}

			var cr = customer.LastCashRequest;

			if (cr != null) {
				cr.OfferStart = customer.OfferStart;
				cr.OfferValidUntil = customer.OfferValidUntil;

				cr.SystemDecision = this.autoDecisionResponse.SystemDecision;
				cr.SystemCalculatedSum = this.modelLoanOffer;
				cr.SystemDecisionDate = now;
				cr.ManagerApprovedSum = this.offeredCreditLine;
				cr.UnderwriterDecision = this.autoDecisionResponse.CreditResult;
				cr.UnderwriterDecisionDate = now;
				cr.UnderwriterComment = this.autoDecisionResponse.DecisionName;
				cr.AutoDecisionID = this.autoDecisionResponse.DecisionCode;
				cr.MedalType = this.medalClassification;
				cr.ScorePoints = (double)this.medalScore;
				cr.ExpirianRating = this.dataGatherer.ExperianConsumerScore;
				cr.AnnualTurnover = (int)this.dataGatherer.TotalSumOfOrders1YTotal;
				cr.LoanType = loanTypeIdToUse;
				cr.LoanSource = isEuToUse ? this._loanSourceRepository.GetByName("EU") : this._loanSourceRepository.GetDefault();

				if (this.autoDecisionResponse.DecidedToApprove)
					cr.InterestRate = interestRateToUse;

				if (repaymentPeriodToUse != 0) {
					cr.ApprovedRepaymentPeriod = repaymentPeriodToUse;
					cr.RepaymentPeriod = repaymentPeriodToUse;
				}

				cr.ManualSetupFeeAmount = (int)setupFeeAmountToUse;
				cr.ManualSetupFeePercent = setupFeePercentToUse;
				cr.UseSetupFee = setupFeeAmountToUse > 0 || setupFeePercentToUse > 0;
				cr.APR = this.dataGatherer.LoanOfferApr;

				if (this.autoDecisionResponse.IsAutoReApproval) {
					cr.UseSetupFee = this.dataGatherer.LoanOfferUseSetupFee != 0;
					cr.EmailSendingBanned = this.autoDecisionResponse.LoanOfferEmailSendingBannedNew;
					cr.IsCustomerRepaymentPeriodSelectionAllowed = this.dataGatherer.IsCustomerRepaymentPeriodSelectionAllowed != 0;
					cr.DiscountPlan = this._discountPlanRepository.Get(this.dataGatherer.LoanOfferDiscountPlanId);
					cr.UseBrokerSetupFee = this.dataGatherer.UseBrokerSetupFee;
				}
			}

			customer.LastStartedMainStrategyEndTime = now;

			this._customers.SaveOrUpdate(customer);

			if (this.autoDecisionResponse.Decision.HasValue) {
				this._decisionHistory.LogAction(this.autoDecisionResponse.Decision.Value, this.autoDecisionResponse.DecisionName, this._session.Get<User>(1), customer
					);
			}
		}

		private readonly CustomerRepository _customers;

		private readonly DecisionHistoryRepository _decisionHistory;

		private readonly DiscountPlanRepository _discountPlanRepository;

		private readonly LoanSourceRepository _loanSourceRepository;

		private readonly LoanTypeRepository _loanTypeRepository;

		private readonly ISession _session;

		private readonly int avoidAutomaticDecision;

		// Inputs
		private readonly int customerId;

		private readonly DataGatherer dataGatherer;

		private readonly FinishWizardArgs finishWizardArgs;

		// Helpers
		private readonly StrategiesMailer mailer;
		private readonly MedalScoreCalculator medalScoreCalculator;
		private readonly NewCreditLineOption newCreditLineOption;
		private readonly Staller staller;
		private readonly StrategyHelper strategyHelper = new StrategyHelper();
		private AutoDecisionResponse autoDecisionResponse;

		private bool isFirstLoan;

		// Calculated based on raw data
		private bool isHomeOwner;

		private Medal medalClassification;

		private TurnoverType? turnoverType;

		private decimal medalScore;

		private Ezbob.Backend.Strategies.MedalCalculations.MedalType medalType;

		private int modelLoanOffer;

		private int offeredCreditLine;

		/// <summary>
		///     Default: true. However when Main strategy is executed as a part of
		///     Finish Wizard strategy and customer is already approved/rejected
		///     then customer's status should not change.
		/// </summary>
		private bool overrideApprovedRejected;

		private bool wasMainStrategyExecutedBefore;
	} // class MainStrategy
} // namespace

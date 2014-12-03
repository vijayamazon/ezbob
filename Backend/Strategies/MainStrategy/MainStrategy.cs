namespace EzBob.Backend.Strategies.MainStrategy
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using ConfigManager;
	using MailStrategies;
	using MailStrategies.API;
	using AutoDecisions;
	using MedalCalculations;
	using Misc;
	using ScoreCalculation;
	using Ezbob.Backend.Models;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EzBob.Models;

	public class MainStrategy : AStrategy
	{
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

		public virtual MainStrategy SetOverrideApprovedRejected(bool bOverrideApprovedRejected)
		{
			overrideApprovedRejected = bOverrideApprovedRejected;
			return this;
		}

		public override void Execute()
		{
			autoDecisionResponse = new AutoDecisionResponse { DecisionName = "Manual" };

			if (newCreditLineOption == NewCreditLineOption.SkipEverything)
			{
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
			if (newCreditLineOption != NewCreditLineOption.SkipEverythingAndApplyAutoRules)
			{
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
			companySeniorityDays = dataGatherer.CompanyIncorporationDate.HasValue ? (DateTime.UtcNow - dataGatherer.CompanyIncorporationDate.Value).Days : 0;

			// Calculate old medal
			ScoreMedalOffer scoringResult = CalculateScoreAndMedal();
			modelLoanOffer = scoringResult.MaxOffer;

			// Make rejection decisions
			ProcessRejections();

			// Gather LR data - must be done after rejection decisions
			if (newCreditLineOption != NewCreditLineOption.SkipEverythingAndApplyAutoRules)
				GetLandRegistryDataIfNotRejected();

			// Calculate new medal
			CalculateNewMedal();

			// Cap offer
			CapOffer();

			// Make approve decisions
			ProcessApprovals();

			// process the decision - DB + mails
			ProcessDecision();

			// TODO: retire this
			DB.ExecuteNonQuery("Update_Main_Strat_Finish_Date",
				CommandSpecies.StoredProcedure,
				new QueryParameter("UserId", customerId),
				new QueryParameter("Now", DateTime.UtcNow)
			);
		}

		private void CalculateNewMedal()
		{
			var instance = new CalculateMedal(DB, Log, customerId, dataGatherer.TypeOfBusiness, dataGatherer.MaxExperianConsumerScore, dataGatherer.MaxCompanyScore, dataGatherer.NumOfHmrcMps,
				dataGatherer.NumOfYodleeMps, dataGatherer.NumOfEbayAmazonPayPalMps, dataGatherer.EarliestHmrcLastUpdateDate, dataGatherer.EarliestYodleeLastUpdateDate);
			instance.Execute();

			medalClassification = instance.Result.MedalClassification;
			medalScore = instance.Result.TotalScoreNormalized;

			modelLoanOffer = (int)Math.Truncate((decimal)instance.Result.OfferedLoanAmount / CurrentValues.Instance.GetCashSliderStep) * CurrentValues.Instance.GetCashSliderStep;
		}

		private void ProcessDecision()
		{
			if (autoDecisionResponse.IsAutoApproval || autoDecisionResponse.IsAutoReApproval)
			{
				modelLoanOffer = autoDecisionResponse.AutoApproveAmount;

				if (modelLoanOffer < offeredCreditLine)
				{
					offeredCreditLine = modelLoanOffer;
				}
			}
			
			if (autoDecisionResponse.IsAutoBankBasedApproval)
			{
				modelLoanOffer = autoDecisionResponse.BankBasedAutoApproveAmount;

				if (modelLoanOffer < offeredCreditLine)
				{
					offeredCreditLine = modelLoanOffer;
				}
			}

			if (autoDecisionResponse.DecidedToReject)
			{
				modelLoanOffer = 0;
				SendRejectionExplanationMail("Mandrill - User is rejected by the strategy", autoDecisionResponse.RejectionModel);

				new RejectUser(customerId, true, DB, Log).Execute();

				strategyHelper.AddRejectIntoDecisionHistory(customerId, autoDecisionResponse.AutoRejectReason);
			}

			UpdateCustomerAndCashRequest();

			
			if (autoDecisionResponse.IsAutoApproval)
			{
				UpdateApprovalData();
				SendApprovalMails();

				strategyHelper.AddApproveIntoDecisionHistory(customerId, "Auto Approval");
			}
				
			if (autoDecisionResponse.IsAutoBankBasedApproval)
			{
				UpdateBankBasedApprovalData();
				SendBankBasedApprovalMails();

				strategyHelper.AddApproveIntoDecisionHistory(customerId, "Auto bank based approval");
			}
				
			if (autoDecisionResponse.IsAutoReApproval)
			{
				UpdateReApprovalData();
				SendReApprovalMails();

				strategyHelper.AddApproveIntoDecisionHistory(customerId, "Auto Re-Approval");
			}
			
			//if no auto decision was made send waiting for decision mail.
			if( !autoDecisionResponse.IsAutoApproval && 
				!autoDecisionResponse.IsAutoBankBasedApproval && 
				!autoDecisionResponse.IsAutoReApproval && 
				!autoDecisionResponse.DecidedToReject)
			{
				SendWaitingForDecisionMail();
			}
		}

		private void ProcessApprovals()
		{
			if (autoDecisionResponse.DecidedToReject)
			{
				Log.Debug("Not processing approvals: reject decision has been made.");
				return;
			} // if

			if (newCreditLineOption == NewCreditLineOption.UpdateEverythingAndGoToManualDecision)
			{
				Log.Debug("Not processing approvals: {0} option selected.", newCreditLineOption);
				return;
			} // if

			if (avoidAutomaticDecision == 1)
			{
				Log.Debug("Not processing approvals: automatic decisions should be avoided.");
				return;
			} // if

			if (!dataGatherer.CustomerStatusIsEnabled)
			{
				Log.Debug("Not processing approvals: customer status is not enabled.");
				return;
			} // if

			if (dataGatherer.CustomerStatusIsWarning)
			{
				Log.Debug("Not processing approvals: customer status is 'warning'.");
				return;
			} // if

			bool bContinue = true;

			// ReSharper disable ConditionIsAlwaysTrueOrFalse
			if (dataGatherer.EnableAutomaticReApproval && bContinue)
			{
				// ReSharper restore ConditionIsAlwaysTrueOrFalse
				new AutoDecisions.ReApproval.Agent(
					customerId,
					DB,
					Log
				).Init().MakeDecision(autoDecisionResponse);

				bContinue = string.IsNullOrWhiteSpace(autoDecisionResponse.SystemDecision);

				if (!bContinue)
					Log.Debug("Auto re-approval has reached decision: {0}.", autoDecisionResponse.SystemDecision);
			}
			else
				Log.Debug("Not processed auto re-approval: it is currently disabled in configuration.");

			if (dataGatherer.EnableAutomaticApproval && bContinue)
			{
				new Approval(
					customerId,
					offeredCreditLine,
					medalClassification,
					DB,
					Log
				).Init().MakeDecision(autoDecisionResponse);

				bContinue = string.IsNullOrEmpty(autoDecisionResponse.SystemDecision);

				if (!bContinue)
					Log.Debug("Auto approval has reached decision: {0}.", autoDecisionResponse.SystemDecision);
			} // if
			else
				Log.Debug("Not processed auto approval: it is currently disabled in configuration or decision has already been made earlier.");

			if (CurrentValues.Instance.BankBasedApprovalIsEnabled && bContinue)
			{
				new BankBasedApproval(customerId, DB, Log).MakeDecision(autoDecisionResponse);

				bContinue = string.IsNullOrEmpty(autoDecisionResponse.SystemDecision);

				if (!bContinue)
					Log.Debug("Bank based approval has reached decision: {0}.", autoDecisionResponse.SystemDecision);
			}
			else
				Log.Debug("Not processed bank based approval: it is currently disabled in configuration or decision has already been made earlier.");

			if (bContinue)
			{ // No decision is made so far
				autoDecisionResponse.CreditResult = "WaitingForDecision";
				autoDecisionResponse.UserStatus = "Manual";
				autoDecisionResponse.SystemDecision = "Manual";

				Log.Debug("Not approval has reached decision: setting it to be 'waiting for decision'.");
			} // if
		} // ProcessApprovals

		private void GetLandRegistryDataIfNotRejected()
		{
			if (!autoDecisionResponse.DecidedToReject && isHomeOwner)
			{
				Log.Debug("Retrieving LandRegistry system decision: {0} residential status: {1}", autoDecisionResponse.DecisionName, dataGatherer.PropertyStatusDescription);
				GetLandRegistry();
			}
			else
			{
				Log.Info("Not retrieving LandRegistry system decision: {0} residential status: {1}", autoDecisionResponse.DecisionName, dataGatherer.PropertyStatusDescription);
			}
		}

		private void ProcessRejections()
		{
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

		private void CapOffer()
		{
			Log.Info("Finalizing and capping offer");

			offeredCreditLine = modelLoanOffer;

			bool isHomeOwnerAccordingToLandRegistry = DB.ExecuteScalar<bool>(
				"GetIsCustomerHomeOwnerAccordingToLandRegistry",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);

			if (isHomeOwnerAccordingToLandRegistry)
			{
				Log.Info("Capped for home owner according to land registry");
				loanOfferReApprovalSum = Math.Min(loanOfferReApprovalSum, dataGatherer.MaxCapHomeOwner);
				offeredCreditLine = Math.Min(offeredCreditLine, dataGatherer.MaxCapHomeOwner);
			}
			else
			{
				Log.Info("Capped for not home owner");
				loanOfferReApprovalSum = Math.Min(loanOfferReApprovalSum, dataGatherer.MaxCapNotHomeOwner);
				offeredCreditLine = Math.Min(offeredCreditLine, dataGatherer.MaxCapNotHomeOwner);
			} // if
		} // CappOffer

		private void UpdateCustomerAndCashRequest()
		{
			DB.ExecuteNonQuery(
				"UpdateScoringResultsNew",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("CreditResult", autoDecisionResponse.CreditResult),
				new QueryParameter("SystemDecision", autoDecisionResponse.SystemDecision),
				new QueryParameter("Status", autoDecisionResponse.UserStatus),
				new QueryParameter("Medal", medalClassification.ToString()),
				new QueryParameter("ValidFor", autoDecisionResponse.AppValidFor),
				new QueryParameter("Now", DateTime.UtcNow),
				new QueryParameter("OverrideApprovedRejected", overrideApprovedRejected)
			);

			decimal interestRateToUse;
			decimal setupFeePercentToUse, setupFeeAmountToUse;
			int repaymentPeriodToUse, loanTypeIdToUse;
			bool isEuToUse;

			if (autoDecisionResponse.IsAutoApproval)
			{
				interestRateToUse = autoDecisionResponse.InterestRate;
				setupFeePercentToUse = autoDecisionResponse.SetupFee;
				setupFeeAmountToUse = setupFeePercentToUse * offeredCreditLine;
				repaymentPeriodToUse = autoDecisionResponse.RepaymentPeriod;
				isEuToUse = autoDecisionResponse.IsEu;
				loanTypeIdToUse = autoDecisionResponse.LoanTypeId;
			}
			else
			{
				// Dont calculate interest if there was an auto approval (the interest was already calculated)
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

				interestRateToUse = interestAccordingToPast == -1 ? 0 : interestAccordingToPast;
				setupFeePercentToUse = dataGatherer.ManualSetupFeePercent;
				setupFeeAmountToUse = dataGatherer.ManualSetupFeeAmount;
				repaymentPeriodToUse = autoDecisionResponse.RepaymentPeriod;
				isEuToUse = false;
				loanTypeIdToUse = 0;
			}

			DB.ExecuteNonQuery(
				"UpdateCashRequestsNew",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("SystemCalculatedAmount", modelLoanOffer),
				new QueryParameter("ManagerApprovedSum", offeredCreditLine),
				new QueryParameter("SystemDecision", autoDecisionResponse.SystemDecision),
				new QueryParameter("MedalType", medalClassification.ToString()), 
				new QueryParameter("ScorePoints", medalScore),
				new QueryParameter("ExpirianRating", dataGatherer.ExperianConsumerScore),
				new QueryParameter("AnnualTurnover", dataGatherer.TotalSumOfOrders1YTotal),
				new QueryParameter("InterestRate", interestRateToUse),
				new QueryParameter("ManualSetupFeeAmount", setupFeeAmountToUse),
				new QueryParameter("ManualSetupFeePercent", setupFeePercentToUse),
				new QueryParameter("RepaymentPeriod", repaymentPeriodToUse),
				new QueryParameter("Now", DateTime.UtcNow),
				new QueryParameter("IsEu", isEuToUse),
				new QueryParameter("LoanTypeId", loanTypeIdToUse)
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
				{"MedalType", medalClassification.ToString()},
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
				{"MedalType", medalClassification.ToString()},
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

		private void SendApprovalMails()
		{
			mailer.Send("Mandrill - User is approved", new Dictionary<string, string> {
				{"ApprovedReApproved", "Approved"},
				{"RegistrationDate", dataGatherer.AppRegistrationDate.ToString(CultureInfo.InvariantCulture)},
				{"userID", customerId.ToString(CultureInfo.InvariantCulture)},
				{"Name", dataGatherer.AppEmail},
				{"FirstName", dataGatherer.AppFirstName},
				{"Surname", dataGatherer.AppSurname},
				{"MP_Counter", dataGatherer.AllMPsNum.ToString(CultureInfo.InvariantCulture)},
				{"MedalType", medalClassification.ToString()},
				{"SystemDecision", autoDecisionResponse.SystemDecision},
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
				{"MedalType", medalClassification.ToString()},
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
				{"MedalType", medalClassification.ToString()},
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
		private int companySeniorityDays;

		private AutoDecisionResponse autoDecisionResponse;
		private decimal loanOfferReApprovalSum;
		private int offeredCreditLine;
		private int modelLoanOffer;
		private MedalClassification medalClassification;
		private decimal medalScore;
	} // class MainStrategy
} // namespace

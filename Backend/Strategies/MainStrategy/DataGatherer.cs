namespace Ezbob.Backend.Strategies.MainStrategy {
	using System;
	using System.Collections.Generic;
	using ConfigManager;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.Experian;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database;

	public class DataGatherer {
		// Preliminary
		public string BwaBusinessCheck { get; private set; }
		public string AppBankAccountType { get; private set; }
		public string AppAccountNumber { get; private set; }
		public string AppSortCode { get; private set; }
		public DateTime? LastStartedMainStrategyEndTime { get; private set; }

		// Config values
		public int RejectDefaultsCreditScore { get; private set; }
		public int RejectDefaultsAccountsNum { get; private set; }
		public int RejectMinimalSeniority { get; private set; }
		public bool EnableAutomaticReRejection { get; private set; }
		public bool EnableAutomaticReApproval { get; private set; }
		public bool EnableAutomaticApproval { get; private set; }
		public bool EnableAutomaticRejection { get; private set; }
		public int MaxCapHomeOwner { get; private set; }
		public int MaxCapNotHomeOwner { get; private set; }
		public int LowCreditScore { get; private set; }
		public int LowTotalAnnualTurnover { get; private set; }
		public int LowTotalThreeMonthTurnover { get; private set; }
		public int DefaultFeedbackValue { get; private set; }
		public int LimitedMedalMinOffer { get; private set; }

		// Customer info
		public bool CustomerStatusIsEnabled { get; private set; }
		public bool CustomerStatusIsWarning { get; private set; }
		public string CustomerStatusName { get; private set; }
		public bool IsOffline { get; private set; }
		public bool IsTest { get; private set; }
		public string AppEmail { get; private set; }
		public string AppFirstName { get; private set; }
		public string AppSurname { get; private set; }
		public string AppGender { get; private set; }
		public bool IsOwnerOfMainAddress { get; private set; }
		public bool IsOwnerOfOtherProperties { get; private set; }
		public string PropertyStatusDescription { get; private set; }
		public int AllMPsNum { get; private set; }
		public DateTime AppRegistrationDate { get; private set; }
		public string TypeOfBusiness { get; private set; }
		public int NumOfLoans { get; private set; }
		public int NumOfHmrcMps { get; private set; }
		public bool IsAlibaba { get; private set; }
		public int? BrokerId { get; private set; }
		public DateTime? CompanyIncorporationDate { get; private set; }
		public int MaxCompanyScore { get; private set; }
		public int MinCompanyScore { get; private set; }
		public int ExperianConsumerScore { get; private set; }
		public int MinExperianConsumerScore { get; private set; }
		public int MaxExperianConsumerScore { get; private set; }
		public int NumOfEbayAmazonPayPalMps { get; private set; }
		public int NumOfYodleeMps { get; private set; }
		public DateTime? EarliestHmrcLastUpdateDate { get; private set; }
		public DateTime? EarliestYodleeLastUpdateDate { get; private set; }
		public List<string> ConsumerCaisDetailWorstStatuses { get; private set; }

		// Online medal
		public int ModelEzbobSeniority { get; private set; }
		public MaritalStatus MaritalStatus { get; private set; }
		public int ModelMaxFeedback { get; private set; }
		public int ModelOnTimeLoans { get; private set; }
		public int ModelLatePayments { get; private set; }
		public int ModelEarlyPayments { get; private set; }
		public bool FirstRepaymentDatePassed { get; private set; }

		// Turnovers and seniority
		public double TotalSumOfOrders1YTotalForRejection { get; private set; }
		public double TotalSumOfOrders3MTotalForRejection { get; private set; }
		public double Yodlee1YForRejection { get; private set; }
		public double Yodlee3MForRejection { get; private set; }
		public double MarketplaceSeniorityDays { get; private set; }
		public decimal TotalSumOfOrdersForLoanOffer { get; private set; }
		public decimal MarketplaceSeniorityYears { get; private set; }
		public decimal EzbobSeniorityMonths { get; private set; }

		// Last cash request
		public decimal LoanOfferReApprovalFullAmount { get; private set; }
		public decimal LoanOfferReApprovalRemainingAmount { get; private set; }
		public decimal LoanOfferReApprovalFullAmountOld { get; private set; }
		public decimal LoanOfferReApprovalRemainingAmountOld { get; private set; }
		public decimal LoanOfferApr { get; private set; }
		public int LoanOfferRepaymentPeriod { get; private set; }
		public decimal LoanOfferInterestRate { get; private set; }
		public int LoanOfferUseSetupFee { get; private set; }
		public int LoanOfferLoanTypeId { get; private set; }
		public int LoanOfferIsLoanTypeSelectionAllowed { get; private set; }
		public int LoanOfferDiscountPlanId { get; private set; }
		public int LoanSourceId { get; private set; }
		public int IsCustomerRepaymentPeriodSelectionAllowed { get; private set; }
		public bool UseBrokerSetupFee { get; private set; }
		public int ManualSetupFeeAmount { get; private set; }
		public decimal ManualSetupFeePercent { get; private set; }

		public DataGatherer(int customerId, AConnection db, ASafeLog log) {
			this.customerId = customerId;
			this.db = db;
			this.log = log;
		}

		public void Gather() {
			ReadConfigurations();
			GetPersonalInfo();
			GetCompanySeniorityDays();
			GetCompanyScore();
			GetCurrentExperianScore();
			GetMinMaxExperianScore();
			GatherOnlineMedalData();
			GatherTurnoversAndSeniority();
			GetLastCashRequestData();
			GetWorstCaisStatuses();
		}

		public void GatherPreliminaryData() {
			BwaBusinessCheck = CurrentValues.Instance.BWABusinessCheck;

			SafeReader results = this.db.GetFirst(
				"GetPersonalInfo",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerId)
				);
			AppBankAccountType = results["BankAccountType"];
			LastStartedMainStrategyEndTime = results["LastStartedMainStrategyEndTime"];
			AppAccountNumber = results["AccountNumber"];
			AppSortCode = results["SortCode"];
			TypeOfBusiness = results["TypeOfBusiness"];
		}

		private void GatherOnlineMedalData() {
			this.log.Info("Starting to calculate score and medal");

			var scoreCardResults = this.db.GetFirst(
				"GetScoreCardData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerId),
				new QueryParameter("Today", DateTime.Today)
				);

			string maritalStatusStr = scoreCardResults["MaritalStatus"];
			MaritalStatus maritalStatusTmp;

			if (!Enum.TryParse(maritalStatusStr, true, out maritalStatusTmp)) {
				this.log.Warn("Cant parse marital status:{0}. Will use 'Other'", maritalStatusStr);
				maritalStatusTmp = MaritalStatus.Other;
			}
			MaritalStatus = maritalStatusTmp;

			ModelMaxFeedback = scoreCardResults["MaxFeedback", DefaultFeedbackValue];

			NumOfEbayAmazonPayPalMps = scoreCardResults["MPsNumber"];
			ModelEzbobSeniority = scoreCardResults["EZBOBSeniority"];
			ModelOnTimeLoans = scoreCardResults["OnTimeLoans"];
			ModelLatePayments = scoreCardResults["LatePayments"];
			ModelEarlyPayments = scoreCardResults["EarlyPayments"];

			FirstRepaymentDatePassed = false;

			DateTime modelFirstRepaymentDate = scoreCardResults["FirstRepaymentDate"];
			if (modelFirstRepaymentDate != default(DateTime))
				FirstRepaymentDatePassed = modelFirstRepaymentDate < DateTime.UtcNow;
		}

		private void GatherTurnoversAndSeniority() {
			this.log.Info("Getting turnovers and seniority");
			MpsTotals totals = new MpsTotals(); // TODO: load marketplace totals by customer id
			TotalSumOfOrders1YTotalForRejection = totals.TotalSumOfOrders1YTotalForRejection;
			TotalSumOfOrders3MTotalForRejection = totals.TotalSumOfOrders3MTotalForRejection;
			Yodlee1YForRejection = totals.Yodlee1YForRejection;
			Yodlee3MForRejection = totals.Yodlee3MForRejection;
			MarketplaceSeniorityDays = totals.MarketplaceSeniorityDays;
			TotalSumOfOrdersForLoanOffer = totals.TotalSumOfOrdersForLoanOffer;
			MarketplaceSeniorityYears = (decimal)totals.MarketplaceSeniorityDays / 365; // It is done this way to fit to the excel
			EzbobSeniorityMonths = (decimal)ModelEzbobSeniority * 12 / 365; // It is done this way to fit to the excel
		}

		private void GetCompanyScore() {
			SafeReader sr = this.db.GetFirst(
				"GetCompanyScore",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerId)
				);

			MaxCompanyScore = sr["MaxScore"];
			MinCompanyScore = sr["MinScore"];
		}

		private void GetCompanySeniorityDays() {
			var getCompanySeniority = new GetCompanySeniority(this.customerId, Utils.IsLimitedCompany(TypeOfBusiness));
			getCompanySeniority.Execute();
			CompanyIncorporationDate = getCompanySeniority.CompanyIncorporationDate;
		}

		private void GetCurrentExperianScore() {
			var scoreStrat = new GetExperianConsumerScore(this.customerId);
			scoreStrat.Execute();
			ExperianConsumerScore = scoreStrat.Score;
		}

		private void GetLastCashRequestData() {
			var lastOfferResults = this.db.GetFirst(
				"GetLastOfferForAutomatedDecision",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerId),
				new QueryParameter("Now", DateTime.UtcNow)
				);

			if (!lastOfferResults.IsEmpty) {
				LoanOfferReApprovalFullAmount = lastOfferResults["ReApprovalFullAmountNew"];
				LoanOfferReApprovalRemainingAmount = lastOfferResults["ReApprovalRemainingAmount"];
				LoanOfferReApprovalFullAmountOld = lastOfferResults["ReApprovalFullAmountOld"];
				LoanOfferReApprovalRemainingAmountOld = lastOfferResults["ReApprovalRemainingAmountOld"];
				LoanOfferApr = lastOfferResults["APR"];
				LoanOfferRepaymentPeriod = lastOfferResults["RepaymentPeriod"];
				LoanOfferInterestRate = lastOfferResults["InterestRate"];
				LoanOfferUseSetupFee = lastOfferResults["UseSetupFee"];
				LoanOfferLoanTypeId = lastOfferResults["LoanTypeId"];
				LoanOfferIsLoanTypeSelectionAllowed = lastOfferResults["IsLoanTypeSelectionAllowed"];
				LoanOfferDiscountPlanId = lastOfferResults["DiscountPlanId"];
				LoanSourceId = lastOfferResults["LoanSourceID"];
				IsCustomerRepaymentPeriodSelectionAllowed = lastOfferResults["IsCustomerRepaymentPeriodSelectionAllowed"];
				UseBrokerSetupFee = lastOfferResults["UseBrokerSetupFee"];
				ManualSetupFeeAmount = lastOfferResults["ManualSetupFeeAmount"];
				ManualSetupFeePercent = lastOfferResults["ManualSetupFeePercent"];
			}
		}

		private void GetMinMaxExperianScore() {
			SafeReader sr = this.db.GetFirst(
				"GetExperianMinMaxConsumerDirectorsScore",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerId),
				new QueryParameter("Now", DateTime.UtcNow)
				);

			if (!sr.IsEmpty) {
				MinExperianConsumerScore = sr["MinExperianScore"];
				MaxExperianConsumerScore = sr["MaxExperianScore"];
			}
		}

		private void GetPersonalInfo() {
			this.log.Info("Getting personal info for customer:{0}", this.customerId);

			SafeReader results = this.db.GetFirst(
				"GetPersonalInfo",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerId)
				);

			CustomerStatusIsEnabled = results["CustomerStatusIsEnabled"];
			CustomerStatusIsWarning = results["CustomerStatusIsWarning"];
			CustomerStatusName = results["CustomerStatusName"];
			IsOffline = results["IsOffline"];
			AppEmail = results["CustomerEmail"];
			AppFirstName = results["FirstName"];
			AppSurname = results["Surname"];
			AppGender = results["Gender"];
			IsOwnerOfMainAddress = results["IsOwnerOfMainAddress"];
			IsOwnerOfOtherProperties = results["IsOwnerOfOtherProperties"];
			PropertyStatusDescription = results["PropertyStatusDescription"];
			AllMPsNum = results["NumOfMps"];
			AppRegistrationDate = results["RegistrationDate"];
			NumOfLoans = results["NumOfLoans"];
			NumOfHmrcMps = results["NumOfHmrcMps"];
			IsAlibaba = results["IsAlibaba"];
			BrokerId = results["BrokerId"];
			NumOfYodleeMps = results["NumOfYodleeMps"];
			EarliestHmrcLastUpdateDate = results["EarliestHmrcLastUpdateDate"];
			EarliestYodleeLastUpdateDate = results["EarliestYodleeLastUpdateDate"];
			IsTest = results["IsTest"];
		}

		private void GetWorstCaisStatuses() {
			ConsumerCaisDetailWorstStatuses = new List<string>();

			this.db.ForEachRowSafe((sr, bRowsetStart) => {
				string worstStatus = sr["WorstStatus"];

				if (!string.IsNullOrEmpty(worstStatus))
					ConsumerCaisDetailWorstStatuses.Add(worstStatus);

				return ActionResult.Continue;
			}, "GetWorstCaisStatuses", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", this.customerId));
		}

		private void ReadConfigurations() {
			RejectDefaultsCreditScore = CurrentValues.Instance.Reject_Defaults_CreditScore;
			RejectDefaultsAccountsNum = CurrentValues.Instance.Reject_Defaults_AccountsNum;
			RejectMinimalSeniority = CurrentValues.Instance.Reject_Minimal_Seniority;
			EnableAutomaticApproval = CurrentValues.Instance.EnableAutomaticApproval;
			EnableAutomaticReApproval = CurrentValues.Instance.EnableAutomaticReApproval;
			EnableAutomaticRejection = CurrentValues.Instance.EnableAutomaticRejection;
			EnableAutomaticReRejection = CurrentValues.Instance.EnableAutomaticReRejection;
			MaxCapHomeOwner = CurrentValues.Instance.MaxCapHomeOwner;
			MaxCapNotHomeOwner = CurrentValues.Instance.MaxCapNotHomeOwner;
			LowCreditScore = CurrentValues.Instance.LowCreditScore;
			LowTotalAnnualTurnover = CurrentValues.Instance.TotalAnnualTurnover;
			LowTotalThreeMonthTurnover = CurrentValues.Instance.TotalThreeMonthTurnover;
			DefaultFeedbackValue = CurrentValues.Instance.DefaultFeedbackValue;
			LimitedMedalMinOffer = CurrentValues.Instance.MedalMinOffer;
		}

		private readonly int customerId;
		private readonly AConnection db;
		private readonly ASafeLog log;
	}
}

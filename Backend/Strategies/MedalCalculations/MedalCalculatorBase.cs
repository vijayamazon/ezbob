namespace Ezbob.Backend.Strategies.MedalCalculations {
	using System.Collections.Generic;
	using System.Linq;
	using ConfigManager;
	using Experian;
	using EzBob.Models.Marketplaces.Builders;
	using EzBob.Models.Marketplaces.Yodlee;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using System;
	using EZBob.DatabaseLib.Model.Database;
	using VatReturn;

	public abstract class MedalCalculatorBase {
		public MedalResult Results { get; set; }

		public MedalResult CalculateMedalScore(int customerId, DateTime calculationTime) {
			Results = new MedalResult(customerId) { CalculationTime = calculationTime };
			SetMedalType();

			try {
				GatherInputData(calculationTime);

				// Process raw input data to data
				CalculateFeedbacks();
				DetermineFlow();
				CalculateRatiosOfAnnualTurnover();
				CalculateNetWorth();

				// Calculate weights
				SetInitialWeights();
				AdjustCompanyScoreWeight();
				AdjustConsumerScoreWeight();
				if (Results.InnerFlowName != "HMRC") {
					RedistributeFreeCashFlowWeight();
				}
				if (Results.FirstRepaymentDatePassed) {
					Results.EzbobSeniorityWeight = 2;
					Results.NumOfLoansWeight = 3.33m;
					Results.NumOfLateRepaymentsWeight = 2.67m;
					Results.NumOfEarlyRepaymentsWeight = 2;

					RedistributeWeightsForPayingCustomer();
				}
				AdjustSumOfWeights();

				CalculateGrades();

				CalculateCustomerScore();
				decimal totalScoreMin = CalculateScoreMin();
				decimal totalScoreMax = CalculateScoreMax();
				Results.TotalScoreNormalized = (Results.TotalScore - totalScoreMin) / (totalScoreMax - totalScoreMin);

				CalculateMedal();

				CalculateOffer();
			} catch (Exception e) {
				log.Error("Failed calculating medal of type:{0} for customer: {1} with error: {2}", Results.MedalType, customerId, e);
				Results.Error = e.Message;
			}

			return Results;
		}

		public abstract void SetInitialWeights();

		protected MedalCalculatorBase(AConnection db, ASafeLog log) {
			this.log = log;
			this.db = db;
		}

		protected abstract void AdjustWeightsWithRatio(decimal ratio);

		protected virtual void GatherInputData(DateTime calculationTime) {
			SafeReader sr = db.GetFirst("GetDataForMedalCalculation1", CommandSpecies.StoredProcedure,
										new QueryParameter("CustomerId", Results.CustomerId),
										new QueryParameter("CalculationTime", Results.CalculationTime));

			if (sr.IsEmpty) {
				throw new Exception("Couldn't gather required data for the medal calculation");
			}

			Results.BusinessScore = sr["BusinessScore"];
			Results.TangibleEquityValue = sr["TangibleEquity"];
			Results.BusinessSeniority = sr["BusinessSeniority"];
			Results.ConsumerScore = sr["ConsumerScore"];
			string maritalStatusStr = sr["MaritalStatus"];
			MaritalStatus maritalStatus;
			if (Enum.TryParse(maritalStatusStr, out maritalStatus)) {
				Results.MaritalStatus = maritalStatus;
			} else {
				log.Error("Unable to parse marital status for customer:{0} will use 'Other'. The value was:{1}", Results.CustomerId, maritalStatusStr);
				Results.MaritalStatus = MaritalStatus.Other;
			}

			Results.FirstRepaymentDatePassed = sr["FirstRepaymentDatePassed"];
			Results.EzbobSeniority = sr["EzbobSeniority"];
			Results.NumOfLoans = sr["OnTimeLoans"];
			Results.NumOfLateRepayments = sr["NumOfLatePayments"];
			Results.NumOfEarlyRepayments = sr["NumOfEarlyPayments"];
			int hmrcId = sr["HmrcId"];
			Results.ZooplaValue = sr["TotalZooplaValue"];
			Results.NumOfHmrcMps = sr["NumOfHmrcMps"];
			Results.NumberOfStores = sr["NumberOfOnlineStores"];

			Results.EarliestHmrcLastUpdateDate = sr["EarliestHmrcLastUpdateDate"];
			Results.EarliestYodleeLastUpdateDate = sr["EarliestYodleeLastUpdateDate"];
			Results.AmazonPositiveFeedbacks = sr["AmazonPositiveFeedbacks"];
			Results.EbayPositiveFeedbacks = sr["EbayPositiveFeedbacks"];
			Results.NumberOfPaypalPositiveTransactions = sr["NumOfPaypalTransactions"];

			Results.OnlineAnnualTurnover = 0; // TODO: strategyHelper.GetOnlineAnnualTurnoverForMedal(Results.CustomerId);

			if (Results.NumOfHmrcMps == 1) {
				var loadVatReturnSummary = new LoadVatReturnSummary(Results.CustomerId, hmrcId);
				loadVatReturnSummary.Execute();
				VatReturnSummary[] summaryData = loadVatReturnSummary.Summary;

				if (summaryData != null && summaryData.Length != 0) {
					foreach (VatReturnSummary singleSummary in summaryData) {
						Results.HmrcAnnualTurnover += singleSummary.AnnualizedTurnover.HasValue ? singleSummary.AnnualizedTurnover.Value : 0;
						Results.FreeCashFlowValue += singleSummary.AnnualizedFreeCashFlow.HasValue ? singleSummary.AnnualizedFreeCashFlow.Value : 0;
						Results.ValueAdded += singleSummary.AnnualizedValueAdded.HasValue ? singleSummary.AnnualizedValueAdded.Value : 0;
					}
				}
			}

			CalculateBankAnnualTurnover();

			Results.MortgageBalance = GetMortgages(Results.CustomerId);
		}

		protected abstract decimal GetCompanyScoreWeightForLowScore();

		protected abstract decimal GetConsumerScoreWeightForLowScore();

		protected abstract decimal GetSumOfNonFixedWeights();

		protected virtual void RedistributeFreeCashFlowWeight() { }

		protected abstract void RedistributeWeightsForPayingCustomer();

		protected abstract void SetMedalType();

		protected readonly AConnection db;
		protected readonly ASafeLog log;
		private void AdjustCompanyScoreWeight() {
			if (Results.BusinessScore <= 30) {
				Results.BusinessScoreWeight = GetCompanyScoreWeightForLowScore();
			}
		}

		private void AdjustConsumerScoreWeight() {
			if (Results.ConsumerScore <= 800) {
				Results.ConsumerScoreWeight = GetConsumerScoreWeightForLowScore();
			}
		}

		private void AdjustSumOfWeights() {
			decimal sumOfWeights = Results.BusinessScoreWeight + Results.FreeCashFlowWeight + Results.AnnualTurnoverWeight +
								   Results.TangibleEquityWeight + Results.BusinessSeniorityWeight + Results.ConsumerScoreWeight +
								   Results.NetWorthWeight + Results.MaritalStatusWeight + Results.NumberOfStoresWeight +
								   Results.PositiveFeedbacksWeight + Results.EzbobSeniorityWeight + Results.NumOfLoansWeight +
								   Results.NumOfLateRepaymentsWeight + Results.NumOfEarlyRepaymentsWeight;

			if (sumOfWeights != 100) {
				decimal sumOfNonFixed = GetSumOfNonFixedWeights();
				decimal sumOfNonFixedDestination = sumOfNonFixed - sumOfWeights + 100;
				decimal ratioForDestination = sumOfNonFixedDestination / sumOfNonFixed;
				AdjustWeightsWithRatio(ratioForDestination);
			}
		}

		private void CalculateAnnualTurnoverGrade() {
			if (Results.AnnualTurnover < 30000) {
				Results.AnnualTurnoverGrade = 0;
			} else if (Results.AnnualTurnover < 100000) {
				Results.AnnualTurnoverGrade = 1;
			} else if (Results.AnnualTurnover < 200000) {
				Results.AnnualTurnoverGrade = 2;
			} else if (Results.AnnualTurnover < 400000) {
				Results.AnnualTurnoverGrade = 3;
			} else if (Results.AnnualTurnover < 800000) {
				Results.AnnualTurnoverGrade = 4;
			} else if (Results.AnnualTurnover < 2000000) {
				Results.AnnualTurnoverGrade = 5;
			} else {
				Results.AnnualTurnoverGrade = 6;
			}
		}

		private void CalculateBankAnnualTurnover() {
			var yodleeMps = new List<int>();

			db.ForEachRowSafe((yodleeSafeReader, bRowsetStart) => {
				int mpId = yodleeSafeReader["Id"];
				yodleeMps.Add(mpId);
				return ActionResult.Continue;
			}, "GetYodleeMps", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", Results.CustomerId));

			foreach (int mpId in yodleeMps) {
				var yodleeModelBuilder = new YodleeMarketplaceModelBuilder();
				YodleeModel yodleeModel = yodleeModelBuilder.BuildYodlee(mpId);

				Results.BankAnnualTurnover += (decimal)yodleeModel.BankStatementAnnualizedModel.Revenues;
			}
		}

		private void CalculateBusinessScoreGrade() {
			if (Results.BusinessScore < 11) {
				Results.BusinessScoreGrade = 0;
			} else if (Results.BusinessScore < 21) {
				Results.BusinessScoreGrade = 1;
			} else if (Results.BusinessScore < 31) {
				Results.BusinessScoreGrade = 2;
			} else if (Results.BusinessScore < 41) {
				Results.BusinessScoreGrade = 3;
			} else if (Results.BusinessScore < 51) {
				Results.BusinessScoreGrade = 4;
			} else if (Results.BusinessScore < 61) {
				Results.BusinessScoreGrade = 5;
			} else if (Results.BusinessScore < 71) {
				Results.BusinessScoreGrade = 6;
			} else if (Results.BusinessScore < 81) {
				Results.BusinessScoreGrade = 7;
			} else if (Results.BusinessScore < 91) {
				Results.BusinessScoreGrade = 8;
			} else {
				Results.BusinessScoreGrade = 9;
			}
		}

		private void CalculateBusinessSeniorityGrade() {
			var dateOnlyCalculationTime = Results.CalculationTime.Date;
			if (!Results.BusinessSeniority.HasValue || Results.BusinessSeniority.Value.AddYears(1) > dateOnlyCalculationTime) {
				Results.BusinessSeniorityGrade = 0;
			} else if (Results.BusinessSeniority.Value.AddYears(3) > dateOnlyCalculationTime) {
				Results.BusinessSeniorityGrade = 1;
			} else if (Results.BusinessSeniority.Value.AddYears(5) > dateOnlyCalculationTime) {
				Results.BusinessSeniorityGrade = 2;
			} else if (Results.BusinessSeniority.Value.AddYears(10) > dateOnlyCalculationTime) {
				Results.BusinessSeniorityGrade = 3;
			} else {
				Results.BusinessSeniorityGrade = 4;
			}
		}

		private void CalculateConsumerScoreGrade() {
			if (Results.ConsumerScore < 481) {
				Results.ConsumerScoreGrade = 0;
			} else if (Results.ConsumerScore < 561) {
				Results.ConsumerScoreGrade = 1;
			} else if (Results.ConsumerScore < 641) {
				Results.ConsumerScoreGrade = 2;
			} else if (Results.ConsumerScore < 721) {
				Results.ConsumerScoreGrade = 3;
			} else if (Results.ConsumerScore < 801) {
				Results.ConsumerScoreGrade = 4;
			} else if (Results.ConsumerScore < 881) {
				Results.ConsumerScoreGrade = 5;
			} else if (Results.ConsumerScore < 961) {
				Results.ConsumerScoreGrade = 6;
			} else if (Results.ConsumerScore < 1041) {
				Results.ConsumerScoreGrade = 7;
			} else {
				Results.ConsumerScoreGrade = 8;
			}
		}

		private void CalculateCustomerScore() {
			Results.AnnualTurnoverScore = Results.AnnualTurnoverWeight * Results.AnnualTurnoverGrade;
			Results.BusinessScoreScore = Results.BusinessScoreWeight * Results.BusinessScoreGrade;
			Results.FreeCashFlowScore = Results.FreeCashFlowWeight * Results.FreeCashFlowGrade;
			Results.TangibleEquityScore = Results.TangibleEquityWeight * Results.TangibleEquityGrade;
			Results.BusinessSeniorityScore = Results.BusinessSeniorityWeight * Results.BusinessSeniorityGrade;
			Results.ConsumerScoreScore = Results.ConsumerScoreWeight * Results.ConsumerScoreGrade;
			Results.NetWorthScore = Results.NetWorthWeight * Results.NetWorthGrade;
			Results.MaritalStatusScore = Results.MaritalStatusWeight * Results.MaritalStatusGrade;
			Results.NumberOfStoresScore = Results.NumberOfStoresWeight * Results.NumberOfStoresGrade;
			Results.PositiveFeedbacksScore = Results.PositiveFeedbacksWeight * Results.PositiveFeedbacksGrade;
			Results.EzbobSeniorityScore = Results.EzbobSeniorityWeight * Results.EzbobSeniorityGrade;
			Results.NumOfLoansScore = Results.NumOfLoansWeight * Results.NumOfLoansGrade;
			Results.NumOfLateRepaymentsScore = Results.NumOfLateRepaymentsWeight * Results.NumOfLateRepaymentsGrade;
			Results.NumOfEarlyRepaymentsScore = Results.NumOfEarlyRepaymentsWeight * Results.NumOfEarlyRepaymentsGrade;

			Results.TotalScore = Results.AnnualTurnoverScore + Results.BusinessScoreScore + Results.FreeCashFlowScore +
								 Results.TangibleEquityScore + Results.BusinessSeniorityScore + Results.ConsumerScoreScore +
								 Results.NetWorthScore + Results.MaritalStatusScore + Results.NumberOfStoresScore +
								 Results.PositiveFeedbacksScore + Results.EzbobSeniorityScore + Results.NumOfLoansScore +
								 Results.NumOfLateRepaymentsScore + Results.NumOfEarlyRepaymentsScore;
		}

		private void CalculateEzbobSeniorityGrade() {
			if (!Results.EzbobSeniority.HasValue) {
				Results.EzbobSeniority = Results.CalculationTime;
			}

			decimal ezbobSeniorityMonths = (decimal)(Results.CalculationTime - Results.EzbobSeniority.Value).TotalDays / (365.0M / 12.0M);
			if (ezbobSeniorityMonths < 1) {
				Results.EzbobSeniorityGrade = 0;
			} else if (ezbobSeniorityMonths < 6) {
				Results.EzbobSeniorityGrade = 2;
			} else if (ezbobSeniorityMonths < 18) {
				Results.EzbobSeniorityGrade = 3;
			} else {
				Results.EzbobSeniorityGrade = 4;
			}
		}

		private void CalculateFeedbacks() {
			Results.PositiveFeedbacks = Results.AmazonPositiveFeedbacks + Results.EbayPositiveFeedbacks;
			if (Results.PositiveFeedbacks == 0) {
				Results.PositiveFeedbacks = Results.NumberOfPaypalPositiveTransactions != 0 ? Results.NumberOfPaypalPositiveTransactions : CurrentValues.Instance.DefaultFeedbackValue;
			}
		}

		private void CalculateFreeCashFlowGrade() {
			if (Results.FreeCashFlow < -0.1m || Results.AnnualTurnover <= 0) // When turnover is zero we can't calc FCF, we want the min grade
			{
				Results.FreeCashFlowGrade = 0;
			} else if (Results.FreeCashFlow < 0) {
				Results.FreeCashFlowGrade = 1;
			} else if (Results.FreeCashFlow < 0.1m) {
				Results.FreeCashFlowGrade = 2;
			} else if (Results.FreeCashFlow < 0.2m) {
				Results.FreeCashFlowGrade = 3;
			} else if (Results.FreeCashFlow < 0.3m) {
				Results.FreeCashFlowGrade = 4;
			} else if (Results.FreeCashFlow < 0.4m) {
				Results.FreeCashFlowGrade = 5;
			} else {
				Results.FreeCashFlowGrade = 6;
			}
		}

		private void CalculateGrades() {
			CalculateBusinessScoreGrade();
			CalculateFreeCashFlowGrade();
			CalculateAnnualTurnoverGrade();
			CalculateTangibleEquityGrade();
			CalculateBusinessSeniorityGrade();
			CalculateConsumerScoreGrade();
			CalculateNetWorthGrade();
			CalculateMaritalStatusGrade();
			CalculateNumberOfStoresGrade();
			CalculatePositiveFeedbacksGrade();
			CalculateEzbobSeniorityGrade();
			CalculateNumOfLoansGrade();
			CalculateNumOfLateRepaymentsGrade();
			CalculateNumOfEarlyRepaymentsGrade();
		}

		private void CalculateMaritalStatusGrade() {
			if (Results.MaritalStatus == MaritalStatus.Married || Results.MaritalStatus == MaritalStatus.Widowed) {
				Results.MaritalStatusGrade = 4;
			} else if (Results.MaritalStatus == MaritalStatus.Divorced || Results.MaritalStatus == MaritalStatus.LivingTogether) {
				Results.MaritalStatusGrade = 3;
			} else // Single, Separated, Other
			{
				Results.MaritalStatusGrade = 2;
			}
		}

		private void CalculateMedal() {
			if (Results.TotalScoreNormalized <= 0.4m) {
				Results.MedalClassification = Medal.Silver;
			} else if (Results.TotalScoreNormalized <= 0.62m) {
				Results.MedalClassification = Medal.Gold;
			} else if (Results.TotalScoreNormalized <= 0.84m) {
				Results.MedalClassification = Medal.Platinum;
			} else {
				Results.MedalClassification = Medal.Diamond;
			}
		}

		private void CalculateNetWorth() {
			if (Results.ZooplaValue != 0) {
				Results.NetWorth = (Results.ZooplaValue - Results.MortgageBalance) / Results.ZooplaValue;
			} else {
				Results.NetWorth = 0;
			}
		}

		private void CalculateNetWorthGrade() {
			if (Results.NetWorth < 0.15m) {
				Results.NetWorthGrade = 0;
			} else if (Results.NetWorth < 0.5m) {
				Results.NetWorthGrade = 1;
			} else if (Results.NetWorth < 1) {
				Results.NetWorthGrade = 2;
			} else {
				// We know that we sometimes miss mortgages the customer has, so instead of grade=3 we give 1
				Results.NetWorthGrade = 1;
			}
		}

		private void CalculateNumberOfStoresGrade() {
			if (Results.NumberOfStores < 3) {
				Results.NumberOfStoresGrade = 1;
			} else if (Results.NumberOfStores < 5) {
				Results.NumberOfStoresGrade = 3;
			} else {
				Results.NumberOfStoresGrade = 5;
			}
		}

		private void CalculateNumOfEarlyRepaymentsGrade() {
			if (Results.NumOfEarlyRepayments == 0) {
				Results.NumOfEarlyRepaymentsGrade = 2;
			} else if (Results.NumOfEarlyRepayments < 4) {
				Results.NumOfEarlyRepaymentsGrade = 3;
			} else {
				Results.NumOfEarlyRepaymentsGrade = 5;
			}
		}

		private void CalculateNumOfLateRepaymentsGrade() {
			if (Results.NumOfLateRepayments == 0) {
				Results.NumOfLateRepaymentsGrade = 5;
			} else if (Results.NumOfLateRepayments == 1) {
				Results.NumOfLateRepaymentsGrade = 2;
			} else {
				Results.NumOfLateRepaymentsGrade = 0;
			}
		}

		private void CalculateNumOfLoansGrade() {
			if (Results.NumOfLoans > 3) {
				Results.NumOfLoansGrade = 4;
			} else if (Results.NumOfLoans > 1) {
				Results.NumOfLoansGrade = 3;
			} else {
				Results.NumOfLoansGrade = 1;
			}
		}

		private void CalculateOffer() {
			if (Results == null || !string.IsNullOrEmpty(Results.Error) || Results.MedalType == MedalType.NoMedal) {
				return;
			}

			SafeReader sr = db.GetFirst(
				"GetMedalCoefficients",
				CommandSpecies.StoredProcedure,
				new QueryParameter("MedalClassification", Results.MedalClassification.ToString())
				);

			if (!sr.IsEmpty) {
				decimal annualTurnoverMedalFactor = sr["AnnualTurnover"];
				decimal freeCashFlowMedalFactor = sr["FreeCashFlow"];
				decimal valueAddedMedalFactor = sr["ValueAdded"];
				decimal offerAccordingToAnnualTurnover = Results.AnnualTurnover * annualTurnoverMedalFactor / 100;
				decimal offerAccordingToFreeCashFlow = Results.FreeCashFlowValue * freeCashFlowMedalFactor / 100;
				decimal offerAccordingToValueAdded = Results.ValueAdded * valueAddedMedalFactor / 100;

				if (IsOnlineMedalNotViaHmrcInnerFlow()) {
					offerAccordingToValueAdded = 0;
				}

				List<decimal> validOfferAmounts = new[] {
					offerAccordingToAnnualTurnover,
					offerAccordingToFreeCashFlow,
					offerAccordingToValueAdded
				}.Where(x => x >= CurrentValues.Instance.MedalMinOffer).ToList();
				if (validOfferAmounts.Count > 0) {
					decimal unroundedValue = validOfferAmounts.Min();
					Results.OfferedLoanAmount = (int)unroundedValue;
				}
			}
		}

		private void CalculatePositiveFeedbacksGrade() {
			if (Results.PositiveFeedbacks < 1) {
				Results.PositiveFeedbacksGrade = 0;
			} else if (Results.PositiveFeedbacks < 5001) {
				Results.PositiveFeedbacksGrade = 2;
			} else if (Results.PositiveFeedbacks < 50000) {
				Results.PositiveFeedbacksGrade = 3;
			} else {
				Results.PositiveFeedbacksGrade = 5;
			}
		}

		private void CalculateRatiosOfAnnualTurnover() {
			if (Results.AnnualTurnover > 0) {
				Results.TangibleEquity = Results.TangibleEquityValue / Results.AnnualTurnover;
				Results.FreeCashFlow = Results.FreeCashFlowValue / Results.AnnualTurnover;
			} else {
				Results.TangibleEquity = 0;
				Results.AnnualTurnover = 0;
				Results.FreeCashFlow = 0;
			}
		}

		private decimal CalculateScoreMax() {
			const int annualTurnoverMaxGrade = 6;
			const int businessScoreMaxGrade = 9;
			const int freeCashflowMaxGrade = 6;
			const int tangibleEquityMaxGrade = 4;
			const int businessSeniorityMaxGrade = 4;
			const int consumerScoreMaxGrade = 8;
			const int netWorthMaxGrade = 2;
			const int maritalStatusMaxGrade = 4;
			const int numberOfStoresMaxGrade = 5;
			const int positiveFeedbacksMaxGrade = 5;
			const int ezbobSeniorityMaxGrade = 4;
			const int numOfLoansMaxGrade = 4;
			const int numOfLateRepaymentsMaxGrade = 5;
			const int numOfEarlyRepaymentsMaxGrade = 5;

			decimal annualTurnoverScoreMax = Results.AnnualTurnoverWeight * annualTurnoverMaxGrade;
			decimal businessScoreScoreMax = Results.BusinessScoreWeight * businessScoreMaxGrade;
			decimal freeCashFlowScoreMax = Results.FreeCashFlowWeight * freeCashflowMaxGrade;
			decimal tangibleEquityScoreMax = Results.TangibleEquityWeight * tangibleEquityMaxGrade;
			decimal businessSeniorityScoreMax = Results.BusinessSeniorityWeight * businessSeniorityMaxGrade;
			decimal consumerScoreScoreMax = Results.ConsumerScoreWeight * consumerScoreMaxGrade;
			decimal netWorthScoreMax = Results.NetWorthWeight * netWorthMaxGrade;
			decimal maritalStatusScoreMax = Results.MaritalStatusWeight * maritalStatusMaxGrade;
			decimal numberOfStoresMax = Results.NumberOfStoresWeight * numberOfStoresMaxGrade;
			decimal positiveFeedbacksScoreMax = Results.PositiveFeedbacksWeight * positiveFeedbacksMaxGrade;
			decimal ezbobSeniorityScoreMax = Results.EzbobSeniorityWeight * ezbobSeniorityMaxGrade;
			decimal ezbobNumOfLoansScoreMax = Results.NumOfLoansWeight * numOfLoansMaxGrade;
			decimal ezbobNumOfLateRepaymentsScoreMax = Results.NumOfLateRepaymentsWeight * numOfLateRepaymentsMaxGrade;
			decimal ezbobNumOfEarlyRepaymentsScoreMax = Results.NumOfEarlyRepaymentsWeight * numOfEarlyRepaymentsMaxGrade;

			return annualTurnoverScoreMax + businessScoreScoreMax + freeCashFlowScoreMax + tangibleEquityScoreMax +
				   businessSeniorityScoreMax + consumerScoreScoreMax + netWorthScoreMax + maritalStatusScoreMax +
				   numberOfStoresMax + positiveFeedbacksScoreMax + ezbobSeniorityScoreMax + ezbobNumOfLoansScoreMax +
				   ezbobNumOfLateRepaymentsScoreMax + ezbobNumOfEarlyRepaymentsScoreMax;
		}

		private decimal CalculateScoreMin() {
			const int annualTurnoverMinGrade = 0;
			const int businessScoreMinGrade = 0;
			const int freeCashflowMinGrade = 0;
			const int tangibleEquityMinGrade = 0;
			const int businessSeniorityMinGrade = 0;
			const int consumerScoreMinGrade = 0;
			const int netWorthMinGrade = 0;
			const int maritalStatusMinGrade = 2;
			const int numberOfStoresMinGrade = 1;
			const int positiveFeedbacksMinGrade = 0;
			const int ezbobSeniorityMinGrade = 0;
			const int numOfLoansMinGrade = 1;
			const int numOfLateRepaymentsMinGrade = 0;
			const int numOfEarlyRepaymentsMinGrade = 2;

			decimal annualTurnoverScoreMin = Results.AnnualTurnoverWeight * annualTurnoverMinGrade;
			decimal businessScoreScoreMin = Results.BusinessScoreWeight * businessScoreMinGrade;
			decimal freeCashFlowScoreMin = Results.FreeCashFlowWeight * freeCashflowMinGrade;
			decimal tangibleEquityScoreMin = Results.TangibleEquityWeight * tangibleEquityMinGrade;
			decimal businessSeniorityScoreMin = Results.BusinessSeniorityWeight * businessSeniorityMinGrade;
			decimal consumerScoreScoreMin = Results.ConsumerScoreWeight * consumerScoreMinGrade;
			decimal netWorthScoreMin = Results.NetWorthWeight * netWorthMinGrade;
			decimal maritalStatusScoreMin = Results.MaritalStatusWeight * maritalStatusMinGrade;
			decimal numberOfStoresMin = Results.NumberOfStoresWeight * numberOfStoresMinGrade;
			decimal positiveFeedbacksScoreMin = Results.PositiveFeedbacksWeight * positiveFeedbacksMinGrade;
			decimal ezbobSeniorityScoreMin = Results.EzbobSeniorityWeight * ezbobSeniorityMinGrade;
			decimal ezbobNumOfLoansScoreMin = Results.NumOfLoansWeight * numOfLoansMinGrade;
			decimal ezbobNumOfLateRepaymentsScoreMin = Results.NumOfLateRepaymentsWeight * numOfLateRepaymentsMinGrade;
			decimal ezbobNumOfEarlyRepaymentsScoreMin = Results.NumOfEarlyRepaymentsWeight * numOfEarlyRepaymentsMinGrade;

			return annualTurnoverScoreMin + businessScoreScoreMin + freeCashFlowScoreMin + tangibleEquityScoreMin +
				   businessSeniorityScoreMin + consumerScoreScoreMin + netWorthScoreMin + maritalStatusScoreMin +
				   numberOfStoresMin + positiveFeedbacksScoreMin + ezbobSeniorityScoreMin + ezbobNumOfLoansScoreMin +
				   ezbobNumOfLateRepaymentsScoreMin + ezbobNumOfEarlyRepaymentsScoreMin;
		}

		private void CalculateTangibleEquityGrade() {
			if (Results.TangibleEquity < -0.05m || Results.AnnualTurnover <= 0) // When turnover is zero we can't calc tangible equity, we want the min grade
			{
				Results.TangibleEquityGrade = 0;
			} else if (Results.TangibleEquity < 0) {
				Results.TangibleEquityGrade = 1;
			} else if (Results.TangibleEquity < 0.1m) {
				Results.TangibleEquityGrade = 2;
			} else if (Results.TangibleEquity < 0.3m) {
				Results.TangibleEquityGrade = 3;
			} else {
				Results.TangibleEquityGrade = 4;
			}
		}

		private void DetermineFlow() {
			if (Results.MedalType.IsOnline()) {
				decimal onlineMedalTurnoverCutoff = CurrentValues.Instance.OnlineMedalTurnoverCutoff;
				if (Results.HmrcAnnualTurnover > onlineMedalTurnoverCutoff * Results.OnlineAnnualTurnover) {
					Results.InnerFlowName = "HMRC";
					Results.AnnualTurnover = Results.HmrcAnnualTurnover;
				} else {
					if (Results.BankAnnualTurnover > onlineMedalTurnoverCutoff * Results.OnlineAnnualTurnover) {
						Results.InnerFlowName = "Bank";
						Results.AnnualTurnover = Results.BankAnnualTurnover;
					} else {
						Results.InnerFlowName = "Online";
						Results.AnnualTurnover = Results.OnlineAnnualTurnover;
					}
				}
			} else {
				if (Results.NumOfHmrcMps == 1) {
					Results.InnerFlowName = "HMRC";
					Results.AnnualTurnover = Results.HmrcAnnualTurnover;
				} else {
					Results.InnerFlowName = "Bank";
					Results.AnnualTurnover = Results.BankAnnualTurnover;
				}
			}
		}

		private decimal GetMortgages(int customerId) {
			var instance = new LoadExperianConsumerMortgageData(customerId);
			instance.Execute();

			return instance.Result.MortgageBalance;
		}

		private bool IsOnlineMedalNotViaHmrcInnerFlow() {
			return Results.MedalType.IsOnline() && Results.InnerFlowName != "HMRC";
		}
	}
}

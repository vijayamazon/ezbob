namespace Ezbob.Backend.Strategies.MedalCalculations {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using AutomationCalculator.Turnover;
	using ConfigManager;
	using Ezbob.Backend.Strategies.Experian;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database;

	/// <summary>
	///     The medal calculator base.
	/// </summary>
	public abstract class MedalCalculatorBase {
		/// <summary>
		///     The db.
		/// </summary>
		protected readonly AConnection db;

		/// <summary>
		///     The log.
		/// </summary>
		protected readonly ASafeLog log;

		/// <summary>
		///     Initializes a new instance of the <see cref="MedalCalculatorBase" /> class.
		/// </summary>
		protected MedalCalculatorBase() {
			this.log = Library.Instance.Log;
			this.db = Library.Instance.DB;
		}

		/// <summary>
		///     Gets or sets the results.
		/// </summary>
		public MedalResult Results { get; set; }

		/// <summary>
		///     The calculate medal score.
		/// </summary>
		/// <param name="customerId">The customer id.</param>
		/// <param name="calculationTime">The calculation time.</param>
		/// <returns>The <see cref="MedalResult" />.</returns>
		public MedalResult CalculateMedalScore(int customerId, DateTime calculationTime) {
			Results = new MedalResult(customerId) {
				CalculationTime = calculationTime
			};

			// set medal type here per each extending class
			SetMedalType();

			try {
				GatherInputData();

				// Process raw input data to data
				CalculateFeedbacks();
				CalculateTurnoverForMedal();
				CalculateRatiosOfAnnualTurnover();
				CalculateNetWorth();

				// Calculate weights
				SetInitialWeights();
				AdjustCompanyScoreWeight();
				AdjustConsumerScoreWeight();

				if (Results.TurnoverType != TurnoverType.HMRC)
					RedistributeFreeCashFlowWeight();

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
				this.log.Error(
					e,
					"Failed calculating medal of type: {0} for customer: {1}",
					Results.MedalType,
					customerId);
				Results.Error = e.Message;
			}

			// try
			return Results;
		}

		/// <summary>
		///     The set initial weights.
		/// </summary>
		public abstract void SetInitialWeights();

		/// <summary>
		///     The adjust weights with ratio.
		/// </summary>
		/// <param name="ratio">The ratio.</param>
		protected abstract void AdjustWeightsWithRatio(decimal ratio);

		// CalculateMedalScore constructor
		/// <summary>
		///     The gather input data.
		/// </summary>
		/// <exception cref="Exception"></exception>
		protected virtual void GatherInputData() {
			SafeReader sr = this.db.GetFirst(
				"GetDataForMedalCalculation1",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", Results.CustomerId),
				new QueryParameter("CalculationTime", Results.CalculationTime)
			);

			if (sr.IsEmpty)
				throw new Exception("Couldn't gather required data for the medal calculation");

			Results.BusinessScore = sr["BusinessScore"];
			Results.TangibleEquityValue = sr["TangibleEquity"];
			Results.BusinessSeniority = sr["BusinessSeniority"];
			Results.ConsumerScore = sr["ConsumerScore"];
			string maritalStatusStr = sr["MaritalStatus"];
			MaritalStatus maritalStatus;

			if (Enum.TryParse(maritalStatusStr, out maritalStatus))
				Results.MaritalStatus = maritalStatus;
			else {
				this.log.Error(
					"Unable to parse marital status for customer:{0} will use 'Other'. The value was:{1}",
					Results.CustomerId,
					maritalStatusStr
				);
				Results.MaritalStatus = MaritalStatus.Other;
			} // if

			Results.FirstRepaymentDatePassed = sr["FirstRepaymentDatePassed"];
			Results.EzbobSeniority = sr["EzbobSeniority"];
			Results.NumOfLoans = sr["OnTimeLoans"];
			Results.NumOfLateRepayments = sr["NumOfLatePayments"];
			Results.NumOfEarlyRepayments = sr["NumOfEarlyPayments"];
			Results.ZooplaValue = sr["TotalZooplaValue"];
			Results.NumOfHmrcMps = sr["NumOfHmrcMps"];
			Results.NumOfBanks = sr["NumOfBanks"];
			Results.NumberOfStores = sr["NumberOfOnlineStores"];

			Results.EarliestHmrcLastUpdateDate = sr["EarliestHmrcLastUpdateDate"];
			Results.EarliestYodleeLastUpdateDate = sr["EarliestYodleeLastUpdateDate"];
			Results.AmazonPositiveFeedbacks = sr["AmazonPositiveFeedbacks"];
			Results.EbayPositiveFeedbacks = sr["EbayPositiveFeedbacks"];
			Results.NumberOfPaypalPositiveTransactions = sr["NumOfPaypalTransactions"];

			/**
			*  medal type: https://drive.draw.io/?#G0B1Io_qu9i44SVzVqV19nbnMxRW8
			*  medal type and medal value: https://drive.draw.io/?#G0B1Io_qu9i44ScEJqeUlLNEhaa28
			*  
			*  19/01/2015: FreeCashFlow, ValueAdded: NO NEED HERE (elina)
			* 
			*  Results.OnlineAnnualTurnover = 0; // TODO: strategyHelper.GetOnlineAnnualTurnoverForMedal(Results.CustomerId);
			   if (Results.NumOfHmrcMps == 1)
				   {
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
			   Results.BankAnnualTurnover = 0; // TODO: fill it
		   */

			Results.MortgageBalance = GetMortgages(Results.CustomerId);
		} // GatherInputData

		/// <summary>
		///     The get company score weight for low score.
		/// </summary>
		/// <returns>The <see cref="decimal" />.</returns>
		protected abstract decimal GetCompanyScoreWeightForLowScore();

		// GatherInputData
		/// <summary>
		///     The get consumer score weight for low score.
		/// </summary>
		/// <returns>The <see cref="decimal" />.</returns>
		protected abstract decimal GetConsumerScoreWeightForLowScore();

		/// <summary>
		///     The get sum of non fixed weights.
		/// </summary>
		/// <returns>The <see cref="decimal" />.</returns>
		protected abstract decimal GetSumOfNonFixedWeights();

		/// <summary>
		///     The redistribute free cash flow weight.
		/// </summary>
		protected virtual void RedistributeFreeCashFlowWeight() { }

		/// <summary>
		///     The redistribute weights for paying customer.
		/// </summary>
		protected abstract void RedistributeWeightsForPayingCustomer();

		/// <summary>
		///     The set medal type.
		/// </summary>
		protected abstract void SetMedalType();

		/**
		 * implementation of https://drive.draw.io/?#G0B1Io_qu9i44ScEJqeUlLNEhaa28
		 */

		/// <summary>
		///     The adjust company score weight.
		/// </summary>
		private void AdjustCompanyScoreWeight() {
			if (Results.BusinessScore <= 30)
				Results.BusinessScoreWeight = GetCompanyScoreWeightForLowScore();
		}// AdjustCompanyScoreWeight

		/// <summary>
		///     The adjust consumer score weight.
		/// </summary>
		private void AdjustConsumerScoreWeight() {
			if (Results.ConsumerScore <= 800)
				Results.ConsumerScoreWeight = GetConsumerScoreWeightForLowScore();
		}// AdjustConsumerScoreWeight

		/// <summary>
		///     The adjust sum of weights.
		/// </summary>
		private void AdjustSumOfWeights() {
			decimal sumOfWeights = Results.BusinessScoreWeight + Results.FreeCashFlowWeight
				+ Results.AnnualTurnoverWeight + Results.TangibleEquityWeight
				+ Results.BusinessSeniorityWeight + Results.ConsumerScoreWeight
				+ Results.NetWorthWeight + Results.MaritalStatusWeight
				+ Results.NumberOfStoresWeight + Results.PositiveFeedbacksWeight
				+ Results.EzbobSeniorityWeight + Results.NumOfLoansWeight
				+ Results.NumOfLateRepaymentsWeight + Results.NumOfEarlyRepaymentsWeight;

			if (sumOfWeights != 100) {
				decimal sumOfNonFixed = GetSumOfNonFixedWeights();
				decimal sumOfNonFixedDestination = sumOfNonFixed - sumOfWeights + 100;
				decimal ratioForDestination = sumOfNonFixedDestination / sumOfNonFixed;
				AdjustWeightsWithRatio(ratioForDestination);
			}// if
		}// AdjustSumOfWeights

		/// <summary>
		///     The calculate annual turnover grade.
		/// </summary>
		private void CalculateAnnualTurnoverGrade() {
			if (Results.AnnualTurnover < 30000)
				Results.AnnualTurnoverGrade = 0;
			else if (Results.AnnualTurnover < 100000)
				Results.AnnualTurnoverGrade = 1;
			else if (Results.AnnualTurnover < 200000)
				Results.AnnualTurnoverGrade = 2;
			else if (Results.AnnualTurnover < 400000)
				Results.AnnualTurnoverGrade = 3;
			else if (Results.AnnualTurnover < 800000)
				Results.AnnualTurnoverGrade = 4;
			else if (Results.AnnualTurnover < 2000000)
				Results.AnnualTurnoverGrade = 5;
			else
				Results.AnnualTurnoverGrade = 6;
		}// CalculateAnnualTurnoverGrade

		/// <summary>
		///     The calculate business score grade.
		/// </summary>
		private void CalculateBusinessScoreGrade() {
			if (Results.BusinessScore < 11)
				Results.BusinessScoreGrade = 0;
			else if (Results.BusinessScore < 21)
				Results.BusinessScoreGrade = 1;
			else if (Results.BusinessScore < 31)
				Results.BusinessScoreGrade = 2;
			else if (Results.BusinessScore < 41)
				Results.BusinessScoreGrade = 3;
			else if (Results.BusinessScore < 51)
				Results.BusinessScoreGrade = 4;
			else if (Results.BusinessScore < 61)
				Results.BusinessScoreGrade = 5;
			else if (Results.BusinessScore < 71)
				Results.BusinessScoreGrade = 6;
			else if (Results.BusinessScore < 81)
				Results.BusinessScoreGrade = 7;
			else if (Results.BusinessScore < 91)
				Results.BusinessScoreGrade = 8;
			else
				Results.BusinessScoreGrade = 9;
		}// CalculateBusinessScoreGrade

		/// <summary>
		///     The calculate business seniority grade.
		/// </summary>
		private void CalculateBusinessSeniorityGrade() {
			var dateOnlyCalculationTime = Results.CalculationTime.Date;
			if (!Results.BusinessSeniority.HasValue
				|| Results.BusinessSeniority.Value.AddYears(1) > dateOnlyCalculationTime)
				Results.BusinessSeniorityGrade = 0;
			else if (Results.BusinessSeniority.Value.AddYears(3) > dateOnlyCalculationTime)
				Results.BusinessSeniorityGrade = 1;
			else if (Results.BusinessSeniority.Value.AddYears(5) > dateOnlyCalculationTime)
				Results.BusinessSeniorityGrade = 2;
			else if (Results.BusinessSeniority.Value.AddYears(10) > dateOnlyCalculationTime)
				Results.BusinessSeniorityGrade = 3;
			else
				Results.BusinessSeniorityGrade = 4;
		}// CalculateBusinessSeniorityGrade

		/// <summary>
		///     The calculate consumer score grade.
		/// </summary>
		private void CalculateConsumerScoreGrade() {
			if (Results.ConsumerScore < 481)
				Results.ConsumerScoreGrade = 0;
			else if (Results.ConsumerScore < 561)
				Results.ConsumerScoreGrade = 1;
			else if (Results.ConsumerScore < 641)
				Results.ConsumerScoreGrade = 2;
			else if (Results.ConsumerScore < 721)
				Results.ConsumerScoreGrade = 3;
			else if (Results.ConsumerScore < 801)
				Results.ConsumerScoreGrade = 4;
			else if (Results.ConsumerScore < 881)
				Results.ConsumerScoreGrade = 5;
			else if (Results.ConsumerScore < 961)
				Results.ConsumerScoreGrade = 6;
			else if (Results.ConsumerScore < 1041)
				Results.ConsumerScoreGrade = 7;
			else
				Results.ConsumerScoreGrade = 8;
		}// CalculateConsumerScoreGrade

		/// <summary>
		///     The calculate customer score.
		/// </summary>
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
			Results.NumOfLateRepaymentsScore = Results.NumOfLateRepaymentsWeight
				* Results.NumOfLateRepaymentsGrade;
			Results.NumOfEarlyRepaymentsScore = Results.NumOfEarlyRepaymentsWeight
				* Results.NumOfEarlyRepaymentsGrade;

			Results.TotalScore = Results.AnnualTurnoverScore + Results.BusinessScoreScore
				+ Results.FreeCashFlowScore + Results.TangibleEquityScore
				+ Results.BusinessSeniorityScore + Results.ConsumerScoreScore
				+ Results.NetWorthScore + Results.MaritalStatusScore
				+ Results.NumberOfStoresScore + Results.PositiveFeedbacksScore
				+ Results.EzbobSeniorityScore + Results.NumOfLoansScore
				+ Results.NumOfLateRepaymentsScore + Results.NumOfEarlyRepaymentsScore;
		}// CalculateCustomerScore

		/// <summary>
		///     The calculate ezbob seniority grade.
		/// </summary>
		private void CalculateEzbobSeniorityGrade() {
			if (!Results.EzbobSeniority.HasValue)
				Results.EzbobSeniority = Results.CalculationTime;

			decimal ezbobSeniorityMonths =
                (decimal)(Results.CalculationTime - Results.EzbobSeniority.Value).TotalDays / (365.0M / 12.0M);
			if (ezbobSeniorityMonths < 1)
				Results.EzbobSeniorityGrade = 0;
			else if (ezbobSeniorityMonths < 6)
				Results.EzbobSeniorityGrade = 2;
			else if (ezbobSeniorityMonths < 18)
				Results.EzbobSeniorityGrade = 3;
			else
				Results.EzbobSeniorityGrade = 4;
		}// CalculateEzbobSeniorityGrade

		/// <summary>
		///     The calculate feedbacks.
		/// </summary>
		private void CalculateFeedbacks() {
			Results.PositiveFeedbacks = Results.AmazonPositiveFeedbacks + Results.EbayPositiveFeedbacks;
			if (Results.PositiveFeedbacks == 0) {
				Results.PositiveFeedbacks = Results.NumberOfPaypalPositiveTransactions != 0
					? Results.NumberOfPaypalPositiveTransactions
					: CurrentValues.Instance.DefaultFeedbackValue;
			}
		}// CalculateFeedbacks

		/// <summary>
		///     The calculate free cash flow grade.
		/// </summary>
		private void CalculateFreeCashFlowGrade() {
			if (Results.FreeCashFlow < -0.1m || Results.AnnualTurnover <= 0) {
				// When turnover is zero we can't calc FCF, we want the min grade
				Results.FreeCashFlowGrade = 0;
			} else if (Results.FreeCashFlow < 0)
				Results.FreeCashFlowGrade = 1;
			else if (Results.FreeCashFlow < 0.1m)
				Results.FreeCashFlowGrade = 2;
			else if (Results.FreeCashFlow < 0.2m)
				Results.FreeCashFlowGrade = 3;
			else if (Results.FreeCashFlow < 0.3m)
				Results.FreeCashFlowGrade = 4;
			else if (Results.FreeCashFlow < 0.4m)
				Results.FreeCashFlowGrade = 5;
			else
				Results.FreeCashFlowGrade = 6;
		}// CalculateFreeCashFlowGrade

		/// <summary>
		///     The calculate grades.
		/// </summary>
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
		}// CalculateGrades

		/// <summary>
		///     The calculate marital status grade.
		/// </summary>
		private void CalculateMaritalStatusGrade() {
			if (Results.MaritalStatus == MaritalStatus.Married || Results.MaritalStatus == MaritalStatus.Widowed)
				Results.MaritalStatusGrade = 4;
			else if (Results.MaritalStatus == MaritalStatus.Divorced
				|| Results.MaritalStatus == MaritalStatus.LivingTogether)
				Results.MaritalStatusGrade = 3;
			else {
				// Single, Separated, Other
				Results.MaritalStatusGrade = 2;
			}
		}// CalculateMaritalStatusGrade

		/// <summary>
		///     The calculate medal.
		/// </summary>
		private void CalculateMedal() {
			if (Results.TotalScoreNormalized <= 0.4m)
				Results.MedalClassification = Medal.Silver;
			else if (Results.TotalScoreNormalized <= 0.62m)
				Results.MedalClassification = Medal.Gold;
			else if (Results.TotalScoreNormalized <= 0.84m)
				Results.MedalClassification = Medal.Platinum;
			else
				Results.MedalClassification = Medal.Diamond;
		}// CalculateMedal

		/// <summary>
		///     The calculate net worth.
		/// </summary>
		private void CalculateNetWorth() {
			if (Results.ZooplaValue != 0)
				Results.NetWorth = (Results.ZooplaValue - Results.MortgageBalance) / Results.ZooplaValue;
			else
				Results.NetWorth = 0;
		}// CalculateNetWorth

		/// <summary>
		///     The calculate net worth grade.
		/// </summary>
		private void CalculateNetWorthGrade() {
			if (Results.NetWorth < 0.15m)
				Results.NetWorthGrade = 0;
			else if (Results.NetWorth < 0.5m)
				Results.NetWorthGrade = 1;
			else if (Results.NetWorth < 1)
				Results.NetWorthGrade = 2;
			else {
				// We know that we sometimes miss mortgages the customer has, so instead of grade=3 we give 1
				Results.NetWorthGrade = 1;
			}// if
		}// CalculateNetWorthGrade

		/// <summary>
		///     The calculate number of stores grade.
		/// </summary>
		private void CalculateNumberOfStoresGrade() {
			if (Results.NumberOfStores < 3)
				Results.NumberOfStoresGrade = 1;
			else if (Results.NumberOfStores < 5)
				Results.NumberOfStoresGrade = 3;
			else
				Results.NumberOfStoresGrade = 5;
		}// CalculateNumberOfStoresGrade

		/// <summary>
		///     The calculate num of early repayments grade.
		/// </summary>
		private void CalculateNumOfEarlyRepaymentsGrade() {
			if (Results.NumOfEarlyRepayments == 0)
				Results.NumOfEarlyRepaymentsGrade = 2;
			else if (Results.NumOfEarlyRepayments < 4)
				Results.NumOfEarlyRepaymentsGrade = 3;
			else
				Results.NumOfEarlyRepaymentsGrade = 5;
		}// CalculateNumOfEarlyRepaymentsGrade

		/// <summary>
		///     The calculate num of late repayments grade.
		/// </summary>
		private void CalculateNumOfLateRepaymentsGrade() {
			if (Results.NumOfLateRepayments == 0)
				Results.NumOfLateRepaymentsGrade = 5;
			else if (Results.NumOfLateRepayments == 1)
				Results.NumOfLateRepaymentsGrade = 2;
			else
				Results.NumOfLateRepaymentsGrade = 0;
		}// CalculateNumOfLateRepaymentsGrade

		/// <summary>
		///     The calculate num of loans grade.
		/// </summary>
		private void CalculateNumOfLoansGrade() {
			if (Results.NumOfLoans > 3)
				Results.NumOfLoansGrade = 4;
			else if (Results.NumOfLoans > 1)
				Results.NumOfLoansGrade = 3;
			else
				Results.NumOfLoansGrade = 1;
		}// CalculateNumOfLoansGrade

		/// <summary>
		///     The calculate offer.
		/// </summary>
		private void CalculateOffer() {
			if (Results == null || !string.IsNullOrEmpty(Results.Error) || Results.MedalType == MedalType.NoMedal)
				return;

			SafeReader sr = this.db.GetFirst(
				"GetMedalCoefficients",
				CommandSpecies.StoredProcedure,
				new QueryParameter("MedalClassification", Results.MedalClassification.ToString())
			);

			if (sr.IsEmpty) {
				this.log.Warn("EXEC GetMedalCoefficients({0}) returned no coefficients", Results.MedalClassification);
				return;
			} // if

			decimal annualTurnoverMedalFactor = sr["AnnualTurnover"];
			decimal freeCashFlowMedalFactor = sr["FreeCashFlow"];
			decimal valueAddedMedalFactor = sr["ValueAdded"];

			decimal offerAccordingToAnnualTurnover = Results.AnnualTurnover * annualTurnoverMedalFactor / 100;
			decimal offerAccordingToFreeCashFlow = Results.FreeCashFlowValue * freeCashFlowMedalFactor / 100;
			decimal offerAccordingToValueAdded = Results.ValueAdded * valueAddedMedalFactor / 100;

			if (IsOnlineMedalNotViaHmrcInnerFlow())
				offerAccordingToValueAdded = 0;

			decimal[] allOfferAmounts = new[] {
				offerAccordingToAnnualTurnover,
				offerAccordingToFreeCashFlow,
				offerAccordingToValueAdded
			};

			List<decimal> validOfferAmounts = allOfferAmounts.Where(x => x >= CurrentValues.Instance.MedalMinOffer).ToList();

			this.log.Debug("All   offer amounts: {0}", string.Join(", ", allOfferAmounts));
			this.log.Debug("Valid offer amounts: {0}", string.Join(", ", validOfferAmounts));

			if (validOfferAmounts.Count > 0) {
				decimal unroundedValue = validOfferAmounts.Min();
				Results.OfferedLoanAmount = (int)unroundedValue;
				this.log.Debug("Offered loan amount calculated to be {0}.", Results.OfferedLoanAmount);
			} else
				this.log.Debug("All the offer amounts are not valid.");
		} // CalculateOffer

		/// <summary>
		///     The calculate positive feedbacks grade.
		/// </summary>
		private void CalculatePositiveFeedbacksGrade() {
			if (Results.PositiveFeedbacks < 1)
				Results.PositiveFeedbacksGrade = 0;
			else if (Results.PositiveFeedbacks < 5001)
				Results.PositiveFeedbacksGrade = 2;
			else if (Results.PositiveFeedbacks < 50000)
				Results.PositiveFeedbacksGrade = 3;
			else
				Results.PositiveFeedbacksGrade = 5;
		}// CalculatePositiveFeedbacksGrade

		/// <summary>
		///     The calculate ratios of annual turnover.
		/// </summary>
		private void CalculateRatiosOfAnnualTurnover() {
			if (Results.AnnualTurnover > 0) {
				Results.TangibleEquity = Results.TangibleEquityValue / Results.AnnualTurnover;
				Results.FreeCashFlow = Results.FreeCashFlowValue / Results.AnnualTurnover;
			} else {
				Results.TangibleEquity = 0;
				Results.AnnualTurnover = 0;
				Results.FreeCashFlow = 0;
			}// if
		}// CalculateRatiosOfAnnualTurnover

		/// <summary>
		///     The calculate score max.
		/// </summary>
		/// <returns>The <see cref="decimal" />.</returns>
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
			decimal ezbobNumOfEarlyRepaymentsScoreMax = Results.NumOfEarlyRepaymentsWeight
				* numOfEarlyRepaymentsMaxGrade;

			return annualTurnoverScoreMax + businessScoreScoreMax + freeCashFlowScoreMax + tangibleEquityScoreMax
				+ businessSeniorityScoreMax + consumerScoreScoreMax + netWorthScoreMax + maritalStatusScoreMax
				+ numberOfStoresMax + positiveFeedbacksScoreMax + ezbobSeniorityScoreMax + ezbobNumOfLoansScoreMax
				+ ezbobNumOfLateRepaymentsScoreMax + ezbobNumOfEarlyRepaymentsScoreMax;
		}// CalculateScoreMax

		/// <summary>
		///     The calculate score min.
		/// </summary>
		/// <returns>The <see cref="decimal" />.</returns>
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

			return annualTurnoverScoreMin + businessScoreScoreMin + freeCashFlowScoreMin + tangibleEquityScoreMin
				+ businessSeniorityScoreMin + consumerScoreScoreMin + netWorthScoreMin + maritalStatusScoreMin
				+ numberOfStoresMin + positiveFeedbacksScoreMin + ezbobSeniorityScoreMin + ezbobNumOfLoansScoreMin
				+ ezbobNumOfLateRepaymentsScoreMin + ezbobNumOfEarlyRepaymentsScoreMin;
		}// CalculateScoreMin

		/// <summary>
		///     The calculate tangible equity grade.
		/// </summary>
		private void CalculateTangibleEquityGrade() {
			if (Results.TangibleEquity < -0.05m || Results.AnnualTurnover <= 0) {
				// When turnover is zero we can't calc tangible equity, we want the min grade
				Results.TangibleEquityGrade = 0;
			} else if (Results.TangibleEquity < 0)
				Results.TangibleEquityGrade = 1;
			else if (Results.TangibleEquity < 0.1m)
				Results.TangibleEquityGrade = 2;
			else if (Results.TangibleEquity < 0.3m)
				Results.TangibleEquityGrade = 3;
			else
				Results.TangibleEquityGrade = 4;
		} // CalculateTangibleEquityGrade

		/// <summary>
		///     The get mortgages.
		/// </summary>
		/// <param name="customerId">The customer id.</param>
		/// <returns>The <see cref="decimal" />.</returns>
		private decimal GetMortgages(int customerId) {
			var instance = new LoadExperianConsumerMortgageData(customerId);
			instance.Execute();

			return instance.Result.MortgageBalance;
		} // GetMortgages

		/// <summary>
		///     The is online medal not via hmrc inner flow.
		/// </summary>
		/// <returns>The <see cref="bool" />.</returns>
		private bool IsOnlineMedalNotViaHmrcInnerFlow() {
			return Results.MedalType.IsOnline() && Results.TurnoverType != TurnoverType.HMRC;
		} // IsOnlineMedalNotViaHmrcInnerFlow

		/// <summary>
		///     Turnover for Medal (https://drive.draw.io/?#G0B1Io_qu9i44ScEJqeUlLNEhaa28)
		/// </summary>
		protected virtual void CalculateTurnoverForMedal() {
			try {
				List<TurnoverDbRow> turnovers = new List<TurnoverDbRow>();
				Results.AnnualTurnover = 0;
				Results.HmrcAnnualTurnover = 0;
				Results.BankAnnualTurnover = 0;
				Results.OnlineAnnualTurnover = 0;

				// get monthly aggregated turnovers for the customer for all his market places. 
				// "IsForApprove" parameter: for  medal and for approval use true.
				this.db.ForEachResult<TurnoverDbRow>(
					r => turnovers.Add(r),
					"GetCustomerTurnoverForAutoDecision",
					new QueryParameter("IsForApprove", true),
					new QueryParameter("CustomerID", Results.CustomerId),
					new QueryParameter("Now", Results.CalculationTime)
				);

				// extract hmrc data only
				var hmrcList = (
					from TurnoverDbRow r in turnovers
					where r.MpTypeID.Equals(TurnoverDbRow.hmrc)
					select r
				).AsQueryable();

				// get hmrc turnover for all months received
				Results.HmrcAnnualTurnover = hmrcList.Sum(t => t.Turnover);
				Results.HmrcAnnualTurnover = (Results.HmrcAnnualTurnover < 0) ? 0 : Results.HmrcAnnualTurnover;

				// this is non-online medal type and has a hmrc
				if (!Results.MedalType.IsOnline() && Results.NumOfHmrcMps > 0) {
					Results.TurnoverType = TurnoverType.HMRC;
					Results.AnnualTurnover = Results.HmrcAnnualTurnover;
					return;
				} // if

				// extract yodlee data only
				var yodleeList = (
					from TurnoverDbRow r in turnovers
					where r.MpTypeID.Equals(TurnoverDbRow.yodlee)
					select r
				).AsQueryable();

				// get yoodlee turnover for all months received
				Results.BankAnnualTurnover = yodleeList.Sum(t => t.Turnover);
				Results.BankAnnualTurnover = (Results.BankAnnualTurnover < 0) ? 0 : Results.BankAnnualTurnover;

				// this is non-online medal type
				if (!Results.MedalType.IsOnline()) {
					// and has a yodlee (bank) data
					if (Results.NumOfBanks > 0) {
						Results.TurnoverType = TurnoverType.Bank;
						Results.AnnualTurnover = Results.BankAnnualTurnover;
						return;
					} // if

					// no turnover
					return;
				} // if

				// Continue for "Online" medal type

				// Calculate for "last month", "last three months", "last six months", and "last twelve months". 
				// Annualize the figures and take the minimum among them. 
				// If a figure is zero it does NOT participate in minimum calculation. 

				const int T1 = 1;
				const int T3 = 3;
				const int T6 = 6;
				const int T12 = 12;

				const int Ec1 = 12;
				const int Ec3 = 4;
				const int Ec6 = 2;
				const int Ec12 = 1;

				List<decimal> list_t1 = new List<decimal>();
				List<decimal> list_t3 = new List<decimal>();
				List<decimal> list_t6 = new List<decimal>();
				List<decimal> list_t12 = new List<decimal>();

				// extact amazon data
				var amazonList = (
					from TurnoverDbRow r in turnovers
					where r.MpTypeID.Equals(TurnoverDbRow.amazon)
					select r
				).AsQueryable();

				// calculate "last month", "last three months", "last six months", and "last twelve months"/annualize for amazon
				list_t1.Add(CalcAnnualTurnoverBasedOnPartialData(amazonList, T1, Ec1));
				list_t3.Add(CalcAnnualTurnoverBasedOnPartialData(amazonList, T3, Ec3));
				list_t6.Add(CalcAnnualTurnoverBasedOnPartialData(amazonList, T6, Ec6));
				list_t12.Add(CalcAnnualTurnoverBasedOnPartialData(amazonList, T12, Ec12));

				// extact ebay data
				var ebayList = (
					from TurnoverDbRow r in turnovers
					where r.MpTypeID.Equals(TurnoverDbRow.ebay)
					select r
				).AsQueryable();

				// calculate "last month", "last three months", "last six months", and "last twelve months"/annualize for ebay
				list_t1.Add(CalcAnnualTurnoverBasedOnPartialData(ebayList, T1, Ec1));
				list_t3.Add(CalcAnnualTurnoverBasedOnPartialData(ebayList, T3, Ec3));
				list_t6.Add(CalcAnnualTurnoverBasedOnPartialData(ebayList, T6, Ec6));
				list_t12.Add(CalcAnnualTurnoverBasedOnPartialData(ebayList, T12, Ec12));

				// extact paypal data
				var paypalList = (
					from TurnoverDbRow r in turnovers
					where r.MpTypeID.Equals(TurnoverDbRow.paypal)
					select r
				).AsQueryable();

				// calculate "last month", "last three months", "last six months", and "last twelve months"/annualize for paypal
				list_t1.Add(CalcAnnualTurnoverBasedOnPartialData(paypalList, T1, Ec1));
				list_t3.Add(CalcAnnualTurnoverBasedOnPartialData(paypalList, T3, Ec3));
				list_t6.Add(CalcAnnualTurnoverBasedOnPartialData(paypalList, T6, Ec6));
				list_t12.Add(CalcAnnualTurnoverBasedOnPartialData(paypalList, T12, Ec12));

				// Online turnover: Amazon + MAX(eBay, Pay Pal)

				// amazon: index 0
				// ebay: index 1
				// paypal: index 2

				Results.OnlineAnnualTurnover = Min(
					list_t1.ElementAt(0) + Math.Max(list_t1.ElementAt(1), list_t1.ElementAt(2)),
					list_t3.ElementAt(0) + Math.Max(list_t3.ElementAt(1), list_t3.ElementAt(2)),
					list_t6.ElementAt(0) + Math.Max(list_t6.ElementAt(1), list_t6.ElementAt(2)),
					list_t12.ElementAt(0) + Math.Max(list_t12.ElementAt(1), list_t12.ElementAt(2))
				);

				decimal onlineMedalTurnoverCutoff = CurrentValues.Instance.OnlineMedalTurnoverCutoff;

				if (Results.HmrcAnnualTurnover > onlineMedalTurnoverCutoff * Results.OnlineAnnualTurnover) {
					Results.TurnoverType = TurnoverType.HMRC;
					Results.AnnualTurnover = Results.HmrcAnnualTurnover;
					return;
				} // if

				if (Results.BankAnnualTurnover > onlineMedalTurnoverCutoff * Results.OnlineAnnualTurnover) {
					Results.TurnoverType = TurnoverType.Bank;
					Results.AnnualTurnover = Results.BankAnnualTurnover;
					return;
				} // if

				Results.TurnoverType = TurnoverType.Online;
				Results.AnnualTurnover = Results.OnlineAnnualTurnover;
			} catch (Exception ex) {
				this.log.Error(
					ex,
					"Failed to get|calculate annual turnover for medal. customerID: {0}, calculationTime: {1}, IsForApprove: true",
					Results.CustomerId,
					Results.CalculationTime
				);
			} // try
		} // CalculateTurnoverForMedal

		private static decimal Min(params decimal[] args) {
			decimal? result = null;

			foreach (decimal x in args) {
				if (x < 0.000000001m)
					continue;

				if (result == null) {
					result = x;
					continue;
				} // if

				if (x < result)
					result = x;
			} // for each

			decimal output = result ?? 0;

			return output <= 0 ? 0 : output;
		} // Min

		/// <summary>
		///     Calculates for "last month", "last three months", "last six months", and "last twelve months".
		///     Annualize the figures and take the minimum among them.
		/// </summary>
		/// <param name="list"></param>
		/// <param name="monthAfter"></param>
		/// <param name="extrapolationCoefficient"></param>
		/// <returns></returns>
		protected virtual decimal CalcAnnualTurnoverBasedOnPartialData(
			IQueryable<TurnoverDbRow> list,
			int monthAfter,
			int extrapolationCoefficient
		) {
			return list.Where(t => (t.Distance < monthAfter)).Sum(t => t.Turnover) * extrapolationCoefficient;
		} // CalcAnnualTurnoverBasedOnPartialData

	} // class MedalCalculatorBase
} // namespace

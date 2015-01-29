namespace Ezbob.Backend.Strategies.MedalCalculations {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using ConfigManager;
	using Ezbob.Backend.Strategies.Experian;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using StructureMap;

	/// <summary>
	/// The medal calculator base.
	/// Medal type: https://drive.draw.io/?#G0B1Io_qu9i44SVzVqV19nbnMxRW8
	/// Medal type and medal value: https://drive.draw.io/?#G0B1Io_qu9i44ScEJqeUlLNEhaa28
	/// </summary>
	public abstract class MedalCalculatorBase {
		/// <summary>
		///     The set initial weights.
		/// </summary>
		public abstract void SetInitialWeights();

		/// <summary>
		///     Gets or sets the results.
		/// </summary>
		public MedalResult Results { get; set; }

		public void Init(
			int customerId,
			DateTime calculationTime,
			int consumerScore,
			int businessScore,
			int hmrcCount,
			int bankCount,
			int onlineCount,
			DateTime? earliestHmrcLastUpdateDate,
			DateTime? earliestYodleeLastUpdateDate
		) {
			Results = new MedalResult(customerId, this.log) {
				CalculationTime = calculationTime,

				BusinessScore = businessScore,
				ConsumerScore = consumerScore,

				NumOfHmrcMps = hmrcCount,
				NumOfBanks = bankCount,
				NumberOfStores = onlineCount,

				EarliestHmrcLastUpdateDate = earliestHmrcLastUpdateDate,
				EarliestYodleeLastUpdateDate = earliestYodleeLastUpdateDate,
			};

			this.isInitialized = true;
		} // Init

		/// <summary>
		///     The calculate medal score.
		/// </summary>
		/// <returns>The <see cref="MedalResult" />.</returns>
		public MedalResult CalculateMedalScore() {
			if (this.isCalculated)
				return Results;

			if (!this.isInitialized)
				throw new Exception("Medal calculator should be initialized first.");

			this.isCalculated = true;

			// set medal type here per each extending class
			SetMedalType();

			try {
				GatherInputData();

				// Process raw input data to data
				CalculateFeedbacks();

				CalculateTurnoverForMedal();

				this.log.Debug(
					"Turnover for customer {5} on {6}: type {0}, final {1}, HMRC {2}, bank {3}, online {4}.",
					Results.TurnoverType,
					Results.AnnualTurnover,
					Results.HmrcAnnualTurnover,
					Results.BankAnnualTurnover,
					Results.OnlineAnnualTurnover,
					Results.CustomerId,
					Results.CalculationTime.ToString("MMM d yyyy H:mm:ss", CultureInfo.InvariantCulture)
				);

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
				} // if

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
					Results.CustomerId);
				Results.Error = e.Message;
			} // try

			return Results;
		} // CalculateMedalScore

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
			this.isInitialized = false;
			this.isCalculated = false;

			this.log = Library.Instance.Log;
			this.db = Library.Instance.DB;
		} // constructor

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

			sr.Fill(Results);

			Results.FreeCashFlowValue = 0;
			Results.ValueAdded = 0;

			decimal newActualLoansRepayment = 0;

			this.db.ForEachRowSafe(
				srfv => {
					RowType rt;

					if (!Enum.TryParse(srfv["RowType"], out rt)) {
						log.Alert("MedalCalculatorBase.GatherInputData: Cannot parse row type from {0}", srfv["RowType"]);
						return;
					} // if

					switch (rt) {
					case RowType.FcfValueAdded:
						Results.FreeCashFlowValue += srfv["FreeCashFlow"];
						Results.ValueAdded += srfv["ValueAdded"];
						break;

					case RowType.NewActualLoansRepayment:
						newActualLoansRepayment = srfv["NewActualLoansRepayment"];
						break;

					default:
						throw new ArgumentOutOfRangeException();
					} // switch
				},
				"GetCustomerAnnualFcfValueAdded",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", Results.CustomerId),
				new QueryParameter("Now", Results.CalculationTime)
			);

			Results.FreeCashFlowValue -= newActualLoansRepayment;

			Results.MortgageBalance = GetMortgages(Results.CustomerId);
		} // GatherInputData

		private enum RowType {
			NewActualLoansRepayment,
			FcfValueAdded,
		} // enum RowType

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
		} // CalculateNumOfLoansGrade

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
				Results.FreeCashFlow = 0;
			} // if
		} // CalculateRatiosOfAnnualTurnover

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
		/// Turnover for medal: https://drive.draw.io/?#G0B1Io_qu9i44ScEJqeUlLNEhaa28
		/// </summary>
		protected virtual void CalculateTurnoverForMedal() {
			this.updatingHistoryRep = ObjectFactory.GetInstance<CustomerMarketPlaceUpdatingHistoryRepository>();
			try {
				Results.AnnualTurnover = 0;
				Results.HmrcAnnualTurnover = 0;
				Results.BankAnnualTurnover = 0;
				Results.OnlineAnnualTurnover = 0;

				var h = this.updatingHistoryRep.GetByCustomerId(Results.CustomerId).Where(x => x.UpdatingEnd < Results.CalculationTime && (x.Error == null || x.Error.Trim().Length == 0));

				if (h.Equals(null)) {
					this.log.Info("Updating history for customer {0}, calculationDate {1} not found", Results.CustomerId, Results.CalculationTime);
					return;
				} // if

				var hmrcs = from row in h.SelectMany(y => y.HmrcAggregations)
							select new MarketplaceTurnover {
								TheMonth = row.TheMonth,
								Turnover = row.Turnover,
								CustomerMarketPlaceUpdatingHistory = row.CustomerMarketPlaceUpdatingHistory
							};

				if (!hmrcs.Equals(null)) {

					var hmrcList = this.LastHistoryTurnovers(hmrcs.ToList(), Results.CalculationTime);

					if (hmrcList != null) {
						// get hmrc turnover for all months received
						Results.HmrcAnnualTurnover = hmrcList.Sum(t => t.Turnover);
						Results.HmrcAnnualTurnover = (Results.HmrcAnnualTurnover < 0) ? 0 : Results.HmrcAnnualTurnover;
					}

					// this is non-online medal type and has a hmrc
					if (!Results.MedalType.IsOnline() && Results.NumOfHmrcMps > 0) {
						Results.TurnoverType = TurnoverType.HMRC;
						Results.AnnualTurnover = Results.HmrcAnnualTurnover;

						this.log.Info("Base: (HMRC) AnnualTurnover: {0}, HmrcAnnualTurnover: {1}, BankAnnualTurnover: {2}, OnlineAnnualTurnover: {3}, Type: {4}", Results.AnnualTurnover, Results.HmrcAnnualTurnover, Results.BankAnnualTurnover, Results.OnlineAnnualTurnover, Results.TurnoverType);

						return;
					} // if
				}

				var yodlees = from row in h.SelectMany(y => y.YodleeAggregations)
							  select new MarketplaceTurnover {
								  TheMonth = row.TheMonth,
								  Turnover = row.Turnover,
								  CustomerMarketPlaceUpdatingHistory = row.CustomerMarketPlaceUpdatingHistory
							  };

				if (!yodlees.Equals(null)) {

					var yodleeList = this.LastHistoryTurnovers(yodlees.ToList(), Results.CalculationTime);

					if (yodleeList != null) {
						// get yoodlee turnover for all months received
						Results.BankAnnualTurnover = yodleeList.Sum(t => t.Turnover);
						Results.BankAnnualTurnover = (Results.BankAnnualTurnover < 0) ? 0 : Results.BankAnnualTurnover;
					}
				}

				// this is non-online medal type
				if (!Results.MedalType.IsOnline()) {
					// and has a yodlee (bank) data
					if (Results.NumOfBanks > 0) {
						Results.TurnoverType = TurnoverType.Bank;
						Results.AnnualTurnover = Results.BankAnnualTurnover;

						this.log.Info("Base: (bank) AnnualTurnover: {0}, HmrcAnnualTurnover: {1}, BankAnnualTurnover: {2}, OnlineAnnualTurnover: {3}, Type: {4}", Results.AnnualTurnover, Results.HmrcAnnualTurnover, Results.BankAnnualTurnover, Results.OnlineAnnualTurnover, Results.TurnoverType);

						return;
					} // if

					// no turnover
					return;
				} // if

				// Continue to online

				// Calculate for "last month", "last three months", "last six months", and "last twelve months". 
				// Annualize the figures and take the minimum among them. 
				// If a figure is zero it does NOT participate in minimum calculation.

				DateTime monthStart = new DateTime(Results.CalculationTime.Year, Results.CalculationTime.Month, 1, 0, 0, 0);

				DateTime T1 = monthStart.AddMonths(-1);
				DateTime T3 = monthStart.AddMonths(-3);
				DateTime T6 = monthStart.AddMonths(-6);

				//this.log.Info("t1: {0}, t3: {1}, t6: {2},", T1, T3, T6);
				const int Ec1 = 12;
				const int Ec3 = 4;
				const int Ec6 = 2;

				decimal[] filltt = { 0, 0, 0, 0 };
				List<decimal> list_t1 = new List<decimal>(filltt);
				List<decimal> list_t3 = new List<decimal>(filltt);
				List<decimal> list_t6 = new List<decimal>(filltt);
				List<decimal> list_t12 = new List<decimal>(filltt);

				var amazons = from row in h.SelectMany(y => y.AmazonAggregations)
							  select new MarketplaceTurnover {
								  TheMonth = row.TheMonth,
								  Turnover = row.Turnover,
								  CustomerMarketPlaceUpdatingHistory = row.CustomerMarketPlaceUpdatingHistory
							  };

				var amazonList = this.LastHistoryTurnovers(amazons.ToList(), Results.CalculationTime);

				if (amazonList != null) {
					// Amazon: calculate "last month", "last three months", "last six months", and "last twelve months"/annualize 
					list_t1.Insert(0, amazonList.Where(t => t.TheMonth >= T1)
						.Sum(t => t.Turnover) * Ec1);
					list_t3.Insert(0, amazonList.Where(t => t.TheMonth >= T3)
						.Sum(t => t.Turnover) * Ec3);
					list_t6.Insert(0, amazonList.Where(t => t.TheMonth >= T6)
						.Sum(t => t.Turnover) * Ec6);
					list_t12.Insert(0, amazonList.Sum(t => t.Turnover));

					//			log.Info(" amazon TT: t1: {0}, t3: {1}, t6: {2}, t12: {3}", list_t1.ElementAtOrDefault(0), list_t3.ElementAtOrDefault(0), list_t6.ElementAtOrDefault(0), list_t12.ElementAtOrDefault(0));

				}

				var ebays = from row in h.SelectMany(y => y.EbayAggregations)
							select new MarketplaceTurnover {
								TheMonth = row.TheMonth,
								Turnover = row.Turnover,
								CustomerMarketPlaceUpdatingHistory = row.CustomerMarketPlaceUpdatingHistory
							};

				var ebayList = this.LastHistoryTurnovers(ebays.ToList(), Results.CalculationTime);

				if (ebayList != null) {
					// Ebay: calculate "last month", "last three months", "last six months", and "last twelve months"/annualize 
					list_t1.Insert(1, ebayList.Where(t => t.TheMonth >= T1)
						.Sum(t => t.Turnover) * Ec1);
					list_t3.Insert(1, ebayList.Where(t => t.TheMonth >= T3)
						.Sum(t => t.Turnover) * Ec3);
					list_t6.Insert(1, ebayList.Where(t => t.TheMonth >= T6)
						.Sum(t => t.Turnover) * Ec6);
					list_t12.Insert(1, ebayList.Sum(t => t.Turnover));

					//			log.Info(" ebay TT: t1: {0}, t3: {1}, t6: {2}, t12: {3}", list_t1.ElementAtOrDefault(1), list_t3.ElementAtOrDefault(1), list_t6.ElementAtOrDefault(1), list_t12.ElementAtOrDefault(1));

				}

				var paypals = from row in h.SelectMany(y => y.PayPalAggregations)
							  select new MarketplaceTurnover {
								  TheMonth = row.TheMonth,
								  Turnover = row.Turnover,
								  CustomerMarketPlaceUpdatingHistory = row.CustomerMarketPlaceUpdatingHistory
							  };

				var paypalList = this.LastHistoryTurnovers(paypals.ToList(), Results.CalculationTime);

				if (paypalList != null) {
					// Paypal: calculate "last month", "last three months", "last six months", and "last twelve months"/annualize
					list_t1.Insert(2, paypalList.Where(t => t.TheMonth >= T1)
						.Sum(t => t.Turnover) * Ec1);
					list_t3.Insert(2, paypalList.Where(t => t.TheMonth >= T3)
						.Sum(t => t.Turnover) * Ec3);
					list_t6.Insert(2, paypalList.Where(t => t.TheMonth >= T6)
						.Sum(t => t.Turnover) * Ec6);
					list_t12.Insert(2, paypalList.Sum(t => t.Turnover));

					//				log.Info(" paypals TT: t1: {0}, t3: {1}, t6: {2}, t12: {3}", list_t1.ElementAtOrDefault(2), list_t3.ElementAtOrDefault(2), list_t6.ElementAtOrDefault(2), list_t12.ElementAtOrDefault(2));
				}
				// Online turnover: Amazon + MAX(eBay, Pay Pal)

				// amazon: index 0
				// ebay: index 1
				// paypal: index 2
				ArrayList onlineList = new ArrayList();

				onlineList.Add(list_t1.ElementAtOrDefault(0) + Math.Max(list_t1.ElementAtOrDefault(1), list_t1.ElementAtOrDefault(2)));
				onlineList.Add(list_t3.ElementAtOrDefault(0) + Math.Max(list_t3.ElementAtOrDefault(1), list_t3.ElementAtOrDefault(2)));
				onlineList.Add(list_t6.ElementAtOrDefault(0) + Math.Max(list_t6.ElementAtOrDefault(1), list_t6.ElementAtOrDefault(2)));
				onlineList.Add(list_t12.ElementAtOrDefault(0) + Math.Max(list_t12.ElementAtOrDefault(1), list_t12.ElementAtOrDefault(2)));

				Results.OnlineAnnualTurnover = (from decimal r in onlineList where r > 0 select r).AsQueryable().DefaultIfEmpty(0).Min();

				decimal onlineMedalTurnoverCutoff = CurrentValues.Instance.OnlineMedalTurnoverCutoff;

				if (Results.HmrcAnnualTurnover > onlineMedalTurnoverCutoff * Results.OnlineAnnualTurnover) {
					Results.TurnoverType = TurnoverType.HMRC;
					Results.AnnualTurnover = Results.HmrcAnnualTurnover;

					this.log.Info("Base: (HmrcAnnualTurnover-><-onlineMedalTurnoverCutoff): AnnualTurnover: {0}, HmrcAnnualTurnover: {1}, BankAnnualTurnover: {2}, OnlineAnnualTurnover: {3}, Type: {4}", Results.AnnualTurnover, Results.HmrcAnnualTurnover, Results.BankAnnualTurnover, Results.OnlineAnnualTurnover, Results.TurnoverType);

					return;
				}

				if (Results.BankAnnualTurnover > onlineMedalTurnoverCutoff * Results.OnlineAnnualTurnover) {
					Results.TurnoverType = TurnoverType.Bank;
					Results.AnnualTurnover = Results.BankAnnualTurnover;

					this.log.Info("Base: (BankAnnualTurnover-><-onlineMedalTurnoverCutoff): AnnualTurnover: {0}, HmrcAnnualTurnover: {1}, BankAnnualTurnover: {2}, OnlineAnnualTurnover: {3}, Type: {4}", Results.AnnualTurnover, Results.HmrcAnnualTurnover, Results.BankAnnualTurnover, Results.OnlineAnnualTurnover, Results.TurnoverType);

					return;
				} // if

				Results.TurnoverType = TurnoverType.Online;
				Results.AnnualTurnover = Results.OnlineAnnualTurnover;

				this.log.Info("Base: AnnualTurnover: {0}, HmrcAnnualTurnover: {1}, BankAnnualTurnover: {2}, OnlineAnnualTurnover: {3}, Type: {4}", Results.AnnualTurnover, Results.HmrcAnnualTurnover, Results.BankAnnualTurnover, Results.OnlineAnnualTurnover, Results.TurnoverType);

			} catch (Exception ex) {
				this.log.Error(
					ex,
					"Failed to get|calculate annual turnover for medal. customerID: {0}, calculationTime: {1}, IsForApprove: true",
					Results.CustomerId,
					Results.CalculationTime
				);
			} // try
		}

		/// <summary>
		///     The is online medal not via hmrc inner flow.
		/// </summary>
		/// <returns>The <see cref="bool" />.</returns>
		private bool IsOnlineMedalNotViaHmrcInnerFlow() {
			return Results.MedalType.IsOnline() && Results.TurnoverType != TurnoverType.HMRC;
		} // IsOnlineMedalNotViaHmrcInnerFlow

		public virtual IEnumerable<FilteredAggregationResult> LastHistoryTurnovers(List<MarketplaceTurnover> inputList, DateTime calculationTime) {

			if (inputList.Count == 0)
				return null;

			//		inputList.ForEach(x => this.log.Info("before: {0}, {1}, historyID: {2}, mpID: {3}", x.TheMonth, x.Turnover, x.CustomerMarketPlaceUpdatingHistory.Id, x.CustomerMarketPlaceUpdatingHistory.CustomerMarketPlace.Id));

			// check type
			var lastUpdated = inputList.OrderByDescending(z => z.CustomerMarketPlaceUpdatingHistory.Id).First();

			if (lastUpdated.Equals(null))
				return null;

			DateTime lastUpdateDate = (DateTime)lastUpdated.CustomerMarketPlaceUpdatingHistory.UpdatingEnd;
			DateTime periodStart = MiscUtils.getPeriodAgo(calculationTime, lastUpdateDate);
			DateTime periodEnd = periodStart.AddMonths(11);

			this.log.Info("calculationTime: {2}, lastUpdateDate: {1}, yearAgo: {0}, yearAgoEnd: {3}", periodStart, lastUpdateDate, calculationTime, periodEnd);

			var histories = inputList.Where(z => z.TheMonth >= periodStart && z.TheMonth <= periodEnd).ToList();

			//			histories.ForEach(x => this.log.Info(" relevant months: {0}, {1}, historyID: {2}, mpID: {3}", x.TheMonth, x.Turnover, x.CustomerMarketPlaceUpdatingHistory.Id, x.CustomerMarketPlaceUpdatingHistory.CustomerMarketPlace.Id));

			if (histories.Equals(null))
				return null;

			var result = from ag in histories
						 group ag by new { ag.CustomerMarketPlaceUpdatingHistory.CustomerMarketPlace.Id, ag.TheMonth } into grouping
						 select new FilteredAggregationResult {
							 TheMonth = grouping.First().TheMonth,
							 MpId = grouping.First().CustomerMarketPlaceUpdatingHistory.CustomerMarketPlace.Id,
							 Turnover = histories.Where(xx => xx.TheMonth == grouping.First().TheMonth && xx.CustomerMarketPlaceUpdatingHistory.Id == grouping.Max(p => p.CustomerMarketPlaceUpdatingHistory.Id)).First().Turnover
						 };

			//	result.ForEach(x => this.log.Info(" filtered: {0}, {1}, {2}", x.TheMonth, x.Turnover,  x.MpId));

			return result;
		} //LastHistoryTurnovers


		private EZBob.DatabaseLib.Model.Database.Repository.CustomerMarketPlaceUpdatingHistoryRepository updatingHistoryRep;

		private static DateTime GetPeriodAgo(DateTime calculationDate, DateTime lastUpdate) {
			int daysInMonth = DateTime.DaysInMonth(calculationDate.Year, calculationDate.Month);
			DateTime months = new DateTime();

			if ((daysInMonth - calculationDate.Date.Day) <= 3 && (daysInMonth - lastUpdate.Date.Day) <= 3 && (calculationDate.Month == lastUpdate.Month && calculationDate.Year == lastUpdate.Year)) {
				months = calculationDate.AddMonths(-11);
			} else
				months = calculationDate.AddMonths(-12);

			return new DateTime(months.Year, months.Month, 1, 0, 0, 0);
		} // GetPeriodAgo

		private bool isInitialized;
		private bool isCalculated;
	} // class MedalCalculatorBase
} // namespace
namespace Ezbob.Backend.Strategies.MedalCalculations.Primary {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using ConfigManager;
	using Ezbob.Backend.Extensions;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.Experian;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Logger;
	using Ezbob.Matrices;
	using Ezbob.Utils;

	/// <summary>
	/// The medal calculator base.
	/// Medal type: https://drive.draw.io/?#G0B1Io_qu9i44SVzVqV19nbnMxRW8
	/// Medal type and medal value: https://drive.draw.io/?#G0B1Io_qu9i44ScEJqeUlLNEhaa28
	/// </summary>
	public abstract class MedalBase {
		public abstract void SetInitialWeights();

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

		public MedalResult CalculateMedalScore() {
			if (this.isCalculated)
				return Results;

			if (!this.isInitialized)
				throw new Exception("Medal calculator should be initialized first.");

			this.isCalculated = true;

			SetMedalType();

			this.log.Debug(
				"Primary {0} medal calculator: medal type was set to '{1}'.",
				GetType().Name,
				Results.MedalType
			);

			try {
				GatherInputData();

				Results.CalculateFeedbacks(CurrentValues.Instance.DefaultFeedbackValue);

				CalculateTurnoverForMedal();

				if (Results.TurnoverType == null) {
					Results.AnnualTurnover = 0;
					Results.HmrcAnnualTurnover = 0;
					Results.BankAnnualTurnover = 0;
					Results.OnlineAnnualTurnover = 0;

					if (Results.MedalType.IsOnline())
						Results.TurnoverType = TurnoverType.Online;
					else {
						if (Results.NumOfHmrcMps > 0)
							Results.TurnoverType = TurnoverType.HMRC;
						else if (Results.NumOfBanks > 0)
							Results.TurnoverType = TurnoverType.Bank;
					} //if
				} // if

				this.log.Debug(
					"Primary {7} medal calculator: " +
					"turnover for customer {5} on {6}: type {0}, final {1}, HMRC {2}, bank {3}, online {4}.",
					Results.TurnoverType,
					Results.AnnualTurnover,
					Results.HmrcAnnualTurnover,
					Results.BankAnnualTurnover,
					Results.OnlineAnnualTurnover,
					Results.CustomerId,
					Results.CalculationTime.ToString("MMM d yyyy H:mm:ss", CultureInfo.InvariantCulture),
					GetType().Name
				);

				CalculateRatiosOfAnnualTurnover();
				CalculateNetWorth();

				SetInitialWeights();

				this.log.Debug(
					"Primary {0} medal calculator: initial weights are\n\t{1}.",
					GetType().Name,
					string.Join(",\n\t", Results.GetValuesByMaskForLog("Weight"))
				);

				AdjustCompanyScoreWeight();

				this.log.Debug(
					"Primary {0} medal calculator: after adjusting business score weight:\n\t{1}.",
					GetType().Name, 
					string.Join(",\n\t", Results.GetValuesByMaskForLog("Weight"))
				);

				AdjustConsumerScoreWeight();

				this.log.Debug(
					"Primary {0} medal calculator: after adjusting consumer score weights are:\n\t{1}.",
					GetType().Name, 
					string.Join(",\n\t", Results.GetValuesByMaskForLog("Weight"))
				);

				if (Results.TurnoverType != TurnoverType.HMRC) {
					RedistributeFreeCashFlowWeight();

					this.log.Debug(
						"Primary {0} medal calculator: turnover is not HMRC, after redistributing FCF weights are\n\t{1}.",
						GetType().Name,
						string.Join(",\n\t", Results.GetValuesByMaskForLog("Weight"))
					);
				} // if

				if (Results.FirstRepaymentDatePassed) {
					Results.EzbobSeniorityWeight = 2;
					Results.NumOfLoansWeight = 3.33m;
					Results.NumOfLateRepaymentsWeight = 2.67m;
					Results.NumOfEarlyRepaymentsWeight = 2;

					RedistributeWeightsForPayingCustomer();

					this.log.Debug(
						"Primary {0} medal calculator: first payment passed, redistributed weights are\n\t{1}.",
						GetType().Name,
						string.Join(",\n\t", Results.GetValuesByMaskForLog("Weight"))
					);
				} // if

				AdjustSumOfWeights();

				this.log.Debug(
					"Primary {0} medal calculator: after adjusting weights sum weights are\n\t{1}.",
					GetType().Name,
					string.Join(",\n\t", Results.GetValuesByMaskForLog("Weight"))
				);

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
					Results.CustomerId
				);
				Results.ExceptionDuringCalculation = e;
			} // try

			return Results;
		} // CalculateMedalScore

		protected readonly AConnection db;
		protected readonly ASafeLog log;

		protected MedalBase() {
			this.isInitialized = false;
			this.isCalculated = false;

			this.log = Library.Instance.Log;
			this.db = Library.Instance.DB;
		} // constructor

		protected abstract void AdjustWeightsWithRatio(decimal ratio);

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
						this.log.Alert(
							"MedalCalculatorBase.GatherInputData: Cannot parse row type from {0}",
							srfv["RowType"]
						);
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

			Results.CapOfferByCustomerScoresTable = new CapOfferByCustomerScoreMatrix(Results.CustomerId, this.db);
			Results.CapOfferByCustomerScoresTable.Load();
		} // GatherInputData

		private enum RowType {
			NewActualLoansRepayment,
			FcfValueAdded,
		} // enum RowType

		protected abstract decimal GetCompanyScoreWeightForLowScore();

		protected abstract decimal GetConsumerScoreWeightForLowScore();

		protected abstract decimal GetSumOfNonFixedWeights();

		protected virtual void RedistributeFreeCashFlowWeight() { }

		protected abstract void RedistributeWeightsForPayingCustomer();

		protected abstract void SetMedalType();

		private void AdjustCompanyScoreWeight() {
			if (Results.BusinessScore <= 30)
				Results.BusinessScoreWeight = GetCompanyScoreWeightForLowScore();
		} // AdjustCompanyScoreWeight

		private void AdjustConsumerScoreWeight() {
			if (Results.ConsumerScore <= 800)
				Results.ConsumerScoreWeight = GetConsumerScoreWeightForLowScore();
		} // AdjustConsumerScoreWeight

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
			} // if
		} // AdjustSumOfWeights

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
		} // CalculateAnnualTurnoverGrade

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
		} // CalculateBusinessScoreGrade

		private void CalculateBusinessSeniorityGrade() {
			DateTime dateOnlyCalculationTime = Results.CalculationTime.Date;

			bool shouldBeZero =
				!Results.BusinessSeniority.HasValue ||
				Results.BusinessSeniority.Value.Date.AddYears(1) > dateOnlyCalculationTime;

			if (shouldBeZero)
				Results.BusinessSeniorityGrade = 0;
			else if (Results.BusinessSeniority.Value.Date.AddYears(3) > dateOnlyCalculationTime)
				Results.BusinessSeniorityGrade = 1;
			else if (Results.BusinessSeniority.Value.Date.AddYears(5) > dateOnlyCalculationTime)
				Results.BusinessSeniorityGrade = 2;
			else if (Results.BusinessSeniority.Value.Date.AddYears(10) > dateOnlyCalculationTime)
				Results.BusinessSeniorityGrade = 3;
			else
				Results.BusinessSeniorityGrade = 4;
		} // CalculateBusinessSeniorityGrade

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
		} // CalculateConsumerScoreGrade

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
		} // CalculateCustomerScore

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
		} // CalculateEzbobSeniorityGrade

		private void CalculateFreeCashFlowGrade() {
			// When turnover is zero we can't calculate FCF, we want the min grade
			if (Results.FreeCashFlow < -0.1m || Results.AnnualTurnover <= 0)
				Results.FreeCashFlowGrade = 0;
			else if (Results.FreeCashFlow < 0)
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

			if (!Results.UseHmrc())
				Results.FreeCashFlowGrade = 0;
		} // CalculateFreeCashFlowGrade

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
		} // CalculateGrades

		private void CalculateMaritalStatusGrade() {
			if (Results.MaritalStatus.In(MaritalStatus.Married, MaritalStatus.Widowed))
				Results.MaritalStatusGrade = 4;
			else if (Results.MaritalStatus.In(MaritalStatus.Divorced, MaritalStatus.LivingTogether))
				Results.MaritalStatusGrade = 3;
			else // Single, Separated, Other
				Results.MaritalStatusGrade = 2;
		} // CalculateMaritalStatusGrade

		private void CalculateMedal() {
			if (Results.TotalScoreNormalized <= 0.4m)
				Results.MedalClassification = Medal.Silver;
			else if (Results.TotalScoreNormalized <= 0.62m)
				Results.MedalClassification = Medal.Gold;
			else if (Results.TotalScoreNormalized <= 0.84m)
				Results.MedalClassification = Medal.Platinum;
			else
				Results.MedalClassification = Medal.Diamond;
		} // CalculateMedal

		private void CalculateNetWorth() {
			Results.NetWorth = (Results.ZooplaValue == 0)
				? 0
				: (Results.ZooplaValue - Results.MortgageBalance) / Results.ZooplaValue;
		} // CalculateNetWorth

		private void CalculateNetWorthGrade() {
			if (Results.NetWorth < 0.15m)
				Results.NetWorthGrade = 0;
			else if (Results.NetWorth < 0.5m)
				Results.NetWorthGrade = 1;
			else if (Results.NetWorth < 1)
				Results.NetWorthGrade = 2;
			else // We know that we sometimes miss mortgages the customer has, so instead of grade=3 we give 1
				Results.NetWorthGrade = 1;
		} // CalculateNetWorthGrade

		private void CalculateNumberOfStoresGrade() {
			if (Results.NumberOfStores < 3)
				Results.NumberOfStoresGrade = 1;
			else if (Results.NumberOfStores < 5)
				Results.NumberOfStoresGrade = 3;
			else
				Results.NumberOfStoresGrade = 5;
		} // CalculateNumberOfStoresGrade

		private void CalculateNumOfEarlyRepaymentsGrade() {
			if (Results.NumOfEarlyRepayments == 0)
				Results.NumOfEarlyRepaymentsGrade = 2;
			else if (Results.NumOfEarlyRepayments < 4)
				Results.NumOfEarlyRepaymentsGrade = 3;
			else
				Results.NumOfEarlyRepaymentsGrade = 5;
		} // CalculateNumOfEarlyRepaymentsGrade

		private void CalculateNumOfLateRepaymentsGrade() {
			if (Results.NumOfLateRepayments == 0)
				Results.NumOfLateRepaymentsGrade = 5;
			else if (Results.NumOfLateRepayments == 1)
				Results.NumOfLateRepaymentsGrade = 2;
			else
				Results.NumOfLateRepaymentsGrade = 0;
		} // CalculateNumOfLateRepaymentsGrade

		private void CalculateNumOfLoansGrade() {
			if (Results.NumOfLoans > 3)
				Results.NumOfLoansGrade = 4;
			else if (Results.NumOfLoans > 1)
				Results.NumOfLoansGrade = 3;
			else
				Results.NumOfLoansGrade = 1;
		} // CalculateNumOfLoansGrade

		private void CalculateOffer() {
			if (Results == null)
				return;

			if (!string.IsNullOrEmpty(Results.Error))
				return;

			if (Results.MedalType == Ezbob.Backend.Strategies.MedalCalculations.MedalType.NoMedal)
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
			decimal offerAccordingToFreeCashFlow = Results.UseHmrc()
				? Results.FreeCashFlowValue * freeCashFlowMedalFactor / 100
				: 0;
			decimal offerAccordingToValueAdded = Results.UseHmrc()
				? Results.ValueAdded * valueAddedMedalFactor / 100
				: 0;

			if (IsOnlineMedalNotViaHmrcInnerFlow())
				offerAccordingToValueAdded = 0;

			decimal[] allOfferAmounts = {
				offerAccordingToAnnualTurnover,
				offerAccordingToFreeCashFlow,
				offerAccordingToValueAdded
			};

			List<decimal> validOfferAmounts = allOfferAmounts.Where(x => x >= CurrentValues.Instance.MedalMinOffer).ToList();

			this.log.Debug("Primary medal - all   offer amounts: {0}", string.Join(", ", allOfferAmounts));
			this.log.Debug("Primary medal - valid offer amounts: {0}", string.Join(", ", validOfferAmounts));

			if (validOfferAmounts.Count > 0) {
				decimal unroundedValue = validOfferAmounts.Min();
				decimal maxUnroundedValue = validOfferAmounts.Max();

				Results.OfferedLoanAmount = (int)(unroundedValue * Results.CapOfferByCustomerScoresValue);
				Results.MaxOfferedLoanAmount = (int)(maxUnroundedValue * Results.CapOfferByCustomerScoresValue);

				this.log.Debug(
					"Primary medal - offered amount is {0}" +
					"(before cap and rounding: {1}, cap value: {2}, consumer score: {3}, business score: {4}).",
					Results.OfferedLoanAmount,
					unroundedValue,
					Results.CapOfferByCustomerScoresValue,
					Results.ConsumerScore,
					Results.BusinessScore
				);

				this.log.Debug(
					"Primary medal - MAX offered amount is {0}" +
					"(before cap and rounding: {1}, cap value: {2}).",
					Results.MaxOfferedLoanAmount,
					maxUnroundedValue,
					Results.CapOfferByCustomerScoresValue
				);
			} else
				this.log.Debug("Primary medal - all the offer amounts are not valid.");
		} // CalculateOffer

		private void CalculatePositiveFeedbacksGrade() {
			if (Results.PositiveFeedbacks < 1)
				Results.PositiveFeedbacksGrade = 0;
			else if (Results.PositiveFeedbacks < 5001)
				Results.PositiveFeedbacksGrade = 2;
			else if (Results.PositiveFeedbacks < 50000)
				Results.PositiveFeedbacksGrade = 3;
			else
				Results.PositiveFeedbacksGrade = 5;
		} // CalculatePositiveFeedbacksGrade

		private void CalculateRatiosOfAnnualTurnover() {
			if (Results.AnnualTurnover > 0) {
				Results.TangibleEquity = Results.TangibleEquityValue / Results.AnnualTurnover;
				Results.FreeCashFlow = Results.FreeCashFlowValue / Results.AnnualTurnover;
			} else {
				Results.TangibleEquity = 0;
				Results.FreeCashFlow = 0;
			} // if
		} // CalculateRatiosOfAnnualTurnover

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
		} // CalculateScoreMax

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
		} // CalculateScoreMin

		private void CalculateTangibleEquityGrade() {
			if (Results.TangibleEquity < -0.05m || Results.AnnualTurnover <= 0) {
				// When turnover is zero we can't calculate tangible equity, we want the min grade
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

		private decimal GetMortgages(int customerId) {
			var instance = new LoadExperianConsumerMortgageData(customerId);
			instance.Execute();

			return instance.Result.MortgageBalance;
		} // GetMortgages

		/// <summary>
		/// Turnover for medal: https://drive.draw.io/?#G0B1Io_qu9i44ScEJqeUlLNEhaa28
		/// </summary>
		protected virtual void CalculateTurnoverForMedal() {
			try {
				Results.AnnualTurnover = 0;
				Results.HmrcAnnualTurnover = 0;
				Results.BankAnnualTurnover = 0;
				Results.OnlineAnnualTurnover = 0;

				var historyRecordIDs = new List<int>();

				this.db.ForEachRowSafe(
					sr => historyRecordIDs.Add(sr["Id"]),
					"GetMarketplaceUpdatingHistoryByCustomerAndTime",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerID", Results.CustomerId),
					new QueryParameter("Now", Results.CalculationTime)
				);

				this.log.Debug(
					"{0} history records loaded for customer {1} and calculation time {2}: '{3}'.",
					historyRecordIDs.Count,
					Results.CustomerId,
					Results.CalculationTime.MomentStr(),
					string.Join(", ", historyRecordIDs)
				);

				if (historyRecordIDs.Count < 1) {
					this.log.Info(
						"Updating history for customer {0}, calculationDate {1} not found",
						Results.CustomerId,
						Results.CalculationTime
					);
					return;
				} // if

				var hmrcs = new List<MarketplaceTurnoverModel>();
				this.db.ForEachRowSafe(
					row => {
						hmrcs.Add(new MarketplaceTurnoverModel {
							TheMonth = row["TheMonth"],
							Turnover = row["Turnover"],
							CustomerMarketPlaceUpdatingHistoryID = row["CustomerMarketPlaceUpdatingHistoryId"],
							CustomerMarketPlaceID = row["CustomerMarketPlaceId"],
							UpdatingEnd = row["UpdatingEnd"],
						});
					},
					"GetActiveHmrcAggregationsByMpHistory",
					CommandSpecies.StoredProcedure,
					this.db.CreateTableParameter("HistoryRecordIDs", (IEnumerable<int>)historyRecordIDs)
				);

				this.log.Debug(
					"{0} HMRC records loaded for customer {1}, calculation time {2} and history records '{3}':\n{4}",
					hmrcs.Count,
					Results.CustomerId,
					Results.CalculationTime.MomentStr(),
					string.Join(", ", historyRecordIDs),
					string.Join("\n", hmrcs)
				);

				if (hmrcs.Count > 0) {
					List<FilteredAggregationResult> hmrcList = new List<FilteredAggregationResult>();

					IEnumerable<int> marketplaceIDs = hmrcs.Select(x => x.CustomerMarketPlaceID).Distinct();

					foreach (int mpID in marketplaceIDs) {
						List<MarketplaceTurnoverModel> thisMp = hmrcs.Where(x => x.CustomerMarketPlaceID == mpID).ToList();

						List<FilteredAggregationResult> filtered =
							LastHistoryTurnovers(thisMp, Results.CalculationTime, thisMp.Max(x => x.TheMonth));

						if (filtered != null)
							hmrcList.AddRange(filtered);
					} // for each marketplace

					if ((hmrcList.Count > 0) && (Results.NumOfHmrcMps > 0)) {
						// get HMRC turnover for all months received
						Results.HmrcAnnualTurnover = hmrcList.Sum(t => t.Turnover);
						Results.HmrcAnnualTurnover = (Results.HmrcAnnualTurnover < 0) ? 0 : Results.HmrcAnnualTurnover;
					} // if
				} // if

				// this is non-online medal type and has a HMRC
				if (!Results.MedalType.IsOnline() && Results.NumOfHmrcMps > 0) {
					Results.TurnoverType = TurnoverType.HMRC;
					Results.AnnualTurnover = Results.HmrcAnnualTurnover;

					this.log.Debug(
						"Base: (HMRC) AnnualTurnover: {0}," + "HmrcAnnualTurnover: {1}," +
						"BankAnnualTurnover: {2}," + "OnlineAnnualTurnover: {3}," + "Type: {4}",
						Results.AnnualTurnover,
						Results.HmrcAnnualTurnover,
						Results.BankAnnualTurnover,
						Results.OnlineAnnualTurnover,
						Results.TurnoverType
					);

					return;
				} // if

				var yodlees = new List<MarketplaceTurnoverModel>();
				this.db.ForEachRowSafe(
					row => {
						yodlees.Add(new MarketplaceTurnoverModel {
							TheMonth = row["TheMonth"],
							Turnover = row["Turnover"],
							CustomerMarketPlaceUpdatingHistoryID = row["CustomerMarketPlaceUpdatingHistoryId"],
							CustomerMarketPlaceID = row["CustomerMarketPlaceId"],
							UpdatingEnd = row["UpdatingEnd"],
						});
					},
					"GetActiveYodleeAggregationsByMpHistory",
					CommandSpecies.StoredProcedure,
					this.db.CreateTableParameter("HistoryRecordIDs", (IEnumerable<int>)historyRecordIDs)
				);

				this.log.Debug(
					"{0} Yodlee records loaded for customer {1}, calculation time {2} and history records '{3}':\n{4}",
					yodlees.Count,
					Results.CustomerId,
					Results.CalculationTime.MomentStr(),
					string.Join(", ", historyRecordIDs),
					string.Join("\n", yodlees)
				);

				if (yodlees.Count > 0) {
					List<FilteredAggregationResult> yodleeList =
						LastHistoryTurnovers(yodlees, Results.CalculationTime);

					if (yodleeList != null) {
						// get yoodlee turnover for all months received
						Results.BankAnnualTurnover = yodleeList.Sum(t => t.Turnover);
						Results.BankAnnualTurnover = (Results.BankAnnualTurnover < 0) ? 0 : Results.BankAnnualTurnover;
					} // if
				} // if

				// this is non-online medal type
				if (!Results.MedalType.IsOnline()) {
					// and has a yodlee (bank) data
					if (Results.NumOfBanks > 0) {
						Results.TurnoverType = TurnoverType.Bank;
						Results.AnnualTurnover = Results.BankAnnualTurnover;

						this.log.Debug(
							"Base: (bank) AnnualTurnover: {0}," + "HmrcAnnualTurnover: {1}," +
							"BankAnnualTurnover: {2}," + "OnlineAnnualTurnover: {3}," + "Type: {4}",
							Results.AnnualTurnover,
							Results.HmrcAnnualTurnover,
							Results.BankAnnualTurnover,
							Results.OnlineAnnualTurnover,
							Results.TurnoverType
						);

						return;
					} // if

					// no turnover
					return;
				} // if

				// Continue to online
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

				decimal[] filltt = { 0, 0, 0, 0 };
				List<decimal> list_t1 = new List<decimal>(filltt);
				List<decimal> list_t3 = new List<decimal>(filltt);
				List<decimal> list_t6 = new List<decimal>(filltt);
				List<decimal> list_t12 = new List<decimal>(filltt);

				var amazons = new List<MarketplaceTurnoverModel>();
				this.db.ForEachRowSafe(
					row => {
						amazons.Add(new MarketplaceTurnoverModel {
							TheMonth = row["TheMonth"],
							Turnover = row["Turnover"],
							CustomerMarketPlaceUpdatingHistoryID = row["CustomerMarketPlaceUpdatingHistoryId"],
							CustomerMarketPlaceID = row["CustomerMarketPlaceId"],
							UpdatingEnd = row["UpdatingEnd"],
						});
					},
					"GetActiveAmazonAggregationsByMpHistory",
					CommandSpecies.StoredProcedure,
					this.db.CreateTableParameter("HistoryRecordIDs", (IEnumerable<int>)historyRecordIDs)
				);

				this.log.Debug(
					"{0} Amazon records loaded for customer {1}, calculation time {2} and history records '{3}':\n{4}",
					amazons.Count,
					Results.CustomerId,
					Results.CalculationTime.MomentStr(),
					string.Join(", ", historyRecordIDs),
					string.Join("\n", amazons)
				);

				List<FilteredAggregationResult> amazonList = LastHistoryTurnovers(amazons, Results.CalculationTime);

				if (amazonList != null) {
					// Amazon: calculate "last month", "last 3 months", "last 6 months", and "last 12 months"/annualize 
					amazonList.ForEach(x => this.log.Debug(
						"TheMonth: {0}, Turnover: {1}, MpId: {2}, Distance: {3}",
						x.TheMonth,
						x.Turnover,
						x.MpId,
						x.Distance
					));

					list_t1.Insert(0, CalcAnnualTurnoverBasedOnPartialData(amazonList, T1, Ec1));
					list_t3.Insert(0, CalcAnnualTurnoverBasedOnPartialData(amazonList, T3, Ec3));
					list_t6.Insert(0, CalcAnnualTurnoverBasedOnPartialData(amazonList, T6, Ec6));
					list_t12.Insert(0, CalcAnnualTurnoverBasedOnPartialData(amazonList, T12, Ec12));

					this.log.Debug(
						"amazon TT: t1: {0}, t3: {1}, t6: {2}, t12: {3}",
						list_t1.ElementAtOrDefault(0),
						list_t3.ElementAtOrDefault(0),
						list_t6.ElementAtOrDefault(0),
						list_t12.ElementAtOrDefault(0)
					);
				} // if

				var ebays = new List<MarketplaceTurnoverModel>();
				this.db.ForEachRowSafe(
					row => {
						ebays.Add(new MarketplaceTurnoverModel {
							TheMonth = row["TheMonth"],
							Turnover = row["Turnover"],
							CustomerMarketPlaceUpdatingHistoryID = row["CustomerMarketPlaceUpdatingHistoryId"],
							CustomerMarketPlaceID = row["CustomerMarketPlaceId"],
							UpdatingEnd = row["UpdatingEnd"],
						});
					},
					"GetActiveEbayAggregationsByMpHistory",
					CommandSpecies.StoredProcedure,
					this.db.CreateTableParameter("HistoryRecordIDs", (IEnumerable<int>)historyRecordIDs)
				);

				this.log.Debug(
					"{0} eBay records loaded for customer {1}, calculation time {2} and history records '{3}':\n{4}",
					ebays.Count,
					Results.CustomerId,
					Results.CalculationTime.MomentStr(),
					string.Join(", ", historyRecordIDs),
					string.Join("\n", ebays)
				);

				List<FilteredAggregationResult> ebayList = LastHistoryTurnovers(ebays, Results.CalculationTime);

				if (ebayList != null) {
					ebayList.ForEach(x => this.log.Debug(
						"TheMonth: {0}, Turnover: {1}, MpId: {2}, Distance: {3}",
						x.TheMonth,
						x.Turnover,
						x.MpId,
						x.Distance
					));

					list_t1.Insert(1, CalcAnnualTurnoverBasedOnPartialData(ebayList, T1, Ec1));
					list_t3.Insert(1, CalcAnnualTurnoverBasedOnPartialData(ebayList, T3, Ec3));
					list_t6.Insert(1, CalcAnnualTurnoverBasedOnPartialData(ebayList, T6, Ec6));
					list_t12.Insert(1, CalcAnnualTurnoverBasedOnPartialData(ebayList, T12, Ec12));

					this.log.Debug(
						"ebay TT: t1: {0}, t3: {1}, t6: {2}, t12: {3}",
						list_t1.ElementAtOrDefault(1),
						list_t3.ElementAtOrDefault(1),
						list_t6.ElementAtOrDefault(1),
						list_t12.ElementAtOrDefault(1)
					);
				} // if

				var paypals = new List<MarketplaceTurnoverModel>();
				this.db.ForEachRowSafe(
					row => {
						paypals.Add(new MarketplaceTurnoverModel {
							TheMonth = row["TheMonth"],
							Turnover = row["Turnover"],
							CustomerMarketPlaceUpdatingHistoryID = row["CustomerMarketPlaceUpdatingHistoryId"],
							CustomerMarketPlaceID = row["CustomerMarketPlaceId"],
							UpdatingEnd = row["UpdatingEnd"],
						});
					},
					"GetActivePayPalAggregationsByMpHistory",
					CommandSpecies.StoredProcedure,
					this.db.CreateTableParameter("HistoryRecordIDs", (IEnumerable<int>)historyRecordIDs)
				);

				this.log.Debug(
					"{0} Pay Pal records loaded for customer {1}, calculation time {2} and history records '{3}':\n{4}",
					paypals.Count,
					Results.CustomerId,
					Results.CalculationTime.MomentStr(),
					string.Join(", ", historyRecordIDs),
					string.Join("\n", paypals)
				);

				List<FilteredAggregationResult> paypalList = LastHistoryTurnovers(paypals, Results.CalculationTime);

				if (paypalList != null) {
					paypalList.ForEach(x => this.log.Debug(
						"TheMonth: {0}, Turnover: {1}, MpId: {2}, Distance: {3}",
						x.TheMonth,
						x.Turnover,
						x.MpId,
						x.Distance
					));

					list_t1.Insert(2, CalcAnnualTurnoverBasedOnPartialData(paypalList, T1, Ec1));
					list_t3.Insert(2, CalcAnnualTurnoverBasedOnPartialData(paypalList, T3, Ec3));
					list_t6.Insert(2, CalcAnnualTurnoverBasedOnPartialData(paypalList, T6, Ec6));
					list_t12.Insert(2, CalcAnnualTurnoverBasedOnPartialData(paypalList, T12, Ec12));

					this.log.Debug(
						"paypals TT: t1: {0}, t3: {1}, t6: {2}, t12: {3}",
						list_t1.ElementAtOrDefault(2),
						list_t3.ElementAtOrDefault(2),
						list_t6.ElementAtOrDefault(2),
						list_t12.ElementAtOrDefault(2)
					);
				} // if

				// Online turnover: Amazon + MAX(eBay, Pay Pal)

				// amazon: index 0
				// ebay: index 1
				// paypal: index 2
				ArrayList onlineList = new ArrayList();

				onlineList.Add(OnlineMax(list_t1));
				onlineList.Add(OnlineMax(list_t3));
				onlineList.Add(OnlineMax(list_t6));
				onlineList.Add(OnlineMax(list_t12));

				foreach (var zz in onlineList)
					this.log.Debug("onlineList {0}", zz);

				Results.OnlineAnnualTurnover = (
					from decimal r in onlineList where r > 0 select r
				).AsQueryable().DefaultIfEmpty(0).Min();

				this.log.Debug("===> min from 4 {0}", Results.OnlineAnnualTurnover);

				decimal onlineMedalTurnoverCutoff = CurrentValues.Instance.OnlineMedalTurnoverCutoff;

				if (Results.HmrcAnnualTurnover > onlineMedalTurnoverCutoff * Results.OnlineAnnualTurnover) {
					Results.TurnoverType = TurnoverType.HMRC;
					Results.AnnualTurnover = Results.HmrcAnnualTurnover;

					this.log.Debug(
						"Base: (HmrcAnnualTurnover-><-onlineMedalTurnoverCutoff): AnnualTurnover: {0}, " +
						"HmrcAnnualTurnover: {1}, BankAnnualTurnover: {2}, OnlineAnnualTurnover: {3}, Type: {4}",
						Results.AnnualTurnover,
						Results.HmrcAnnualTurnover,
						Results.BankAnnualTurnover,
						Results.OnlineAnnualTurnover,
						Results.TurnoverType
					);

					return;
				} // if

				if (Results.BankAnnualTurnover > onlineMedalTurnoverCutoff * Results.OnlineAnnualTurnover) {
					Results.TurnoverType = TurnoverType.Bank;
					Results.AnnualTurnover = Results.BankAnnualTurnover;

					this.log.Debug(
						"Base: (BankAnnualTurnover-><-onlineMedalTurnoverCutoff): AnnualTurnover: {0}, " +
						"HmrcAnnualTurnover: {1}, BankAnnualTurnover: {2}, OnlineAnnualTurnover: {3}, Type: {4}",
						Results.AnnualTurnover,
						Results.HmrcAnnualTurnover,
						Results.BankAnnualTurnover,
						Results.OnlineAnnualTurnover,
						Results.TurnoverType
					);

					return;
				} // if

				Results.TurnoverType = TurnoverType.Online;
				Results.AnnualTurnover = Results.OnlineAnnualTurnover;

				this.log.Debug(
					"Base: AnnualTurnover: {0}, HmrcAnnualTurnover: {1}," +
					" BankAnnualTurnover: {2}, OnlineAnnualTurnover: {3}, Type: {4}",
					Results.AnnualTurnover,
					Results.HmrcAnnualTurnover,
					Results.BankAnnualTurnover,
					Results.OnlineAnnualTurnover,
					Results.TurnoverType
				);
			} catch (Exception ex) {
				this.log.Error(
					ex,
					"Failed to get/calculate annual turnover for medal. " +
					"customerID: {0}, calculationTime: {1}, IsForApprove: true",
					Results.CustomerId,
					Results.CalculationTime
				);
			} // try
		} // CalculateTurnoverForMedal

		// Online turnover: Amazon + MAX(eBay, Pay Pal)
		// amazon: index 0
		// ebay: index 1
		// paypal: index 2
		private decimal OnlineMax(List<decimal> lst) {
			return lst.ElementAtOrDefault(0) + Math.Max(lst.ElementAtOrDefault(1), lst.ElementAtOrDefault(2));
		} // OnlineMax

		private bool IsOnlineMedalNotViaHmrcInnerFlow() {
			return Results.MedalType.IsOnline() && Results.TurnoverType != TurnoverType.HMRC;
		} // IsOnlineMedalNotViaHmrcInnerFlow

		private List<FilteredAggregationResult> LastHistoryTurnovers(
			List<MarketplaceTurnoverModel> inputList,
			DateTime calculationTime,
			DateTime? lastExistingDataMonth = null
		) {
			if (inputList.Count == 0)
				return null;

			inputList.ForEach(x => this.log.Debug(
				"before: {0}, {1}, historyID: {2}, mpID: {3}, updatingEnd: {4}",
				x.TheMonth.MomentStr(),
				x.Turnover,
				x.CustomerMarketPlaceUpdatingHistoryID,
				x.CustomerMarketPlaceID,
				x.UpdatingEnd.MomentStr() 
			));

			DateTime lastUpdateDate = inputList.Max(z => z.UpdatingEnd);

			DateTime periodStart = MiscUtils.GetPeriodAgo(
				calculationTime,
				lastUpdateDate,
				CurrentValues.Instance.TotalsMonthTail,
				lastExistingDataMonth
			);

			DateTime periodEnd = periodStart.AddMonths(11);

			this.log.Debug(
				"calculationTime: {2}, lastUpdateDate: {1}, yearAgo: {0}, yearAgoEnd: {3}, HMRC last known: '{4}'",
				periodStart.MomentStr(),
				lastUpdateDate.MomentStr(),
				calculationTime.MomentStr(),
				periodEnd.MomentStr(),
				lastExistingDataMonth.MomentStr()
			);

			List<MarketplaceTurnoverModel> histories = inputList.Where(
				z => z.TheMonth >= periodStart && z.TheMonth <= periodEnd
			).ToList();

			histories.ForEach(x => this.log.Debug(
				"Relevant month: {0}, {1}, historyID: {2}, mpID: {3}",
				x.TheMonth.MomentStr(),
				x.Turnover,
				x.CustomerMarketPlaceUpdatingHistoryID,
				x.CustomerMarketPlaceID
			));

			if (histories.Count < 1)
				return null;

			List<FilteredAggregationResult> result = new List<FilteredAggregationResult>();

			var groups = histories.GroupBy(ag => new {
				ag.CustomerMarketPlaceID,
				ag.TheMonth
			});

			foreach (var grp in groups) {
				MarketplaceTurnoverModel first = grp.OrderByDescending(p => p.AggID).First();

				var far = new FilteredAggregationResult {
					Distance = (11 - MiscUtils.DateDiffInMonths(periodStart, first.TheMonth)),
					TheMonth = first.TheMonth,
					MpId = first.CustomerMarketPlaceID,
					Turnover = first.Turnover
				};

				result.Add(far);
			} // for each

			result.ForEach(x => this.log.Debug("Filtered: {0}, {1}, {2}", x.TheMonth.MomentStr(), x.Turnover, x.MpId));

			return result;
		} // LastHistoryTurnovers

		/// <summary>
		/// 
		/// </summary>
		/// <param name="list"></param>
		/// <param name="monthAfter"></param>
		/// <param name="extrapolationCoefficient"></param>
		/// <returns></returns>
		private decimal CalcAnnualTurnoverBasedOnPartialData(
			IEnumerable<FilteredAggregationResult> list,
			int monthAfter,
			int extrapolationCoefficient
		) {
			return list.Where(t => (t.Distance < monthAfter)).Sum(t => t.Turnover) * extrapolationCoefficient;
		} // CalcAnnualTurnoverBasedOnPartialData

		private bool isInitialized;
		private bool isCalculated;
	} // class MedalBase
} // namespace

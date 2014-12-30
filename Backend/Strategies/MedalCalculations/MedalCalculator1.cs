namespace Ezbob.Backend.Strategies.MedalCalculations {
	using System;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class MedalCalculator1 {
		public MedalCalculator1(
			int customerId,
			DateTime calculationTime,
			string typeOfBusiness,
			int consumerScore,
			int companyScore,
			int numOfHmrcMps,
			int numOfYodleeMps,
			int numOfEbayAmazonPayPalMps,
			DateTime? earliestHmrcLastUpdateDate,
			DateTime? earliestYodleeLastUpdateDate,
			AConnection db,
			ASafeLog log
			) {
			this.customerId = customerId;
			this.calculationTime = calculationTime;
			this.typeOfBusiness = typeOfBusiness;
			this.consumerScore = consumerScore;
			this.companyScore = companyScore;
			this.numOfHmrcMps = numOfHmrcMps;
			this.numOfYodleeMps = numOfYodleeMps;
			this.numOfEbayAmazonPayPalMps = numOfEbayAmazonPayPalMps;
			this.earliestHmrcLastUpdateDate = earliestHmrcLastUpdateDate;
			this.earliestYodleeLastUpdateDate = earliestYodleeLastUpdateDate;

			this.db = db;
			this.log = log;
		} // constructor

		public MedalResult CalculateMedal(
			) {
			double relevancyLen = CurrentValues.Instance.MedalDaysOfMpRelevancy;

			bool badHmrc =
				(this.numOfHmrcMps > 0) && this.earliestHmrcLastUpdateDate.HasValue &&
					(this.earliestHmrcLastUpdateDate.Value.AddDays(relevancyLen) < this.calculationTime);

			if (badHmrc) {
				return SetNoMedal(
					"Customer has out of date HMRC marketplace that its last update was at:{0}",
					this.earliestHmrcLastUpdateDate.Value
					);
			} // if

			bool badYodlee =
				(this.numOfYodleeMps > 0) && this.earliestYodleeLastUpdateDate.HasValue &&
					(this.earliestYodleeLastUpdateDate.Value.AddDays(relevancyLen) < this.calculationTime);

			if (badYodlee) {
				return SetNoMedal(
					"Customer has out of date Yodlee marketplace that its last update was at:{0}",
					this.earliestYodleeLastUpdateDate.Value
					);
			} // if

			MedalCalculatorBase calculator = ChooseCalculator();

			return (calculator == null)
				? SetNoMedal("Customer doesn't fit any of the existing medals")
				: calculator.CalculateMedalScore(this.customerId, this.calculationTime);
		} // CalculateMedal

		private MedalCalculatorBase ChooseCalculator() {
			if (this.typeOfBusiness == "LLP" || this.typeOfBusiness == "Limited")
				return ChooseLimitedCalculator();

			if (this.companyScore > 0)
				return ChooseNonLimitedCalculatorWithScore();

			if (this.numOfEbayAmazonPayPalMps > 0)
				return new OnlineNonLimitedNoBusinessScoreMedalCalculator1(this.db, this.log);

			if ((this.consumerScore > 0) && ((this.numOfHmrcMps > 0) || (this.numOfYodleeMps > 0)))
				return new SoleTraderMedalCalculator1(this.db, this.log);

			return null;
		} // ChooseCalculator

		private MedalCalculatorBase ChooseLimitedCalculator() {
			if (this.numOfEbayAmazonPayPalMps > 0)
				return new OnlineLimitedMedalCalculator1(this.db, this.log);

			return new LimitedMedalCalculator1(this.db, this.log);
		} // ChooseLimitedCalculator

		private MedalCalculatorBase ChooseNonLimitedCalculatorWithScore() {
			if (this.numOfEbayAmazonPayPalMps > 0)
				return new OnlineNonLimitedWithBusinessScoreMedalCalculator1(this.db, this.log);

			return new NonLimitedMedalCalculator1(this.db, this.log);
		} // ChooseNonLimitedCalculatorWithScore

		private MedalResult SetNoMedal(string errorMessageFormat, params object[] args) {
			this.log.Warn("No medal was calculated for customer {0}.", this.customerId);

			return new MedalResult(this.customerId) {
				CalculationTime = this.calculationTime,
				MedalType = MedalType.NoMedal,
				Error = string.Format(errorMessageFormat, args),
			};
		} // SetNoMedal

		private readonly AConnection db;
		private readonly ASafeLog log;

		private readonly int customerId;
		private readonly DateTime calculationTime;
		private readonly string typeOfBusiness;
		private readonly int consumerScore;
		private readonly int companyScore;
		private readonly int numOfHmrcMps;
		private readonly int numOfYodleeMps;
		private readonly int numOfEbayAmazonPayPalMps;
		private readonly DateTime? earliestHmrcLastUpdateDate;
		private readonly DateTime? earliestYodleeLastUpdateDate;
	} // class MedalCalculator1
} // namespace

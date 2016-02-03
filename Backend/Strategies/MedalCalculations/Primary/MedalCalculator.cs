namespace Ezbob.Backend.Strategies.MedalCalculations.Primary {
	using System;
	using System.Globalization;
	using ConfigManager;

	public class MedalCalculator {
		public MedalCalculator(
			int customerId,
			DateTime calculationTime,
			string typeOfBusiness,
			int consumerScore,
			int companyScore,
			int numOfHmrcMps,
			int numOfYodleeMps,
			int numOfEbayAmazonPayPalMps,
			DateTime? earliestHmrcLastUpdateDate,
			DateTime? earliestYodleeLastUpdateDate
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
		} // constructor

		/// <summary>
		/// implementation of 
		/// https://drive.draw.io/?#G0B1Io_qu9i44SVzVqV19nbnMxRW8
		/// https://drive.draw.io/?#G0B1Io_qu9i44ScEJqeUlLNEhaa28 (get medal type and medal value)
		/// </summary>
		public MedalResult CalculateMedal() {
			double relevancyLen = CurrentValues.Instance.MedalDaysOfMpRelevancy;

			bool? badHmrc;

			if (this.numOfHmrcMps > 0) {
				if (this.earliestHmrcLastUpdateDate.HasValue)
					badHmrc = (this.earliestHmrcLastUpdateDate.Value.AddDays(relevancyLen) < this.calculationTime);
				else
					badHmrc = true; // Has accounts which have never been checked (or always failed to check) - it's bad.
			} else
				badHmrc = false; // No accounts -> there are no bad accounts.

			if (badHmrc.Value) {
				return SetNoMedal(
					"Customer has out of date HMRC marketplace that its last update was at: {0}",
					this.earliestHmrcLastUpdateDate.HasValue
						? this.earliestHmrcLastUpdateDate.Value.ToString(
							"MMM d yyyy H:mm:ss",
							CultureInfo.InvariantCulture
						)
						: "never"
				);
			} // if

			bool? badYodlee;

			if (this.numOfYodleeMps > 0) {
				if (this.earliestYodleeLastUpdateDate.HasValue)
					badYodlee = (this.earliestYodleeLastUpdateDate.Value.AddDays(relevancyLen) < this.calculationTime);
				else
					badYodlee = true; // Has accounts which have never been checked (or always failed to check) - it's bad.
			} else 
				badYodlee = false; // No accounts -> there are no bad accounts.

			if (badYodlee.Value) {
				return SetNoMedal(
					"Customer has out of date Yodlee marketplace that its last update was at: {0}",
					this.earliestYodleeLastUpdateDate.HasValue
						? this.earliestYodleeLastUpdateDate.Value.ToString(
							"MMM d yyyy H:mm:ss",
							CultureInfo.InvariantCulture
						)
						: "never"
				);
			} // if

			Type calculatorType = ChooseCalculator();

			if (calculatorType == null)
				return SetNoMedal("Customer doesn't fit any of the existing medals");

			// choose medal type
			MedalBase calculator = Activator.CreateInstance(calculatorType) as MedalBase;

			if (calculator == null)
				return SetNoMedal("Failed to create medal calculator of type " + calculatorType);

			calculator.Init(
				this.customerId,
				this.calculationTime,

				this.consumerScore,
				this.companyScore,

				this.numOfHmrcMps,
				this.numOfYodleeMps,
				this.numOfEbayAmazonPayPalMps,

				this.earliestHmrcLastUpdateDate,
				this.earliestYodleeLastUpdateDate
			);

			return calculator.CalculateMedalScore();
		} // CalculateMedal

		private Type ChooseCalculator() {
			if (this.typeOfBusiness == "LLP" || this.typeOfBusiness == "Limited")
				return ChooseLimitedCalculator();

			if (this.companyScore > 0)
				return ChooseNonLimitedCalculatorWithScore();

			if (this.numOfEbayAmazonPayPalMps > 0)
				return typeof(Online.NonLimited.NoBusinessScore);

			if ((this.consumerScore > 0) && ((this.numOfHmrcMps > 0) || (this.numOfYodleeMps > 0)))
				return typeof(Offline.SoleTrader);

			return null;
		} // ChooseCalculator

		private Type ChooseLimitedCalculator() {
			return this.numOfEbayAmazonPayPalMps > 0 ? typeof(Online.Limited) : typeof(Offline.Limited);
		} // ChooseLimitedCalculator

		private Type ChooseNonLimitedCalculatorWithScore() {
			return this.numOfEbayAmazonPayPalMps > 0
				? typeof(Online.NonLimited.WithBusinessScore)
				: typeof(Offline.NonLimited);
		} // ChooseNonLimitedCalculatorWithScore

		private MedalResult SetNoMedal(string errorMessageFormat, params object[] args) {
			Library.Instance.Log.Warn("No medal was calculated for customer {0}.", this.customerId);

			return new MedalResult(this.customerId, Library.Instance.Log, string.Format(errorMessageFormat, args)) {
				CalculationTime = this.calculationTime,
			};
		} // SetNoMedal

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
	} // class MedalCalculator
} // namespace

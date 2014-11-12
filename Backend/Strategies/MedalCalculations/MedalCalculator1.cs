namespace EzBob.Backend.Strategies.MedalCalculations
{
	using System;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class MedalCalculator1
	{
		private readonly AConnection db;
		private readonly ASafeLog log;

		public MedalCalculator1(AConnection db, ASafeLog log)
		{
			this.db = db;
			this.log = log;
		}

		public MedalResult CalculateMedal(int customerId, DateTime calculationTime, string typeOfBusiness, int consumerScore, int companyScore, int numOfHmrcMps, int numOfYodleeMps, int numOfEbayAmazonPayPalMps, DateTime? earliestHmrcLastUpdateDate, DateTime? earliestYodleeLastUpdateDate)
		{
			if (numOfHmrcMps > 0 && earliestHmrcLastUpdateDate.HasValue &&
				earliestHmrcLastUpdateDate.Value.AddDays(CurrentValues.Instance.MedalDaysOfMpRelevancy) < calculationTime)
			{
				return SetNoMedal(customerId, calculationTime, string.Format("Customer has out of date HMRC marketplace that its last update was at:{0}", earliestHmrcLastUpdateDate.Value));
			}

			if (numOfYodleeMps > 0 && earliestYodleeLastUpdateDate.HasValue &&
				earliestYodleeLastUpdateDate.Value.AddDays(CurrentValues.Instance.MedalDaysOfMpRelevancy) < calculationTime)
			{
				return SetNoMedal(customerId, calculationTime, string.Format("Customer has out of date Yodlee marketplace that its last update was at:{0}", earliestYodleeLastUpdateDate.Value));
			}

			if (numOfHmrcMps > 1)
			{
				return SetNoMedal(customerId, calculationTime, string.Format("Customer has more than 1 HMRC marketplace:{0}", numOfHmrcMps));
			}

			if (typeOfBusiness == "LLP" || typeOfBusiness == "Limited")
			{
				if (numOfEbayAmazonPayPalMps > 0)
				{
					var onlineLimitedMedalCalculator1 = new OnlineLimitedMedalCalculator1(db, log);
					return onlineLimitedMedalCalculator1.CalculateMedalScore(customerId, calculationTime);
				}

				var limitedMedalCalculator1 = new LimitedMedalCalculator1(db, log);
				return limitedMedalCalculator1.CalculateMedalScore(customerId, calculationTime);
			}

			if (companyScore > 0)
			{
				if (numOfEbayAmazonPayPalMps > 0)
				{
					var onlineNonLimitedWithBusinessScoreMedalCalculator1 = new OnlineNonLimitedWithBusinessScoreMedalCalculator1(db, log);
					return onlineNonLimitedWithBusinessScoreMedalCalculator1.CalculateMedalScore(customerId, calculationTime);
				}

				var nonLimitedMedalCalculator1 = new NonLimitedMedalCalculator1(db, log);
				return nonLimitedMedalCalculator1.CalculateMedalScore(customerId, calculationTime);
			}

			if (numOfEbayAmazonPayPalMps > 0)
			{
				var onlineNonLimitedNoBusinessScoreMedalCalculator1 = new OnlineNonLimitedNoBusinessScoreMedalCalculator1(db, log);
				return onlineNonLimitedNoBusinessScoreMedalCalculator1.CalculateMedalScore(customerId, calculationTime);
			}

			if (consumerScore > 0 && (numOfHmrcMps > 0 || numOfYodleeMps > 0))
			{
				var soleTraderMedalCalculator1 = new SoleTraderMedalCalculator1(db, log);
				return soleTraderMedalCalculator1.CalculateMedalScore(customerId, calculationTime);
			}

			return SetNoMedal(customerId, calculationTime, "Customer doesn't fit any of the existing medals");
		}

		private MedalResult SetNoMedal(int customerId, DateTime calculationTime, string errorMessage)
		{
			log.Warn("No medal was calculated for customer:{0}", customerId);
			var result = new MedalResult();
			result.CustomerId = customerId;
			result.CalculationTime = calculationTime;
			result.MedalType = MedalType.NoMedal;
			result.Error = errorMessage;
			return result;
		}
	}
}

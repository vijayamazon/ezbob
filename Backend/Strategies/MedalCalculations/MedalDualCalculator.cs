namespace EzBob.Backend.Strategies.MedalCalculations
{
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;
	using System;

	public class MedalDualCalculator
	{
		private readonly ASafeLog log;
		private readonly AConnection db;

		public ScoreResult Results { get; set; }

		public MedalDualCalculator(AConnection db, ASafeLog log)
		{
			this.log = log;
			this.db = db;
		}

		public ScoreResult CalculateMedalScore(int customerId, DateTime calculationTime, string typeOfBusiness, int consumerScore, int companyScore, int numOfHmrcMps, int numOfYodleeMps, int numOfEbayAmazonPayPalMps, DateTime? earliestHmrcLastUpdateDate, DateTime? earliestYodleeLastUpdateDate)
		{
			try
			{
				ScoreResult result1 = null, result2 = null;

				result1 = AssignMedal(customerId, calculationTime, typeOfBusiness, consumerScore, companyScore, numOfHmrcMps, numOfYodleeMps, numOfEbayAmazonPayPalMps, earliestHmrcLastUpdateDate, earliestYodleeLastUpdateDate);
				// TODO: Calculate result2 here

				// TODO: remove these 2 lines
				result1.SaveToDb(db);
				return result1;

				// TODO: uncomment this code
				//if (result1 != null && result2 != null && result1.IsIdentical(result2))
				//{
				//	result1.SaveToDb(db);
				//	return result1;
				//}

				//log.Error("Mismatch found in the 2 medal calculations of customer: {0}", customerId);
				//return null;
			}
			catch (Exception e)
			{
				log.Warn("Medal calculation for customer {0} failed with exception:{1}", customerId, e);
			}

			return null;
		}

		private ScoreResult AssignMedal(int customerId, DateTime calculationTime, string typeOfBusiness, int consumerScore, int companyScore, int numOfHmrcMps, int numOfYodleeMps, int numOfEbayAmazonPayPalMps, DateTime? earliestHmrcLastUpdateDate, DateTime? earliestYodleeLastUpdateDate)
		{
			// TODO: rename LimitedMedalDaysOfMpRelevancy
			if (numOfHmrcMps > 0 && earliestHmrcLastUpdateDate.HasValue &&
				earliestHmrcLastUpdateDate.Value.AddDays(CurrentValues.Instance.LimitedMedalDaysOfMpRelevancy) < calculationTime)
			{
				return SetNoMedal(customerId, calculationTime, string.Format("Customer has out of date HMRC marketplace that its last update was at:{0}", earliestHmrcLastUpdateDate.Value));
			}

			if (numOfYodleeMps > 0 && earliestYodleeLastUpdateDate.HasValue &&
				earliestYodleeLastUpdateDate.Value.AddDays(CurrentValues.Instance.LimitedMedalDaysOfMpRelevancy) < calculationTime)
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

		private ScoreResult SetNoMedal(int customerId, DateTime calculationTime, string errorMessage)
		{
			log.Warn("No medal was calculated for customer:{0}", customerId);
			var result = new ScoreResult();
			result.CustomerId = customerId;
			result.CalculationTime = calculationTime;
			result.MedalType = MedalType.NoMedal;
			result.Error = errorMessage;
			return result;
		}
	}
}

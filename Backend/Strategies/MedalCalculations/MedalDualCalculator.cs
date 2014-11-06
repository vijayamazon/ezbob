namespace EzBob.Backend.Strategies.MedalCalculations
{
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

		public ScoreResult CalculateMedalScore(int customerId, DateTime calculationTime, string typeOfBusiness, int consumerScore, int companyScore, int numOfHmrcMps, int numOfYodleeMps, int numOfEbayAmazonPayPalMps)
		{
			try
			{
				ScoreResult result1 = null, result2 = null;
				bool medalCalculated = false;
				if (typeOfBusiness == "LLP" || typeOfBusiness == "Limited")
				{
					if (numOfEbayAmazonPayPalMps > 0 && numOfHmrcMps < 2)
					{
						var onlineLimitedMedalCalculator1 = new OnlineLimitedMedalCalculator1(db, log);
						result1 = onlineLimitedMedalCalculator1.CalculateMedalScore(customerId, calculationTime);
						// TODO: Calculate result2 here
						medalCalculated = true;
					}
					else if (numOfHmrcMps < 2)
					{
						var limitedMedalCalculator1 = new LimitedMedalCalculator1(db, log);
						result1 = limitedMedalCalculator1.CalculateMedalScore(customerId, calculationTime);
						// TODO: Calculate result2 here
						medalCalculated = true;
					}
				}
				else if (companyScore > 0 && numOfHmrcMps < 2)
				{
					var nonLimitedMedalCalculator1 = new NonLimitedMedalCalculator1(db, log);
					result1 = nonLimitedMedalCalculator1.CalculateMedalScore(customerId, calculationTime);
					// TODO: Calculate result2 here
					medalCalculated = true;
				}

				if (!medalCalculated)
				{
					if (consumerScore > 0 && (numOfHmrcMps > 0 || numOfYodleeMps > 0) && numOfHmrcMps < 2)
					{
						var soleTraderMedalCalculator1 = new SoleTraderMedalCalculator1(db, log);
						result1 = soleTraderMedalCalculator1.CalculateMedalScore(customerId, calculationTime);
						// TODO: Calculate result2 here
					}
					else
					{
						log.Warn("No medal was calculated for customer:{0}", customerId);
						result1 = new ScoreResult();
						result1.CustomerId = customerId;
						result1.CalculationTime = calculationTime;
						result1.MedalType = "NoMedal";
						result1.Error = "Customer doesn't fit any of the existing medals";
					}
				}

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
	}
}

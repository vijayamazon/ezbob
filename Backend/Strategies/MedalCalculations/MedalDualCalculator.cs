namespace EzBob.Backend.Strategies.MedalCalculations
{
	using Ezbob.Database;
	using Ezbob.Logger;
	using System;

	public class MedalDualCalculator
	{
		private readonly ASafeLog log;
		private readonly AConnection db;
		private readonly MedalCalculator1 medalCalculator1;

		public MedalResult Results { get; set; }

		public MedalDualCalculator(AConnection db, ASafeLog log)
		{
			this.log = log;
			this.db = db;

			medalCalculator1 = new MedalCalculator1(db, log);
		}

		public MedalResult CalculateMedalScore(int customerId, DateTime calculationTime, string typeOfBusiness, int consumerScore, int companyScore, int numOfHmrcMps, int numOfYodleeMps, int numOfEbayAmazonPayPalMps, DateTime? earliestHmrcLastUpdateDate, DateTime? earliestYodleeLastUpdateDate)
		{
			try
			{
				MedalResult result1 = null, result2 = null;

				result1 = medalCalculator1.CalculateMedal(customerId, calculationTime, typeOfBusiness, consumerScore, companyScore, numOfHmrcMps, numOfYodleeMps, numOfEbayAmazonPayPalMps, earliestHmrcLastUpdateDate, earliestYodleeLastUpdateDate);
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
	}
}

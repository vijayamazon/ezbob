namespace EzBob.Backend.Strategies.OfferCalculation
{
	using Ezbob.Database;
	using Ezbob.Logger;
	using System;
	using MedalCalculations;

	public class OfferDualCalculator
	{
		private readonly ASafeLog log;
		private readonly AConnection db;
		private readonly OfferCalculator1 offerCalculator1;

		public OfferDualCalculator(AConnection db, ASafeLog log)
		{
			this.log = log;
			this.db = db;

			offerCalculator1 = new OfferCalculator1(db, log);
		}

		public OfferResult CalculateOffer(int customerId, DateTime calculationTime, int amount, bool hasLoans, MedalClassification medalClassification)
		{
			try
			{
				OfferResult result1 = null, result2 = null;

				result1 = offerCalculator1.CalculateOffer(customerId, calculationTime, amount, hasLoans, medalClassification);
				// TODO: Calculate result2 here

				// TODO: remove these 2 lines
				//result1.SaveToDb(db);
				return result1;

				// TODO: uncomment this code
				//if (result1 != null && result2 != null && result1.IsIdentical(result2))
				//{
				//	result1.SaveToDb(db);
				//	return result1;
				//}

				//log.Error("Mismatch found in the 2 offer calculations of customer: {0}", customerId);
				//return null;
			}
			catch (Exception e)
			{
				log.Warn("Offer calculation for customer {0} failed with exception:{1}", customerId, e);
			}

			return null;
		}
	}
}

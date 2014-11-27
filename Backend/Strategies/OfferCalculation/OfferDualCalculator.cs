namespace EzBob.Backend.Strategies.OfferCalculation
{
	using AutomationCalculator.Common;
	using AutomationCalculator.OfferCalculation;
	using Ezbob.Database;
	using Ezbob.Logger;
	using System;
	using MedalCalculations;

	public class OfferDualCalculator
	{
		private readonly ASafeLog log;
		private readonly AConnection db;
		private readonly OfferCalculator1 offerCalculator1;
		private readonly OfferCalculator offerCalculator2;

		public OfferDualCalculator(AConnection db, ASafeLog log)
		{
			this.log = log;
			this.db = db;

			offerCalculator1 = new OfferCalculator1(db, log);
			offerCalculator2 = new OfferCalculator(log);
		}

		public OfferResult CalculateOffer(int customerId, DateTime calculationTime, int amount, bool hasLoans, MedalClassification medalClassification)
		{
			try
			{
				OfferResult result1 = offerCalculator1.CalculateOffer(customerId, calculationTime, amount, hasLoans, medalClassification);

				var medal = (Medal) Enum.Parse(typeof (Medal), medalClassification.ToString());

				var input = new OfferInputModel {
					Amount = amount,
					AspireToMinSetupFee = ConfigManager.CurrentValues.Instance.AspireToMinSetupFee,
					HasLoans = hasLoans,
					Medal = medal,
					CustomerId = customerId
				};

				OfferOutputModel result2 = offerCalculator2.GetOfferBySeek(input);
				OfferOutputModel result3 = offerCalculator2.GetOfferByBoundaries(input);

				result2.SaveToDb(log, db, OfferCalculationType.Seek);
				result3.SaveToDb(log, db, OfferCalculationType.Boundaries);

				if (!result2.Equals(result3)) {
					log.Info("the two verification implementations mismatch \nby seek:\n {0}\nby boundaries\n {1}", result2, result3);
				}

				if (result1.Equals(result2)) {
					log.Debug("Main implementation of offer calculation result: \n{0}", result1);
					result1.SaveToDb(db);
					return result1;
				}
				
				result1.SaveToDb(db);
				log.Error("Mismatch found in the 2 offer calculations of customer: {0} ", customerId);
				return null;
			}
			catch (Exception e)
			{
				log.Warn("Offer calculation for customer {0} failed with exception:{1}", customerId, e);
			}

			return null;
		}
	}
}

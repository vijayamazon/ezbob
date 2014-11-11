namespace EzBob.Backend.Strategies.OfferCalculation
{
	using Ezbob.Database;
	using Ezbob.Logger;
	using System;
	using MedalCalculations;
	using PricingModel;

	public class OfferDualCalculator
	{
		private readonly ASafeLog log;
		private readonly AConnection db;

		public OfferResult Results { get; set; }

		public OfferDualCalculator(AConnection db, ASafeLog log)
		{
			this.log = log;
			this.db = db;
		}

		public OfferResult CalculateOffer(int customerId, DateTime calculationTime, int amount, MedalClassification medalClassification)
		{
			try
			{
				OfferResult result1 = null, result2 = null;

				result1 = CalculateOffer1(customerId, calculationTime, amount, medalClassification);
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

		private OfferResult CalculateOffer1(int customerId, DateTime calculationTime, int amount, MedalClassification medalClassification)
		{
			var result = new OfferResult
				{
					CustomerId = customerId,
					CalculationTime = calculationTime,
					Amount = amount,
					MedalClassification = medalClassification
				};

			// Period is always 12
			result.Period = 12;

			// We always use standard loan type
			result.IsEu = false;

			// TODO: vitas to define rules for scenario usage
			result.ScenarioName = "Basic";
			// TODO: load min max setup fee
			// TODO: load min max interest rate

			decimal interestRate, setupFee;

			CalculateInterestRateAndSetupFee(customerId, result.ScenarioName, out interestRate, out setupFee);

			result.InterestRate = interestRate;
			result.SetupFee = setupFee;
			
			return result;
		}

		private void CalculateInterestRateAndSetupFee(int customerId, string scenarioName, out decimal interestRate, out decimal setupFee)
		{
			// TODO: implement
			interestRate = 1;
			setupFee = 100;

			// should use pricing model
			//var instance = new GetPricingModelModel(customerId, scenarioName, db, log);
		}
	}
}

namespace EzBob.Backend.Strategies.MedalCalculations
{
	using AutomationCalculator.Common;
	using AutomationCalculator.MedalCalculation;
	using Ezbob.Database;
	using Ezbob.Logger;
	using System;

	public class MedalDualCalculator
	{
		private readonly ASafeLog log;
		private readonly AConnection db;
		private readonly MedalCalculator1 medalCalculator1;
		private readonly MedalChooser medalCalculatorVerification;
		public MedalResult Results { get; set; }

		public MedalDualCalculator(AConnection db, ASafeLog log)
		{
			this.log = log;
			this.db = db;

			medalCalculator1 = new MedalCalculator1(db, log);
			medalCalculatorVerification = new MedalChooser(log);
		}

		public MedalResult CalculateMedalScore(int customerId, DateTime calculationTime, string typeOfBusiness, int consumerScore, int companyScore, int numOfHmrcMps, int numOfYodleeMps, int numOfEbayAmazonPayPalMps, DateTime? earliestHmrcLastUpdateDate, DateTime? earliestYodleeLastUpdateDate)
		{
			try
			{
				MedalResult result1 = medalCalculator1.CalculateMedal(customerId, calculationTime, typeOfBusiness, consumerScore, companyScore, numOfHmrcMps, numOfYodleeMps, numOfEbayAmazonPayPalMps, earliestHmrcLastUpdateDate, earliestYodleeLastUpdateDate);
				MedalOutputModel result2 = medalCalculatorVerification.GetMedal(customerId, calculationTime);
				
				result2.SaveToDb(log);

				if (result1 != null && result1.IsIdentical(result2)) {
					result1.SaveToDb(db);
					return result1;
				}
				
				//Mismatch in medal calculations
				if (result1 == null) {
					result1 = new MedalResult{ CustomerId =  customerId};
				}
				result1.PrintToLog(log);
				result1.MedalClassification = MedalClassification.NoClassification;
				result1.Error = "Mismatch found in the 2 medal calculations";
				result1.SaveToDb(db);
				

				log.Error("Mismatch found in the 2 medal calculations of customer: {0}", customerId);
				return null;
			}
			catch (Exception e)
			{
				log.Warn("Medal calculation for customer {0} failed with exception:{1}", customerId, e);
			}

			return null;
		}
	}
}

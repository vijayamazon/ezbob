namespace Ezbob.Backend.Strategies.MedalCalculations 
{
	using System;
	using Ezbob.Database;

	// This strategy assumes that the table MedalCalculations is empty
	// It will go over all the customers in the DB and fill the medal for them
	public class Temp_BackFillMedals : AStrategy
	{
		public override string Name {
			get { return "Temp_BackFillMedals"; }
		}

		public override void Execute()
		{
			DB.ForEachRowSafe(
				(sr, bRowsetStart) =>
				{
					string typeOfBusiness = sr["TypeOfBusiness"];
					int numOfEbayAmazonPayPalMps = sr["NumOfEbayAmazonPayPalMps"];
					int numOfHmrcMps = sr["NumOfHmrcMps"];
					int companyScore = sr["CompanyScore"];
					int numOfYodleeMps = sr["NumOfYodleeMps"];
					int consumerScore = sr["ConsumerScore"];
					int customerId = sr["CustomerId"];
					DateTime? earliestHmrcLastUpdateDate = sr["EarliestHmrcLastUpdateDate"];
					DateTime? earliestYodleeLastUpdateDate = sr["EarliestYodleeLastUpdateDate"];

					Log.Info("Will calculate medal for customer:{0}", customerId);
					
					try
					{
						var instance = new CalculateMedal(customerId, typeOfBusiness, consumerScore, companyScore, numOfHmrcMps, numOfYodleeMps, numOfEbayAmazonPayPalMps, earliestHmrcLastUpdateDate, earliestYodleeLastUpdateDate);
						instance.Execute();
					}
					catch (Exception e)
					{
						Log.Error("Exception during medal calculation for customer:{0} The exception:{1}", customerId, e);
					}
					return ActionResult.Continue;
				},
				"Temp_GetAllCustomersForMedalComparison",
				CommandSpecies.StoredProcedure
			);
		}
	}
}

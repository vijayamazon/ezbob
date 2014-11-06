namespace EzBob.Backend.Strategies.MedalCalculations 
{
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	// This strategy assumes that the table MedalCalculations is empty
	// It will go over all the customers in the DB and fill the medal for them
	public class Temp_BackFillMedals : AStrategy
	{
		public Temp_BackFillMedals(AConnection db, ASafeLog log)
			: base(db, log)
		{
		}

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
					
					try
					{
						var instance = new CalculateMedal(DB, Log, customerId, typeOfBusiness, consumerScore, companyScore, numOfHmrcMps, numOfYodleeMps, numOfEbayAmazonPayPalMps);
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

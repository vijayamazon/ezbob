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
						bool medalCalculated = false;
						if (typeOfBusiness == "LLP" || typeOfBusiness == "Limited")
						{
							if (numOfEbayAmazonPayPalMps > 0)
							{
								var instance = new CalculateOnlineLimitedMedal(DB, Log, customerId);
								instance.Execute();
								medalCalculated = true;
							}
							else if (numOfHmrcMps < 2)
							{
								var instance = new CalculateLimitedMedal(DB, Log, customerId);
								instance.Execute();
								medalCalculated = true;
							}
						}
						else if (companyScore > 0 && numOfHmrcMps < 2)
						{
							var instance = new CalculateNonLimitedMedal(DB, Log, customerId);
							instance.Execute();
							medalCalculated = true;
						}

						if (!medalCalculated)
						{
							if (consumerScore > 0 && (numOfHmrcMps > 0 || numOfYodleeMps > 0) && numOfHmrcMps < 2)
							{
								var instance = new CalculateSoleTraderMedal(DB, Log, customerId);
								instance.Execute();
							}
							else
							{
								Log.Warn("No new medal was calculated for customer:{0}", customerId);
							}
						}
					}
					catch (Exception e)
					{
						Log.Error("Exception during medal calculation for customer:{0}", customerId);
					}
					return ActionResult.SkipAll;
				},
				"Temp_GetAllCustomersForMedalComparison",
				CommandSpecies.StoredProcedure
			);
		}
	}
}

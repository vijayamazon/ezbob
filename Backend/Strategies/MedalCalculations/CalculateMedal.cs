namespace EzBob.Backend.Strategies.MedalCalculations 
{
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class CalculateMedal : AStrategy
	{
		private readonly MedalDualCalculator medalDualCalculator;
		private readonly int customerId;
		private readonly string typeOfBusiness;
		private readonly int consumerScore;
		private readonly int companyScore;
		private readonly int numOfHmrcMps;
		private readonly int numOfYodleeMps;
		private readonly int numOfEbayAmazonPayPalMps;
		private readonly DateTime? earliestHmrcLastUpdateDate;
		private readonly DateTime? earliestYodleeLastUpdateDate;

		public CalculateMedal(AConnection db, ASafeLog log, int customerId)
			: base(db, log)
		{
			this.customerId = customerId;
			SafeReader sr = DB.GetFirst(
				"GetCustomerDataForMedalCalculation",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId));

			if (!sr.IsEmpty)
			{
				typeOfBusiness = sr["TypeOfBusiness"];
				consumerScore = sr["ConsumerScore"];
				companyScore = sr["CompanyScore"];
				numOfHmrcMps = sr["NumOfHmrcMps"];
				numOfYodleeMps = sr["NumOfYodleeMps"];
				numOfEbayAmazonPayPalMps = sr["NumOfEbayAmazonPayPalMps"];
				earliestHmrcLastUpdateDate = sr["EarliestHmrcLastUpdateDate"];
				earliestYodleeLastUpdateDate = sr["EarliestYodleeLastUpdateDate"];
			}
			medalDualCalculator = new MedalDualCalculator(db, log);
		}

		public CalculateMedal(AConnection db, ASafeLog log, int customerId, string typeOfBusiness, int consumerScore, int companyScore, int numOfHmrcMps, int numOfYodleeMps, int numOfEbayAmazonPayPalMps, DateTime? earliestHmrcLastUpdateDate, DateTime? earliestYodleeLastUpdateDate)
			: base(db, log)
		{
			this.customerId = customerId;
			this.typeOfBusiness = typeOfBusiness;
			this.consumerScore = consumerScore;
			this.companyScore = companyScore;
			this.numOfHmrcMps = numOfHmrcMps;
			this.numOfYodleeMps = numOfYodleeMps;
			this.numOfEbayAmazonPayPalMps = numOfEbayAmazonPayPalMps;
			this.earliestHmrcLastUpdateDate = earliestHmrcLastUpdateDate;
			this.earliestYodleeLastUpdateDate = earliestYodleeLastUpdateDate;
			medalDualCalculator = new MedalDualCalculator(db, log);
		}

		public override string Name {
			get { return "CalculateMedal"; }
		}

		public ScoreResult Result { get; private set; }

		public override void Execute()
		{
			Result = medalDualCalculator.CalculateMedalScore(customerId, DateTime.UtcNow, typeOfBusiness, consumerScore, companyScore, numOfHmrcMps, numOfYodleeMps, numOfEbayAmazonPayPalMps, earliestHmrcLastUpdateDate, earliestYodleeLastUpdateDate);
		}
	}
}

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
			}
			medalDualCalculator = new MedalDualCalculator(db, log);
		}

		public CalculateMedal(AConnection db, ASafeLog log, int customerId, string typeOfBusiness, int consumerScore, int companyScore, int numOfHmrcMps, int numOfYodleeMps, int numOfEbayAmazonPayPalMps)
			: base(db, log)
		{
			this.customerId = customerId;
			this.typeOfBusiness = typeOfBusiness;
			this.consumerScore = consumerScore;
			this.companyScore = companyScore;
			this.numOfHmrcMps = numOfHmrcMps;
			this.numOfYodleeMps = numOfYodleeMps;
			this.numOfEbayAmazonPayPalMps = numOfEbayAmazonPayPalMps;
			medalDualCalculator = new MedalDualCalculator(db, log);
		}

		public override string Name {
			get { return "CalculateMedal"; }
		}

		public ScoreResult Result { get; private set; }

		public override void Execute()
		{
			Result = medalDualCalculator.CalculateMedalScore(customerId, DateTime.UtcNow, typeOfBusiness, consumerScore, companyScore, numOfHmrcMps, numOfYodleeMps, numOfEbayAmazonPayPalMps);
		}
	}
}

namespace EzBob.Backend.Strategies.PricingModel
{
	using Experian;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class GetPricingModelDefaultRate : AStrategy
	{
		private readonly decimal companyShare;
		private readonly int customerId;

		public GetPricingModelDefaultRate(int customerId, decimal companyShare, AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			this.companyShare = companyShare;
			this.customerId = customerId;
		}

		public override string Name {
			get { return "Get pricing model default rate"; }
		}

		public decimal DefaultRate { get; private set; }
		
		public override void Execute()
		{
			decimal customerShare = 1 - companyShare;
			
			var sr = DB.GetFirst("GetOfferConsumerBusinessDefaultRates", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", customerId));

			decimal companyValue = sr["BusinessDefaultRate"];
			decimal customerValue = sr["ConsumerDefaultRate"];

			DefaultRate = companyShare * companyValue + customerShare * customerValue;
		}
	}
}

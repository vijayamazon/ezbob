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
			var scoreStrat = new GetExperianConsumerScore(customerId, DB, Log);
			scoreStrat.Execute();

			int consumerScore = scoreStrat.Score;

			int companyScore = DB.ExecuteScalar<int>(
				"GetCompanyScore",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);

			decimal companyValue = GetDefaultRateCompany(companyScore);
			decimal customerValue = GetDefaultRateCustomer(consumerScore);

			DefaultRate = companyShare * companyValue + customerShare * customerValue;
		}

		private decimal GetDefaultRateCompany(int key)
		{
			SafeReader sr = DB.GetFirst("GetConfigTableValue", CommandSpecies.StoredProcedure, new QueryParameter("ConfigTableName", "DefaultRateCompany"), new QueryParameter("Key", key));
			return sr["Value"];
		}

		private decimal GetDefaultRateCustomer(int key)
		{
			SafeReader sr = DB.GetFirst("GetConfigTableValue", CommandSpecies.StoredProcedure, new QueryParameter("ConfigTableName", "DefaultRateCustomer"), new QueryParameter("Key", key));
			return sr["Value"];
		}
	}
}

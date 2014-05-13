namespace EzBob.Backend.Strategies.PricingModel
{
	using System.Data;
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

			int consumerScore = DB.ExecuteScalar<int>(
				"GetExperianScore",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);

			int companyScore = 0;
			DataTable dt = DB.ExecuteReader(
				"GetCompanyScore",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);

			if (dt.Rows.Count == 1)
			{
				var sr = new SafeReader(dt.Rows[0]);
				companyScore = sr["Score"];
			}

			decimal companyValue = GetDefaultRateCompany(companyScore);
			decimal customerValue = GetDefaultRateCustomer(consumerScore);

			DefaultRate = companyShare * companyValue + customerShare * customerValue;
		}

		private decimal GetDefaultRateCompany(int key)
		{
			DataTable dt = DB.ExecuteReader("GetConfigTableValue", CommandSpecies.StoredProcedure, new QueryParameter("ConfigTableName", "DefaultRateCompany"), new QueryParameter("Key", key));
			var sr = new SafeReader(dt.Rows[0]);
			return sr["Value"];
		}

		private decimal GetDefaultRateCustomer(int key)
		{
			DataTable dt = DB.ExecuteReader("GetConfigTableValue", CommandSpecies.StoredProcedure, new QueryParameter("ConfigTableName", "DefaultRateCustomer"), new QueryParameter("Key", key));
			var sr = new SafeReader(dt.Rows[0]);
			return sr["Value"];
		}
	}
}

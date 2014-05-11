namespace EzBob.Backend.Strategies.PricingModel
{
	using System.Data;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class GetPricingModelModel : AStrategy
	{
		private readonly int customerId;

		public GetPricingModelModel(int customerId, AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			this.customerId = customerId;
		}

		public override string Name {
			get { return "Get pricing model model"; }
		}

		public PricingModelModel Model { get; private set; }
		
		public override void Execute()
		{
			decimal defaultRateCompanyShare, defaultRateCustomerShare;
			decimal defaultRate = GetDefaultRate(out defaultRateCompanyShare, out defaultRateCustomerShare);

			Model = new PricingModelModel
				{
					LoanAmount = customerId,
					DefaultRate = defaultRate,
					DefaultRateCompanyShare = defaultRateCompanyShare,
					DefaultRateCustomerShare = defaultRateCustomerShare,
					SetupFeePounds = 150,
					SetupFeePercents = 0.07m,
					LoanTerm = 12,
					InterestOnlyPeriod = 3,
					TenurePercents = 0.5m,
					TenureMonths = 0.22m,
					CollectionRate = 6.88m,
					Cogs = 1000,
					DebtPercentOfCapital = 0.65m,
					CostOfDebt = 0.65m,
					OpexAndCapex = 180,
					ProfitMarkup = 0.29m
				};
		}

		private decimal GetDefaultRate(out decimal defaultRateCompanyShare, out decimal defaultRateCustomerShare)
		{
			defaultRateCompanyShare = CurrentValues.Instance.PricingModelDefaultRateCompanyShare;
			defaultRateCustomerShare = 1 - defaultRateCompanyShare;

			int companyScore = 0;
			int consumerScore = DB.ExecuteScalar<int>(
				"GetExperianScore",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);

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

			return 100 * (defaultRateCompanyShare*companyValue + defaultRateCustomerShare*customerValue);
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

namespace EzBob.Backend.Strategies.PricingModel
{
	using System.Data;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class GetPricingModelModel : AStrategy
	{
		private readonly int customerId;
		private decimal tenurePercents;
		private decimal setupFee;
		private decimal profitMarkupPercentsOfRevenue;
		private decimal opexAndCapex;
		private int interestOnlyPeriod;
		private decimal euCollectionRate;
		private decimal defaultRateCompanyShare;
		private decimal debtPercentOfCapital;
		private decimal costOfDebtPA;
		private decimal collectionRate;
		private decimal cogs;
		private decimal brokerSetupFee;

		public GetPricingModelModel(int customerId, string scenarioName, AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			this.customerId = customerId;
			ReadConfigurations(scenarioName);
		}

		private void ReadConfigurations(string scenarioName)
		{
			DataTable dt = DB.ExecuteReader(
				   "GetPricingModelConfigsForScenario",
				   CommandSpecies.StoredProcedure,
				   new QueryParameter("ScenarioName", scenarioName)
			   );
			if (dt.Rows.Count == 1)
			{
				var sr = new SafeReader(dt.Rows[0]);
				tenurePercents = sr["TenurePercents"];
				setupFee = sr["SetupFee"];
				profitMarkupPercentsOfRevenue = sr["ProfitMarkupPercentsOfRevenue"];
				opexAndCapex = sr["OpexAndCapex"];
				interestOnlyPeriod = sr["InterestOnlyPeriod"];
				euCollectionRate = sr["EuCollectionRate"];
				defaultRateCompanyShare = sr["DefaultRateCompanyShare"];
				debtPercentOfCapital = sr["DebtPercentOfCapital"];
				costOfDebtPA = sr["CostOfDebtPA"];
				collectionRate = sr["CollectionRate"];
				cogs = sr["Cogs"];
				brokerSetupFee = sr["BrokerSetupFee"];
			}
		}

		public override string Name {
			get { return "Get pricing model model"; }
		}

		public PricingModelModel Model { get; private set; }
		
		public override void Execute()
		{
			decimal defaultRateCustomerShare;
			decimal defaultRate = GetDefaultRate(out defaultRateCustomerShare);
			int loanAmount, loanTerm;
			GetDataFromCashRequest(out loanAmount, out loanTerm);
			decimal tenureMonths = tenurePercents * loanTerm;
			
			Model = new PricingModelModel
				{
					LoanAmount = loanAmount,
					DefaultRate = defaultRate,
					DefaultRateCompanyShare = defaultRateCompanyShare,
					DefaultRateCustomerShare = defaultRateCustomerShare,
					SetupFeePounds = setupFee * loanAmount,
					SetupFeePercents = setupFee,
					BrokerSetupFeePounds = brokerSetupFee * loanAmount,
					BrokerSetupFeePercents = brokerSetupFee,
					LoanTerm = loanTerm,
					InterestOnlyPeriod = interestOnlyPeriod,
					TenurePercents = tenurePercents,
					TenureMonths = tenureMonths,
					CollectionRate = collectionRate,
					EuCollectionRate = euCollectionRate,
					Cogs = cogs,
					DebtPercentOfCapital = debtPercentOfCapital,
					CostOfDebt = costOfDebtPA,
					OpexAndCapex = opexAndCapex,
					ProfitMarkup = profitMarkupPercentsOfRevenue
				};
		}

		private void GetDataFromCashRequest(out int loanAmount, out int loanTerm)
		{
			loanAmount = 0;
			loanTerm = 12;
			DataTable dt = DB.ExecuteReader(
				"GetLastCashRequestForPricingModel",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);

			if (dt.Rows.Count == 1)
			{
				var sr = new SafeReader(dt.Rows[0]);
				loanAmount = sr["ApprovedAmount"];
				loanTerm = sr["RepaymentPeriod"];
			}
		}

		private decimal GetDefaultRate(out decimal defaultRateCustomerShare)
		{
			defaultRateCustomerShare = 1 - defaultRateCompanyShare;

			var instance = new GetPricingModelDefaultRate(customerId, defaultRateCompanyShare, DB, Log);
			instance.Execute();
			return instance.DefaultRate;
		}
	}
}

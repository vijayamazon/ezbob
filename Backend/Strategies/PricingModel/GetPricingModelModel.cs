namespace Ezbob.Backend.Strategies.PricingModel {
	using Ezbob.Database;

	public class GetPricingModelModel : AStrategy {
		private readonly int customerId;
		private decimal tenurePercents;
		private decimal setupFee;
		private decimal profitMarkupPercentsOfRevenue;
		private decimal opexAndCapex;
		private int interestOnlyPeriod;
		private decimal euCollectionRate;
		private decimal cosmeCollectionRate;
		private decimal defaultRateCompanyShare;
		private decimal debtPercentOfCapital;
		private decimal costOfDebtPA;
		private decimal collectionRate;
		private decimal cogs;
		private decimal brokerSetupFee;
		

		public GetPricingModelModel(int customerId, string scenarioName) {
			this.customerId = customerId;
			ReadConfigurations(scenarioName);
		}

		private void ReadConfigurations(string scenarioName) {
			SafeReader sr = DB.GetFirst(
				   "GetPricingModelConfigsForScenario",
				   CommandSpecies.StoredProcedure,
				   new QueryParameter("ScenarioName", scenarioName)
			   );
			if (!sr.IsEmpty) {
				tenurePercents = sr["TenurePercents"];
				setupFee = sr["SetupFee"];
				profitMarkupPercentsOfRevenue = sr["ProfitMarkupPercentsOfRevenue"];
				opexAndCapex = sr["OpexAndCapex"];
				interestOnlyPeriod = sr["InterestOnlyPeriod"];
				euCollectionRate = sr["EuCollectionRate"];
				cosmeCollectionRate = sr["COSMECollectionRate"];
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

		public override void Execute() {
			decimal defaultRateCustomerShare;
			decimal defaultRate = GetDefaultRate(out defaultRateCustomerShare);
			int loanAmount, loanTerm;
			GetDataFromCashRequest(out loanAmount, out loanTerm);
			decimal tenureMonths = tenurePercents * loanTerm;

			Model = new PricingModelModel {
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
				CosmeCollectionRate = cosmeCollectionRate,
				Cogs = cogs,
				DebtPercentOfCapital = debtPercentOfCapital,
				CostOfDebt = costOfDebtPA,
				OpexAndCapex = opexAndCapex,
				ProfitMarkup = profitMarkupPercentsOfRevenue
			};
		}

		private void GetDataFromCashRequest(out int loanAmount, out int loanTerm) {
			loanAmount = 0;
			loanTerm = 12;
			SafeReader sr = DB.GetFirst(
				"GetLastCashRequestForPricingModel",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);

			if (!sr.IsEmpty) {
				loanAmount = sr["ApprovedAmount"];
				loanTerm = sr["RepaymentPeriod"];
			}
		}

		private decimal GetDefaultRate(out decimal defaultRateCustomerShare) {
			defaultRateCustomerShare = 1 - defaultRateCompanyShare;

			var instance = new GetPricingModelDefaultRate(customerId, defaultRateCompanyShare);
			instance.Execute();
			return instance.DefaultRate;
		}
	}
}

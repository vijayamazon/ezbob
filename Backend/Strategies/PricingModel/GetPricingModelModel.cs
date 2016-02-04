namespace Ezbob.Backend.Strategies.PricingModel {
	using Ezbob.Database;

	public class GetPricingModelModel : AStrategy {
		public GetPricingModelModel(int customerId, string scenarioName) {
			this.customerId = customerId;
			ReadConfigurations(scenarioName);
		} // constructor

		public override string Name {
			get { return "Get pricing model model"; }
		} // Name

		public PricingModelModel Model { get; private set; }

		public override void Execute() {
			decimal defaultRateCustomerShare;
			var defaultRateModel = GetDefaultRate(out defaultRateCustomerShare);
			int loanAmount, loanTerm;
			GetDataFromCashRequest(out loanAmount, out loanTerm);
			decimal tenureMonths = this.tenurePercents * loanTerm;

			Model = new PricingModelModel {
				LoanAmount = loanAmount,
				DefaultRate = defaultRateModel.DefaultRate,
				DefaultRateCompanyShare = this.defaultRateCompanyShare,
				DefaultRateCustomerShare = defaultRateCustomerShare,
				SetupFeePercents = this.setupFee,
				BrokerSetupFeePercents = this.brokerSetupFee,
				LoanTerm = loanTerm,
				InterestOnlyPeriod = this.interestOnlyPeriod,
				TenurePercents = this.tenurePercents,
				TenureMonths = tenureMonths,
				CollectionRate = this.collectionRate,
				EuCollectionRate = this.euCollectionRate,
				CosmeCollectionRate = this.cosmeCollectionRate,
				Cogs = this.cogs,
				DebtPercentOfCapital = this.debtPercentOfCapital,
				CostOfDebt = this.costOfDebtPA,
				OpexAndCapex = this.opexAndCapex,
				ProfitMarkup = this.profitMarkupPercentsOfRevenue,
				ConsumerScore = defaultRateModel.ConsumerScore,
				CompanyScore = defaultRateModel.BusinessScore,
			};
		} // Execute

		private void ReadConfigurations(string scenarioName) {
			SafeReader sr = DB.GetFirst(
				"GetPricingModelConfigsForScenario",
				CommandSpecies.StoredProcedure,
				new QueryParameter("ScenarioName", scenarioName)
			);

			if (!sr.IsEmpty) {
				this.tenurePercents = sr["TenurePercents"];
				this.setupFee = sr["SetupFee"];
				this.profitMarkupPercentsOfRevenue = sr["ProfitMarkupPercentsOfRevenue"];
				this.opexAndCapex = sr["OpexAndCapex"];
				this.interestOnlyPeriod = sr["InterestOnlyPeriod"];
				this.euCollectionRate = sr["EuCollectionRate"];
				this.cosmeCollectionRate = sr["COSMECollectionRate"];
				this.defaultRateCompanyShare = sr["DefaultRateCompanyShare"];
				this.debtPercentOfCapital = sr["DebtPercentOfCapital"];
				this.costOfDebtPA = sr["CostOfDebtPA"];
				this.collectionRate = sr["CollectionRate"];
				this.cogs = sr["Cogs"];
				this.brokerSetupFee = sr["BrokerSetupFee"];
			} // if
		} // ReadConfigurations

		private void GetDataFromCashRequest(out int loanAmount, out int loanTerm) {
			loanAmount = 0;
			loanTerm = 12;

			SafeReader sr = DB.GetFirst(
				"GetLastCashRequestForPricingModel",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerId)
			);

			if (!sr.IsEmpty) {
				loanAmount = sr["ApprovedAmount"];
				loanTerm = sr["RepaymentPeriod"];
			} // if
		} // GetDataFromCashRequest

		private GetPricingModelDefaultRate GetDefaultRate(out decimal defaultRateCustomerShare) {
			defaultRateCustomerShare = 1 - this.defaultRateCompanyShare;

			var instance = new GetPricingModelDefaultRate(this.customerId, this.defaultRateCompanyShare);
			instance.Execute();
			return instance;
		} // GetDefaultRate

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
	} // class GetPricingModelModel
} // namespace

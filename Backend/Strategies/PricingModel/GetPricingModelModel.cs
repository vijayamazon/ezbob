namespace Ezbob.Backend.Strategies.PricingModel {
	using Ezbob.Database;

	public class GetPricingModelModel : AStrategy {
		public GetPricingModelModel(int customerId, string scenarioName) {
			this.customerId = customerId;
			this.scenarioName = scenarioName;

			Model = new PricingModelModel();
		} // constructor

		public override string Name {
			get { return "Get pricing model model"; }
		} // Name

		public PricingModelModel Model { get; private set; }

		public override void Execute() {
			var sr = DB.GetFirst(
				"GetPricingModelConfigsForScenario",
				CommandSpecies.StoredProcedure,
				new QueryParameter("ScenarioName", this.scenarioName),
				new QueryParameter("CustomerID", this.customerId)
			);

			if (!sr.IsEmpty)
				sr.Stuff(Model);

			AppendDataFromCashRequest();
			AppendDefaultRate();
			SetCustomerOriginID();
		} // Execute

		private void AppendDataFromCashRequest() {
			int loanAmount = 0;
			int loanTerm = 12;

			SafeReader sr = DB.GetFirst(
				"GetLastCashRequestForPricingModel",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerId)
			);

			if (!sr.IsEmpty) {
				loanAmount = sr["ApprovedAmount"];
				loanTerm = sr["RepaymentPeriod"];
			} // if

			Model.LoanAmount = loanAmount;
			Model.LoanTerm = loanTerm;
			Model.TenureMonths = Model.TenurePercents * loanTerm;
		} // AppendDataFromCashRequest

		private void AppendDefaultRate() {
			var instance = new GetPricingModelDefaultRate(this.customerId, Model.DefaultRateCompanyShare);
			instance.Execute();

			Model.DefaultRateCustomerShare = 1 - Model.DefaultRateCompanyShare;
			Model.DefaultRate = instance.DefaultRate;
			Model.ConsumerScore = instance.ConsumerScore;
			Model.CompanyScore = instance.BusinessScore;
		} // AppendDefaultRate

		private void SetCustomerOriginID() {
			Model.OriginID = DB.ExecuteScalar<int>(
				"GetCustomerOrigin",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerId)
			);
		} // SetCustomerOriginID

		private readonly int customerId;
		private readonly string scenarioName;
	} // class GetPricingModelModel
} // namespace

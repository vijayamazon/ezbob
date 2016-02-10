namespace Ezbob.Backend.Strategies.PricingModel {
	using System;
	using DbConstants;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Database;
	using Ezbob.Utils.Extensions;

	public class GetPricingModelModel : AStrategy {
		public GetPricingModelModel(int customerId, PricingCalcuatorScenarioNames name) {
			this.customerId = customerId;
			this.scenarioName = name.DescriptionAttr();
			LoadFromLastCashRequest = true;

			Model = new PricingModelModel();
			Error = null;
		} // constructor

		public override string Name {
			get { return "Get pricing model model"; }
		} // Name

		public PricingModelModel Model { get; private set; }

		public string Error { get; private set; }

		public bool LoadFromLastCashRequest { get; set; }

		public override void Execute() {
			try {
				var sr = DB.GetFirst(
					"GetPricingModelConfigsForScenario",
					CommandSpecies.StoredProcedure,
					new QueryParameter("ScenarioName", this.scenarioName),
					new QueryParameter("CustomerID", this.customerId)
				);

				if (!sr.IsEmpty)
					sr.Stuff(Model);
				else {
					Error = string.Format(
						"Failed to load configuration for scenario '{0}' and origin of the customer {1}.",
						this.scenarioName,
						this.customerId
					);
					return;
				} // if

				AppendDataFromCashRequest();
				AppendDefaultRate();
				SetCustomerOriginID();
			} catch (Exception e) {
				Error = e.Message;
				Log.Alert(e, "Exception during creating pricing calculator model.");
			} // try
		} // Execute

		private void AppendDataFromCashRequest() {
			if (!LoadFromLastCashRequest)
				return;

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
		} // AppendDataFromCashRequest

		private void AppendDefaultRate() {
			var instance = new GetPricingModelDefaultRate(this.customerId, Model);
			instance.Execute();
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

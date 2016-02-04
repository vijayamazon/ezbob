namespace EzService.EzServiceImplementation {
	using Ezbob.Backend.Strategies.PricingModel;
	using EzService.ActionResults;

	partial class EzServiceImplementation {
		public PricingModelModelActionResult GetPricingModelModel(int customerId, int underwriterId, string scenarioName) {
			GetPricingModelModel instance;
			ActionMetaData result = ExecuteSync(out instance, customerId, underwriterId, customerId, scenarioName);

			return new PricingModelModelActionResult {
				MetaData = result,
				Value = instance.Model,
			};
		} // GetPricingModelModel

		public PricingScenarioNameListActionResult GetPricingModelScenarios(int underwriterId) {
			GetPricingModelScenarios instance;

			ActionMetaData result = ExecuteSync(out instance, null, underwriterId);

			return new PricingScenarioNameListActionResult {
				MetaData = result,
				Names = instance.Scenarios,
			};
		} // GetPricingModelScenarios

		public PricingModelModelActionResult PricingModelCalculate(
			int customerId,
			int underwriterId,
			PricingModelModel model
		) {
			PricingModelCalculate instance;

			ActionMetaData result = ExecuteSync(out instance, customerId, underwriterId, customerId, model);

			return new PricingModelModelActionResult {
				MetaData = result,
				Value = instance.Model,
			};
		} // PricingModelCalculate

		public DecimalActionResult GetPricingModelDefaultRate(int customerId, int underwriterId, decimal companyShare) {
			GetPricingModelDefaultRate instance;
			ActionMetaData result = ExecuteSync(out instance, customerId, underwriterId, customerId, companyShare);

			return new DecimalActionResult {
				MetaData = result,
				Value = instance.DefaultRate
			};
		} // GetPricingModelDefaultRate

		public ActionMetaData SavePricingModelSettings(int underwriterId, long scenarioID, PricingModelModel model) {
			SavePricingModelSettings instance;
			return ExecuteSync(out instance, null, underwriterId, scenarioID, model);
		} // SavePricingModelSettings

		public PricingModelModelActionResult GetPricingScenarioDetails(int underwriterId, long scenarioID) {
			GetPricingScenarioDetails instance;
			ActionMetaData result = ExecuteSync(out instance, null, underwriterId, scenarioID);

			return new PricingModelModelActionResult {
				MetaData = result,
				Value = instance.Model,
			};
		} // GetPricingScenarioDetails
	} // class EzServiceImplementation
} // namespace EzService

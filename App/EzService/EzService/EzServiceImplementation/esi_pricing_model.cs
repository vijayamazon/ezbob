namespace EzService.EzServiceImplementation {
	using Ezbob.Backend.Strategies.PricingModel;

	partial class EzServiceImplementation 
	{
		public PricingModelModelActionResult GetPricingModelModel(int customerId, int underwriterId, string scenarioName)
		{
			GetPricingModelModel instance;
			ActionMetaData result = ExecuteSync(out instance, customerId, underwriterId, customerId, scenarioName);

			return new PricingModelModelActionResult
			{
				MetaData = result,
				Value = instance.Model
			};
		}

		public StringListActionResult GetPricingModelScenarios(int underwriterId)
		{
			GetPricingModelScenarios instance;
			ActionMetaData result = ExecuteSync(out instance, 0, underwriterId);

			return new StringListActionResult
			{
				MetaData = result,
				Records = instance.Scenarios
			};
		}

		public PricingModelModelActionResult PricingModelCalculate(int customerId, int underwriterId, PricingModelModel model)
		{
			PricingModelCalculate instance;
			ActionMetaData result = ExecuteSync(out instance, customerId, underwriterId, customerId, model);

			return new PricingModelModelActionResult
			{
				MetaData = result,
				Value = instance.Model
			};
		}

		public DecimalActionResult GetPricingModelDefaultRate(int customerId, int underwriterId, decimal companyShare)
		{
			GetPricingModelDefaultRate instance;
			ActionMetaData result = ExecuteSync(out instance, customerId, underwriterId, customerId, companyShare);

			return new DecimalActionResult
			{
				MetaData = result,
				Value = instance.DefaultRate
			};
		}

		public ActionMetaData SavePricingModelSettings(int underwriterId, string scenarioName, PricingModelModel model)
		{
			SavePricingModelSettings instance;
			return ExecuteSync(out instance, 0, underwriterId, scenarioName, model);
		}
	} // class EzServiceImplementation
} // namespace EzService

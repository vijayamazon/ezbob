namespace EzService.EzServiceImplementation {
	using EzBob.Backend.Strategies.PricingModel;

	partial class EzServiceImplementation {
		public PricingModelModelActionResult GetPricingModelModel(int customerId, int underwriterId)
		{
			GetPricingModelModel instance;
			ActionMetaData result = ExecuteSync(out instance, customerId, underwriterId, customerId);

			return new PricingModelModelActionResult
			{
				MetaData = result,
				Value = instance.Model
			};
		}

		public PricingModelModelActionResult PricingModelCalculate(int customerId, int underwriterId, PricingModelModel model)
		{
			PricingModelCalculate instance;
			ActionMetaData result = ExecuteSync(out instance, customerId, underwriterId, model);

			return new PricingModelModelActionResult
			{
				MetaData = result,
				Value = instance.Model
			};
		}
	} // class EzServiceImplementation
} // namespace EzService

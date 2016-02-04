namespace EzService {
	using System.ServiceModel;
	using Ezbob.Backend.Strategies.PricingModel;
	using EzService.ActionResults;

	[ServiceContract(SessionMode = SessionMode.Allowed)]
	public interface IEzServicePricing {
		[OperationContract]
		PricingModelModelActionResult GetPricingModelModel(int customerId, int underwriterId, string scenarioName);

		[OperationContract]
		PricingScenarioNameListActionResult GetPricingModelScenarios(int underwriterId);

		[OperationContract]
		PricingModelModelActionResult PricingModelCalculate(int customerId, int underwriterId, PricingModelModel model);

		[OperationContract]
		DecimalActionResult GetPricingModelDefaultRate(int customerId, int underwriterId, decimal companyShare);

		[OperationContract]
		ActionMetaData SavePricingModelSettings(int underwriterId, long scenarioID, PricingModelModel model);

		[OperationContract]
		PricingModelModelActionResult GetPricingScenarioDetails(int underwriterId, long scenarioID);
	} // interface IEzServicePricing
} // namespace

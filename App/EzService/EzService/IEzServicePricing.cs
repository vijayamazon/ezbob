namespace EzService {
	using System.ServiceModel;
	using DbConstants;
	using Ezbob.Backend.ModelsWithDB;
	using EzService.ActionResults;

	[ServiceContract(SessionMode = SessionMode.Allowed)]
	public interface IEzServicePricing {
		[OperationContract]
		PricingModelModelActionResult GetPricingModelModel(
			int customerId,
			int underwriterId,
			PricingCalcuatorScenarioNames scenarioName
		);

		[OperationContract]
		PricingScenarioNameListActionResult GetPricingModelScenarios(int underwriterId);

		[OperationContract]
		PricingModelModelActionResult PricingModelCalculate(int customerId, int underwriterId, PricingModelModel model);

		[OperationContract]
		ActionMetaData SavePricingModelSettings(int underwriterId, long scenarioID, PricingModelModel model);

		[OperationContract]
		PricingModelModelActionResult GetPricingScenarioDetails(int underwriterId, long scenarioID);
	} // interface IEzServicePricing
} // namespace

using System.ServiceModel;

namespace EzService {
	#region interface IEzServiceClient

	[ServiceContract(SessionMode = SessionMode.Allowed)]
	public interface IEzServiceClient {
		[OperationContract]
		StringListActionResult GetStrategiesList();

		[OperationContract]
		ActionMetaData GreetingMailStrategy(int nCustomerID, string sConfirmationEmail);

		[OperationContract]
		ActionMetaData CustomerMarketplaceAdded(int nCustomerID, int nMarketplaceID);
	} // interface IEzServiceClient

	#endregion interface IEzServiceClient
} // namespace EzService

using System.ServiceModel;

namespace EzService {
	#region interface IEzServiceClient

	[ServiceContract(SessionMode = SessionMode.Allowed)]
	public interface IEzServiceClient {
		[OperationContract]
		StringListActionResult GetStrategiesList();
	} // interface IEzServiceClient

	#endregion interface IEzServiceClient
} // namespace EzService

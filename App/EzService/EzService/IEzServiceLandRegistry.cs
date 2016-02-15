namespace EzService {
    using System.ServiceModel;
    using EzService.ActionResults;

	[ServiceContract(SessionMode = SessionMode.Allowed)]
	public interface IEzServiceLandRegistry {
	    [OperationContract]
		LandRegistryActionResult LandRegistryLoad(int customerID, int userID);

		[OperationContract]
		ActionMetaData BackfillLandRegistry2PropertyLink();

		[OperationContract]
		string LandRegistryEnquiry(
			int userId,
			int customerId,
			string buildingNumber,
			string buildingName,
			string streetName,
			string cityName,
			string postCode
		);

		[OperationContract]
		string LandRegistryRes(int userId, int customerId, string titleNumber);

		[OperationContract]
		ActionMetaData GetZooplaData(int customerId, bool reCheck);

		[OperationContract]
		PropertyStatusesActionResult GetPropertyStatuses();

	} // interface IEzServiceLandRegistry
} // namespace  

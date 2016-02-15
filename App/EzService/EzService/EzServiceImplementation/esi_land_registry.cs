namespace EzService.EzServiceImplementation {
	using Ezbob.Backend.Strategies.Backfill;
	using Ezbob.Backend.Strategies.LandRegistry;
	using Ezbob.Backend.Strategies.Misc;
	using EzService.ActionResults;

	partial class EzServiceImplementation : IEzServiceLandRegistry {
		public string LandRegistryEnquiry(int userId, int customerId, string buildingNumber, string buildingName, string streetName, string cityName, string postCode) {
			LandRegistryEnquiry oInstance;
			ExecuteSync(out oInstance, customerId, userId, customerId, buildingNumber, buildingName, streetName, cityName, postCode);
			return oInstance.Result;
		}//LandRegistryEnquiry

		public string LandRegistryRes(int userId, int customerId, string titleNumber) {
			LandRegistryRes oInstance;
			ExecuteSync(out oInstance, customerId, userId, customerId, titleNumber);
			return oInstance.Result;
		}//LandRegistryRes

		public LandRegistryActionResult LandRegistryLoad(int customerID, int userID) {
			LandRegistryLoad oInstance;
			var meta = ExecuteSync(out oInstance, customerID, userID, customerID);
			return new LandRegistryActionResult {
				Value = oInstance.Result,
				MetaData = meta
			};
		}//LandRegistryLoad

		public ActionMetaData BackfillLandRegistry2PropertyLink() {
			BackfillLandRegistry2PropertyLink instance;
			return ExecuteSync(out instance, 0, 0);
		}//BackfillLandRegistry2PropertyLink

		public PropertyStatusesActionResult GetPropertyStatuses() {
			GetPropertyStatuses instance;

			ActionMetaData result = ExecuteSync(out instance, 0, 0);

			return new PropertyStatusesActionResult {
				MetaData = result,
				Groups = instance.Groups
			};
		}//GetPropertyStatuses

		public ActionMetaData GetZooplaData(int customerId, bool reCheck) {
			return ExecuteSync<ZooplaStub>(customerId, null, customerId, reCheck);
		}//GetZooplaData
	}//class
}//ns

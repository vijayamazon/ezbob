namespace EzService {
	using EzBob.Backend.Strategies;

	partial class EzServiceImplementation {
		public string LandRegistryEnquiry(int customerId, string buildingNumber, string streetName, string cityName, string postCode) {
			LandRegistryEnquiry oInstance;
			ExecuteSync(out oInstance, customerId, null, customerId, buildingNumber, streetName, cityName, postCode);
			return oInstance.Result;
		} // LandRegistryEnquiry

		public string LandRegistryRes(int customerId, string titleNumber) {
			LandRegistryRes oInstance;
			ExecuteSync(out oInstance, customerId, null, customerId, titleNumber);
			return oInstance.Result;
		}// LandRegistryRes
	} // class EzServiceImplementation
} // namespace EzService

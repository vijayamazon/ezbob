namespace EzService.EzServiceImplementation
{
	using EzBob.Backend.Strategies.Misc;

	partial class EzServiceImplementation
	{
		public string LandRegistryEnquiry(int customerId, string buildingNumber, string buildingName, string streetName, string cityName, string postCode)
		{
			LandRegistryEnquiry oInstance;
			ExecuteSync(out oInstance, customerId, null, customerId, buildingNumber, buildingName, streetName, cityName, postCode);
			return oInstance.Result;
		}

		public string LandRegistryRes(int customerId, string titleNumber)
		{
			LandRegistryRes oInstance;
			ExecuteSync(out oInstance, customerId, null, customerId, titleNumber);
			return oInstance.Result;
		}
	}
}

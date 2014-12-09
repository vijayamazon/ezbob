namespace EzService.EzServiceImplementation
{
	using Ezbob.Backend.Strategies.Misc;

	partial class EzServiceImplementation
	{
		public string LandRegistryEnquiry(int userId, int customerId, string buildingNumber, string buildingName, string streetName, string cityName, string postCode)
		{
			LandRegistryEnquiry oInstance;
			ExecuteSync(out oInstance, customerId, userId, customerId, buildingNumber, buildingName, streetName, cityName, postCode);
			return oInstance.Result;
		}

		public string LandRegistryRes(int userId, int customerId, string titleNumber)
		{
			LandRegistryRes oInstance;
			ExecuteSync(out oInstance, customerId, userId, customerId, titleNumber);
			return oInstance.Result;
		}
	}
}

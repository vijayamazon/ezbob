namespace Ezbob.Backend.Strategies.Extensions {
	using LandRegistryLib.LREnquiryServiceNS;

	public static class Q1AddressTypeExt {
		public static bool IsA(
			this Q1AddressType lrAddress,
			string buildingNumber,
			string buildingName,
			string streetName,
			string cityName,
			string postCode
		) {
			if (lrAddress == null)
				return false;

			return
				lrAddress.BuildingName.IsEqualTo(buildingName) &&
				lrAddress.BuildingNumber.IsEqualTo(buildingNumber) &&
				lrAddress.CityName.IsEqualTo(cityName) &&
				lrAddress.PostcodeZone.IsEqualTo(postCode) &&
				lrAddress.StreetName.IsEqualTo(streetName);
		} // IsA
	} // class Q1AddressTypeExt
} // namespace

namespace Ezbob.Backend.Strategies {
	using LandRegistryLib.LREnquiryServiceNS;

	internal static class StringExt {
		public static bool IsEqualTo(this string a, string b) {
			return string.IsNullOrEmpty(a) ? string.IsNullOrEmpty(b) : string.Equals(a, b);
		} // IsEqualTo
	} // class StringExt

	internal static class Q1AddressTypeExt {
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

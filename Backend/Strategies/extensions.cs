namespace Ezbob.Backend.Strategies {
	using System;
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

	internal static class DateTimeExt {
		public static string DateStr(this DateTime dt) {
			return dt.ToString("MMM d yyyy", Library.Instance.Culture);
		} // DateStr

		public static string TimeStr(this DateTime dt) {
			return dt.ToString("H:mm:ss", Library.Instance.Culture);
		} // TimeStr

		public static string MomentStr(this DateTime dt) {
			return dt.ToString("MMM d yyyy H:mm:ss", Library.Instance.Culture);
		} // MomentStr

		public static string DateStr(this DateTime? dt) {
			return dt == null ? "UNSPECIFIED DATE" : dt.Value.DateStr();
		} // DateStr

		public static string TimeStr(this DateTime? dt) {
			return dt == null ? "UNSPECIFIED TIME" : dt.Value.TimeStr();
		} // TimeStr

		public static string MomentStr(this DateTime? dt) {
			return dt == null ? "UNSPECIFIED MOMENT" : dt.Value.MomentStr();
		} // MomentStr
	} // DateTimeExt
} // namespace

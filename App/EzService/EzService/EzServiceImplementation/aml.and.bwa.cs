namespace EzService.EzServiceImplementation {
	using EzBob.Backend.Strategies.Misc;

	partial class EzServiceImplementation {
		public ActionMetaData CheckAml(int customerId) {
			return Execute<AmlChecker>(customerId, null, customerId);
		} // CheckAml

		public ActionMetaData CheckAmlCustom(
			int customerId, string idhubHouseNumber, string idhubHouseName, string idhubStreet,
			string idhubDistrict, string idhubTown, string idhubCounty, string idhubPostCode
		) {
			return Execute<AmlChecker>(customerId, null, customerId, idhubHouseNumber, idhubHouseName, idhubStreet, idhubDistrict, idhubTown, idhubCounty, idhubPostCode);
		} // CheckAmlCustom

		public ActionMetaData CheckBwa(int customerId) {
			return Execute<BwaChecker>(customerId, null, customerId);
		} // CheckBwa

		public ActionMetaData CheckBwaCustom(
			int customerId, string idhubHouseNumber, string idhubHouseName, string idhubStreet,
			string idhubDistrict, string idhubTown, string idhubCounty, string idhubPostCode,
			string idhubBranchCode, string idhubAccountNumber
		) {
			return Execute<BwaChecker>(customerId, null,
				customerId, idhubHouseNumber, idhubHouseName, idhubStreet,
				idhubDistrict, idhubTown, idhubCounty, idhubPostCode,
				idhubBranchCode, idhubAccountNumber
			);
		} // CheckBwaCustom
	} // class EzServiceImplementation
} // namespace EzService

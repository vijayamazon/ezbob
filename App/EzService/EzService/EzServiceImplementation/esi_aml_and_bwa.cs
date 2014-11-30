namespace EzService.EzServiceImplementation {
	using EzBob.Backend.Strategies.Misc;

	partial class EzServiceImplementation {
		public ActionMetaData CheckAml(int customerId, int userId) {
			return Execute<AmlChecker>(customerId, userId, customerId);
		} // CheckAml

		public ActionMetaData CheckAmlCustom(int userId,
			int customerId, string idhubHouseNumber, string idhubHouseName, string idhubStreet,
			string idhubDistrict, string idhubTown, string idhubCounty, string idhubPostCode
		) {
			return Execute<AmlChecker>(customerId, userId, customerId, idhubHouseNumber, idhubHouseName, idhubStreet, idhubDistrict, idhubTown, idhubCounty, idhubPostCode);
		} // CheckAmlCustom

		public ActionMetaData CheckBwa(int customerId, int userId) {
			return Execute<BwaChecker>(customerId, userId, customerId);
		} // CheckBwa

		public ActionMetaData CheckBwaCustom(int userId,
			int customerId, string idhubHouseNumber, string idhubHouseName, string idhubStreet,
			string idhubDistrict, string idhubTown, string idhubCounty, string idhubPostCode,
			string idhubBranchCode, string idhubAccountNumber
		) {
			return Execute<BwaChecker>(customerId, userId,
				customerId, idhubHouseNumber, idhubHouseName, idhubStreet,
				idhubDistrict, idhubTown, idhubCounty, idhubPostCode,
				idhubBranchCode, idhubAccountNumber
			);
		} // CheckBwaCustom
	} // class EzServiceImplementation
} // namespace EzService

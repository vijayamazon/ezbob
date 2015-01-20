namespace EzService.EzServiceImplementation {
	using Ezbob.Backend.Strategies.AutomationVerification;

	partial class EzServiceImplementation {
		public ActionMetaData MaamMedalAndPricing(int nCustomerCount, int nLastCheckedCashRequestID) {
			return Execute<MaamMedalAndPricing>(null, null, nCustomerCount, nLastCheckedCashRequestID);
		} // MaamMedalAndPricing

		public ActionMetaData VerifyReapproval(int nCustomerCount, int nLastCheckedCustomerID) {
			return Execute<VerifyReapproval>(null, null, nCustomerCount, nLastCheckedCustomerID);
		} // VerifyReapproval

		public ActionMetaData VerifyApproval(int nCustomerCount, int nLastCheckedCustomerID) {
			return Execute<VerifyApproval>(null, null, nCustomerCount, nLastCheckedCustomerID);
		} // VerifyApproval

		public ActionMetaData VerifyRerejection(int nCustomerCount, int nLastCheckedCustomerID) {
			return Execute<VerifyRerejection>(null, null, nCustomerCount, nLastCheckedCustomerID);
		} // VerifyRerejection

		public ActionMetaData VerifyReject(int nCustomerCount, int nLastCheckedCustomerID) {
			return Execute<VerifyReject>(null, null, nCustomerCount, nLastCheckedCustomerID);
		} // VerifyReject

	} // class EzServiceImplementation
} // namespace

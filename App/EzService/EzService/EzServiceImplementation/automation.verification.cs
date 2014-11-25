namespace EzService.EzServiceImplementation {
	using EzBob.Backend.Strategies.AutomationVerification;

	partial class EzServiceImplementation {
		#region method VerifyReapproval

		public ActionMetaData VerifyReapproval(int nCustomerCount, int nLastCheckedCustomerID) {
			return Execute<VerifyReapproval>(null, null, nCustomerCount, nLastCheckedCustomerID);
		} // VerifyReapproval

		#endregion method VerifyReapproval

		#region method VerifyApproval

		public ActionMetaData VerifyApproval(int nCustomerCount, int nLastCheckedCustomerID) {
			return Execute<VerifyApproval>(null, null, nCustomerCount, nLastCheckedCustomerID);
		} // VerifyApproval

		#endregion method VerifyApproval

		#region method VerifyRerejection

		public ActionMetaData VerifyRerejection(int nCustomerCount, int nLastCheckedCustomerID) {
			return Execute<VerifyRerejection>(null, null, nCustomerCount, nLastCheckedCustomerID);
		} // VerifyRerejection

		#endregion method VerifyRerejection
	} // class EzServiceImplementation
} // namespace

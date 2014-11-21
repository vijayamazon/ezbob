namespace EzService.EzServiceImplementation {
	using EzBob.Backend.Strategies.AutomationVerification;

	partial class EzServiceImplementation {
		#region method VerifyReapproval

		public ActionMetaData VerifyReapproval(int nCustomerCount, int nLastCheckedCustomerID) {
			return Execute<VerifyReapproval>(null, null, nCustomerCount, nLastCheckedCustomerID);
		} // VerifyReapproval

		#endregion method VerifyReapproval
	} // class EzServiceImplementation
} // namespace

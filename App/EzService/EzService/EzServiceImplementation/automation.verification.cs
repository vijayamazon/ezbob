namespace EzService.EzServiceImplementation {
	using EzBob.Backend.Strategies.AutomationVerification;

	partial class EzServiceImplementation {
		#region method VerifyReapproval

		public ActionMetaData VerifyReapproval(int nCustomerCount) {
			return Execute<VerifyReapproval>(null, null, nCustomerCount);
		} // VerifyReapproval

		#endregion method VerifyReapproval
	} // class EzServiceImplementation
} // namespace

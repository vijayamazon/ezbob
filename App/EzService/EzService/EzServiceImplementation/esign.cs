namespace EzService.EzServiceImplementation {
	using EzBob.Backend.Strategies.Esign;

	partial class EzServiceImplementation {
		#region method EsignProcessPending

		public ActionMetaData EsignProcessPending(int? nCustomerID) {
			return Execute<EsignProcessPending>(null, null, nCustomerID);
		} // EsignProcessPending

		#endregion method EsignProcessPending
	} // class EzServiceImplementation
} // namespace

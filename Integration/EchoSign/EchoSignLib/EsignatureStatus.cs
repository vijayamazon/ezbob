namespace EchoSignLib {
	using EchoSignService;

	public struct EsignatureStatus {
		public int CustomerID { get; set; }
		public int EsignatureID { get; set; }
		public AgreementStatus Status { get; set; }
	} // struct EsignatureStatus
} // namespace

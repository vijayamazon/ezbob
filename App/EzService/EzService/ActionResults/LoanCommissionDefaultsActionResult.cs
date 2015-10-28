namespace EzService {
	using System.Runtime.Serialization;

	[DataContract]
	public class LoanCommissionDefaultsActionResult : ActionResult {
		[DataMember]
		public decimal BrokerCommission { get; set; }

		[DataMember]
		public decimal ManualSetupFee { get; set; }
	} // class LoanCommissionDefaultsActionResult
} // namespace EzService

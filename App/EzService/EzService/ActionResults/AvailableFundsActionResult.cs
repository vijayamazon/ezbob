namespace EzService {
	using System.Runtime.Serialization;

	[DataContract]
	public class AvailableFundsActionResult : ActionResult {
		[DataMember]
		public decimal AvailableFunds { get; set; }

		[DataMember]
		public decimal ReservedAmount { get; set; }
	} // class AvailableFundsActionResult
} // namespace

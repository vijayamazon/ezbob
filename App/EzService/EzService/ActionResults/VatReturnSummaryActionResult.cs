namespace EzService {
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models;

	[DataContract]
	public class VatReturnSummaryActionResult : ActionResult {
		[DataMember]
		public VatReturnSummary Summary { get; set; }
	} // class VatReturnSummaryActionResult
} // namespace EzService

namespace EzService {
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models;

	[DataContract]
	public class VatReturnDataActionResult : ActionResult {
		[DataMember]
		public VatReturnRawData[] VatReturnRawData { get; set; }

		[DataMember]
		public RtiTaxMonthRawData[] RtiTaxMonthRawData { get; set; }

		[DataMember]
		public VatReturnSummary[] Summary { get; set; }
	} // class VatReturnDataActionResult
} // namespace EzService

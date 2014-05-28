namespace EzService {
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models;

	[DataContract]
	public class VatReturnPeriodsActionResult : ActionResult {
		[DataMember]
		public VatReturnPeriod[] Periods { get; set; }
	} // class VatReturnPeriodsActionResult
} // namespace EzService

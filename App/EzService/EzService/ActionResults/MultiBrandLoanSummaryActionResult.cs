namespace EzService {
	using System.Runtime.Serialization;
	using Ezbob.Backend.ModelsWithDB;

	[DataContract]
	public class MultiBrandLoanSummaryActionResult : ActionResult {
		[DataMember]
		public MultiBrandLoanSummary Summary { get; set; }
	} // class MultiBrandLoanSummaryActionResult
} // namespace EzService

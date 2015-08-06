namespace EzService.ActionResults {
	using System.Runtime.Serialization;
	using Ezbob.Backend.ModelsWithDB.NewLoan;

	[DataContract]
	public class NewLoanModelActionResult : ActionResult {
		[DataMember]
		public NL_Model Value { get; set; }
	} // class NewLoanModelActionResult
} // namespace

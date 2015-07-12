namespace EzService.ActionResults {
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models.NewLoan;

	[DataContract]
	public class NewLoanModelActionResult {

		[DataMember]
		public NL_Model Value { get; set; }
	}
}

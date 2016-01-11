namespace EzService.ActionResults {
	using System.Runtime.Serialization;
	using Ezbob.Backend.ModelsWithDB.NewLoan;

	[DataContract]
    public class NLModelActionResult : ActionResult {
		[DataMember]
		public NL_Model Value { get; set; }

        [DataMember]
        public string Error { get; set; }

	} // class NewLoanModelActionResult
} // namespace

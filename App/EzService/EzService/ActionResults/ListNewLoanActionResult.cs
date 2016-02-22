namespace EzService.ActionResults {
	using System.Runtime.Serialization;
	using Ezbob.Backend.ModelsWithDB.NewLoan;

	[DataContract]
    public class ListNewLoanActionResult : ActionResult {
		[DataMember]
		public NL_Loans[] Value { get; set; }

        [DataMember]
        public string Error { get; set; }

	} // class NewLoanModelActionResult
} // namespace

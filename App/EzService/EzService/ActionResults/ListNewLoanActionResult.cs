namespace EzService.ActionResults {
    using System.Collections.Generic;
    using System.Runtime.Serialization;
	using Ezbob.Backend.ModelsWithDB.NewLoan;

	[DataContract]
    public class ListNewLoanActionResult : ActionResult {
		[DataMember]
		public List<NL_Loans> Value { get; set; }

        [DataMember]
        public string Error { get; set; }

	} // class NewLoanModelActionResult
} // namespace

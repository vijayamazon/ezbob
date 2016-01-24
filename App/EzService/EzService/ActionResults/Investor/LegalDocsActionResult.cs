namespace EzService.ActionResults.Investor {
    using System.Runtime.Serialization;
    using Ezbob.Backend.ModelsWithDB.LegalDocs;

    [DataContract]
    public class LegalDocActionResult : ActionResult {
		[DataMember]
        public LoanAgreementTemplate LoanAgreementTemplate { get; set; }
	}
}

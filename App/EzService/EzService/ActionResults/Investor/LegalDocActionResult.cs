namespace EzService.ActionResults.Investor {
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Ezbob.Backend.ModelsWithDB.LegalDocs;

    [DataContract]
    public class LegalDocsActionResult : ActionResult {
		[DataMember]
        public List<LoanAgreementTemplate> LoanAgreementTemplates { get; set; }
	}
}

namespace Ezbob.Backend.Models.LegalDocs {
    using System.ComponentModel;

    public class LegalDocsEnums {
        public enum LoanAgreementTemplateType {
            [Description("Guaranty Agreement")]
            GuarantyAgreement = 1,
            [Description("Pre Contract")]
            PreContract = 2,
            [Description("Regulated Loan Agreement")]
            RegulatedLoanAgreement = 3,
            [Description("Private Company Loan Agreement")]
            PrivateCompanyLoanAgreement = 4,
            [Description("Credit Facility")]
            CreditFacility = 5,
            [Description("Board Resolution")]
            BoardResolution = 6
        }
    }
}

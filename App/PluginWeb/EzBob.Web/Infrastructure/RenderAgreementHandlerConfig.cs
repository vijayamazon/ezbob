namespace EzBob.Web.Infrastructure
{
	using Scorto.Configuration;

	public class RenderAgreementHandlerConfig : ConfigurationRoot
    {
        public string PdfLoanAgreement { get { return GetValue<string>("PdfLoanAgreement"); } }
        public string PdfLoanAgreement2 { get { return GetValue<string>("PdfLoanAgreement2"); } }
        public string PdfConsentAgreement { get { return GetValue<string>("PdfConsentAgreement"); } }
        public string PdfConsentAgreement2 { get { return GetValue<string>("PdfConsentAgreement2"); } }
    }
}
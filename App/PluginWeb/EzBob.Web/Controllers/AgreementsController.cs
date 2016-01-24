namespace EzBob.Web.Controllers {
    using System.Linq;
    using System.Web.Mvc;
    using Backend.Models;
    using Code;
    using Ezbob.Backend.ModelsWithDB.LegalDocs;
    using EZBob.DatabaseLib.Exceptions;
    using EZBob.DatabaseLib.Model.Loans;
    using Infrastructure;
    using Newtonsoft.Json;
    using ServiceClientProxy;
    using ActionResult = System.Web.Mvc.ActionResult;

    [Authorize]
    public class AgreementsController : Controller {
        private readonly IEzbobWorkplaceContext context;
        private readonly ILoanAgreementRepository agreements;
        private readonly AgreementRenderer agreementRenderer;
        private readonly ServiceClient serviceClient;

        public AgreementsController(IEzbobWorkplaceContext context,
            ILoanAgreementRepository agreements,
            AgreementRenderer agreementRenderer,
            ServiceClient serviceClient) {
            this.agreementRenderer = agreementRenderer;
            this.serviceClient = serviceClient;
            this.context = context;
            this.agreements = agreements;
        }

        public ActionResult Download(int id) {
            var agreement = this.agreements.Get(id);

            try {
                if (this.context.Customer != null && agreement.Loan.Customer != this.context.Customer) return new HttpNotFoundResult();
            }
            catch (InvalidCustomerException) {
                //if customer is not found, assume that it is underwriter
            }

            var agreementModel = JsonConvert.DeserializeObject<AgreementModel>(agreement.Loan.AgreementModel);

            var customer = agreement.Loan.Customer;
            if (string.IsNullOrEmpty(agreementModel.FullName)) {

                agreementModel.FullName = customer.PersonalInfo.Fullname;
                var company = customer.Company;
                if (company != null) {
                    agreementModel.CompanyName = company.ExperianCompanyName ?? company.CompanyName;
                    agreementModel.CompanyNumber = company.ExperianRefNum ?? company.CompanyNumber;
                    try {
                        agreementModel.CompanyAdress = company.CompanyAddress.First()
                            .FormattedAddress;
                    } catch {
                        //nothing
                    }

                }
            }

            LoanAgreementTemplate loanAgreementTemplate = this.serviceClient.Instance.GetLegalDocById(customer.Id, this.context.UserId, agreement.TemplateID).LoanAgreementTemplate;

            var pdf = this.agreementRenderer.RenderAgreementToPdf(loanAgreementTemplate.Template, agreementModel);
            return File(pdf, "application/pdf", GenerateFileName(agreement));
        }

        private string GenerateFileName(LoanAgreement agreement) {
            return this.context.UserRoles.Any(r => r == "Underwriter") ? agreement.LongFilename() : agreement.ShortFilename();
        }
    }
}
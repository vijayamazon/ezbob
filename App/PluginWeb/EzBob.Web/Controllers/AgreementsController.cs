namespace EzBob.Web.Controllers
{
	using System.Linq;
	using System.Web.Mvc;
	using Backend.Models;
	using Code;
	using EZBob.DatabaseLib.Exceptions;
	using EZBob.DatabaseLib.Model.Loans;
	using Infrastructure;
	using Newtonsoft.Json;
	using ActionResult = System.Web.Mvc.ActionResult;

	[Authorize]
    public class AgreementsController : Controller
    {
        private readonly IEzbobWorkplaceContext _context;
        private readonly ILoanAgreementRepository _agreements;
        private readonly AgreementRenderer _agreementRenderer;
		private readonly ILoanAgreementTemplateRepository _loanAgreementTemplateRepository;
        public AgreementsController(IEzbobWorkplaceContext context,
			ILoanAgreementRepository agreements, 
			AgreementRenderer agreementRenderer, 
			ILoanAgreementTemplateRepository loanAgreementTemplateRepository)
        {
            _agreementRenderer = agreementRenderer;
	        _loanAgreementTemplateRepository = loanAgreementTemplateRepository;
	        _context = context;
            _agreements = agreements;
        }

        public ActionResult Download(int id)
        {
            var agreement = _agreements.Get(id);

            try
            {
                var customer = _context.Customer;
                if (customer != null && agreement.Loan.Customer != customer) return new HttpNotFoundResult();
            }
            catch (InvalidCustomerException )
            {
                //if customer is not found, assume that it is underwriter
            }

	        var agreementModel = JsonConvert.DeserializeObject<AgreementModel>(agreement.Loan.AgreementModel);
	        string template = agreement.TemplateRef.Template;

			if (string.IsNullOrEmpty(agreementModel.FullName)) {
				var customer = agreement.Loan.Customer;
				agreementModel.FullName = customer.PersonalInfo.Fullname;
				var company = customer.Company;
				if (company != null) {
					agreementModel.CompanyName = company.ExperianCompanyName ?? company.CompanyName;
					agreementModel.CompanyNumber = company.ExperianRefNum ?? company.CompanyNumber;
					try {
						agreementModel.CompanyAdress = company.CompanyAddress.First().FormattedAddress;
					}catch {}

				}

				template = _loanAgreementTemplateRepository.GetUpdateTemplate(agreement.TemplateRef.TemplateType) ?? template;
			}

            var pdf = _agreementRenderer.RenderAgreementToPdf(template, agreementModel);
            return File(pdf, "application/pdf", GenerateFileName(agreement));
        }

        private string GenerateFileName(LoanAgreement agreement)
        {
            return _context.UserRoles.Any(r => r == "Underwriter") ? agreement.LongFilename() : agreement.ShortFilename();
        }
    }
}
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

        public AgreementsController(IEzbobWorkplaceContext context, ILoanAgreementRepository agreements, AgreementRenderer agreementRenderer)
        {
            _agreementRenderer = agreementRenderer;
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

            var pdf = _agreementRenderer.RenderAgreementToPdf(agreement.TemplateRef.Template, JsonConvert.DeserializeObject<AgreementModel>(agreement.Loan.AgreementModel));
            return File(pdf, "application/pdf", GenerateFileName(agreement));
        }

        private string GenerateFileName(LoanAgreement agreement)
        {
            return _context.User.Roles.Any(r => r.Name == "Underwriter") ? agreement.LongFilename() : agreement.ShortFilename();
        }
    }
}
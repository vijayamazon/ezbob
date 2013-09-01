using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Exceptions;
using EZBob.DatabaseLib.Model.Loans;
using EzBob.Web.Areas.Customer.Models;
using EzBob.Web.Code;
using EzBob.Web.Code.Agreements;
using EzBob.Web.Infrastructure;
using Newtonsoft.Json;

namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
    [Authorize]
    public class AgreementsController : Controller
    {
        private readonly IEzbobWorkplaceContext _context;
        private readonly ILoanAgreementRepository _agreements;
        private AgreementRenderer _agreementRenderer;
        private readonly DatabaseDataHelper _helper;

        public AgreementsController(IEzbobWorkplaceContext context, ILoanAgreementRepository agreements, AgreementRenderer agreementRenderer, DatabaseDataHelper helper)
        {
            _agreementRenderer = agreementRenderer;
            _context = context;
            _agreements = agreements;
            _helper = helper;

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

            var pdf = _agreementRenderer.RenderAgreementToPdf(_helper.GetLoanAgreementTemplate(agreement.TemplateRef), JsonConvert.DeserializeObject<AgreementModel>(agreement.Loan.AgreementModel));
            return File(pdf, "application/pdf", GenerateFileName(agreement));
        }

        private string GenerateFileName(LoanAgreement agreement)
        {
            return _context.User.Roles.Any(r => r.Name == "Underwriter") ? agreement.LongFilename() : agreement.ShortFilename();
        }
    }
}
using System;
using System.Web.Mvc;
using EzBob.Web.Code;

namespace EzBob.Web.Areas.Customer.Controllers
{
    public class ConsentController : Controller
    {
        private readonly IConcentAgreementHelper _concentAgreementHelper;

        public ConsentController()
        {
            _concentAgreementHelper = new ConcentAgreementHelper();
        }

        public FileResult Download(int id, string firstName, string middleInitial, string surname)
        {
            var fullName = string.Format("{0} {1} {2}", firstName, middleInitial, surname);
            var pdf = _concentAgreementHelper.Generate(fullName, DateTime.UtcNow);
            var fileName = _concentAgreementHelper.GetFileName(id, firstName, surname, DateTime.UtcNow);
            return File(pdf, "application/pdf", fileName);
        }
    }
}
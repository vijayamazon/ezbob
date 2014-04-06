namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System.Data;
	using Code;
	using System.Web.Mvc;
	using Infrastructure.Attributes;
	using Models;
	using EZBob.DatabaseLib.Model.Database.Repository;

	public class MedalCalculationsController : Controller
    {
        private readonly CustomerRepository _customerRepository;
       
        public MedalCalculationsController(CustomerRepository customersRepository)
        {
            _customerRepository = customersRepository;
        }

        [Ajax]
        [HttpGet]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
        public ActionResult Index(int id)
        {
            var customer = _customerRepository.Get(id);
            var medalCalculator = new MedalCalculators(customer);
            return Json(medalCalculator, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ExportToExel(int id)
        {
            var customer = _customerRepository.Get(id);
            return new MedalExcelReportResult(customer);
        }
    }
}
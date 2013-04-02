using EzBob.Web.Code;
using Scorto.Web;
using System.Web.Mvc;
using EzBob.Web.Areas.Underwriter.Models;
using EZBob.DatabaseLib.Model.Database.Repository;

namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
    public class MedalCalculationsController : Controller
    {
        private readonly CustomerRepository _customerRepository;
       
        public MedalCalculationsController(CustomerRepository customersRepository)
        {
            _customerRepository = customersRepository;
        }

        [Ajax]
        [HttpGet]
        [Transactional]
        public ActionResult Index(int id)
        {
            var customer = _customerRepository.Get(id);
            var medalCalculator = new MedalCalculators(customer);
            return this.JsonNet(medalCalculator);
        }

        [HttpGet]
        public ActionResult ExportToExel(int id)
        {
            var customer = _customerRepository.Get(id);
            return new MedalExcelReportResult(customer);
        }
    }
}
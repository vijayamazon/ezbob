using System;
using System.Web.Mvc;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EzBob.Web.Areas.Underwriter.Models;
using Scorto.Web;

namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using EzBob.Models;
    public class CrossCheckController : Controller
    {
        private readonly CustomerRepository _customerRepository;

        public CrossCheckController(CustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        [Ajax]
        [Transactional]
        public ActionResult Index(int id)
        {
            var model = new CrossCheckModel(_customerRepository.Get(id));
            return View(model);
        }

        [Ajax]
        [Transactional]
				var sh = new StrategyHelper();
				sh.GetZooplaData(customerId);
				zoopla = _zooplaRepository.GetByAddress(address);
				if(zoopla == null)
					return this.JsonNet(new { error = "zoopla info not found" });
        public void SaveRefNum(int customerId, string companyRefNum)
        {
            var customer = _customerRepository.Get(customerId);

            switch (customer.PersonalInfo.TypeOfBusiness.Reduce())
            {
                case TypeOfBusinessReduced.NonLimited:
                    customer.NonLimitedInfo.NonLimitedRefNum = companyRefNum;
                    break;
                case TypeOfBusinessReduced.Limited:
                    customer.LimitedInfo.LimitedRefNum = companyRefNum;
                    break;
            }
        }
    }
}

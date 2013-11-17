namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Models;
	using Scorto.Web;
	using System.Linq;
	using EzBob.Models;
	public class CrossCheckController : Controller
	{
		private readonly CustomerRepository _customerRepository;
		private readonly ZooplaRepository _zooplaRepository;

		public CrossCheckController(CustomerRepository customerRepository, ZooplaRepository zooplaRepository)
		{
			_customerRepository = customerRepository;
			_zooplaRepository = zooplaRepository;
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
		[HttpGet]
		public JsonNetResult Zoopla(int customerId)
		{
			var customer = _customerRepository.Get(customerId);
			if (customer == null)
			{
				return this.JsonNet(new { error = "customer not found" });
			}

			var address = customer.AddressInfo.PersonalAddress.FirstOrDefault();
			if (address == null)
			{
				return this.JsonNet(new { error = "address not found" });
			}
			var zoopla = _zooplaRepository.GetByAddress(address);

			if (zoopla == null)
			{
				var sh = new StrategyHelper();
				sh.GetZooplaData(customerId);
				zoopla = _zooplaRepository.GetByAddress(address);
				if (zoopla == null)
					return this.JsonNet(new {error = "zoopla info not found"});
			}

			return this.JsonNet(zoopla);
		}

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

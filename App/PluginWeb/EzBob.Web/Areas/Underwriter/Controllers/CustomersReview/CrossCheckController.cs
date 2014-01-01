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
		private readonly CreditBureauModelBuilder _creditBureauModelBuilder;
		public CrossCheckController(CustomerRepository customerRepository, CreditBureauModelBuilder creditBureauModelBuilder)
		{
			_customerRepository = customerRepository;
			_creditBureauModelBuilder = creditBureauModelBuilder;
		}

		[Ajax]
		[Transactional]
		public ActionResult Index(int id)
		{
			var model = new CrossCheckModel(_customerRepository.Get(id), _creditBureauModelBuilder);
			return View(model);
		}

		[Ajax]
		[Transactional]
		[HttpGet]
		public JsonNetResult Zoopla(int customerId, bool recheck)
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
			var zoopla = address.Zoopla.LastOrDefault();

			if (zoopla == null || recheck)
			{
				var sh = new StrategyHelper();
				sh.GetZooplaData(customerId, recheck);
				zoopla = address.Zoopla.LastOrDefault();
				if (zoopla == null)
					return this.JsonNet(new {error = "zoopla info not found"});
			}

			return this.JsonNet(zoopla);
		}

		[Ajax]
		[Transactional]
		[HttpGet]
		public JsonNetResult LandRegistry(int customerId, bool recheck)
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
			
			string landregistry = null;// address.LandRegistry.LastOrDefault();

			if (landregistry == null || recheck)
			{
				var sh = new StrategyHelper();
				landregistry = sh.GetLandRegistryDate(customerId, recheck);
				//landregistry = address.Zoopla.LastOrDefault();
				if (landregistry == null)
					return this.JsonNet(new { error = "land registry info not found" });
			}

			return this.JsonNet(new { response = landregistry} );
		}

		[Ajax]
		[Transactional]
		[HttpPost]
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

			_customerRepository.Update(customer);
		}
	}
}

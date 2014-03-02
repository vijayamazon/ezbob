namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System.Data;
	using System.Web.Mvc;
	using Code.ApplicationCreator;
	using CommonLib;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using LandRegistryLib;
	using Models;
	using Scorto.Web;
	using System.Linq;
	using EzBob.Models;
	public class CrossCheckController : Controller
	{
		private readonly IAppCreator _appCreator;
		private readonly CustomerRepository _customerRepository;
		private readonly CustomerAddressRepository _customerAddressRepository;
		private readonly CreditBureauModelBuilder _creditBureauModelBuilder;
		public CrossCheckController(CustomerRepository customerRepository, 
			CreditBureauModelBuilder creditBureauModelBuilder, 
			CustomerAddressRepository customerAddressRepository,
			IAppCreator appCreator)
		{
			_appCreator = appCreator;
			_customerRepository = customerRepository;
			_creditBureauModelBuilder = creditBureauModelBuilder;
			_customerAddressRepository = customerAddressRepository;
		}

		[Ajax]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public ActionResult Index(int id)
		{
			var model = new CrossCheckModel(_customerRepository.Get(id), _creditBureauModelBuilder);
			return View(model);
		}

		[Ajax]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[HttpGet]
		public JsonNetResult Zoopla(int customerId, bool recheck)
		{
			var address = _customerAddressRepository.GetAll().FirstOrDefault(a => a.Customer.Id == customerId && a.AddressType == CustomerAddressType.PersonalAddress);
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
					return this.JsonNet(new { error = "zoopla info not found" });
			}

			return this.JsonNet(zoopla);
		}

		[Ajax]
		[HttpGet]
		public JsonNetResult LandRegistryEnquiry(int customerId, string buildingNumber, string streetName, string cityName, string postCode)
		{
			var client = _appCreator.GetServiceClient();
			var landregistryXml = client.LandRegistryEnquiry(customerId, buildingNumber, streetName, cityName, postCode);
			var landregistry = SerializeDataHelper.DeserializeTypeFromString<LandRegistryDataModel>(landregistryXml);

			return this.JsonNet(new { titles = landregistry.Enquery.Titles, rejection = landregistry.Rejection, ack = landregistry.Acknowledgement });
		}

		[Ajax]
		[HttpGet]
		public JsonNetResult LandRegistry(int customerId, string titleNumber = null)
		{
			var client = _appCreator.GetServiceClient();
			var landregistryXml = client.LandRegistryRes(customerId, titleNumber);
			var landregistry = SerializeDataHelper.DeserializeTypeFromString<LandRegistryDataModel>(landregistryXml);

			if (landregistry == null)
					return this.JsonNet(new { error = "land registry info not found" });

			//todo return the full model 
			return this.JsonNet(new { response = landregistry.Res, rejection = landregistry.Rejection, ack = landregistry.Acknowledgement });
		}

		[Ajax]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[HttpPost]
		public void SaveTargetingData(int customerId, string companyRefNum, string companyName, string addr1, string addr2, string addr3, string addr4, string postcode)
		{
			var customer = _customerRepository.Get(customerId);
			var company = customer.Company;
			if (company != null)
			{
				company.ExperianRefNum = companyRefNum;
				company.ExperianCompanyName = companyName;
				if (!string.IsNullOrEmpty(postcode))
				{
					company.ExperianCompanyAddress.Add(new CustomerAddress
						{
							AddressType = CustomerAddressType.ExperianCompanyAddress,
							Company = company,
							Customer = customer,
							Line1 = addr1,
							Line2 = addr2,
							Line3 = addr3,
							Town = addr4,
							Postcode = postcode,
						});
				}
			}

			_customerRepository.Update(customer);
		}
	}
}

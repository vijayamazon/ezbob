namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System.Data;
	using System.Web.Mvc;
	using Code;
	using CommonLib;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using LandRegistryLib;
	using Models;
	using Scorto.Web;
	using System.Linq;
	using EzBob.Models;
	using ActionResult = System.Web.Mvc.ActionResult;
	using LandRegistryResponseType = LandRegistryLib.LandRegistryResponseType;

	public class CrossCheckController : Controller {
		private readonly ServiceClient m_oServiceClient;
		private readonly CustomerRepository _customerRepository;
		private readonly CustomerAddressRepository _customerAddressRepository;
		private readonly CreditBureauModelBuilder _creditBureauModelBuilder;

		public CrossCheckController(
			CustomerRepository customerRepository, 
			CreditBureauModelBuilder creditBureauModelBuilder, 
			CustomerAddressRepository customerAddressRepository
		) {
			m_oServiceClient = new ServiceClient();
			_customerRepository = customerRepository;
			_creditBureauModelBuilder = creditBureauModelBuilder;
			_customerAddressRepository = customerAddressRepository;
		} // constructor

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
			var landregistryXml = m_oServiceClient.Instance.LandRegistryEnquiry(customerId, buildingNumber, streetName, cityName, postCode);
			var landregistry = SerializeDataHelper.DeserializeTypeFromString<LandRegistryDataModel>(landregistryXml);

			return this.JsonNet(new { titles = landregistry.Enquery.Titles, rejection = landregistry.Enquery.Rejection, ack = landregistry.Enquery.Acknowledgement });
		}

		[Ajax]
		[HttpGet]
		public JsonNetResult LandRegistry(int customerId, string titleNumber = null)
		{
			var landregistryXml = m_oServiceClient.Instance.LandRegistryRes(customerId, titleNumber);
			var landregistry = SerializeDataHelper.DeserializeTypeFromString<LandRegistryDataModel>(landregistryXml);

			if (landregistry.ResponseType == LandRegistryResponseType.None )
					return this.JsonNet(new { noRes = true });

			//todo return the full model 
			return this.JsonNet(new { response = landregistry.Res, rejection = landregistry.Res.Rejection, ack = landregistry.Res.Acknowledgement });
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

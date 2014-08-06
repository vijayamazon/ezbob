namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview {
	using System;
	using System.Collections.Generic;
	using System.Web.Mvc;
	using Customer.Controllers;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Ezbob.Utils.Serialization;
	using Infrastructure.Attributes;
	using Infrastructure.csrf;
	using LandRegistryLib;
	using Models;
	using MoreLinq;
	using NHibernate;
	using System.Linq;
	using EzBob.Models;
	using ServiceClientProxy;
	using log4net;
	using ActionResult = System.Web.Mvc.ActionResult;

	public class CrossCheckController : Controller {
		#region public

		#region constructor

		public CrossCheckController(
			CustomerRepository customerRepository,
			CustomerAddressRepository customerAddressRepository,
			ISession oSession
		) {
			m_oServiceClient = new ServiceClient();
			_customerRepository = customerRepository;
			_customerAddressRepository = customerAddressRepository;
			m_oSession = oSession;
		} // constructor

		#endregion constructor

		#region action Index

		[Ajax]
		[HttpGet]
		public ActionResult Index(int id) {
			var model = new CrossCheckModel(_customerRepository.Get(id));
			return View(model);
		} // Index

		#endregion action Index

		#region action Zoopla

		// TODO: this method should be removed after testing is done
		[Ajax]
		[Transactional]
		[HttpGet]
		public JsonResult Zoopla(int customerId, bool recheck) {
			var address = _customerAddressRepository.GetAll().FirstOrDefault(a => a.Customer.Id == customerId && a.AddressType == CustomerAddressType.PersonalAddress);

			if (address == null)
				return Json(new { error = "address not found" }, JsonRequestBehavior.AllowGet);

			var zoopla = address.Zoopla.LastOrDefault();

			if (zoopla == null || recheck) {
				var sh = new StrategyHelper();
				sh.GetZooplaData(customerId, recheck);
				zoopla = address.Zoopla.LastOrDefault();

				if (zoopla == null)
					return Json(new { error = "zoopla info not found" }, JsonRequestBehavior.AllowGet);
			} // if

			return Json(zoopla, JsonRequestBehavior.AllowGet);
		} // Zoopla

		#endregion action Zoopla

		#region Land Registry related

		#region action LandRegistryEnquiry

		[Ajax]
		[HttpPost]
		public JsonResult LandRegistryEnquiry(int customerId, string titleNumber, string buildingNumber, string buildingName, string streetName, string cityName, string postCode)
		{
			if (!string.IsNullOrEmpty(titleNumber))
			{
				m_oServiceClient.Instance.LandRegistryRes(customerId, titleNumber);
				return Json(new {isTitle = true});
			}
			else
			{
				var landregistryXml = m_oServiceClient
					.Instance
					.LandRegistryEnquiry(
						customerId,
						buildingNumber,
						buildingName,
						streetName,
						cityName,
						postCode);
				var landregistry = Serialized.Deserialize<LandRegistryDataModel>(landregistryXml);

				return Json(new
					{
						titles = landregistry.Enquery.Titles,
						rejection = landregistry.Enquery.Rejection,
						ack = landregistry.Enquery.Acknowledgement,
						isCache = landregistry.DataSource == LandRegistryDataSource.Cache
					}, JsonRequestBehavior.AllowGet);
			}
		} // LandRegistryEnquiry

		#endregion action LandRegistryEnquiry

		// TODO: method should be removed after testing
		[Ajax]
		[HttpPost]
		public JsonResult LandRegistryEnquiries(int customerId)
		{
			var customer = _customerRepository.Get(customerId);
			var b = new LandRegistryModelBuilder();
			var landRegistryEnquiries = new List<LandRegistryEnquiryTitle>();
			var lrEnqs = customer.LandRegistries.Where(x => x.RequestType == LandRegistryRequestType.Enquiry).Select(x => x.Response);
			foreach (var lr in lrEnqs)
			{
				try
				{
					var lrModel = b.BuildEnquiryModel(lr);

					landRegistryEnquiries.AddRange(lrModel.Titles);
				}
				catch (Exception ex)
				{
					
				}
			}

			landRegistryEnquiries = landRegistryEnquiries.DistinctBy(x => x.TitleNumber).ToList();
			return Json(new {titles = landRegistryEnquiries});
		}

		#region action LandRegistry

		[Ajax]
		[HttpPost]
		public JsonResult LandRegistry(int customerId, string titleNumber = null) {
			ms_oLog.DebugFormat("Loading Land Registry data for customer id {0} and title number {1}...", customerId, titleNumber ?? "--null--");
			m_oServiceClient.Instance.LandRegistryRes(customerId, titleNumber);
			return Json(new {},JsonRequestBehavior.AllowGet);
		} // LandRegistry

		#endregion action LandRegistry

		#endregion Land Registry related

		#region action SaveTargetingData

		[Ajax]
		[Transactional]
		[HttpPost]
		public void SaveTargetingData(
			int customerId,
			string companyRefNum,
			string companyName,
			string addr1,
			string addr2,
			string addr3,
			string addr4,
			string postcode
		) {
			var customer = _customerRepository.Get(customerId);

			var company = customer.Company;

			if (company != null) {
				company.ExperianRefNum = companyRefNum;
				company.ExperianCompanyName = companyName;

				if (!string.IsNullOrEmpty(postcode)) {
					company.ExperianCompanyAddress.Add(new CustomerAddress {
						AddressType = CustomerAddressType.ExperianCompanyAddress,
						Company = company,
						Customer = customer,
						Line1 = addr1,
						Line2 = addr2,
						Line3 = addr3,
						Town = addr4,
						Postcode = postcode
					});
				} // postcode is not empty
			} // if company is not null

			_customerRepository.Update(customer);
		} // SaveTargetingData

		#endregion action SaveTargetingData

		#region action AddDirector

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		[Transactional]
		public JsonResult AddDirector(int nCustomerID, DirectorModel director) {
			var customer = _customerRepository.Get(nCustomerID);

			if (customer == null)
				return Json(new { error = "Customer not found" });

			return Json(CustomerDetailsController.AddDirectorToCustomer(director, customer, m_oSession, false));
		} // AddDirector

		#endregion method AddDirector

		#endregion public

		#region private

		private readonly ServiceClient m_oServiceClient;
		private readonly CustomerRepository _customerRepository;
		private readonly CustomerAddressRepository _customerAddressRepository;
		private readonly ISession m_oSession;

		private static readonly ILog ms_oLog = LogManager.GetLogger(typeof (CrossCheckController));

		#endregion private
	} // class CrossCheckController
} // namespace

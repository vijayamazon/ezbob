namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Mvc;
	using Ezbob.Logger;
	using Ezbob.Utils.Serialization;
	using EzBob.Models;
	using EzBob.Web.Areas.Customer.Controllers;
	using EzBob.Web.Areas.Underwriter.Models;
	using EzBob.Web.Infrastructure;
	using EzBob.Web.Infrastructure.Attributes;
	using EzBob.Web.Infrastructure.csrf;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using LandRegistryLib;
	using MoreLinq;
	using NHibernate;
	using ServiceClientProxy;

	public class CrossCheckController : Controller {
		public CrossCheckController(
			CustomerRepository customerRepository,
			CustomerAddressRepository customerAddressRepository,
			ISession oSession, IWorkplaceContext context) {
			this.m_oServiceClient = new ServiceClient();
			this._customerRepository = customerRepository;
			this._customerAddressRepository = customerAddressRepository;
			this.m_oSession = oSession;
			this._context = context;
		} // constructor

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		[Transactional]
		public JsonResult AddDirector(int nCustomerID, DirectorModel director) {
			var customer = this._customerRepository.Get(nCustomerID);

			if (customer == null) {
				return Json(new {
					error = "Customer not found"
				});
			}
			var response = CustomerDetailsController.AddDirectorToCustomer(director, customer, this.m_oSession, false);
			m_oServiceClient.Instance.SalesForceAddUpdateContact(this._context.UserId, customer.Id, null, director.Email);
			return Json(response);
		}

		[Ajax]
		[HttpGet]
		public ActionResult Index(int id) {
			var model = new CrossCheckModel(this._context.UserId, this._customerRepository.Get(id));
			return View(model);
		} // Index

		[Ajax]
		[HttpPost]
		public JsonResult LandRegistry(int customerId, string titleNumber = null) {
			ms_oLog.Debug("Loading Land Registry data for customer id {0} and title number {1}...", customerId, titleNumber ?? "--null--");
			this.m_oServiceClient.Instance.LandRegistryRes(this._context.UserId, customerId, titleNumber);
			return Json(new {}, JsonRequestBehavior.AllowGet);
		}

		// TODO: method should be removed after testing
		[Ajax]
		[HttpPost]
		public JsonResult LandRegistryEnquiries(int customerId) {
			var customer = this._customerRepository.Get(customerId);
			var b = new LandRegistryModelBuilder();
			var landRegistryEnquiries = new List<LandRegistryEnquiryTitle>();
			var lrEnqs = customer.LandRegistries.Where(x => x.RequestType == LandRegistryRequestType.Enquiry)
				.Select(x => x.Response);
			foreach (var lr in lrEnqs) {
				try {
					var lrModel = b.BuildEnquiryModel(lr);

					landRegistryEnquiries.AddRange(lrModel.Titles);
				} catch (Exception ex) {
					ms_oLog.Info(ex, "Exception during building enquiry model.");
				}
			}

			landRegistryEnquiries = landRegistryEnquiries.DistinctBy(x => x.TitleNumber)
				.ToList();
			return Json(new {
				titles = landRegistryEnquiries
			});
		}

		[Ajax]
		[HttpPost]
		public JsonResult LandRegistryEnquiry(int customerId, string titleNumber, string buildingNumber, string buildingName, string streetName, string cityName, string postCode) {
			if (!string.IsNullOrEmpty(titleNumber)) {
				this.m_oServiceClient.Instance.LandRegistryRes(this._context.UserId, customerId, titleNumber);
				return Json(new {
					isTitle = true
				});
			} else {
				var landregistryXml = this.m_oServiceClient
					.Instance
					.LandRegistryEnquiry(this._context.UserId,
						customerId,
						buildingNumber,
						buildingName,
						streetName,
						cityName,
						postCode);
				var landregistry = Serialized.Deserialize<LandRegistryDataModel>(landregistryXml);

				return Json(new {
					titles = landregistry.Enquery.Titles,
					rejection = landregistry.Enquery.Rejection,
					ack = landregistry.Enquery.Acknowledgement,
					isCache = landregistry.DataSource == LandRegistryDataSource.Cache
				}, JsonRequestBehavior.AllowGet);
			}
		}

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
			var customer = this._customerRepository.Get(customerId);

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

			this._customerRepository.Update(customer);
		}

		// TODO: this method should be removed after testing is done
		[Ajax]
		[Transactional]
		[HttpGet]
		public JsonResult Zoopla(int customerId, bool recheck) {
			var address = this._customerAddressRepository.GetAll()
				.FirstOrDefault(a => a.Customer.Id == customerId && a.AddressType == CustomerAddressType.PersonalAddress);

			if (address == null) {
				return Json(new {
					error = "address not found"
				}, JsonRequestBehavior.AllowGet);
			}

			var zoopla = address.Zoopla.LastOrDefault();

			if (zoopla == null || recheck) {
				this.m_oServiceClient.Instance.GetZooplaData(customerId, recheck);
				zoopla = address.Zoopla.LastOrDefault();

				if (zoopla == null) {
					return Json(new {
						error = "zoopla info not found"
					}, JsonRequestBehavior.AllowGet);
				}
			} // if

			return Json(zoopla, JsonRequestBehavior.AllowGet);
		} // Zoopla


        [Ajax]
        [HttpPost]
        [Transactional]
        public JsonResult ChangeAddress(string addressInput, List<CustomerAddress> customerAddress, int customerId) {
            ms_oLog.Info("ChangeAddress was called {0} {1} {2}", customerId, customerAddress.Count, customerAddress[0].Line1);

            var customer = this._customerRepository.Get(customerId);
            
            MakeAddress(
                customerAddress,
                customer.AddressInfo.PrevPersonAddresses,
                CustomerAddressType.PrevPersonAddresses,
                customer.AddressInfo.PersonalAddress,
                CustomerAddressType.PersonalAddress
            );

            this._customerRepository.SaveOrUpdate(customer);
            return Json(new {});
        }

        private void MakeAddress(
            IEnumerable<CustomerAddress> newAddress,
            Iesi.Collections.Generic.ISet<CustomerAddress> prevAddress,
            CustomerAddressType prevAddressType,
            Iesi.Collections.Generic.ISet<CustomerAddress> currentAddress,
            CustomerAddressType currentAddressType
        ) {
            var newAddresses = newAddress as IList<CustomerAddress> ?? newAddress.ToList();
            var addAddress = newAddresses.Where(i => i.AddressId == 0).ToList();
            var curAddress = addAddress.LastOrDefault() ?? currentAddress.LastOrDefault();

            if (curAddress == null)
                return;

            foreach (var address in newAddresses) {
                address.Director = currentAddress.First().Director;
                address.Customer = currentAddress.First().Customer;
                address.Company = currentAddress.First().Company;
            } // for each new address

            foreach (var item in currentAddress) {
                item.AddressType = prevAddressType;
                prevAddress.Add(item);
            } // for each old address

            curAddress.AddressType = currentAddressType;
            currentAddress.Add(curAddress);
        } // MakeAddress

		private static readonly ASafeLog ms_oLog = new SafeILog(typeof(CrossCheckController));
		private readonly IWorkplaceContext _context;
		private readonly CustomerAddressRepository _customerAddressRepository;
		private readonly CustomerRepository _customerRepository;
		private readonly ServiceClient m_oServiceClient;
		private readonly ISession m_oSession;
	} // class CrossCheckController
} // namespace

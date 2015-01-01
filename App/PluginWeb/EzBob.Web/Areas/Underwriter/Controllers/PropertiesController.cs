namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Mvc;
	using Ezbob.Logger;
	using EzBob.Web.Areas.Underwriter.Models;
	using EzBob.Web.Infrastructure;
	using EzBob.Web.Infrastructure.Attributes;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Repository;
	using LandRegistryLib;
	using MoreLinq;
	using ServiceClientProxy;

	public class PropertiesController : Controller {
		public PropertiesController(IEzbobWorkplaceContext context,
			CustomerRepository customerRepository,
			CustomerAddressRepository customerAddressRepository,
			LandRegistryRepository landRegistryRepository) {
			this._customerRepository = customerRepository;
			this._customerAddressRepository = customerAddressRepository;
			this._landRegistryRepository = landRegistryRepository;
			this._context = context;
		}

		[Ajax]
		[HttpPost]
		public void AddAddress(int customerId, string addressId, string organisation, string line1, string line2, string line3, string town, string county, string postcode,
			string country, string rawpostcode, string deliverypointsuffix, string nohouseholds, string smallorg, string pobox, string mailsortcode, string udprn) {
			var customer = this._customerRepository.Get(customerId);

			var addedAddress = new CustomerAddress {
				AddressType = CustomerAddressType.OtherPropertyAddress,
				Id = addressId,
				Customer = customer,
				Organisation = organisation,
				Line1 = line1,
				Line2 = line2,
				Line3 = line3,
				Town = town,
				County = county,
				Postcode = postcode,
				Country = country,
				Rawpostcode = rawpostcode,
				Deliverypointsuffix = deliverypointsuffix,
				Nohouseholds = nohouseholds,
				Smallorg = smallorg,
				Pobox = pobox,
				Mailsortcode = mailsortcode,
				Udprn = udprn
			};

			this._customerAddressRepository.SaveOrUpdate(addedAddress);
		}

		[Ajax]
		[HttpGet]
		public JsonResult Index(int id) {
			var customer = this._customerRepository.TryGet(id);
			var builder = new PropertiesModelBuilder(this._customerAddressRepository, this._landRegistryRepository, this._context);
			PropertiesModel data = builder.Create(customer);

			return Json(data, JsonRequestBehavior.AllowGet);
		}

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
					log.Info(ex, "Failed to build enquiry model.");
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
		public void RemoveAddress(int addressId) {
			CustomerAddress noLongerOwnedAddress = this._customerAddressRepository.Get(addressId);
			noLongerOwnedAddress.AddressType = CustomerAddressType.OtherPropertyAddressRemoved;
			this._customerAddressRepository.SaveOrUpdate(noLongerOwnedAddress);
		}

		[Ajax]
		[HttpGet]
		public JsonResult Zoopla(int customerId, bool recheck) {
			new ServiceClient().Instance.GetZooplaData(customerId, true);
			return Json(new {}, JsonRequestBehavior.AllowGet);
		}

		private static readonly ASafeLog log = new SafeILog(typeof(PropertiesController));
		private readonly IEzbobWorkplaceContext _context;
		private readonly CustomerAddressRepository _customerAddressRepository;
		private readonly CustomerRepository _customerRepository;
		private readonly LandRegistryRepository _landRegistryRepository;
	}
}

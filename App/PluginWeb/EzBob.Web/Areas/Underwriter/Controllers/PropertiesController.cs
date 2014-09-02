namespace EzBob.Web.Areas.Underwriter.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Repository;
	using EzBob.Models;
	using Infrastructure;
	using Infrastructure.Attributes;
	using LandRegistryLib;
	using Models;
	using System.Web.Mvc;
	using MoreLinq;

	public class PropertiesController : Controller
	{
		private readonly CustomerRepository _customerRepository;
		private readonly CustomerAddressRepository _customerAddressRepository;
		private readonly LandRegistryRepository _landRegistryRepository;
		private readonly IEzbobWorkplaceContext _context;
		
		public PropertiesController(IEzbobWorkplaceContext context, 
			CustomerRepository customerRepository, 
			CustomerAddressRepository customerAddressRepository, 
			LandRegistryRepository landRegistryRepository) {
			_customerRepository = customerRepository;
			_customerAddressRepository = customerAddressRepository;
			_landRegistryRepository = landRegistryRepository;
			_context = context;
		}

		[Ajax]
		[HttpGet]
		public JsonResult Index(int id)
		{
			var customer = _customerRepository.TryGet(id);
			var builder = new PropertiesModelBuilder(_customerAddressRepository, _landRegistryRepository, _context);
			PropertiesModel data = builder.Create(customer);

			return Json(data, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpGet]
		public JsonResult Zoopla(int customerId, bool recheck)
		{
			var sh = new StrategyHelper();
			sh.GetZooplaData(customerId, true);

			return Json(new { }, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		public void RemoveAddress(int addressId)
		{
			CustomerAddress noLongerOwnedAddress = _customerAddressRepository.Get(addressId);
			noLongerOwnedAddress.AddressType = CustomerAddressType.OtherPropertyAddressRemoved;
			_customerAddressRepository.SaveOrUpdate(noLongerOwnedAddress);
		}

		[Ajax]
		[HttpPost]
		public void AddAddress(int customerId, string addressId, string organisation, string line1, string line2, string line3, string town, string county, string postcode, 
			string country, string rawpostcode, string deliverypointsuffix, string nohouseholds, string smallorg, string pobox, string mailsortcode, string udprn)
		{
			var customer = _customerRepository.Get(customerId);

			var addedAddress = new CustomerAddress
				{
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

			_customerAddressRepository.SaveOrUpdate(addedAddress);
		}

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
			return Json(new { titles = landRegistryEnquiries });
		}
	}
}

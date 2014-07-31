namespace EzBob.Web.Areas.Underwriter.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EzBob.Models;
	using Infrastructure.Attributes;
	using LandRegistryLib;
	using Models;
	using System.Web.Mvc;
	using MoreLinq;
	using StructureMap;

	public class PropertiesController : Controller
	{
		private readonly CustomerRepository customerRepository;
		private readonly CustomerAddressRepository customerAddressRepository;

		public PropertiesController()
		{
			customerRepository = ObjectFactory.GetInstance<CustomerRepository>();
			customerAddressRepository = ObjectFactory.GetInstance<CustomerAddressRepository>();
		}

		[Ajax]
		[HttpGet]
		public JsonResult Index(int id)
		{
			var customer = customerRepository.TryGet(id);
			int numberOfProperties = customer.PropertyStatus.IsOwner ? 1 : 0;
			int otherPropertiesCount = customerAddressRepository.GetAll().Count(a =>
										 a.Customer.Id == customer.Id &&
										 (a.AddressType == CustomerAddressType.OtherPropertyAddress ||
										 a.AddressType == CustomerAddressType.OtherPropertyAddressPrev));

			numberOfProperties += otherPropertiesCount;
			var currentAddress = customer.AddressInfo.PersonalAddress.FirstOrDefault(x => x.AddressType == CustomerAddressType.PersonalAddress);
			Zoopla zoopla = null;
			if (currentAddress != null) zoopla = currentAddress.Zoopla.LastOrDefault();
			int zooplaValue = 0;
			int experianMortgage = 0;
			int experianMortgageCount = 0;
			int zooplaAverage1YearPrice = 0;
			DateTime? zooplaUpdateDate = null;
			if (zoopla != null)
			{
				zooplaAverage1YearPrice = zoopla.AverageSoldPrice1Year;
				zooplaUpdateDate = zoopla.UpdateDate;
				CrossCheckModel.GetZooplaAndMortgagesData(customer, zoopla.ZooplaEstimate, zoopla.AverageSoldPrice1Year, out zooplaValue, out experianMortgage, out experianMortgageCount);
			}

			// TODO: fetch all required data for all owned properties
			var data = new PropertiesModel(numberOfProperties, experianMortgageCount, zooplaValue, experianMortgage, zooplaAverage1YearPrice, zooplaUpdateDate);

			var lrs = customer.LandRegistries.Where(x => x.RequestType == LandRegistryRequestType.Res).Select(x => new { Response = x.Response, Title = x.TitleNumber });
			var b = new LandRegistryModelBuilder();
			data.LandRegistries = new List<LandRegistryResModel>();
			
			foreach (var lr in lrs)
			{
				data.LandRegistries.Add(b.BuildResModel(lr.Response, lr.Title));
			}
			
			var current = customer.AddressInfo.PersonalAddress.FirstOrDefault(x => x.AddressType == CustomerAddressType.PersonalAddress);
			data.Postcode = current.Postcode;
			data.FormattedAddress = current.FormattedAddress;

			return Json(data, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpGet]
		public JsonResult Zoopla(int customerId, bool recheck)
		{
			// TODO: this method should be changed to handle all owned properties of the customer
			var address = customerAddressRepository.GetAll().FirstOrDefault(a => a.Customer.Id == customerId && a.AddressType == CustomerAddressType.PersonalAddress);

			if (address == null)
				return Json(new { error = "address not found" }, JsonRequestBehavior.AllowGet);

			var zoopla = address.Zoopla.LastOrDefault();

			if (zoopla == null || recheck)
			{
				var sh = new StrategyHelper();
				sh.GetZooplaData(customerId, recheck);
				zoopla = address.Zoopla.LastOrDefault();

				if (zoopla == null)
					return Json(new { error = "zoopla info not found" }, JsonRequestBehavior.AllowGet);
			}

			return Json(zoopla, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		public JsonResult LandRegistryEnquiries(int customerId)
		{
			var customer = customerRepository.Get(customerId);
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

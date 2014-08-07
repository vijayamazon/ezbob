namespace EzBob.Web.Areas.Underwriter.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Repository;
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
		private readonly LandRegistryRepository landRegistryRepository;

		public PropertiesController()
		{
			customerRepository = ObjectFactory.GetInstance<CustomerRepository>();
			customerAddressRepository = ObjectFactory.GetInstance<CustomerAddressRepository>();
			landRegistryRepository = ObjectFactory.GetInstance<LandRegistryRepository>();
		}

		[Ajax]
		[HttpGet]
		public JsonResult Index(int id)
		{
			var customer = customerRepository.TryGet(id);
			PropertiesModel data = GetPropertiesModelData(customer, customerAddressRepository, landRegistryRepository);

			return Json(data, JsonRequestBehavior.AllowGet);
		}

		public static PropertiesModel GetPropertiesModelData(Customer customer, CustomerAddressRepository customerAddressRepository, LandRegistryRepository landRegistryRepository)
		{
			int numberOfProperties = customer.PropertyStatus.IsOwnerOfMainAddress ? 1 : 0;

			int otherPropertiesCount = customerAddressRepository.GetAll().Count(a =>
										 a.Customer.Id == customer.Id &&
										 a.AddressType == CustomerAddressType.OtherPropertyAddress);
			numberOfProperties += otherPropertiesCount;

			int zooplaAverage1YearPrice = 0;
			int zooplaValue = 0;
			int experianMortgage = 0;
			int experianMortgageCount = 0;
			DateTime? zooplaUpdateDate = null;
			string postcode = null;
			string formattedAddress = null;

			var data = new PropertiesModel();
			var lrs = customer.LandRegistries.Where(x => x.RequestType == LandRegistryRequestType.Res && 
														 x.ResponseType == LandRegistryResponseType.Success &&
														 x.CustomerAddress != null && 
														 x.CustomerAddress.Customer != null && 
														 x.CustomerAddress.Customer.Id == customer.Id)
											 .Select(x => new { Response = x.Response, Title = x.TitleNumber });

			var b = new LandRegistryModelBuilder();
			data.LandRegistries = new List<LandRegistryResModel>();
			foreach (var lr in lrs)
			{
				LandRegistryResModel lrData = b.BuildResModel(lr.Response, lr.Title);
				data.LandRegistries.Add(lrData);
			}

			if (customer.PropertyStatus.IsOwnerOfMainAddress)
			{
				var currentAddress = customer.AddressInfo.PersonalAddress.FirstOrDefault(x => x.AddressType == CustomerAddressType.PersonalAddress);
				if (currentAddress != null)
				{
					postcode = currentAddress.Postcode;
					formattedAddress = currentAddress.FormattedAddress;

					if (currentAddress.IsOwnerAccordingToLandRegistry)
					{
						Zoopla zoopla = currentAddress.Zoopla.LastOrDefault();

						if (zoopla != null)
						{
							zooplaAverage1YearPrice = zoopla.AverageSoldPrice1Year;
							zooplaUpdateDate = zoopla.UpdateDate;
							CrossCheckModel.GetZooplaData(customer, zoopla.ZooplaEstimate, zoopla.AverageSoldPrice1Year, out zooplaValue);
						}
						
						var personalAddressPropertyModel = new PropertyModel { Address = formattedAddress, MarketValue = zooplaValue };
						LandRegistry matchingLandRegistryEntry = landRegistryRepository.GetAll().FirstOrDefault(x => x.CustomerAddress.AddressId == currentAddress.AddressId);
						personalAddressPropertyModel.YearOfOwnership = 2014;// TODO: get from lrData.???;
						if (matchingLandRegistryEntry != null)
						{
							personalAddressPropertyModel.NumberOfOwners = matchingLandRegistryEntry.Owners.Count;
						}
						data.Properties.Add(personalAddressPropertyModel);
					}
				}
			}

			CrossCheckModel.GetMortgagesData(customer, out experianMortgage, out experianMortgageCount);

			data.Init(numberOfProperties, experianMortgageCount, zooplaValue, experianMortgage, zooplaAverage1YearPrice, zooplaUpdateDate);
			data.Postcode = postcode;
			data.FormattedAddress = formattedAddress;

			foreach (CustomerAddress ownedProperty in customer.AddressInfo.OtherPropertiesAddresses)
			{
				if (!ownedProperty.IsOwnerAccordingToLandRegistry)
				{
					continue;
				}
				
				var ownedPropertyModel = new PropertyModel();
				Zoopla zoopla = ownedProperty.Zoopla.LastOrDefault();

				if (zoopla != null)
				{
					ownedPropertyModel.MarketValue = GetZoopla1YearEstimate(zoopla);

					// Add to totals
					zooplaValue += ownedPropertyModel.MarketValue;
					zooplaAverage1YearPrice += zoopla.AverageSoldPrice1Year;
				}

				ownedPropertyModel.Address = ownedProperty.FormattedAddress;
				LandRegistry matchingLandRegistryEntry = landRegistryRepository.GetAll().FirstOrDefault(x => x.CustomerAddress.AddressId == ownedProperty.AddressId);
				ownedPropertyModel.YearOfOwnership = 2014;// TODO: get from lrData.???;
				if (matchingLandRegistryEntry != null)
				{
					ownedPropertyModel.NumberOfOwners = matchingLandRegistryEntry.Owners.Count;
				}

				data.Properties.Add(ownedPropertyModel);
			}

			return data;
		}

		private static int GetZoopla1YearEstimate(Zoopla zoopla)
		{
			var regexObj = new Regex(@"[^\d]");
			var stringVal = string.IsNullOrEmpty(zoopla.ZooplaEstimate) ? "" : regexObj.Replace(zoopla.ZooplaEstimate.Trim(), "");
			int intVal;
			if (!int.TryParse(stringVal, out intVal))
			{
				intVal = zoopla.AverageSoldPrice1Year;
			}

			return intVal;
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

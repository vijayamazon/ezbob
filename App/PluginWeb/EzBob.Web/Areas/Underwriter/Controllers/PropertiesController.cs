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
			int experianMortgage = 0;
			int experianMortgageCount = 0;
			var data = new PropertiesModel();

			if (customer.PropertyStatus.IsOwnerOfMainAddress)
			{
				var currentAddress = customer.AddressInfo.PersonalAddress.FirstOrDefault(x => x.AddressType == CustomerAddressType.PersonalAddress);
				if (currentAddress != null) {
					var property = GetPropertyModel(customer, currentAddress);
					if(property != null){
						data.Properties.Add(property);
					}
				}
			}

			foreach (CustomerAddress ownedProperty in customer.AddressInfo.OtherPropertiesAddresses.Where(x => x.IsOwnerAccordingToLandRegistry))
			{
				var property = GetPropertyModel(customer, ownedProperty);
				if (property != null)
				{
					data.Properties.Add(property);
				}
			}

			CrossCheckModel.GetMortgagesData(customer, out experianMortgage, out experianMortgageCount);
			data.Init(
				data.Properties.Count(),
				experianMortgageCount, 
				data.Properties.Sum(x => x.MarketValue), 
				experianMortgage, 
				data.Properties.Where(x => x.Zoopla != null).Sum(x => x.Zoopla.AverageSoldPrice1Year));
			
			return data;
		}


		private static PropertyModel GetPropertyModel(Customer customer, CustomerAddress address) {
			int zooplaValue = 0;
			var lrBuilder = new LandRegistryModelBuilder();

			if (address != null)
			{
			
				if (address.IsOwnerAccordingToLandRegistry)
				{
					Zoopla zoopla = address.Zoopla.LastOrDefault();

					if (zoopla != null)
					{
						CrossCheckModel.GetZooplaData(customer, zoopla.ZooplaEstimate, zoopla.AverageSoldPrice1Year, out zooplaValue);
					}

					var personalAddressPropertyModel = new PropertyModel
					{
						AddressId = address.AddressId,
						Address = address.FormattedAddress,
						FormattedAddress = address.FormattedAddress, 
						MarketValue = zooplaValue, 
						Zoopla = zoopla
					};

					var lr = address.LandRegistry
										   .OrderByDescending(x => x.InsertDate)
										   .FirstOrDefault(
											   x =>
											   x.RequestType == LandRegistryRequestType.Res &&
											   x.ResponseType == LandRegistryResponseType.Success);

					if (lr != null)
					{
						personalAddressPropertyModel.LandRegistry = lrBuilder.BuildResModel(lr.Response);
						personalAddressPropertyModel.YearOfOwnership = personalAddressPropertyModel.LandRegistry.Proprietorship.CurrentProprietorshipDate;
						personalAddressPropertyModel.NumberOfOwners = lr.Owners.Count();
					}

					return  personalAddressPropertyModel;
				}
			}

			return null;
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

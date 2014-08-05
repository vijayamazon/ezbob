﻿namespace EzBob.Web.Areas.Underwriter.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
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
			PropertiesModel data = GetPropertiesModelData(customer, customerAddressRepository);

			return Json(data, JsonRequestBehavior.AllowGet);
		}

		public static PropertiesModel GetPropertiesModelData(Customer customer, CustomerAddressRepository customerAddressRepository)
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

			if (customer.PropertyStatus.IsOwnerOfMainAddress)
			{
				var currentAddress = customer.AddressInfo.PersonalAddress.FirstOrDefault(x => x.AddressType == CustomerAddressType.PersonalAddress);
				if (currentAddress != null)
				{
					postcode = currentAddress.Postcode;
					formattedAddress = currentAddress.FormattedAddress;
					Zoopla zoopla = currentAddress.Zoopla.LastOrDefault();

					if (zoopla != null)
					{
						zooplaAverage1YearPrice = zoopla.AverageSoldPrice1Year;
						zooplaUpdateDate = zoopla.UpdateDate;
						CrossCheckModel.GetZooplaAndMortgagesData(customer, zoopla.ZooplaEstimate, zoopla.AverageSoldPrice1Year, out zooplaValue, out experianMortgage, out experianMortgageCount);
					}
				}
			}

			var data = new PropertiesModel(numberOfProperties, experianMortgageCount, zooplaValue, experianMortgage, zooplaAverage1YearPrice, zooplaUpdateDate);
			data.Postcode = postcode;
			data.FormattedAddress = formattedAddress;


			// TODO: Get all owned addr - most expensive first
			//data.Properties.Add(new PropertyModel() { Address = "test addr", NumberOfOwners = 7, YearOfOwnership = 1991 });// Fill properly
			//data.Properties.Add(new PropertyModel() { Address = "test addr2", NumberOfOwners = 71, YearOfOwnership = 1992 });// Fill properly


			foreach (CustomerAddress ownedProperty in customer.AddressInfo.OtherPropertiesAddresses)
			{
				// TODO: Verify ownership in LR

				var ddd = new PropertyModel();
				Zoopla zoopla = ownedProperty.Zoopla.LastOrDefault();

				if (zoopla != null)
				{
					ddd.MarketValue = GetZoopla1YearEstimate(zoopla);

					// Add to totals
					zooplaValue += ddd.MarketValue;
					zooplaAverage1YearPrice += zoopla.AverageSoldPrice1Year;
				}


				ddd.Address = ownedProperty.FormattedAddress;


				// Get matching LR
				//ddd.YearOfOwnership = LR.x
				//ddd.NumberOfOwners = LR.x


			}



			var lrs = customer.LandRegistries.Where(x => x.RequestType == LandRegistryRequestType.Res).Select(x => new { Response = x.Response, Title = x.TitleNumber });
			var b = new LandRegistryModelBuilder();
			data.LandRegistries = new List<LandRegistryResModel>();

			foreach (var lr in lrs)
			{
				data.LandRegistries.Add(b.BuildResModel(lr.Response, lr.Title));
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

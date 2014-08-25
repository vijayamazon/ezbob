namespace EzBob.Web.Areas.Underwriter.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Repository;
	using EzBob.Models;
	using Infrastructure;
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
		private readonly IWorkplaceContext _context;
		public PropertiesController()
		{
			_context = ObjectFactory.GetInstance<IWorkplaceContext>();
			customerRepository = ObjectFactory.GetInstance<CustomerRepository>();
			customerAddressRepository = ObjectFactory.GetInstance<CustomerAddressRepository>();
			landRegistryRepository = ObjectFactory.GetInstance<LandRegistryRepository>();
		}

		[Ajax]
		[HttpGet]
		public JsonResult Index(int id)
		{
			var customer = customerRepository.TryGet(id);
			var builder = new PropertiesModelBuilder(customerAddressRepository, landRegistryRepository, _context);
			PropertiesModel data = builder.Create(customer);

			return Json(data, JsonRequestBehavior.AllowGet);
		}
		
		[Ajax]
		[HttpGet]
		public JsonResult Zoopla(int customerId, bool recheck)
		{
			var sh = new StrategyHelper();
			sh.GetZooplaData(customerId, true);

			return Json(new {}, JsonRequestBehavior.AllowGet);
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

namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using Code;
	using System.Web.Mvc;
	using Infrastructure;
	using Infrastructure.Attributes;
	using Models;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using ServiceClientProxy;

	public class MedalController : Controller
	{
		private readonly CustomerRepository _customerRepository;
		private readonly ServiceClient serviceClient;
		private readonly IWorkplaceContext context;

		public MedalController(CustomerRepository customersRepository, IWorkplaceContext context)
		{
			_customerRepository = customersRepository;
			serviceClient = new ServiceClient();
			this.context = context;
		}

		[Ajax]
		[HttpGet]
		public ActionResult Index(int id)
		{
			var customer = _customerRepository.Get(id);
			var medalCalculator = new MedalCalculators(customer);
			return Json(medalCalculator, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpGet]
		public ActionResult ExportToExel(int id)
		{
			var customer = _customerRepository.Get(id);
			return new MedalExcelReportResult(customer);
		}

		[Ajax]
		[HttpPost]
		public void RecalculateMedal(int customerId)
		{
			serviceClient.Instance.CalculateMedal(context.UserId, customerId);
		}
	}
}
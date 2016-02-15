namespace EzBob.Web.Areas.Underwriter.Controllers
{
	using CommonLib;
	using Infrastructure.Attributes;
	using Models.Fraud;
	using System.Web.Mvc;
	using EzBob.Web.Infrastructure;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;

	public class FraudStatusController : Controller
	{
		private readonly ICustomerRepository _customerRepository;

		public FraudStatusController(ICustomerRepository customerRepository)
		{
			_customerRepository = customerRepository;
		}

		public JsonResult Index(int id)
		{
			var customer = _customerRepository.Get(id);

			var data = new FraudStatusModel
				{
					CurrentStatusId = (int)customer.FraudStatus,
					CurrentStatus = customer.FraudStatus.Description()
				};
			return Json(data, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		[Transactional]
		[Ajax]
		[Permission(Name = "FraudStatus")]
		public JsonResult Save(int customerId, int currentStatus)
		{
			var customer = _customerRepository.Get(customerId);
			customer.FraudStatus = (FraudStatus)currentStatus;
			return Json(new { });
		}
	}
}

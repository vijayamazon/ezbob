namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Infrastructure.Attributes;
	using Models;

	public class CustomerInfoController : Controller
	{
		private readonly CustomerRepository customerRepository;

		public CustomerInfoController(CustomerRepository customerRepository) {
			this.customerRepository = customerRepository;
		}

		[Ajax]
		[HttpGet]
		public JsonResult Index(int id)
		{
			var customer = customerRepository.Get(id);
			var model = new PersonalInfoModel();
			model.InitFromCustomer(customer);
			return Json(model, JsonRequestBehavior.AllowGet);
		}
	}
}

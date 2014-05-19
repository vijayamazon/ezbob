namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Infrastructure.Attributes;
	using Web.Models;
	using ActionResult = System.Web.Mvc.ActionResult;

	public class ProfileController : Controller
	{
		private readonly ProfileSummaryModelBuilder _summaryModelBuilder;
		private CustomerRepository CustomerRepository { get; set; }

		public ProfileController(CustomerRepository customerRepository, ProfileSummaryModelBuilder summaryModelBuilder)
		{
			_summaryModelBuilder = summaryModelBuilder;
			CustomerRepository = customerRepository;
		}

		[Ajax]
		[HttpGet]
		public ActionResult Index(int id)
		{
			var customer = CustomerRepository.Get(id);
			var model = _summaryModelBuilder.CreateProfile(customer);
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		[Transactional]
		public JsonResult SaveComment(string comment, int id)
		{
			var customer = CustomerRepository.Get(id);
			customer.Comment = comment;
			return Json(new { Saved = "true" });
		}
	}
}
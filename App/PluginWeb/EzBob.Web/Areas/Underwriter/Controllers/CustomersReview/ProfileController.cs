namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview {
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Infrastructure.Attributes;
	using Web.Models;
	using ActionResult = System.Web.Mvc.ActionResult;

	public class ProfileController : Controller {
		public ProfileController(CustomerRepository customerRepository, ProfileSummaryModelBuilder summaryModelBuilder) {
			this.summaryModelBuilder = summaryModelBuilder;
			this.customerRepository = customerRepository;
		} // constructor

		[Ajax]
		[HttpGet]
		public ActionResult Index(int id) {
			var customer = this.customerRepository.Get(id);
			var model = this.summaryModelBuilder.CreateProfile(customer);
			return Json(model, JsonRequestBehavior.AllowGet);
		} // Index

		[Ajax]
		[HttpPost]
		[Transactional]
		public JsonResult SaveComment(string comment, int id) {
			var customer = this.customerRepository.Get(id);
			customer.Comment = comment;
			return Json(new { Saved = "true" });
		} // SaveComment

		private readonly CustomerRepository customerRepository;
		private readonly ProfileSummaryModelBuilder summaryModelBuilder;
	} // class ProfileController
} // namespace

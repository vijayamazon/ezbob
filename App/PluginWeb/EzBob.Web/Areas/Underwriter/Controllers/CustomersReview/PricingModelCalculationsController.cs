namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System.Data;
	using System.Web.Mvc;
	using Infrastructure.Attributes;
	using Infrastructure.csrf;
	using Models;

	public class PricingModelCalculationsController : Controller
    {
		public PricingModelCalculationsController()
        {
        }

        [Ajax]
        [HttpGet]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
        public ActionResult Index(int id)
		{
			//TODO: complete logic
			var model = new PricingModelModel(id);
			return Json(model, JsonRequestBehavior.AllowGet);
        }

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Calculate(decimal loanAmount)
		{
			//TODO: complete logic
			var model = new PricingModelModel(1);
			model.MonthlyInterestToCharge = 1.1m;
			return Json(model, JsonRequestBehavior.AllowGet);
		}
    }
}
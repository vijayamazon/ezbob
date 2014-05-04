namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System.Data;
	using System.Web.Mvc;
	using Infrastructure.Attributes;

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
            return Json(JsonRequestBehavior.AllowGet);
        }
    }
}
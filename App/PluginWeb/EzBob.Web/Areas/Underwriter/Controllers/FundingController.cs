namespace EzBob.Web.Areas.Underwriter.Controllers
{
	using System.Web.Mvc;

    public class FundingController : Controller
    {
		public JsonResult GetCurrentFundingStatus()
        {
           
            return Json(new {Funds = 165477}, JsonRequestBehavior.AllowGet);
        }
    }
}
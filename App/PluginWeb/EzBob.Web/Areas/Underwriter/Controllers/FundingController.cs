namespace EzBob.Web.Areas.Underwriter.Controllers
{
	using System.Web.Mvc;
	using Infrastructure;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;
	using StructureMap;

	public class FundingController : Controller
	{
		private readonly ServiceClient serviceClient;
		private readonly IWorkplaceContext context;

		public FundingController()
		{
			context = ObjectFactory.GetInstance<IWorkplaceContext>();
			serviceClient = new ServiceClient();
		}

		public JsonResult GetCurrentFundingStatus()
		{
			AvailableFundsActionResult availableFunds = serviceClient.Instance.GetAvailableFunds(context.UserId);
			return Json(new { AvailableFunds = availableFunds.AvailableFunds, ReservedAmount = availableFunds.ReservedAmount }, JsonRequestBehavior.AllowGet);
        }
    }
}
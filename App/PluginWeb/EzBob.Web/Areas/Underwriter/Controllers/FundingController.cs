namespace EzBob.Web.Areas.Underwriter.Controllers
{
	using System;
	using System.Web.Mvc;
	using ConfigManager;
	using Infrastructure;
	using Infrastructure.Attributes;
	using Infrastructure.csrf;
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

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpPost]
		public JsonResult GetAvailableFundsInterval()
		{
			return Json(CurrentValues.Instance.AvailableFundsRefreshInterval.Value, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[HttpPost]
		public JsonResult GetRequiredFunds()
		{
			var today = DateTime.UtcNow;
			int relevantLimit = (today.DayOfWeek == DayOfWeek.Thursday || today.DayOfWeek == DayOfWeek.Friday) ? CurrentValues.Instance.PacnetBalanceWeekendLimit : CurrentValues.Instance.PacnetBalanceWeekdayLimit;
			return Json(relevantLimit, JsonRequestBehavior.AllowGet);
		}
    }
}
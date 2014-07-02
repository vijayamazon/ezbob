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
			var today = DateTime.UtcNow;
			int relevantLimit = (today.DayOfWeek == DayOfWeek.Thursday || today.DayOfWeek == DayOfWeek.Friday) ? CurrentValues.Instance.PacnetBalanceWeekendLimit : CurrentValues.Instance.PacnetBalanceWeekdayLimit;
			int refreshInterval = CurrentValues.Instance.AvailableFundsRefreshInterval;
			return Json(new { AvailableFunds = availableFunds.AvailableFunds, ReservedAmount = availableFunds.ReservedAmount, RequiredFunds = relevantLimit, RefreshInterval = refreshInterval }, JsonRequestBehavior.AllowGet);
        }

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult SavePacnetManual(int amount, int limit) // remove limit from here and from call
		{
			serviceClient.Instance.RecordManualPacnetDeposit(context.UserId, User.Identity.Name, amount);
			return Json(true);
		}

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult DisableCurrentManualPacnetDeposits(bool isSure)
		{
			if (isSure)
			{
				serviceClient.Instance.DisableCurrentManualPacnetDeposits(context.UserId);
			}
			return Json(true);
		}
	}
}
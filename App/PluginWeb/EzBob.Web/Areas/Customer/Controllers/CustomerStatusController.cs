using System.Web.Mvc;

namespace EzBob.Web.Areas.Customer.Controllers
{
	using ConfigManager;
	using Infrastructure.Attributes;
	using Infrastructure.csrf;
	using ServiceClientProxy;

	public class CustomerStatusController : Controller
    {

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult GetRefreshInterval()
		{
			int refreshInterval = CurrentValues.Instance.CustomerStateRefreshInterval;
			return Json(new { Interval = refreshInterval });
		} // GetRefreshInterval

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult GetCustomerStatus(int customerId)
		{
			string state = new ServiceClient().Instance.GetCustomerState(customerId).Value;
			return Json(new { State = state });
		} // GetCustomerStatus

    }
}

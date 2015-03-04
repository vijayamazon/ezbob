namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System.Web.Mvc;
	using Ezbob.Logger;
	using Infrastructure;
	using Infrastructure.Attributes;
	using Infrastructure.csrf;
	using Newtonsoft.Json;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;
	using ActionResult = System.Web.Mvc.ActionResult;

	public class PricingModelCalculationsController : Controller
	{
		private readonly ServiceClient serviceClient;
		private readonly IWorkplaceContext context;

		public PricingModelCalculationsController(IWorkplaceContext context)
		{
			this.context = context;
			serviceClient = new ServiceClient();
		}

		[Ajax]
		[HttpPost]
		public ActionResult GetScenarioConfigs(int customerId, string scenarioName)
		{
			PricingModelModelActionResult getPricingModelModelResponse = serviceClient.Instance.GetPricingModelModel(customerId, context.UserId, scenarioName);
			return Json(getPricingModelModelResponse.Value, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpGet]
		public ActionResult Index(int id)
		{
			PricingModelModelActionResult getPricingModelModelResponse = serviceClient.Instance.GetPricingModelModel(id, context.UserId, "Basic New");
			return Json(getPricingModelModelResponse.Value, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpGet]
		public ActionResult GetScenarios()
		{
			StringListActionResult getPricingModelScenariosResponse = serviceClient.Instance.GetPricingModelScenarios(context.UserId);
			return Json(new {scenarios = getPricingModelScenariosResponse.Records}, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Calculate(int customerId, string pricingModelModel)
		{
			var oLog = new SafeILog(this);

			oLog.Debug("Model received: {0}", pricingModelModel);

			PricingModelModel inputModel = JsonConvert.DeserializeObject<PricingModelModel>(pricingModelModel);

			oLog.Debug("Parsed model: {0}", JsonConvert.SerializeObject(inputModel));

			PricingModelModelActionResult pricingModelCalculateResponse = serviceClient.Instance.PricingModelCalculate(
				customerId, context.UserId, inputModel
			);

			return Json(pricingModelCalculateResponse.Value, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult GetDefaultRate(int customerId, decimal companyShare)
		{
			DecimalActionResult getDefaultRateResponse = serviceClient.Instance.GetPricingModelDefaultRate(
				customerId, context.UserId, companyShare
			);

			return Json(getDefaultRateResponse.Value, JsonRequestBehavior.AllowGet);
		}
	}
}
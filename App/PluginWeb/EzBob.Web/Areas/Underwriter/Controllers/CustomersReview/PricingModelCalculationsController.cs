namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview {
	using System.Web.Mvc;
	using Ezbob.Logger;
	using Infrastructure;
	using Infrastructure.Attributes;
	using Infrastructure.csrf;
	using Newtonsoft.Json;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;
	using ActionResult = System.Web.Mvc.ActionResult;

	public class PricingModelCalculationsController : Controller {
		public PricingModelCalculationsController(IWorkplaceContext context) {
			this.context = context;
			this.serviceClient = new ServiceClient();
		} // constructor

		[Ajax]
		[HttpPost]
		public ActionResult GetScenarioConfigs(int customerId, string scenarioName) {
			PricingModelModelActionResult getPricingModelModelResponse =
				this.serviceClient.Instance.GetPricingModelModel(customerId, this.context.UserId, scenarioName);

			return Json(getPricingModelModelResponse.Value, JsonRequestBehavior.AllowGet);
		} // GetScenarioConfigs

		[Ajax]
		[HttpGet]
		public ActionResult Index(int id) {
			return GetScenarioConfigs(id, "Basic New");
		} // Index

		[Ajax]
		[HttpGet]
		public ActionResult GetScenarios() {
			PricingScenarioNameListActionResult getPricingModelScenariosResponse =
				this.serviceClient.Instance.GetPricingModelScenarios(this.context.UserId);

			return Json(new { scenarios = getPricingModelScenariosResponse.Names }, JsonRequestBehavior.AllowGet);
		} // GetScenarios

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Calculate(int customerId, string pricingModelModel) {
			var oLog = new SafeILog(this);

			oLog.Debug("Model received: {0}", pricingModelModel);

			PricingModelModel inputModel = JsonConvert.DeserializeObject<PricingModelModel>(pricingModelModel);

			oLog.Debug("Parsed model: {0}", JsonConvert.SerializeObject(inputModel));

			PricingModelModelActionResult pricingModelCalculateResponse =
				this.serviceClient.Instance.PricingModelCalculate(customerId, this.context.UserId, inputModel);

			oLog.Debug("Calculated model: {0}", JsonConvert.SerializeObject(pricingModelCalculateResponse.Value));

			return Json(pricingModelCalculateResponse.Value, JsonRequestBehavior.AllowGet);
		} // Calculate

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult GetDefaultRate(int customerId, decimal companyShare) {
			DecimalActionResult getDefaultRateResponse = this.serviceClient.Instance.GetPricingModelDefaultRate(
				customerId, this.context.UserId, companyShare
			);

			return Json(getDefaultRateResponse.Value, JsonRequestBehavior.AllowGet);
		} // GetDefaultRate

		private readonly ServiceClient serviceClient;
		private readonly IWorkplaceContext context;
	} // class PricingModelCalculationsController
} // namespace

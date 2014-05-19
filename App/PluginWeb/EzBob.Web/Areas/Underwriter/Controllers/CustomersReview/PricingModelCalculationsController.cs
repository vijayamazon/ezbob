﻿namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System.Data;
	using System.Web.Mvc;
	using Ezbob.Logger;
	using Infrastructure;
	using Infrastructure.Attributes;
	using Infrastructure.csrf;
	using Newtonsoft.Json;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;
	using StructureMap;
	using ActionResult = System.Web.Mvc.ActionResult;

	public class PricingModelCalculationsController : Controller
	{
		private readonly ServiceClient serviceClient;
		private readonly IWorkplaceContext context;

		public PricingModelCalculationsController()
		{
			context = ObjectFactory.GetInstance<IWorkplaceContext>();
			serviceClient = new ServiceClient();
		}

		[Ajax]
		[HttpGet]
		public ActionResult Index(int customerId)
		{
			PricingModelModelActionResult getPricingModelModelResponse = serviceClient.Instance.GetPricingModelModel(customerId, context.UserId);
			return Json(getPricingModelModelResponse.Value, JsonRequestBehavior.AllowGet);
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
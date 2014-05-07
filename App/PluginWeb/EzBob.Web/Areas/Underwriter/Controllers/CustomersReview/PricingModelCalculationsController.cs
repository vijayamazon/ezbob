namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System.Data;
	using System.Web.Mvc;
	using Infrastructure;
	using Infrastructure.Attributes;
	using Infrastructure.csrf;
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
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
        public ActionResult Index(int customerId)
        {
			PricingModelModelActionResult getPricingModelModelResponse = serviceClient.Instance.GetPricingModelModel(customerId, context.UserId);
			return Json(getPricingModelModelResponse.Value, JsonRequestBehavior.AllowGet);
        }

		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Calculate(decimal loanAmount)
		{
			int customerId = 889; // TODO: Should be input param
			var inputModel = new PricingModelModel(); // TODO: Should be input param

			PricingModelModelActionResult pricingModelCalculateesponse = serviceClient.Instance.PricingModelCalculate(customerId, context.UserId, inputModel);
			return Json(pricingModelCalculateesponse.Value, JsonRequestBehavior.AllowGet);
		}
    }
}
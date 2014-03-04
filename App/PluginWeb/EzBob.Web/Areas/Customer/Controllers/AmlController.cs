namespace EzBob.Web.Areas.Customer.Controllers {
	using System.Data;
	using System.Web.Mvc;
	using Code;
	using EzServiceReference;
	using Infrastructure;
	using Infrastructure.csrf;
	using Scorto.Web;
	
	public class AmlController : Controller {
		public AmlController(IEzbobWorkplaceContext context) {
			m_oServiceClient = ServiceClient.Instance;
			this.context = context;
		} // constructor

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonNetResult PerformAmlCheck() {
			m_oServiceClient.CheckAml(context.Customer.Id);
			return this.JsonNet(new { });
		} // PerformAmlCheck

		private readonly EzServiceClient m_oServiceClient;
		private readonly IEzbobWorkplaceContext context;
	} // class AMlController
} // namespace EzBob.Web.Areas.Customer.Controllers

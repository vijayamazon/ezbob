namespace EzBob.Web.Areas.Customer.Controllers
{
	using System.Web.Mvc;
	using Code.ApplicationCreator;
	using Infrastructure;
	using Infrastructure.csrf;
	using Scorto.Web;
	
	public class AmlController : Controller
	{
		public AmlController(IAppCreator creator, IEzbobWorkplaceContext context)
		{
			this.creator = creator;
			this.context = context;
		} // constructor
		
		[Transactional]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonNetResult PerformAmlCheck()
		{
			creator.PerformAmlCheck(context.Customer.Id);

			return this.JsonNet(new { });
		} // PerformAmlCheck
		
		private readonly IAppCreator creator;
		private readonly IEzbobWorkplaceContext context;
	} // class AMlController
} // namespace EzBob.Web.Areas.Customer.Controllers

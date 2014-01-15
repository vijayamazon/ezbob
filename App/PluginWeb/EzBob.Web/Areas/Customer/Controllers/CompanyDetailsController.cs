namespace EzBob.Web.Areas.Customer.Controllers
{
	using System.Web.Mvc;
	using Code.ApplicationCreator;
	using Infrastructure.csrf;
	using Scorto.Web;
	using log4net;

	#region class CompanyDetailsController

	public class CompanyDetailsController : Controller
	{
		#region public

		#region constructor

		public CompanyDetailsController(IAppCreator creator)
		{
			this.creator = creator;
		} // constructor

		#endregion constructor

		#region method PerformCompanyCheck

		[Transactional]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonNetResult PerformCompanyCheck(int customerId)
		{
			creator.PerformCompanyCheck(customerId);

			return this.JsonNet(new { });
		} // PerformCompanyCheck

		#endregion method PerformCompanyCheck
		
		#region private properties

		private static readonly ILog log = LogManager.GetLogger(typeof(CompanyDetailsController));
		private readonly IAppCreator creator;

		#endregion private properties

		#endregion private
	} // class CompanyDetailsController

	#endregion class CompanyDetailsController
} // namespace EzBob.Web.Areas.Customer.Controllers

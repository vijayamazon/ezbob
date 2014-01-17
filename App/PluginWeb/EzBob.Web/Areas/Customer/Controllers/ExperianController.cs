namespace EzBob.Web.Areas.Customer.Controllers
{
	using System;
	using System.Web.Mvc;
	using Code.ApplicationCreator;
	using Infrastructure;
	using Infrastructure.csrf;
	using Scorto.Web;
	using log4net;

	#region class ExperianController

	public class ExperianController : Controller
	{
		#region public

		#region constructor

		public ExperianController(IAppCreator creator, IEzbobWorkplaceContext context)
		{
			this.creator = creator;

			this.context = context;
		} // constructor

		#endregion constructor
		
		[Transactional]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonNetResult PerformCompanyCheck()
		{
			creator.PerformCompanyCheck(context.Customer.Id);

			return this.JsonNet(new { });
		} // PerformCompanyCheck
		
		[Transactional]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonNetResult PerformConsumerCheck()
		{
			creator.PerformConsumerCheck(context.Customer.Id, 0);

			return this.JsonNet(new { });
		} // PerformCompanyCheck
		
		#region private properties

		private static readonly ILog log = LogManager.GetLogger(typeof(ExperianController));
		private readonly IAppCreator creator;
		private readonly IEzbobWorkplaceContext context;

		#endregion private properties

		#endregion private
	} // class ExperianController

	#endregion class ExperianController
} // namespace EzBob.Web.Areas.Customer.Controllers

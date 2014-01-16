namespace EzBob.Web.Areas.Customer.Controllers
{
	using System;
	using System.Web.Mvc;
	using Code.ApplicationCreator;
	using Infrastructure.csrf;
	using Scorto.Web;
	using log4net;

	#region class ExperianController

	public class ExperianController : Controller
	{
		#region public

		#region constructor

		public ExperianController(IAppCreator creator)
		{
			this.creator = creator;
		} // constructor

		#endregion constructor
		
		[Transactional]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonNetResult PerformCompanyCheck(int customerId)
		{
			creator.PerformCompanyCheck(customerId);

			return this.JsonNet(new { });
		} // PerformCompanyCheck
		
		[Transactional]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonNetResult PerformConsumerCheck(int customerId)
		{
			creator.PerformConsumerCheck(customerId, 0);

			return this.JsonNet(new { });
		} // PerformCompanyCheck
		
		#region private properties

		private static readonly ILog log = LogManager.GetLogger(typeof(ExperianController));
		private readonly IAppCreator creator;

		#endregion private properties

		#endregion private
	} // class ExperianController

	#endregion class ExperianController
} // namespace EzBob.Web.Areas.Customer.Controllers

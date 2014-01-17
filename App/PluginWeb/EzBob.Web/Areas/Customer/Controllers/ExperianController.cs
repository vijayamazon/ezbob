namespace EzBob.Web.Areas.Customer.Controllers
{
	using System.Web.Mvc;
	using Code.ApplicationCreator;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Mapping;
	using Infrastructure;
	using Infrastructure.csrf;
	using Scorto.Web;
	using log4net;
	using System.Linq;

	#region class ExperianController

	public class ExperianController : Controller
	{
		#region public

		#region constructor

		public ExperianController(IAppCreator creator, IEzbobWorkplaceContext context, DirectorRepository directorRepository)
		{
			this.creator = creator;
			this.context = context;
			this.directorRepository = directorRepository;
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
		} // PerformConsumerCheck
		
		[Transactional]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonNetResult PerformConsumerCheckForDirectors()
		{
			int customerId = context.Customer.Id;
			IQueryable<Director> directors = directorRepository.GetAll().Where(x => x.Customer.Id == customerId);
			foreach (Director director in directors)
			{
				creator.PerformConsumerCheck(customerId, director.Id);
			}

			return this.JsonNet(new { });
		} // PerformConsumerCheckForDirectors
		
		#region private properties

		private static readonly ILog log = LogManager.GetLogger(typeof(ExperianController));
		private readonly IAppCreator creator;
		private readonly IEzbobWorkplaceContext context;
		private readonly DirectorRepository directorRepository;

		#endregion private properties

		#endregion private
	} // class ExperianController

	#endregion class ExperianController
} // namespace EzBob.Web.Areas.Customer.Controllers

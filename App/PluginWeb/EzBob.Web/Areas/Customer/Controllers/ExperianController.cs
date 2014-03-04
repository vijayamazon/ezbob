namespace EzBob.Web.Areas.Customer.Controllers {
	using System.Data;
	using System.Web.Mvc;
	using Code;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Mapping;
	using EzServiceReference;
	using Infrastructure;
	using Infrastructure.csrf;
	using Scorto.Web;
	using System.Linq;

	#region class ExperianController

	public class ExperianController : Controller {
		#region public

		#region constructor

		public ExperianController(IEzbobWorkplaceContext context, DirectorRepository directorRepository) {
			this.context = context;
			this.directorRepository = directorRepository;
			m_oServiceClient = ServiceClient.Instance;
		} // constructor

		#endregion constructor

		#region method PerformConsumerCheck

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonNetResult PerformConsumerCheck() {
			m_oServiceClient.CheckExperianConsumer(context.Customer.Id, 0);

			return this.JsonNet(new { });
		} // PerformConsumerCheck

		#endregion method PerformConsumerCheck

		#region method PerformConsumerCheckForDirectors

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonNetResult PerformConsumerCheckForDirectors() {
			int customerId = context.Customer.Id;

			IQueryable<Director> directors = directorRepository.GetAll().Where(x => x.Customer.Id == customerId);

			foreach (Director director in directors)
				m_oServiceClient.CheckExperianConsumer(customerId, director.Id);

			return this.JsonNet(new { });
		} // PerformConsumerCheckForDirectors

		#endregion method PerformConsumerCheckForDirectors

		#region private properties

		private readonly IEzbobWorkplaceContext context;
		private readonly DirectorRepository directorRepository;
		private readonly EzServiceClient m_oServiceClient;

		#endregion private properties

		#endregion private
	} // class ExperianController

	#endregion class ExperianController
} // namespace EzBob.Web.Areas.Customer.Controllers

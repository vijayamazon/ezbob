namespace EzBob.Web.Areas.Customer.Controllers {
	using System.Data;
	using System.Web.Mvc;
	using Code;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Mapping;
	using Infrastructure;
	using Infrastructure.Attributes;
	using Infrastructure.csrf;
	using System.Linq;

	#region class ExperianController

	public class ExperianController : Controller {
		#region public

		#region constructor

		public ExperianController(IEzbobWorkplaceContext context, DirectorRepository directorRepository) {
			this.context = context;
			this.directorRepository = directorRepository;
			m_oServiceClient = new ServiceClient();
		} // constructor

		#endregion constructor

		#region method PerformConsumerCheck

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult PerformConsumerCheck() {
			m_oServiceClient.Instance.CheckExperianConsumer(context.Customer.Id, 0, false);

			return Json(new { });
		} // PerformConsumerCheck

		#endregion method PerformConsumerCheck

		#region method PerformConsumerCheckForDirectors

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult PerformConsumerCheckForDirectors() {
			int customerId = context.Customer.Id;

			IQueryable<Director> directors = directorRepository.GetAll().Where(x => x.Customer.Id == customerId);

			foreach (Director director in directors)
				m_oServiceClient.Instance.CheckExperianConsumer(customerId, director.Id, false);

			return Json(new { });
		} // PerformConsumerCheckForDirectors

		#endregion method PerformConsumerCheckForDirectors

		#region private properties

		private readonly IEzbobWorkplaceContext context;
		private readonly DirectorRepository directorRepository;
		private readonly ServiceClient m_oServiceClient;

		#endregion private properties

		#endregion private
	} // class ExperianController

	#endregion class ExperianController
} // namespace EzBob.Web.Areas.Customer.Controllers

namespace EzBob.Web.Areas.Broker.Controllers {
	using System.Web.Mvc;
	using ApplicationMng.Repository;
	using Code.ApplicationCreator;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database.Broker;

	public class BrokerHomeController : Controller {
		#region public

		#region constructor

		public BrokerHomeController(
			DatabaseDataHelper oHelper,
			IAppCreator oAppCreator
		) {
			m_oHelper = oHelper;
			m_oAppCreator = oAppCreator;
		} // constructor

		#endregion constructor

		#region action Index (default)

		// GET: /Broker/BrokerHome/
		public ActionResult Index() {
			const string sAuth = "auth";
			const string sForbidden = "-";

			ViewData[sAuth] = string.Empty;

			if (User.Identity.IsAuthenticated) {
				Broker brkr = BrokerRepo.Find(User.Identity.Name);
				ViewData[sAuth] = ReferenceEquals(brkr, null) ? sForbidden : User.Identity.Name;
			} // if

			return View();
		} // Index

		#endregion action Index (default)

		#endregion public

		#region private

		#region property BrokerRepo

		private BrokerRepository BrokerRepo {
			get { return m_oHelper.BrokerRepository; } // get
		} // BrokerRepo

		#endregion property BrokerRepo

		#region fields

		private readonly DatabaseDataHelper m_oHelper;
		private readonly IAppCreator m_oAppCreator;

		#endregion fields

		#endregion private
	} // class BrokerHomeController
} // namespace

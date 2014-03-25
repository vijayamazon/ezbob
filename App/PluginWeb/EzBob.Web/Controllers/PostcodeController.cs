namespace EzBob.Web.Controllers {
	using System.Data;
	using System.Web.Mvc;
	using Code.PostCode;
	using Infrastructure;
	using Scorto.Web;

	[Authorize]
	public class PostcodeController : Controller {
		#region constructor

		public PostcodeController(IEzbobWorkplaceContext context, IPostCodeFacade facade) {
			m_oContext = context;
			m_oFacade = facade;
		} // constructor

		#endregion constructor

		#region action GetAddressFromPostCode

		[OutputCache(VaryByParam = "postCode", Duration = 3600 * 24 * 7)]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonResult GetAddressFromPostCode(string postCode) {
			return Json(m_oFacade.GetAddressFromPostCode(postCode, m_oContext.User.Id), JsonRequestBehavior.AllowGet);
		} // GetAddressFromPostCode

		#endregion action GetAddressFromPostCode

		#region action GetFullAddressFromPostCode

		[OutputCache(VaryByParam = "id", Duration = 3600 * 24 * 7)]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonResult GetFullAddressFromPostCode(string id) {
			return Json(m_oFacade.GetFullAddressFromPostCode(id, m_oContext.User.Id), JsonRequestBehavior.AllowGet);
		} // GetFullAddressFromPostCode

		#endregion action GetFullAddressFromPostCode

		#region private

		private readonly IEzbobWorkplaceContext m_oContext;
		private readonly IPostCodeFacade m_oFacade;

		#endregion private
	} // class PostcodeController
} // namespace

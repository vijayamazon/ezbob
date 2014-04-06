using System.Web.Mvc;
using EZBob.DatabaseLib.Repository;
using StructureMap;

namespace EzBob.Web.Areas.Underwriter.Controllers.Reports {
	public class CaisReportsHistoryController : Controller {
		private readonly CaisReportsHistoryRepository _caisReportsHistoryRepository;

		public CaisReportsHistoryController() {
			_caisReportsHistoryRepository = ObjectFactory.GetInstance<CaisReportsHistoryRepository>();
		}

		public JsonResult Index() {
			var caisReportsHistorys = _caisReportsHistoryRepository.GetAll();
			return Json(caisReportsHistorys, JsonRequestBehavior.AllowGet);
		}
	}
}

using System.Web.Mvc;
using EZBob.DatabaseLib.Repository;
using Scorto.Web;
using StructureMap;

namespace EzBob.Web.Areas.Underwriter.Controllers.Reports
{
    public class CaisReportsHistoryController : Controller
    {
        private readonly CaisReportsHistoryRepository _caisReportsHistoryRepository;

        public CaisReportsHistoryController()
        {
            _caisReportsHistoryRepository = ObjectFactory.GetInstance<CaisReportsHistoryRepository>();
        }

        public JsonNetResult Index()
        {
            var caisReportsHistorys = _caisReportsHistoryRepository.GetAll();
            return this.JsonNet(caisReportsHistorys);
        }
    }
}

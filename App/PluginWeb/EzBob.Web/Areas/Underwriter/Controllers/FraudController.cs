using System.Linq;
using System.Web.Mvc;
using EZBob.DatabaseLib.Repository;
using Scorto.Web;

namespace EzBob.Web.Areas.Underwriter.Controllers
{
    public class FraudController : Controller
    {
        private readonly FraudUserRepository _fraudUserRepository;

        public FraudController(FraudUserRepository fraudUserRepository)
        {
            _fraudUserRepository = fraudUserRepository;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonNetResult GetAll()
        {
            var model = _fraudUserRepository.GetAll().ToList();
            return this.JsonNet(model);
        }
    }
}

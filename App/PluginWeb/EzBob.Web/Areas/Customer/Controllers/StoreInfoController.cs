using System.Web.Mvc;
using Scorto.Web;
using log4net;

namespace EzBob.Web.Areas.Customer.Controllers
{
    public class StoreInfoController : Controller
    {
        private readonly IWorkplaceContext _context;

        private static readonly ILog _log = log4net.LogManager.GetLogger(typeof(StoreInfoController));


        public StoreInfoController(IWorkplaceContext context)
        {
            _context = context;
        }

        public ActionResult Index()
        {
            return View();
        }
    }
}

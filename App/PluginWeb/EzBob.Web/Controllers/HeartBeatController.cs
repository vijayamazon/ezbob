using System.Web.Mvc;
using Scorto.Web;

namespace EzBob.Web.Controllers
{
    [Transactional]
    public class HeartBeatController: Controller
    {
         public JsonNetResult Index()
         {
             return new JsonNetResult();
         }
    }
}
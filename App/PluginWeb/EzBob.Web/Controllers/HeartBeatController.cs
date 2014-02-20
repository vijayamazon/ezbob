namespace EzBob.Web.Controllers
{
	using System.Data;
	using System.Web.Mvc;
	using Scorto.Web;

	[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
    public class HeartBeatController: Controller
    {
         public JsonNetResult Index()
         {
             return new JsonNetResult();
         }
    }
}
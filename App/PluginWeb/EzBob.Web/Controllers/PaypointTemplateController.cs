using System.IO;
using System.Text;
using System.Web.Mvc;

namespace EzBob.Web.Controllers
{
    public class PaypointTemplateController : Controller
    {
        //
        // GET: /PaypointTemplate/

        public ActionResult Index()
        {
            return View();
        }

        public ViewResult DownloadPage()
        {
            return View();
        }

        public ActionResult DownloadProdTemplate()
        {
            var html = RenderRazorViewToString(@"Index", true);
            var bytes = Encoding.UTF8.GetBytes(html);
            return File(bytes, "text/plain", "ezbob-template.html");

        }

		public ActionResult DownloadTestTemplate()
        {
            var html = RenderRazorViewToString(@"Index", false);
            var bytes = Encoding.UTF8.GetBytes(html);
            return File(bytes, "text/plain", "ezbob-template-test.html");

        }

        public string RenderRazorViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }
    }
}

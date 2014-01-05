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
            return View(model: "Prod");
        }

        public ViewResult DownloadPage()
        {
            return View();
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

	    public ActionResult DownloadTemplate(string type)
	    {
			var html = RenderRazorViewToString(@"Index", type);
			var bytes = Encoding.UTF8.GetBytes(html);
		    switch (type)
		    {
				case "Prod":
					return File(bytes, "text/plain", "ezbob-template.html");
				default:
					return File(bytes, "text/plain", "ezbob-template-test.html");
		    }
			
	    }
    }
}

using System.IO;
using System.Text;
using System.Web.Mvc;

namespace EzBob.Web.Controllers
{
	using EzBob.Web.Infrastructure;

	public class PaypointTemplateController : Controller
    {
        //
        // GET: /PaypointTemplate/

        public ActionResult Index(string type = "Prod")
        {
			UiCustomerOrigin.Set(ViewBag);
            return View(model: type);
        }

        public ViewResult DownloadPage()
        {
			UiCustomerOrigin.Set(ViewBag);
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
			UiCustomerOrigin.Set(ViewBag);
			var origin = ViewBag.CustomerOrigin.GetOrigin();
			var html = RenderRazorViewToString(@"Index", type);
			var bytes = Encoding.UTF8.GetBytes(html);
		    switch (type)
		    {
				case "Prod":
					return File(bytes, "text/plain", origin + "-template.html");
				case "Dev":
					return File(bytes, "text/plain", origin + "-template-dev.html");
				case "Qa":
					return File(bytes, "text/plain", origin + "-template-test.html");
				default:
					return File(bytes, "text/plain", origin + "-template.html");
		    }
			
	    }
    }
}

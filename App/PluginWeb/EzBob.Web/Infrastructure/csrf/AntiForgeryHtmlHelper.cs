using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace EzBob.Web.Infrastructure.csrf
{
    public static class AntiForgeryHtmlHelper
    {
        public static MvcHtmlString AntiForgeryTokenNoUserCheck(this HtmlHelper html, string salt = null, string domain=null, string path=null)
        {
            var httpContext = new JsonAntiForgeryHttpContextWrapper(HttpContext.Current/*html.ViewContext.HttpContext*/);
            return new MvcHtmlString(AntiForgery.GetHtml(httpContext, salt, domain, path).ToString()); 
        }
  
    }
}
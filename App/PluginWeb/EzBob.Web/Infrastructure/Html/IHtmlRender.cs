using System.Web.Mvc;

namespace EzBob.Web.Infrastructure.Html
{
    public interface IHtmlRender
    {
        MvcHtmlString Render();
    }
}
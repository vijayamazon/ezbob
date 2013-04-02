using System.Collections.Specialized;
using System.Security.Principal;
using System.Web;

namespace EzBob.Web.Infrastructure.csrf
{
    internal class JsonAntiForgeryHttpContextWrapper : HttpContextWrapper
    {
        readonly HttpRequestBase _request;
        public JsonAntiForgeryHttpContextWrapper(HttpContext httpContext)
            : base(httpContext)
        {
            _request = new JsonAntiForgeryHttpRequestWrapper(httpContext.Request);
        }

        public override HttpRequestBase Request
        {
            get
            {
                return _request;
            }
        }

        /// <summary>
        /// return always null no allow ajax user sign up
        /// </summary>
        public override IPrincipal User
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        private class JsonAntiForgeryHttpRequestWrapper : HttpRequestWrapper
        {
            readonly NameValueCollection _form;

            public JsonAntiForgeryHttpRequestWrapper(HttpRequest request)
                : base(request)
            {
                _form = new NameValueCollection(request.Form);
                if (request.Headers["X-Request-Verification-Token"] != null)
                {
                    _form["__RequestVerificationToken"]
                        = request.Headers["X-Request-Verification-Token"];
                }
            }

            public override NameValueCollection Form
            {
                get
                {
                    return _form;
                }
            }
        }
    }
}
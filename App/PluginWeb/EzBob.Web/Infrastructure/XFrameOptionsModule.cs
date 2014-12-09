using System;
using System.Web;

namespace EzBob.Web.Infrastructure
{
    public class XFrameOptionsModule : IHttpModule
    {

        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            context.PreSendRequestHeaders += new EventHandler(context_PreSendRequestHeaders);
        }

        private void context_PreSendRequestHeaders(object sender, EventArgs e)
        {
            var application = sender as HttpApplication;

            if (application == null)
                return;

            if (application.Response.Headers["X-FRAME-OPTIONS"] != null)

                return;

            application.Response.Headers.Add("X-FRAME-OPTIONS", "DENY");
        }
    }
}

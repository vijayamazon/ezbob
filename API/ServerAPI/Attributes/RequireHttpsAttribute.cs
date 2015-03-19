namespace Ezbob.API.ServerAPI.Attributes {
	using System;
	using System.Diagnostics;
	using System.Net;
	using System.Net.Http;
	using System.Web.Http.Controllers;
	using System.Web.Http.Filters;

	public class RequireHttpsAttribute : AuthorizationFilterAttribute {
		public override void OnAuthorization(HttpActionContext actionContext) {
			//Trace.WriteLine("--OnAuthorization -");
			if (actionContext.Request.RequestUri.Scheme != Uri.UriSchemeHttps) {
				//Trace.WriteLine("---" + actionContext.Request.RequestUri.Scheme + "===" + Uri.UriSchemeHttps);
				actionContext.Response = new HttpResponseMessage(HttpStatusCode.Forbidden) { ReasonPhrase = "HTTPS Required" };
			} else {
				//	Trace.WriteLine("-- OnAuthorization ELSE -");
				base.OnAuthorization(actionContext);
			}
		}
	}
}
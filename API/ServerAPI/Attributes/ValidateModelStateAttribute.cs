namespace Ezbob.API.ServerAPI.Attributes {
	using System;
	using System.Diagnostics;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Web.Http.Controllers;
	using System.Web.Http.Filters;

	public class ValidateModelStateAttribute : ActionFilterAttribute {
		public override void OnActionExecuting(HttpActionContext actionContext) {

			/*foreach (var v in actionContext.ActionArguments) {
				Trace.WriteLine(v.Key + "=" + v.Value);
			}*/
			// check null
			if (actionContext.ActionArguments.Any(v => v.Value == null)) {
				actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.BadRequest);
			}
			
			if (!actionContext.ModelState.IsValid) {
				actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest, actionContext.ModelState);
			}
		}
	}
}
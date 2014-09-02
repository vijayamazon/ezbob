namespace Demo.Filters {
	using System;
	using System.Globalization;
	using System.Linq;
	using System.Net;
	using System.Web.Http.Filters;

	/// <summary>
	/// 
	/// </summary>
	public class CopyStatusToHeaderAttribute : ActionFilterAttribute {
		public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext) {
			base.OnActionExecuted(actionExecutedContext);

			actionExecutedContext.Response.Headers.Add(
				OutputStatusHeaderName,
				((int)actionExecutedContext.Response.StatusCode).ToString(CultureInfo.InvariantCulture)
			);

			if (!actionExecutedContext.Request.Headers.Contains(InputHeaderName))
				return;

			string sHeader = actionExecutedContext.Request.Headers.GetValues(InputHeaderName).First();

			if (!sHeader.Equals(Yes, StringComparison.InvariantCultureIgnoreCase) && !sHeader.Equals(True, StringComparison.InvariantCultureIgnoreCase))
				return;

			actionExecutedContext.Response.StatusCode = HttpStatusCode.OK;
		} // OnActionExecuted

		private const string InputHeaderName = "output-status-header-only";
		private const string Yes = "yes";
		private const string True = "true";

		private const string OutputStatusHeaderName = "output-status-code";
	} // class CopyStatusToHeaderAttribute
} // namespace

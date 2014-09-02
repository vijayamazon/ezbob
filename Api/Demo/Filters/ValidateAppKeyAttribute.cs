namespace Demo.Filters {
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Web.Http;
	using System.Web.Http.Filters;

	/// <summary>
	/// Validates existence and validity of the App key header.
	/// </summary>
	public class ValidateAppKeyAttribute : ActionFilterAttribute {
		public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext) {
			base.OnActionExecuting(actionContext);

			HttpRequestMessage oRequest = actionContext.Request;

			if (!oRequest.Headers.Contains(AppKeyHeader)) {
				throw new HttpResponseException(new HttpResponseMessage {
					StatusCode = HttpStatusCode.Forbidden,
					ReasonPhrase = "No API key specified.",
				});
			} // if

			string sAppKey = oRequest.Headers.GetValues(AppKeyHeader).First();

			if (sAppKey != ValidAppKey) {
				throw new HttpResponseException(new HttpResponseMessage {
					StatusCode = HttpStatusCode.Unauthorized,
					ReasonPhrase = string.Format("Invalid API key specified ({0}).", sAppKey),
				});
			} // if
		} // OnActionExecuting

		private const string AppKeyHeader = "appkey";
		private const string ValidAppKey = "1234";
	} // ValidateAppKeyAttribute
} // namespace

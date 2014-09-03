namespace Demo.Filters {
	using System;
	using System.Globalization;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Web.Http.Filters;

	/// <summary>
	/// 
	/// </summary>
	public class HandleActionExecutedAttribute : ActionFilterAttribute {
		public static HttpResponseMessage CreateResponse(HttpRequestMessage oRequest, HttpStatusCode nCode, string sFormat, params object[] args) {
			string sMsg = string.IsNullOrWhiteSpace(sFormat) ? nCode.ToString() : string.Format(sFormat, args);

			var oResponse = new HttpResponseMessage(nCode) { ReasonPhrase = sMsg, };

			HandleActionExecutedAttribute.FillResponse(oResponse, oRequest);

			return oResponse;
		} // CreateResponse

		public static void FillResponse(HttpResponseMessage oResponse, HttpRequestMessage oRequest = null) {
			if (oResponse == null)
				return;

			oResponse.Headers.Add(
				OutputStatusHeaderName,
				((int)oResponse.StatusCode).ToString(CultureInfo.InvariantCulture)
			);

			if (oRequest == null)
				return;

			if (!oRequest.Headers.Contains(InputHeaderName))
				return;

			string sHeader = oRequest.Headers.GetValues(InputHeaderName).First();

			if (!sHeader.Equals(Yes, StringComparison.InvariantCultureIgnoreCase) && !sHeader.Equals(True, StringComparison.InvariantCultureIgnoreCase))
				return;

			oResponse.StatusCode = HttpStatusCode.OK;
		} // FillResponse

		public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext) {
			base.OnActionExecuted(actionExecutedContext);

			FillResponse(actionExecutedContext.Response, actionExecutedContext.Request);
		} // OnActionExecuted

		private const string InputHeaderName = "output-status-header-only";
		private const string Yes = "yes";
		private const string True = "true";

		private const string OutputStatusHeaderName = "output-status-code";
	} // class HandleActionExecutedAttribute
} // namespace

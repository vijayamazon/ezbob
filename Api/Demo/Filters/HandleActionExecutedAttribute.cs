namespace Demo.Filters {
	using System;
	using System.Globalization;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Web.Http.Filters;
	using Ezbob.Logger;
	using Infrastructure;

	/// <summary>
	/// 
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	internal class HandleActionExecutedAttribute : DemoActionFilterAttribute {
		public static HttpResponseMessage CreateResponse(int nApiVersion, HttpRequestMessage oRequest, HttpStatusCode nCode, string sFormat, params object[] args) {
			string sMsg = string.IsNullOrWhiteSpace(sFormat) ? nCode.ToString() : string.Format(sFormat, args);

			var oResponse = new HttpResponseMessage(nCode) { ReasonPhrase = sMsg, };

			HandleActionExecutedAttribute.FillResponse(nApiVersion, oResponse, oRequest);

			ms_oLog.Debug("An HTTP response message has been created with code {0}.", oResponse.StatusCode);

			return oResponse;
		} // CreateResponse

		public static void FillResponse(int nApiVersion, HttpResponseMessage oResponse, HttpRequestMessage oRequest = null) {
			if (oResponse == null)
				return;

			if (!oResponse.Headers.Contains(Const.Headers.ApiVersion)) {
				oResponse.Headers.Add(
					Const.Headers.ApiVersion,
					nApiVersion.ToString(CultureInfo.InvariantCulture)
				);
			} // if

			if (!oResponse.Headers.Contains(Const.Headers.OutputStatus)) {
				oResponse.Headers.Add(
					Const.Headers.OutputStatus,
					((int)oResponse.StatusCode).ToString(CultureInfo.InvariantCulture)
				);
			} // if

			if (oRequest == null)
				return;

			if (!oRequest.Headers.Contains(Const.Headers.OutputStatusCfg))
				return;

			string sHeader = oRequest.Headers.GetValues(Const.Headers.OutputStatusCfg).First();

			if (!sHeader.Equals(Const.Yes, StringComparison.InvariantCultureIgnoreCase) && !sHeader.Equals(Const.True, StringComparison.InvariantCultureIgnoreCase))
				return;

			oResponse.StatusCode = HttpStatusCode.OK;
		} // FillResponse

		public HandleActionExecutedAttribute(int nApiVersion) {
			ApiVersion = nApiVersion;
		} // constructor

		public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext) {
			FillResponse(ApiVersion, actionExecutedContext.Response, actionExecutedContext.Request);

			ms_oLog.Debug("Action has been executed with status code {0}.", actionExecutedContext.Response.StatusCode);
		} // OnActionExecuted

		public int ApiVersion { get; private set; }

		private static readonly ASafeLog ms_oLog = new SafeILog(typeof (HandleActionExecutedAttribute));
	} // class HandleActionExecutedAttribute
} // namespace

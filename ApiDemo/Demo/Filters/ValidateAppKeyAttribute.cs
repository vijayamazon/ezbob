namespace Demo.Filters {
	using System;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using Ezbob.Logger;
	using Infrastructure;

	/// <summary>
	/// Validates existence and validity of the App key header.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	internal class ValidateAppKeyAttribute : DemoActionFilterAttribute {
		public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext) {
			string sPattern = string.Format(
				"Validating app key header for '{0} {1}' from '{2}'",
				actionContext.Request.Method,
				actionContext.Request.RequestUri,
				actionContext.Request.GetRemoteIp()
			);

			ms_oLog.Debug("{0} started.", sPattern);

			HttpRequestMessage oRequest = actionContext.Request;

			if (!oRequest.Headers.Contains(Const.Headers.AppKey)) {
				actionContext.Response = HandleActionExecutedAttribute.CreateResponse(
					GetApiVersion(actionContext),
					actionContext.Request,
					HttpStatusCode.Unauthorized,
					"No app key specified."
				);

				ms_oLog.Debug("{0} failed: no app key header found.", sPattern);

				return;
			} // if

			string sAppKey = oRequest.Headers.GetValues(Const.Headers.AppKey).First();

			var oSec = new SecurityStub();

			if (!oSec.IsAppKeyValid(sAppKey)) {
				actionContext.Response = HandleActionExecutedAttribute.CreateResponse(
					GetApiVersion(actionContext),
					actionContext.Request,
					HttpStatusCode.Forbidden,
					"Invalid app key specified ({0}).",
					sAppKey
				);

				ms_oLog.Debug("{0} failed: invalid app key header found: '{1}'.", sPattern, sAppKey);
				return;
			} // if

			ms_oLog.Debug("{0} succeeded.", sPattern);
		} // OnActionExecuting

		private static readonly ASafeLog ms_oLog = new SafeILog(typeof(ValidateAppKeyAttribute));
	} // ValidateAppKeyAttribute
} // namespace

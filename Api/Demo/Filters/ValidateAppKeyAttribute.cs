namespace Demo.Filters {
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Web.Http.Filters;
	using Infrastructure;

	/// <summary>
	/// Validates existence and validity of the App key header.
	/// </summary>
	internal class ValidateAppKeyAttribute : ActionFilterAttribute {
		public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext) {
			base.OnActionExecuting(actionContext);

			HttpRequestMessage oRequest = actionContext.Request;

			if (!oRequest.Headers.Contains(AppKeyHeader)) {
				actionContext.Response = HandleActionExecutedAttribute.CreateResponse(
					actionContext.Request,
					HttpStatusCode.Unauthorized,
					"No app key specified."
				);

				return;
			} // if

			string sAppKey = oRequest.Headers.GetValues(AppKeyHeader).First();

			var oSec = new SecurityStub();

			if (!oSec.IsAppKeyValid(sAppKey)) {
				actionContext.Response = HandleActionExecutedAttribute.CreateResponse(
					actionContext.Request,
					HttpStatusCode.Forbidden,
					"Invalid app key specified ({0}).",
					sAppKey
				);
			} // if
		} // OnActionExecuting

		private const string AppKeyHeader = "app-key";
	} // ValidateAppKeyAttribute
} // namespace

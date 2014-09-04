namespace Demo.Filters {
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Web.Http.Filters;
	using Infrastructure;

	/// <summary>
	/// Validates existence and validity of the App key header.
	/// </summary>
	internal class ValidateSessionTokenAttribute : ActionFilterAttribute {
		public ValidateSessionTokenAttribute() {
			m_oSecurity = new SecurityStub();
		} // constructor

		public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext) {
			base.OnActionExecuting(actionContext);

			HttpRequestMessage oRequest = actionContext.Request;

			if (!oRequest.Headers.Contains(Const.Headers.SessionToken)) {
				actionContext.Response = HandleActionExecutedAttribute.CreateResponse(
					actionContext.Request,
					HttpStatusCode.Forbidden,
					"No session token specified."
				);
				return;
			} // if

			string sToken = oRequest.Headers.GetValues(Const.Headers.SessionToken).First();

			ActiveUserInfo oUserInfo = m_oSecurity.ValidateSessionToken(sToken, actionContext.Request.GetRemoteIp());

			actionContext.Request.SetUserName(null);

			switch (oUserInfo.TokenValidity) {
			case TokenValidity.Valid:
				actionContext.Request.SetUserName(oUserInfo.UserName);
				return;

			case TokenValidity.Expired:
				actionContext.Response = HandleActionExecutedAttribute.CreateResponse(
					actionContext.Request,
					HttpStatusCode.RequestTimeout,
					"Your session has expired, please login again."
				);
				return;

			case TokenValidity.Invalid:
				actionContext.Response = HandleActionExecutedAttribute.CreateResponse(
					actionContext.Request,
					HttpStatusCode.Unauthorized,
					"Invalid session token specified ({0}).",
					sToken
				);
				return;
			} // switch
		} // OnActionExecuting

		protected SecurityStub m_oSecurity;

	} // ValidateSessionTokenAttribute
} // namespace

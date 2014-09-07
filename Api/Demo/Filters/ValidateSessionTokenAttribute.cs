namespace Demo.Filters {
	using System;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using Infrastructure;

	/// <summary>
	/// Validates existence and validity of the App key header.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	internal class ValidateSessionTokenAttribute : DemoActionFilterAttribute {
		public ValidateSessionTokenAttribute() {
			m_oSecurity = new SecurityStub();
		} // constructor

		public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext) {
			HttpRequestMessage oRequest = actionContext.Request;

			if (!oRequest.Headers.Contains(Const.Headers.SessionToken)) {
				actionContext.Response = HandleActionExecutedAttribute.CreateResponse(
					GetApiVersion(actionContext),
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
					GetApiVersion(actionContext),
					actionContext.Request,
					HttpStatusCode.Forbidden,
					"Your session has expired, please login again."
				);
				return;

			case TokenValidity.Invalid:
				actionContext.Response = HandleActionExecutedAttribute.CreateResponse(
					GetApiVersion(actionContext),
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

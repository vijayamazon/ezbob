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
			m_sToken = null;
		} // constructor

		public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext) {
			base.OnActionExecuting(actionContext);

			HttpRequestMessage oRequest = actionContext.Request;

			if (!oRequest.Headers.Contains(SessionTokenHeader)) {
				actionContext.Response = HandleActionExecutedAttribute.CreateResponse(
					actionContext.Request,
					HttpStatusCode.Forbidden,
					"No session token specified."
				);
				return;
			} // if

			m_sToken = oRequest.Headers.GetValues(SessionTokenHeader).First();

			TokenValidity nValidity = m_oSecurity.ValidateSessionToke(m_sToken);

			switch (nValidity) {
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
					m_sToken
				);
				return;
			} // switch
		} // OnActionExecuting

		protected string m_sToken;
		protected SecurityStub m_oSecurity;

		private const string SessionTokenHeader = "session-token";
	} // ValidateSessionTokenAttribute
} // namespace

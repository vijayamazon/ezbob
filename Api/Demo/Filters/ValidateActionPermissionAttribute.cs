namespace Demo.Filters {
	using System.Net;
	using Infrastructure;

	internal class ValidateActionPermissionAttribute : ValidateSessionTokenAttribute {
		public ValidateActionPermissionAttribute(Action nAction) {
			m_nAction = nAction;
		} // constructor

		public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext) {
			base.OnActionExecuting(actionContext);

			if (!m_oSecurity.IsActionEnabled(actionContext.Request.GetUserName(), m_nAction)) {
				actionContext.Response = HandleActionExecutedAttribute.CreateResponse(
					actionContext.Request,
					HttpStatusCode.Forbidden,
					"You are not authorized to perform this action."
				);
			} // if
		} // OnActionExecuting

		private readonly Action m_nAction;
	} // class ValidateActionPermissionAttribute
} // namespace
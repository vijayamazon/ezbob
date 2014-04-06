namespace EzBob.Web.Infrastructure.Attributes {
	using System;
	using System.Reflection;
	using System.Web.Mvc;

	public class AjaxAttribute : ActionMethodSelectorAttribute {
		public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo) {
			if (controllerContext == null)
				throw new ArgumentNullException("controllerContext");

			return controllerContext.HttpContext.Request.IsAjaxRequest();
		} // IsValidForRequest
	} // class AjaxAttribute
} // namespace

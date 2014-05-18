namespace EzBob.Web.Infrastructure.csrf {
	using System;
	using System.Web;
	using System.Web.Helpers;
	using System.Web.Mvc;

	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class ValidateJsonAntiForgeryTokenAttribute : FilterAttribute, IAuthorizationFilter {
		public void OnAuthorization(AuthorizationContext filterContext) {
			if (filterContext == null)
				throw new ArgumentNullException("filterContext");

			string sToken = HttpContext.Current.Request.Headers["X-Request-Verification-Token"];

			if (string.IsNullOrWhiteSpace(sToken))
				AntiForgery.Validate();
			else {
				HttpCookie antiForgeryCookie = HttpContext.Current.Request.Cookies[AntiForgeryConfig.CookieName];
				AntiForgery.Validate(antiForgeryCookie == null ? null : antiForgeryCookie.Value, sToken);
			} // if
		} // OnAuthorization
	} // class ValidateJsonAntiForgeryTokenAttribute
} // namespace

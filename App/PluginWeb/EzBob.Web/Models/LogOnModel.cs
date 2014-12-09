namespace EzBob.Web.Models {
	using System.ComponentModel.DataAnnotations;
	using System.Security.Principal;
	using System.Web;
	using System.Web.Security;

	public class LogOnModel {
		public enum Roles {
			Customer,
			Underwriter,
		} // enum Roles

		public static void SetCookie(Roles nRole, string sUserName, bool bRememberMe) {
			FormsAuthentication.SetAuthCookie(sUserName, bRememberMe);
			HttpContext.Current.User = new GenericPrincipal(new GenericIdentity(sUserName), new[] { nRole.ToString() });
		} // SetCookie

		[Required]
		[Display(Name = @"User name")]
		public string UserName { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[Display(Name = @"Password")]
		public string Password { get; set; }

		[Display(Name = @"Remember me?")]
		public bool RememberMe { get; set; }

		public string ReturnUrl { get; set; }

		public void SetCookie(Roles nRole) {
			SetCookie(nRole, UserName, RememberMe);
		} // SetCookie

	} // class LogOnModel
} // namespace

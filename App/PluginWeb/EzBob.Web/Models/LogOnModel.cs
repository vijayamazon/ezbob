namespace EzBob.Web.Models {
	using System.ComponentModel.DataAnnotations;
	using System.Web.Security;

	public class LogOnModel {
		#region property UserName

		[Required]
		[Display(Name = @"User name")]
		public string UserName { get; set; }

		#endregion property UserName

		#region property Password

		[Required]
		[DataType(DataType.Password)]
		[Display(Name = @"Password")]
		public string Password { get; set; }

		#endregion property Password

		#region property RememberMe

		[Display(Name = @"Remember me?")]
		public bool RememberMe { get; set; }

		#endregion property RememberMe

		#region property ReturnUrl

		public string ReturnUrl { get; set; }

		#endregion property ReturnUrl

		#region method SetCookie

		public void SetCookie() {
			FormsAuthentication.SetAuthCookie(UserName, RememberMe);
		} // SetCookie

		#endregion method SetCookie
	} // class LogOnModel
} // namespace

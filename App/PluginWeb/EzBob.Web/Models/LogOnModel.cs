﻿namespace EzBob.Web.Models {
	using System;
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
		[MaxLength(128)]
		[Display(Name = @"User name")]
		public string UserName { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[Display(Name = @"Password")]
		public string Password { get; set; }

		[Display(Name = @"Remember me?")]
		public bool RememberMe { get; set; }

		public string ReturnUrl { get; set; }

		public string PromotionName { get; set; }
		public DateTime? PromotionPageVisitTime { get; set; }

		public void SetCookie(Roles nRole) {
			SetCookie(nRole, UserName, RememberMe);
		} // SetCookie

		public string PromotionDisplayData {
			get {
				return (!string.IsNullOrWhiteSpace(PromotionName) && PromotionPageVisitTime.HasValue)
					? PromotionName + " at " + PromotionPageVisitTime.Value.ToString("MMM d yyyy H:mm:ss")
					: "none";
			} // get
		} // PromotionDisplayData
	} // class LogOnModel
} // namespace

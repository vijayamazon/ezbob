using Ezbob.Database;
using Ezbob.Logger;

namespace EzReportsWeb {
	using ReportAuthenticationLib;
	using System;
	using System.Web.Security;
	using System.Web.UI;
	using System.Web.UI.WebControls;

	public partial class Login : Page {
		protected void Page_Load(object sender, EventArgs e) {
			if (!IsPostBack)
				divChangePassword.Visible = false;
		} // Page_Load

		protected void LoginControl_Authenticate(object sender, AuthenticateEventArgs e) {
			var ra = new ReportAuthentication(new SqlConnection());
			var log = new LegacyLog();

			bool authenticated = ra.IsValidPassword(LoginControl.UserName, LoginControl.Password);

			log.Debug("authenticated: {0}", authenticated);

			if (authenticated)
				FormsAuthentication.RedirectFromLoginPage(LoginControl.UserName, LoginControl.RememberMeSet);
		} // LoginControl_Authenticate

		protected void btnShowChangePassword_Click(object sender, EventArgs e) {
			divChangePassword.Visible = true;
			divLogin.Visible = false;
		} // btnShowChangePassword_Click

		protected void btnChangePassword_Click(object sender, EventArgs e) {
			var ra = new ReportAuthentication(new SqlConnection());

			divChangePassword.Visible = true;
			divLogin.Visible = false;

			if (ra.UpdatePassword(txtUserName.Text, txtOldPassword.Text, txtNewPassword1.Text)) {
				lblMessage.Text = "Password changed";
				divChangePassword.Visible = false;
				divLogin.Visible = true;
			}
			else
				lblMessage.Text = "Error occured, password was not changed";

			lblMessage.Visible = true;
		} // btnChangePassword_Click

		protected void btnBack_Click(object sender, EventArgs e) {
			divLogin.Visible = true;
			divChangePassword.Visible = false;
		} // btnBack_Click
	} // class Login
} // namespace EzReportsWeb

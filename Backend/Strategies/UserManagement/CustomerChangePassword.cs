namespace Ezbob.Backend.Strategies.UserManagement {
	using Ezbob.Backend.Models;
	using MailStrategies;

	public class CustomerChangePassword : UserChangePassword {
		public CustomerChangePassword(string sEmail, Password oOldPassword, Password oNewPassword)
			: base(sEmail, oOldPassword, oNewPassword, false) {
		} // constructor

		public override string Name {
			get { return "Customer change password"; }
		} // Name

		public override void Execute() {
			base.Execute();

			if (string.IsNullOrWhiteSpace(ErrorMessage))
				new PasswordChanged(UserID, Password).Execute();
		} // Execute
	} // class CustomerChangePassword
} // namespace Ezbob.Backend.Strategies.UserManagement

namespace EzBob.Backend.Strategies.UserManagement {
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using MailStrategies;

	public class CustomerChangePassword : UserChangePassword {

		public CustomerChangePassword(string sEmail, Password oOldPassword, Password oNewPassword, AConnection oDB, ASafeLog oLog)
			: base(sEmail, oOldPassword, oNewPassword, false, oDB, oLog)
		{
		} // constructor

		public override string Name {
			get { return "Customer change password"; }
		} // Name

		public override void Execute() {
			base.Execute();

			if (string.IsNullOrWhiteSpace(ErrorMessage))
				new PasswordChanged(UserID, Password, DB, Log).Execute();
		} // Execute

	} // class CustomerChangePassword
} // namespace EzBob.Backend.Strategies.UserManagement

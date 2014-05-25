namespace EzBob.Backend.Strategies.UserManagement {
	using Ezbob.Database;
	using Ezbob.Logger;
	using MailStrategies;

	public class CustomerChangePassword : UserChangePassword {
		#region public

		#region constructor

		public CustomerChangePassword(string sEmail, string sOldPassword, string sNewPassword, AConnection oDB, ASafeLog oLog)
			: base(sEmail, sOldPassword, sNewPassword, false, oDB, oLog)
		{
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Customer change password"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			base.Execute();

			if (string.IsNullOrWhiteSpace(ErrorMessage))
				new PasswordChanged(UserID, Password, DB, Log).Execute();
		} // Execute

		#endregion method Execute

		#endregion public
	} // class CustomerChangePassword
} // namespace EzBob.Backend.Strategies.UserManagement

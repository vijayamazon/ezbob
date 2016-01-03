namespace Ezbob.Backend.Strategies.UserManagement {
	using Ezbob.Backend.Models;
	using EZBob.DatabaseLib.Model.Database;
	using MailStrategies;

	public class CustomerChangePassword : UserChangePassword {
		public CustomerChangePassword(
			string email,
			CustomerOriginEnum origin,
			DasKennwort oldPassword,
			DasKennwort newPassword
		) : base(email, origin, oldPassword, newPassword, false) {
		} // constructor

		public override string Name {
			get { return "Customer change password"; }
		} // Name

		public override void Execute() {
			base.Execute();

			if (string.IsNullOrWhiteSpace(ErrorMessage))
				new PasswordChanged(UserID, RawPassword).Execute();
		} // Execute
	} // class CustomerChangePassword
} // namespace Ezbob.Backend.Strategies.UserManagement

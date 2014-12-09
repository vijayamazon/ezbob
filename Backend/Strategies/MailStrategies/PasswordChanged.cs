namespace Ezbob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using UserManagement.EmailConfirmation;

	public class PasswordChanged : AMailStrategyBase {

		public PasswordChanged(int customerId, Password oPassword) : base(customerId, true) {
			m_sPassword = oPassword.Primary;
		} // constructor

		internal PasswordChanged(int customerId, string sPassword) : base(customerId, true) {
			m_sPassword = sPassword;
		} // constructor

		public override string Name { get { return "Password Changed"; } } // Name

		protected override void SetTemplateAndVariables() {
			var ecl = new EmailConfirmationLoad(CustomerData.UserID);
			ecl.Execute();

			if (!ecl.IsConfirmed) {
				SendToCustomer = false;

				Log.Warn(
					"Not sending new password to user {0} (user ID: {1}) because user's email {2} is not confirmed (error message: '{3}').",
					FirstName, CustomerData.UserID, CustomerData.Mail, ecl.ErrorMessage
				);
			} // if

			TemplateName = "Mandrill - New password";

			Variables = new Dictionary<string, string> {
				{ "Password", m_sPassword },
				{ "FirstName", FirstName }
			};
		} // SetTemplateAndVariables

		protected virtual string FirstName {
			get { return CustomerData.FirstName; } // get
		} // FirstName

		private readonly string m_sPassword;

	} // class PasswordChanged
} // namespace

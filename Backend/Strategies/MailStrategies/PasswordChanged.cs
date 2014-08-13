namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using UserManagement.EmailConfirmation;

	public class PasswordChanged : AMailStrategyBase {
		#region constructor

		public PasswordChanged(int customerId, Password oPassword, AConnection oDb, ASafeLog oLog) : base(customerId, true, oDb, oLog) {
			m_sPassword = oPassword.Primary;
		} // constructor

		internal PasswordChanged(int customerId, string sPassword, AConnection oDb, ASafeLog oLog) : base(customerId, true, oDb, oLog) {
			m_sPassword = sPassword;
		} // constructor

		#endregion constructor

		public override string Name { get { return "Password Changed"; } } // Name

		#region protected

		#region method SetTemplateAndVariables

		protected override void SetTemplateAndVariables() {
			var ecl = new EmailConfirmationLoad(CustomerData.UserID, DB, Log);
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

		#endregion method SetTemplateAndVariables

		#region property FirstName

		protected virtual string FirstName {
			get { return CustomerData.FirstName; } // get
		} // FirstName

		#endregion property FirstName

		#endregion protected

		#region private

		private readonly string m_sPassword;

		#endregion private
	} // class PasswordChanged
} // namespace

namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using ConfigManager;
	using Exceptions;
	using Ezbob.Database;
	using Ezbob.Logger;
	using UserManagement;

	public class PasswordRestored : AMailStrategyBase {
		#region constructor

		public PasswordRestored(int customerId, AConnection oDb, ASafeLog oLog) : base(customerId, true, oDb, oLog) {
		} // constructor

		#endregion constructor

		public override string Name { get { return "Password Restored"; } } // Name

		#region method SetTemplateAndVariables

		protected override void SetTemplateAndVariables() {
			string sNewPassword = ForcedPassword;

			if (string.IsNullOrWhiteSpace(sNewPassword)) {
				var oNewPassGenerator = new UserResetPassword(CustomerData.Mail, DB, Log);
				oNewPassGenerator.Execute();

				if (!oNewPassGenerator.Success)
					throw new StrategyAlert(this, "Failed to generate a new password for customer " + CustomerData.Mail);

				sNewPassword = oNewPassGenerator.Password.RawValue;
			} // if

			TemplateName = "Mandrill - EZBOB password was restored";

			Variables = new Dictionary<string, string> {
				{"ProfilePage", ProfilePage},
				{"Password", sNewPassword},
				{"FirstName", string.IsNullOrWhiteSpace(CustomerData.FirstName) ? Salutation : CustomerData.FirstName }
			};
		} // SetTemplateAndVariables

		#endregion method SetTemplateAndVariables

		#region property ForcedPassword

		protected virtual string ForcedPassword { get; set; } // ForcedPassword

		#endregion property ForcedPassword

		#region property ProfilePage

		protected virtual string ProfilePage {
			get { return CurrentValues.Instance.CustomerSite + "/Account/LogOn"; }
		} // ProfilePage

		#endregion property ProfilePage

		#region property Salutation

		protected virtual string Salutation {
			get { return "Dear customer"; }
		} // Salutation

		#endregion property Salutation
	} // class PasswordRestored
} // namespace

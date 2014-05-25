namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using Exceptions;
	using Ezbob.Database;
	using Ezbob.Logger;
	using UserManagement;

	public class ThreeInvalidAttempts : AMailStrategyBase {
		#region constructor

		public ThreeInvalidAttempts(int customerId, AConnection oDb, ASafeLog oLog)
			: base(customerId, true, oDb, oLog) {
		} // constructor

		#endregion constructor

		public override string Name { get {return "Three Invalid Attempts"; } } // Name

		#region method SetTemplateAndVariables

		protected override void SetTemplateAndVariables() {
			var oNewPassGenerator = new UserResetPassword(CustomerData.Mail, DB, Log);
			oNewPassGenerator.Execute();

			if (!oNewPassGenerator.Success)
				throw new StrategyAlert(this, "Failed to generate a new password for customer " + CustomerData.Mail);

			TemplateName = "Mandrill - Temporary password";

			Variables = new Dictionary<string, string> {
				{"Password", oNewPassGenerator.Password.RawValue},
				{"FirstName", string.IsNullOrEmpty(CustomerData.FirstName) ? "customer" : CustomerData.FirstName},
				{"ProfilePage", "https://app.ezbob.com/Customer/Profile"},
				{"NIMRODTELEPHONENUMBER", "+44 800 011 4787"} // TODO: change name of variable here and in mandrill\mailchimp
			};
		} // SetTemplateAndVariables

		#endregion method SetTemplateAndVariables
	} // class ThreeInvalidAttempts
} // namespace

namespace EzBob.Backend.Strategies.MailStrategies {
	using System;
	using System.Collections.Generic;
	using Exceptions;
	using Ezbob.Database;
	using Ezbob.Logger;
	using StoredProcs;
	using UserManagement;

	public class ThreeInvalidAttempts : AMailStrategyBase {
		#region constructor

		public ThreeInvalidAttempts(int customerId, AConnection oDb, ASafeLog oLog) : base(customerId, true, oDb, oLog) {
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

			Guid oToken = InitCreatePasswordToken.Execute(DB, CustomerData.Mail);

			Variables = new Dictionary<string, string> {
				{"FirstName", string.IsNullOrEmpty(CustomerData.FirstName) ? "customer" : CustomerData.FirstName},
				{"Link", CustomerSite + "/Account/CreatePassword?token=" + oToken.ToString("N")},
				{"NIMRODTELEPHONENUMBER", "+44 800 011 4787"} // TODO: change name of variable here and in mandrill\mailchimp
			};
		} // SetTemplateAndVariables

		#endregion method SetTemplateAndVariables
	} // class ThreeInvalidAttempts
} // namespace

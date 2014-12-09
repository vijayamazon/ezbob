namespace Ezbob.Backend.Strategies.MailStrategies {
	using System;
	using System.Collections.Generic;
	using Exceptions;
	using StoredProcs;
	using UserManagement;

	public class ThreeInvalidAttempts : AMailStrategyBase {

		public ThreeInvalidAttempts(int customerId) : base(customerId, true) {
		} // constructor

		public override string Name { get {return "Three Invalid Attempts"; } } // Name

		protected override void SetTemplateAndVariables() {
			var oNewPassGenerator = new UserResetPassword(CustomerData.Mail);
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

	} // class ThreeInvalidAttempts
} // namespace

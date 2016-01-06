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
			var oNewPassGenerator = new UserResetPassword(CustomerId);
			oNewPassGenerator.Execute();

			if (!oNewPassGenerator.Success)
				throw new StrategyAlert(this, "Failed to generate a new password for customer " + CustomerData.Mail);

			TemplateName = "Mandrill - Temporary password";

			var sp = new InitCreatePasswordTokenByUserID(CustomerId, DB, Log);
			sp.Execute();

			if (sp.Token == Guid.Empty) {
				throw new StrategyAlert(
					this,
					"Failed to generate a change password token for customer " + CustomerId
				);
			} // if

			Variables = new Dictionary<string, string> {
				{"FirstName", string.IsNullOrEmpty(CustomerData.FirstName) ? "customer" : CustomerData.FirstName},
				{"Link", CustomerData.OriginSite + "/Account/CreatePassword?token=" + sp.Token.ToString("N")}
			};
		} // SetTemplateAndVariables
	} // class ThreeInvalidAttempts
} // namespace

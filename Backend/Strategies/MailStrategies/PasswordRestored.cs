namespace Ezbob.Backend.Strategies.MailStrategies {
	using System;
	using System.Collections.Generic;
	using Exceptions;
	using StoredProcs;
	using UserManagement;

	public class PasswordRestored : AMailStrategyBase {
		public PasswordRestored(int customerId) : base(customerId, true) {
		} // constructor

		public override string Name { get { return "Password Restored"; } } // Name

		protected override void SetTemplateAndVariables() {
			var oNewPassGenerator = new UserResetPassword(CustomerId);
			oNewPassGenerator.Execute();

			if (!oNewPassGenerator.Success)
				throw new StrategyAlert(this, "Failed to generate a new password for customer " + CustomerData.Mail);

			var sp = new InitCreatePasswordTokenByUserID(CustomerId, DB, Log);
			sp.Execute();

			if (sp.Token == Guid.Empty) {
				throw new StrategyAlert(
					this,
					"Failed to generate a change password token for customer " + CustomerId
				);
			} // if

			Variables = new Dictionary<string, string> {
				{"Link", CustomerData.OriginSite + "/Account/CreatePassword?token=" + sp.Token.ToString("N")},
				{"FirstName", string.IsNullOrWhiteSpace(CustomerData.FirstName) ? Salutation : CustomerData.FirstName}
			};

			TemplateName = "Mandrill - EZBOB password was restored";
		} // SetTemplateAndVariables

		protected virtual string Salutation {
			get { return "Dear customer"; }
		} // Salutation
	} // class PasswordRestored
} // namespace

namespace EzBob.Backend.Strategies.MailStrategies {
	using System;
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
			var oNewPassGenerator = new UserResetPassword(CustomerData.Mail, DB, Log);
			oNewPassGenerator.Execute();

			if (!oNewPassGenerator.Success)
				throw new StrategyAlert(this, "Failed to generate a new password for customer " + CustomerData.Mail);

			Guid oToken = CreateToken();

			if (oToken == Guid.Empty)
				throw new StrategyAlert(this, "Failed to generate a change password token for customer " + CustomerData.Mail);

			Variables = new Dictionary<string, string> {
				{"Link", CurrentValues.Instance.CustomerSite + "/Account/CreatePassword?token=" + oToken.ToString("N")},
				{"FirstName", string.IsNullOrWhiteSpace(CustomerData.FirstName) ? Salutation : CustomerData.FirstName}
			};

			if (true) { // TODO: check email confirmed
				TemplateName = "Mandrill - EZBOB password was restored";
			}
			else {
				SendToCustomer = false;
			}
		} // SetTemplateAndVariables

		#endregion method SetTemplateAndVariables

		#region property Salutation

		protected virtual string Salutation {
			get { return "Dear customer"; }
		} // Salutation

		#endregion property Salutation

		#region private

		private Guid CreateToken() {
			Guid oToken = Guid.NewGuid();

			bool bSuccess = DB.ExecuteScalar<bool>(
				"InitCreatePasswordToken",
				CommandSpecies.StoredProcedure,
				new QueryParameter("TokenID", oToken),
				new QueryParameter("Email", CustomerData.Mail),
				new QueryParameter("Now", DateTime.UtcNow)
			);

			return bSuccess ? oToken : Guid.Empty;
		} // CreateToken

		#endregion private
	} // class PasswordRestored
} // namespace

namespace EzBob.Backend.Strategies.MailStrategies {
	using System;
	using System.Collections.Generic;
	using ConfigManager;
	using Exceptions;
	using Ezbob.Database;
	using Ezbob.Logger;
	using UserManagement;

	public class BrokerForceResetCustomerPassword : AMailStrategyBase {
		#region public

		#region constructor

		public BrokerForceResetCustomerPassword(
			int nCustomerID,
			AConnection oDB,
			ASafeLog oLog
		) : base(nCustomerID, true, oDB, oLog) {
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name { get { return "Broker force reset customer password"; } } // Name

		#endregion property Name

		#endregion public

		#region protected

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
				{"Link", CurrentValues.Instance.CustomerSite + "/Account/CreatePassword?token=" + oToken.ToString("N") },
				{"FirstName", CustomerData.FirstName }
			};

			TemplateName = "Broker force reset customer password";
		} // SetTemplateAndVariables

		#endregion method SetTemplateAndVariables

		#endregion protected

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
	} // class BrokerForceResetCustomerPassword
} // namespace EzBob.Backend.Strategies.Broker

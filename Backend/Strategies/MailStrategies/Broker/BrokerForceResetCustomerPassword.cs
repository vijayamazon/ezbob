namespace EzBob.Backend.Strategies.MailStrategies {
	using System;
	using System.Collections.Generic;
	using ConfigManager;
	using Exceptions;
	using Ezbob.Database;
	using Ezbob.Logger;
	using StoredProcs;
	using UserManagement;

	public class BrokerForceResetCustomerPassword : AMailStrategyBase {
		#region method GetFromDB

		public static string GetFromDB(AMailStrategyBase oStrategy) {
			var oNewPassGenerator = new UserResetPassword(oStrategy.CustomerData.Mail, oStrategy.DB, oStrategy.Log);
			oNewPassGenerator.Execute();

			if (!oNewPassGenerator.Success)
				throw new StrategyAlert(oStrategy, "Failed to generate a new password for customer " + oStrategy.CustomerData);

			Guid oToken = InitCreatePasswordToken.Execute(oStrategy.DB, oStrategy.CustomerData.Mail);

			if (oToken == Guid.Empty)
				throw new StrategyAlert(oStrategy, "Failed to generate a change password token for customer " + oStrategy.CustomerData);

			return CurrentValues.Instance.CustomerSite + "/Account/CreatePassword?token=" + oToken.ToString("N");
		} // GetFromDB

		#endregion method GetFromDB

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
			Variables = new Dictionary<string, string> {
				{ "Link", GetFromDB(this) },
				{ "FirstName", CustomerData.FirstName }
			};

			TemplateName = "Broker force reset customer password";
		} // SetTemplateAndVariables

		#endregion method SetTemplateAndVariables

		#endregion protected
	} // class BrokerForceResetCustomerPassword
} // namespace EzBob.Backend.Strategies.Broker

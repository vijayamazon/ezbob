namespace Ezbob.Backend.Strategies.MailStrategies {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Strategies.Exceptions;
	using Ezbob.Backend.Strategies.StoredProcs;
	using Ezbob.Backend.Strategies.UserManagement;

	public class BrokerForceResetCustomerPassword : AMailStrategyBase {
		public static string GetFromDB(AMailStrategyBase oStrategy) {
			var oNewPassGenerator = new UserResetPassword(oStrategy.CustomerData.Mail);
			oNewPassGenerator.Execute();

			if (!oNewPassGenerator.Success) {
				throw new StrategyAlert(
					oStrategy,
					"Failed to generate a new password for customer " + oStrategy.CustomerData
				);
			} // if

			Guid oToken = InitCreatePasswordToken.Execute(oStrategy.DB, oStrategy.CustomerData.Mail);

			if (oToken == Guid.Empty) {
				throw new StrategyAlert(
					oStrategy,
					"Failed to generate a change password token for customer " + oStrategy.CustomerData
				);
			} // if

			return oStrategy.CustomerData.OriginSite + "/Account/CreatePassword?token=" + oToken.ToString("N");
		} // GetFromDB

		public BrokerForceResetCustomerPassword(int nCustomerID) : base(nCustomerID, true) {
		} // constructor

		public override string Name { get { return "Broker force reset customer password"; } } // Name

		protected override void SetTemplateAndVariables() {
			Variables = new Dictionary<string, string> {
				{ "Link", GetFromDB(this) },
				{ "FirstName", CustomerData.FirstName },
			};

			TemplateName = "Broker force reset customer password";
		} // SetTemplateAndVariables
	} // class BrokerForceResetCustomerPassword
} // namespace Ezbob.Backend.Strategies.Broker

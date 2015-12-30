namespace Ezbob.Backend.Strategies.MailStrategies {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Strategies.Exceptions;
	using Ezbob.Backend.Strategies.StoredProcs;
	using Ezbob.Backend.Strategies.UserManagement;

	public class BrokerForceResetCustomerPassword : AMailStrategyBase {
		public BrokerForceResetCustomerPassword(int nCustomerID) : base(nCustomerID, true) {
		} // constructor

		public override string Name { get { return "Broker force reset customer password"; } } // Name

		protected override void SetTemplateAndVariables() {
			Variables = new Dictionary<string, string> {
				{ "Link", GetFromDB() },
				{ "FirstName", CustomerData.FirstName },
			};

			TemplateName = "Broker force reset customer password";
		} // SetTemplateAndVariables

		private string GetFromDB() {
			var oNewPassGenerator = new UserResetPassword(CustomerId);
			oNewPassGenerator.Execute();

			if (!oNewPassGenerator.Success) {
				throw new StrategyAlert(
					this,
					"Failed to generate a new password for customer " + CustomerId
				);
			} // if

			var sp = new InitCreatePasswordTokenByUserID(CustomerId, DB, Log);
			sp.Execute();

			if (sp.Token == Guid.Empty) {
				throw new StrategyAlert(
					this,
					"Failed to generate a change password token for customer " + CustomerId
				);
			} // if

			return CustomerData.OriginSite + "/Account/CreatePassword?token=" + sp.Token.ToString("N");
		} // GetFromDB
	} // class BrokerForceResetCustomerPassword
} // namespace Ezbob.Backend.Strategies.Broker

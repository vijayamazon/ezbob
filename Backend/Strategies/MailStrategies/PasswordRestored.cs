namespace Ezbob.Backend.Strategies.MailStrategies {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using Exceptions;
	using Ezbob.Database;
	using StoredProcs;
	using UserManagement;
	using UserManagement.EmailConfirmation;

	public class PasswordRestored : AMailStrategyBase {
		public PasswordRestored(int customerId) : base(customerId, true) {
		} // constructor

		public override string Name { get { return "Password Restored"; } } // Name

		protected override void SetTemplateAndVariables() {
			var oNewPassGenerator = new UserResetPassword(CustomerData.Mail);
			oNewPassGenerator.Execute();

			if (!oNewPassGenerator.Success)
				throw new StrategyAlert(this, "Failed to generate a new password for customer " + CustomerData.Mail);

			Guid oToken = InitCreatePasswordToken.Execute(DB, CustomerData.Mail);

			if (oToken == Guid.Empty) {
				throw new StrategyAlert(
					this,
					"Failed to generate a change password token for customer " + CustomerData.Mail
				);
			} // if

			Variables = new Dictionary<string, string> {
				{"Link", CustomerData.OriginSite + "/Account/CreatePassword?token=" + oToken.ToString("N")},
				{"FirstName", string.IsNullOrWhiteSpace(CustomerData.FirstName) ? Salutation : CustomerData.FirstName}
			};

			var ecl = new EmailConfirmationLoad(CustomerData.UserID);
			ecl.Execute();

			if (ecl.IsConfirmed)
				TemplateName = "Mandrill - EZBOB password was restored";
			else {
				int nBrokerID = DB.ExecuteScalar<int>(
					"BrokerIsBroker",
					CommandSpecies.StoredProcedure,
					new QueryParameter("ContactEmail", CustomerData.Mail),
					new QueryParameter("UiOriginID")
				);

				SendToCustomer = false;

				Variables["UserType"] = (nBrokerID == 0) ? "customer" : "broker";

				Variables["ErrMsg"] = ecl.ErrorMessage;

				Variables["UserID"] = CustomerData.Id.ToString(CultureInfo.InvariantCulture);

				Variables["ProfileLink"] = (nBrokerID == 0)
					? UnderwriterSite + "/Underwriter/Customers#profile/" + CustomerData.Id
					: UnderwriterSite + "/Underwriter/Customers#broker/" + nBrokerID;

				TemplateName = "Mandrill - EZBOB password was restored - to staff";
			} // if
		} // SetTemplateAndVariables

		protected virtual string Salutation {
			get { return "Dear customer"; }
		} // Salutation
	} // class PasswordRestored
} // namespace

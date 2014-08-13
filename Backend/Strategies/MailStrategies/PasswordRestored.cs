namespace EzBob.Backend.Strategies.MailStrategies {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using Exceptions;
	using Ezbob.Database;
	using Ezbob.Logger;
	using StoredProcs;
	using UserManagement;
	using UserManagement.EmailConfirmation;

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

			Guid oToken = InitCreatePasswordToken.Execute(DB, CustomerData.Mail);

			if (oToken == Guid.Empty)
				throw new StrategyAlert(this, "Failed to generate a change password token for customer " + CustomerData.Mail);

			Variables = new Dictionary<string, string> {
				{"Link", CustomerSite + "/Account/CreatePassword?token=" + oToken.ToString("N")},
				{"FirstName", string.IsNullOrWhiteSpace(CustomerData.FirstName) ? Salutation : CustomerData.FirstName}
			};

			var ecl = new EmailConfirmationLoad(CustomerData.UserID, DB, Log);
			ecl.Execute();

			if (ecl.IsConfirmed)
				TemplateName = "Mandrill - EZBOB password was restored";
			else {
				int nBrokerID = DB.ExecuteScalar<int>(
					"BrokerIsBroker",
					CommandSpecies.StoredProcedure,
					new QueryParameter("ContactEmail", CustomerData.Mail)
				);

				SendToCustomer = false;

				Variables["UserType"] = (nBrokerID == 0) ? "customer" : "broker";

				Variables["ErrMsg"] = ecl.ErrorMessage;

				Variables["UserID"] = CustomerData.Id.ToString(CultureInfo.InvariantCulture);

				Variables["ProfileLink"] = (nBrokerID == 0)
					? UnderwriterSite + "/Underwriter/Customers#profile/" + CustomerData.Id
					: UnderwriterSite + "/Underwriter/Customers#broker/" + nBrokerID;

				TemplateName = "Mandrill - EZBOB password was restored - to staff";
			}
		} // SetTemplateAndVariables

		#endregion method SetTemplateAndVariables

		#region property Salutation

		protected virtual string Salutation {
			get { return "Dear customer"; }
		} // Salutation

		#endregion property Salutation
	} // class PasswordRestored
} // namespace

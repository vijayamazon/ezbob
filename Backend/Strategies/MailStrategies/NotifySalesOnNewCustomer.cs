namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using API;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;

	#region class NotifySalesOnNewCustomer

	public class NotifySalesOnNewCustomer : AMailStrategyBase {
		#region public
		public NotifySalesOnNewCustomer(int nCustomerID, AConnection oDB, ASafeLog oLog) : base(nCustomerID, false, oDB, oLog) {
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Notify sales on new customer"; }
		} // Name

		#endregion property Name

		#endregion public

		#region protected

		#region method SetTemplateAndVariables

		protected override void SetTemplateAndVariables() {
			Log.Debug("Notifying sales about customer {0}", CustomerData);

			TemplateName = "Mandrill - Sales - New customer";

			Variables = new Dictionary<string, string> {
				{ "ID", CustomerId.ToString(CultureInfo.InvariantCulture) },
				{ "ProfileLink", "https://" + UnderwriterSite + "/UnderWriter/Customers?customerid=" + CustomerId },
				{ "FirstName", CustomerData.FirstName },
				{ "LastName", CustomerData.Surname },
				{ "Email", CustomerData.Mail },
				{ "MobilePhone", CustomerData.MobilePhone },
				{ "DaytimePhone", CustomerData.DaytimePhone },
			};
		} // SetTemplateAndVariables

		#endregion method SetTemplateAndVariables

		#region method GetRecipients

		protected override Addressee[] GetRecipients() {
			string sEmail = CurrentValues.Instance.SalesEmail;

			if (string.IsNullOrWhiteSpace(sEmail))
				return new Addressee[0];

			return sEmail.Split(',').Select(addr => new Addressee(addr)).ToArray();
		} // GetRecipients
		#endregion method GetCustomerEmail

		#endregion protected
	} // class NotifySalesOnNewCustomer
	#endregion class NotifySalesOnNewCustomer
} // namespace EzBob.Backend.Strategies.MailStrategies

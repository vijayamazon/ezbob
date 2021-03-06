﻿namespace Ezbob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using API;
	using ConfigManager;

	public class NotifySalesOnNewCustomer : AMailStrategyBase {
		public NotifySalesOnNewCustomer(int nCustomerID) : base(nCustomerID, false) {
		} // constructor

		public override string Name {
			get { return "Notify sales on new customer"; }
		} // Name

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

		protected override Addressee[] GetRecipients() {
			string sEmail = CurrentValues.Instance.SalesEmail;

			if (string.IsNullOrWhiteSpace(sEmail))
				return new Addressee[0];

			return sEmail.Split(',').Select(addr => new Addressee(addr, bShouldRegister: false)).ToArray();
		} // GetRecipients
	} // class NotifySalesOnNewCustomer
} // namespace Ezbob.Backend.Strategies.MailStrategies

using System;
using System.Collections.Generic;

namespace Ezbob.Backend.Strategies.MailStrategies {
	using System.Linq;
	using ConfigManager;
	using Ezbob.Backend.Strategies.MailStrategies.API;

	public class SendEverlineRefinanceMails : AMailStrategyBase {
		private readonly int customerId;
		private readonly string customerName;
		private readonly DateTime now;
		private readonly decimal amount;

		public SendEverlineRefinanceMails(int customerId, string customerName, DateTime now, decimal amount)
			: base(customerId, false) {
			this.customerId = customerId;
			this.customerName = customerName;
			this.now = now;
			this.amount = amount;
		}

		public override string Name {
			get {
				return "SendEverlineRefinanceMails";
			}
		}

		protected override void SetTemplateAndVariables() {
			SendToCustomer = false;
			TemplateName = "Mandrill - EverlineRefinanceMail";
			Variables = new Dictionary<string, string> {
				{"CustomerId", customerId.ToString() },
				{"CustomerEmail", customerName},
				{"LoanDate", now.ToString("dd/MM/yyyy") },
				{"LoanAmount", amount.ToString("N2")}
			};
		}

		protected override Addressee[] GetRecipients() {
			string sEmail = CurrentValues.Instance.EverlineRefinanceEmailReciever;

			if (string.IsNullOrWhiteSpace(sEmail))
				return new Addressee[0];

			return sEmail.Split(',').Select(addr => new Addressee(addr, bShouldRegister: false)).ToArray();
		} // GetRecipients
	}
}

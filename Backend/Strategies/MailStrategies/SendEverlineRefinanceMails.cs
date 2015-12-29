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
		private readonly decimal loanAmount;
		private readonly decimal transferedAmount;

		public SendEverlineRefinanceMails(int customerId, string customerName, DateTime now, decimal loanAmount, decimal transferedAmount)
			: base(customerId, false) {
			this.customerId = customerId;
			this.customerName = customerName;
			this.now = now;
			this.loanAmount = loanAmount;
			this.transferedAmount = transferedAmount;
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
				{"LoanAmount", loanAmount.ToString("N2")},
				{"TransferedAmount", transferedAmount.ToString("N2")}
			};
		}

		protected override Addressee[] GetRecipients() {
			string sEmail = CurrentValues.Instance.EverlineRefinanceEmailReciever;

			if (string.IsNullOrWhiteSpace(sEmail))
				return new Addressee[0];

			return sEmail.Split(',').Select(addr => new Addressee(addr, bShouldRegister: false, addSalesforceActivity: false)).ToArray();
		} // GetRecipients
	}
}

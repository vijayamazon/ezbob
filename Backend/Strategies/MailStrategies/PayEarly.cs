using System;
using System.Globalization;
using System.Collections.Generic;
using Ezbob.Database;
using Ezbob.Logger;

namespace EzBob.Backend.Strategies.MailStrategies {
	public class PayEarly : AMailStrategyBase {
		#region constructor

		public PayEarly(int customerId, decimal amount, string loanRefNumber, AConnection oDB, ASafeLog oLog) : base(customerId, true, oDB, oLog) {
			this.amount = amount;
			this.loanRefNumber = loanRefNumber;
		} // constructor

		#endregion constructor

		public override string Name { get { return "Pay Early"; } } // Name

		#region method SetTemplateAndSubjectAndVariables

		protected override void SetTemplateAndSubjectAndVariables() {
			Subject = string.Format("Dear {0}, your payment of £{1} has been credited to your ezbob account.", CustomerData.FirstName, amount);
			TemplateName = "Mandrill - Repayment confirmation";

			Variables = new Dictionary<string, string> {
				{"FirstName", CustomerData.FirstName},
				{"AMOUNT", amount.ToString(CultureInfo.InvariantCulture)},
				{"DATE", DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss").Replace('-','/')},
				{"RefNum", loanRefNumber}
			};
		} // SetTemplateAndSubjectAndVariables

		#endregion method SetTemplateAndSubjectAndVariables

		private readonly decimal amount;
		private readonly string loanRefNumber;
	} // class PayEarly
} // namespace

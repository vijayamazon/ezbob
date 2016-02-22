namespace Ezbob.Backend.Strategies.MailStrategies {
	using System;
	using System.Globalization;
	using System.Collections.Generic;

	// TODO   rename to SendPaymentConfirmationEmail
	public class PayEarly : AMailStrategyBase {
		public PayEarly(int customerId, decimal amount, string loanRefNumber) : base(customerId, true) {
			this.amount = amount;
			this.loanRefNumber = loanRefNumber;
		} // constructor

		public override string Name { get { return "Pay Early"; } } // Name

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - Repayment confirmation";

			Variables = new Dictionary<string, string> {
				{"FirstName", CustomerData.FirstName},
				{"AMOUNT", amount.ToString(CultureInfo.InvariantCulture)},
				{"DATE", DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss").Replace('-','/')},
				{"RefNum", loanRefNumber}
			};
		} // SetTemplateAndSubjectAndVariables

		private readonly decimal amount;
		private readonly string loanRefNumber;
	} // class PayEarly
} // namespace

namespace EzBob.Backend.Strategies.MailStrategies
{
	using System;
	using System.Globalization;
	using System.Collections.Generic;

	public class PayEarly : MailStrategyBase
	{
		private readonly int amount;
		private readonly string loanRefNumber;

		public PayEarly(int customerId, int amount, string loanRefNumber)
			: base(customerId, true)
		{
			this.amount = amount;
			this.loanRefNumber = loanRefNumber;
		}

		public override void SetTemplateAndSubjectAndVariables()
		{
			Subject = string.Format("Dear {0}, your payment of £{1} has been credited to your ezbob account.", CustomerData.FirstName, amount);
			TemplateName = "Mandrill - Repayment confirmation";

			Variables = new Dictionary<string, string>
				{
					{"FirstName", CustomerData.FirstName},
					{"AMOUNT", amount.ToString(CultureInfo.InvariantCulture)},
					{"DATE", DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss").Replace('-','/')},
					{"RefNum", loanRefNumber}
				};
		}
	}
}

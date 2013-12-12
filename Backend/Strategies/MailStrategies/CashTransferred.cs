namespace EzBob.Backend.Strategies.MailStrategies
{
	using System.Globalization;
	using System.Collections.Generic;

	public class CashTransferred : MailStrategyBase
	{
		private readonly int amount;

		public CashTransferred(int customerId, int amount)
			: base(customerId, true)
		{
			this.amount = amount;
		}

		public override void SetTemplateAndSubjectAndVariables()
		{
			Variables = new Dictionary<string, string>
				{
					{"FirstName", CustomerData.FirstName},
					{"Amount", amount.ToString(CultureInfo.InvariantCulture)}
				};

			if (CustomerData.NumOfLoans == 1)
			{
				Subject = "Welcome to the EZBOB family";
				TemplateName = CustomerData.IsOffline ? "Mandrill - Took Offline Loan (1st loan)" : "Mandrill - Took Loan (1st loan)";
			}
			else
			{
				Subject = "Thanks for choosing EZBOB as your funding partner";
				TemplateName = CustomerData.IsOffline ? "Mandrill - Took Offline Loan (not 1st loan)" : "Mandrill - Took Loan (not 1st loan)";
			}
		}
	}
}

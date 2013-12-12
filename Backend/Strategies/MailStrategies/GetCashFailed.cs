namespace EzBob.Backend.Strategies.MailStrategies
{
	using System.Collections.Generic;

	public class GetCashFailed : MailStrategyBase
	{
		public GetCashFailed(int customerId)
			: base(customerId, true)
		{
		}

		public override void SetTemplateAndSubjectAndVariables()
		{
			Subject = "Get cash - problem with the card";
			TemplateName = "Mandrill - Debit card authorization problem";

			Variables = new Dictionary<string, string>
				{
					{"DashboardPage", "https://app.ezbob.com/Customer/Profile"},
					{"FirstName", CustomerData.FirstName}
				};
		}
	}
}

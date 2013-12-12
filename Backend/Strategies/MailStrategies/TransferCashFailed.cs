namespace EzBob.Backend.Strategies.MailStrategies
{
	using System.Collections.Generic;

	public class TransferCashFailed : MailStrategyBase
	{
		public TransferCashFailed(int customerId)
			: base(customerId, true)
		{
		}

		public override void SetTemplateAndSubjectAndVariables()
		{
			Subject = "Bank account couldn’t be verified";
			TemplateName = "Mandrill - Problem with bank account";

			Variables = new Dictionary<string, string>
				{
					{"FirstName", CustomerData.FirstName}
				};
		}
	}
}

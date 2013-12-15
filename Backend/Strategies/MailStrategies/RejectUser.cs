namespace EzBob.Backend.Strategies.MailStrategies
{
	using System.Collections.Generic;

	public class RejectUser : MailStrategyBase
	{
		public RejectUser(int customerId)
			: base(customerId, true)
		{
		}

		public override void SetTemplateAndSubjectAndVariables()
		{
			Subject = "Sorry, ezbob cannot make you a loan offer at this time";
			TemplateName = "Mandrill - Rejection email";

			Variables = new Dictionary<string, string>
				{
					{"FirstName", CustomerData.FirstName},
					{"EzbobAccount", "https://app.ezbob.com/Customer/Profile"}
				};
		}
	}
}

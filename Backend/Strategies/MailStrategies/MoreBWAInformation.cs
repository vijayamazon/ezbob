namespace EzBob.Backend.Strategies.MailStrategies
{
	using System.Collections.Generic;

	public class MoreBWAInformation : MailStrategyBase
	{
		public MoreBWAInformation(int customerId)
			: base(customerId, true)
		{
		}

		public override void SetTemplateAndSubjectAndVariables()
		{
			Subject = "We require a proof of bank account ownership to make you a loan offer";
			TemplateName = "Mandrill - Application incompleted Bank";

			Variables = new Dictionary<string, string>
				{
					{"FirstName", CustomerData.FirstName}
				};
		}
	}
}

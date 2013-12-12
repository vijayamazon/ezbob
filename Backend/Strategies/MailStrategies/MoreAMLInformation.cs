namespace EzBob.Backend.Strategies.MailStrategies
{
	using System.Collections.Generic;

	public class MoreAMLInformation : MailStrategyBase
	{
		public MoreAMLInformation(int customerId)
			: base(customerId, true)
		{
		}

		public override void SetTemplateAndSubjectAndVariables()
		{
			Subject = "Proof of ID required to make you a loan offer";
			TemplateName = "Mandrill - Application incompleted AML";

			Variables = new Dictionary<string, string>
				{
					{"FirstName", CustomerData.FirstName}
				};
		}
	}
}

namespace EzBob.Backend.Strategies.MailStrategies
{
	using System.Collections.Generic;

	public class MoreAMLandBWAInformation : MailStrategyBase
	{
		public MoreAMLandBWAInformation(int customerId)
			: base(customerId, true)
		{
		}

		public override void SetTemplateAndSubjectAndVariables()
		{
			Subject = "We require a proof of bank account ownership and proof of ID to make you a loan offer";
			TemplateName = "Mandrill - Application incompleted AML & Bank";

			Variables = new Dictionary<string, string>
				{
					{"FirstName", CustomerData.FirstName}
				};
		}
	}
}

namespace EzBob.Backend.Strategies.MailStrategies
{
	using System.Collections.Generic;

	public class EmailUnderReview : MailStrategyBase
	{
		public EmailUnderReview(int customerId)
			: base(customerId, true)
		{
		}

		public override void SetTemplateAndSubjectAndVariables()
		{
			Subject = "Your completed application is currently under review";
			TemplateName = "Mandrill - Application completed under review";

			Variables = new Dictionary<string, string>
				{
					{"FirstName", CustomerData.FirstName}
				};
		}
	}
}

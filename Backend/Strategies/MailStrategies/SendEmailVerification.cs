namespace EzBob.Backend.Strategies.MailStrategies
{
	using System.Collections.Generic;

	public class SendEmailVerification : MailStrategyBase
	{
		private readonly string address;

		public SendEmailVerification(int customerId, string address)
			: base(customerId, true)
		{
			this.address = address;
		}

		public override void SetTemplateAndSubjectAndVariables()
		{
			Subject = "Please verify your email";
			TemplateName = "Mandrill - Confirm your email";

			Variables = new Dictionary<string, string>
				{
					{"FirstName", CustomerData.FirstName},
					{"Email", CustomerData.Mail},
					{"ConfirmEmailAddress", address}
				};
		}
	}
}

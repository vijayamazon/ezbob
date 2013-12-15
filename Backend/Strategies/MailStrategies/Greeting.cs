namespace EzBob.Backend.Strategies.MailStrategies
{
	using DbConnection;
	using System.Collections.Generic;

	public class Greeting : MailStrategyBase
	{
		private readonly string confirmEmailAddress;

		public Greeting(int customerId, string confirmEmailAddress)
			: base(customerId, true)
		{
			this.confirmEmailAddress = confirmEmailAddress;
		}

		public override void SetTemplateAndSubjectAndVariables()
		{
			Variables = new Dictionary<string, string>
				{
					{"Email", CustomerData.Mail},
					{"ConfirmEmailAddress", confirmEmailAddress}
				};

			Subject = "Thank you for registering with ezbob!";
			TemplateName = "Greeting";
		}

		public override void ActionAtEnd()
		{
			DbConnection.ExecuteSpNonQuery("Greeting_Mail_Sent",
				DbConnection.CreateParam("UserId", CustomerId),
				DbConnection.CreateParam("GreetingMailSent", 1));
		}
	}
}

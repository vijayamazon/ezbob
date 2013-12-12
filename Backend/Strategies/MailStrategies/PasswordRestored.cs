namespace EzBob.Backend.Strategies.MailStrategies
{
	using System.Collections.Generic;

	public class PasswordRestored : MailStrategyBase
	{
		private readonly string password;

		public PasswordRestored(int customerId, string password)
			: base(customerId, true)
		{
			this.password = password;
		}

		public override void SetTemplateAndSubjectAndVariables()
		{
			Subject = "RestorePassword";
			TemplateName = "Mandrill - EZBOB password was restored";

			Variables = new Dictionary<string, string>
				{
					{"ProfilePage", "https://app.ezbob.com/Account/LogOn"},
					{"Password", password},
					{"FirstName", CustomerData.FirstName}
				};
		}
	}
}

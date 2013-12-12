namespace EzBob.Backend.Strategies.MailStrategies
{
	using System.Collections.Generic;

	public class ThreeInvalidAttempts : MailStrategyBase
	{
		private readonly string password;

		public ThreeInvalidAttempts(int customerId, string password)
			: base(customerId, true)
		{
			this.password = password;
		}

		public override void SetTemplateAndSubjectAndVariables()
		{
			Subject = "Three unsuccessful login attempts to your account have been made.";
			TemplateName = "Mandrill - Temporary password";

			Variables = new Dictionary<string, string>
				{
					{"Password", password},
					{"FirstName", CustomerData.FirstName},
					{"ProfilePage", "https://app.ezbob.com/Customer/Profile"},
					{"NIMRODTELEPHONENUMBER", "+44 800 011 4787"} // TODO: change name of variable here and in mandrill\mailchimp
				};
		}
	}
}

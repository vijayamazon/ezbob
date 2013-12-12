namespace EzBob.Backend.Strategies.MailStrategies
{
	using System.Collections.Generic;

	public class PasswordChanged : MailStrategyBase
	{
		private readonly string password;

		public PasswordChanged(int customerId, string password)
			: base(customerId, true)
		{
			this.password = password;
		}

		public override void SetTemplateAndSubjectAndVariables()
		{
			Subject = "Your new EZBOB password has been registered.";
			TemplateName = "Mandrill - New password";

			Variables = new Dictionary<string, string>
				{
					{"Password", password},
					{"FirstName", CustomerData.FirstName}
				};
		}
	}
}

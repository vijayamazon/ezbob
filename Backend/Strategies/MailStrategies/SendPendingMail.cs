namespace EzBob.Backend.Strategies.MailStrategies 
{
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class SendPendingMail : AMailStrategyBase
	{
		public SendPendingMail(int customerId, AConnection oDb, ASafeLog oLog)
			: base(customerId,true, oDb, oLog)
		{
		}

		public override string Name {
			get { return "SendPendingMail"; }
		}

		protected override void SetTemplateAndVariables()
		{
			// TODO: change to actual mail
			TemplateName = "Greeting";

			Variables = new Dictionary<string, string> {
				{"Email", CustomerData.Mail},
				{"ConfirmEmailAddress", "bla"}
			};
		}
	}
}

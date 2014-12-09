namespace Ezbob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;

	public class SendPendingMail : AMailStrategyBase {
		private readonly List<string> actionItems;

		public SendPendingMail(int customerId, List<string> actionItems)
			: base(customerId, false) // TODO: EZ-2712: change the false to true once template is ready
		{
			this.actionItems = actionItems;
		}

		public override string Name {
			get { return "SendPendingMail"; }
		}

		protected override void SetTemplateAndVariables() {
			string actionItemsString = string.Empty;

			foreach (string actionItem in actionItems) {
				actionItemsString += actionItem + "<br/>";
			}

			TemplateName = "Mandrill - Action items";

			Variables = new Dictionary<string, string> {
				{"ActionItems", actionItemsString}
			};
		}
	}
}

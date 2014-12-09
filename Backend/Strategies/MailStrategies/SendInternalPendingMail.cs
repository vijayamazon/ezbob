namespace Ezbob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;

	public class SendInternalPendingMail : AMailStrategyBase {
		private readonly List<string> internalActionItems;

		public SendInternalPendingMail(int customerId, List<string> internalActionItems)
			: base(customerId, false) {
			this.internalActionItems = internalActionItems;
		}

		public override string Name {
			get { return "SendInternalPendingMail"; }
		}

		protected override void SetTemplateAndVariables() {
			string internalActionItemsString = string.Empty;

			foreach (string internalActionItem in internalActionItems) {
				internalActionItemsString += internalActionItem + "<br/>";
			}

			TemplateName = "Mandrill - Internal action items";

			Variables = new Dictionary<string, string> {
				{"ActionItems", internalActionItemsString}
			};
		}
	}
}

namespace Ezbob.Backend.Strategies.Misc {
	using System.Collections.Generic;
	using Ezbob.Database;
	using MailStrategies;

	public class SendPendingMails : AStrategy {
		private readonly int customerId;

		public SendPendingMails(int customerId) {
			this.customerId = customerId;
		}

		public override string Name {
			get { return "SendPendingMails"; }
		}

		public override void Execute() {
			var actionItems = new List<string>();
			var internalActionItems = new List<string>();

			DB.ForEachRowSafe(
				(sr, rowsetStart) => {
					bool mailToCustomer = sr["MailToCustomer"];
					string item = sr["Item"];

					if (mailToCustomer) {
						actionItems.Add(item);
					}
					else {
						internalActionItems.Add(item);
					}

					return ActionResult.Continue;
				},
				"GetCustomerActionItems",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
				);

			if (actionItems.Count > 0) {
				var sendPendingMailInstance = new SendPendingMail(customerId, actionItems);
				sendPendingMailInstance.Execute();
			}

			if (internalActionItems.Count > 0) {
				var sendInternalPendingMailInstance = new SendInternalPendingMail(customerId, internalActionItems);
				sendInternalPendingMailInstance.Execute();
			}
		}
	}
}

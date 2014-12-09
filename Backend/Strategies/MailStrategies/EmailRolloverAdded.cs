namespace Ezbob.Backend.Strategies.MailStrategies {
	using System.Globalization;
	using System.Collections.Generic;

	public class EmailRolloverAdded : AMailStrategyBase {
		public EmailRolloverAdded(int customerId, decimal amount)
			: base(customerId, true) {
			this.amount = amount;
		} // constructor

		public override string Name { get { return "Email Rollover Added"; } }

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - Rollover added";

			Variables = new Dictionary<string, string> {
				{"FirstName", CustomerData.FirstName},
				{"RolloverAmount", amount.ToString(CultureInfo.InvariantCulture)}
			};
		} // SetTemplateAndVariables

		private readonly decimal amount;
	} // class EmailRolloverAdded
} // namespace

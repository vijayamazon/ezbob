namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Globalization;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class EmailRolloverAdded : AMailStrategyBase {

		public EmailRolloverAdded(int customerId, decimal amount, AConnection oDb, ASafeLog oLog)
			: base(customerId, true, oDb, oLog)
		{
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

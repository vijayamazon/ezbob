namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class TransferCashFailed : AMailStrategyBase {
		public TransferCashFailed(int customerId, AConnection oDb, ASafeLog oLog) : base(customerId, true, oDb, oLog) {
		} // constructor

		public override string Name { get { return "Transfer Cash Failed"; } } // Name

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - Problem with bank account";

			Variables = new Dictionary<string, string> {
				{"FirstName", CustomerData.FirstName}
			};
		} // SetTemplateAndVariables

	} // class 
} // namespace

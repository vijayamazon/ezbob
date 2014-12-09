namespace Ezbob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;

	public class TransferCashFailed : AMailStrategyBase {
		public TransferCashFailed(int customerId) : base(customerId, true) {
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

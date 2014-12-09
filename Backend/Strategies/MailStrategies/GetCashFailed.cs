namespace Ezbob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class GetCashFailed : AMailStrategyBase {

		public GetCashFailed(int customerId) : base(customerId, true) {
		} // constructor

		public override string Name { get { return "Get Cash Failed"; } } // Name

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - Debit card authorization problem";

			Variables = new Dictionary<string, string> {
				{"DashboardPage", "https://app.ezbob.com/Customer/Profile"},
				{"FirstName", CustomerData.FirstName}
			};
		} // SetTemplateAndVariables

	} // class GetCashFailed
} // namespace

namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class GetCashFailed : AMailStrategyBase {
		#region constructor

		public GetCashFailed(int customerId, AConnection oDb, ASafeLog oLog) : base(customerId, true, oDb, oLog) {
		} // constructor

		#endregion constructor

		public override string Name { get { return "Get Cash Failed"; } } // Name

		#region method SetTemplateAndVariables

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - Debit card authorization problem";

			Variables = new Dictionary<string, string> {
				{"DashboardPage", "https://app.ezbob.com/Customer/Profile"},
				{"FirstName", CustomerData.FirstName}
			};
		} // SetTemplateAndVariables

		#endregion method SetTemplateAndVariables
	} // class GetCashFailed
} // namespace

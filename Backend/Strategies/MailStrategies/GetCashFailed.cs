using System.Collections.Generic;
using Ezbob.Database;
using Ezbob.Logger;

namespace EzBob.Backend.Strategies.MailStrategies {
	public class GetCashFailed : AMailStrategyBase {
		#region constructor

		public GetCashFailed(int customerId, AConnection oDB, ASafeLog oLog) : base(customerId, true, oDB, oLog) {
		} // constructor

		#endregion constructor

		public override string Name { get { return "Get Cash Failed"; } } // Name

		#region method SetTemplateAndSubjectAndVariables

		protected override void SetTemplateAndSubjectAndVariables() {
			Subject = "Get cash - problem with the card";
			TemplateName = "Mandrill - Debit card authorization problem";

			Variables = new Dictionary<string, string> {
				{"DashboardPage", "https://app.ezbob.com/Customer/Profile"},
				{"FirstName", CustomerData.FirstName}
			};
		} // SetTemplateAndSubjectAndVariables

		#endregion method SetTemplateAndSubjectAndVariables
	} // class GetCashFailed
} // namespace

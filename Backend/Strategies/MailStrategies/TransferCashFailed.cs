using System.Collections.Generic;
using Ezbob.Database;
using Ezbob.Logger;

namespace EzBob.Backend.Strategies.MailStrategies {
	public class TransferCashFailed : AMailStrategyBase {
		public TransferCashFailed(int customerId, AConnection oDB, ASafeLog oLog) : base(customerId, true, oDB, oLog) {
		} // constructor

		public override string Name { get { return "Transfer Cash Failed"; } } // Name

		#region method SetTemplateAndSubjectAndVariables

		protected override void SetTemplateAndSubjectAndVariables() {
			Subject = "Bank account couldn’t be verified";
			TemplateName = "Mandrill - Problem with bank account";

			Variables = new Dictionary<string, string> {
				{"FirstName", CustomerData.FirstName}
			};
		} // SetTemplateAndSubjectAndVariables

		#endregion method SetTemplateAndSubjectAndVariables
	} // class 
} // namespace

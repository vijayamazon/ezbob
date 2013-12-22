using System.Collections.Generic;
using Ezbob.Database;
using Ezbob.Logger;

namespace EzBob.Backend.Strategies.MailStrategies {
	public class RequestCashWithoutTakenLoan : AMailStrategyBase {
		public RequestCashWithoutTakenLoan(int customerId, AConnection oDB, ASafeLog oLog) : base(customerId, true, oDB, oLog) {
		} // constructor

		public override string Name { get {return "RequestCashWithoutTakenLoan"; } } // Name

		#region method SetTemplateAndSubjectAndVariables

		protected override void SetTemplateAndSubjectAndVariables() {
			Subject = string.Format("{0}, we are currently re-analysing your business in order to make you a new funding offer.", CustomerData.FirstName);
			TemplateName = "Mandrill - Re-analyzing customer";

			Variables = new Dictionary<string, string> {
				{"FirstName", CustomerData.FirstName}
			};
		} // SetTemplateAndSubjectAndVariables

		#endregion method SetTemplateAndSubjectAndVariables
	} // class RequestCashWithoutTakenLoan
} // namespace

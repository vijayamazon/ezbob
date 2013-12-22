using System.Collections.Generic;
using Ezbob.Database;
using Ezbob.Logger;

namespace EzBob.Backend.Strategies.MailStrategies {
	public class LoanFullyPaid : AMailStrategyBase {
		#region constructor

		public LoanFullyPaid(int customerId, string loanRefNum, AConnection oDB, ASafeLog oLog) : base(customerId, true, oDB, oLog) {
			this.loanRefNum = loanRefNum;
		} // constructor

		#endregion constructor

		public override string Name { get { return "Loan Fully Paid"; } } // Name

		#region method SetTemplateAndSubjectAndVariables

		protected override void SetTemplateAndSubjectAndVariables() {
			Subject = string.Format("{0}, we are currently re-analysing your business in order to make you a new funding offer.", CustomerData.FirstName);
			TemplateName = "Mandrill - Loan paid in full";

			Variables = new Dictionary<string, string> {
				{"FirstName", CustomerData.FirstName},
				{"RefNum", loanRefNum}
			};
		} // SetTemplateAndSubjectAndVariables

		#endregion method SetTemplateAndSubjectAndVariables

		private readonly string loanRefNum;
	} // class LoanFullyPaid
} // namespace

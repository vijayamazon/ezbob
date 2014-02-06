namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class LoanFullyPaid : AMailStrategyBase {
		#region constructor

		public LoanFullyPaid(int customerId, string loanRefNum, AConnection oDb, ASafeLog oLog) : base(customerId, true, oDb, oLog) {
			this.loanRefNum = loanRefNum;
		} // constructor

		#endregion constructor

		public override string Name { get { return "Loan Fully Paid"; } } // Name

		#region method SetTemplateAndVariables

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - Loan paid in full";

			Variables = new Dictionary<string, string> {
				{"FirstName", CustomerData.FirstName},
				{"RefNum", loanRefNum}
			};
		} // SetTemplateAndVariables

		#endregion method SetTemplateAndSubjectAndVariables

		private readonly string loanRefNum;
	} // class LoanFullyPaid
} // namespace

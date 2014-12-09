namespace Ezbob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;

	public class LoanFullyPaid : AMailStrategyBase {

		public LoanFullyPaid(int customerId, string loanRefNum) : base(customerId, true) {
			this.loanRefNum = loanRefNum;
		} // constructor

		public override string Name { get { return "Loan Fully Paid"; } } // Name

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - Loan paid in full";

			Variables = new Dictionary<string, string> {
				{"FirstName", CustomerData.FirstName},
				{"RefNum", loanRefNum}
			};
		} // SetTemplateAndVariables

		private readonly string loanRefNum;
	} // class LoanFullyPaid
} // namespace

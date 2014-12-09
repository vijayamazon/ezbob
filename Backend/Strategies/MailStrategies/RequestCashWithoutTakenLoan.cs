namespace Ezbob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;

	public class RequestCashWithoutTakenLoan : AMailStrategyBase {
		public RequestCashWithoutTakenLoan(int customerId) : base(customerId, true) {
		} // constructor

		public override string Name { get {return "RequestCashWithoutTakenLoan"; } } // Name

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - Re-analyzing customer";

			Variables = new Dictionary<string, string> {
				{"FirstName", CustomerData.FirstName}
			};
		} // SetTemplateAndSubjectAndVariables

	} // class RequestCashWithoutTakenLoan
} // namespace

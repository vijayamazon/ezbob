namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class TransferCashFailed : AMailStrategyBase {
		public TransferCashFailed(int customerId, AConnection oDb, ASafeLog oLog) : base(customerId, true, oDb, oLog) {
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

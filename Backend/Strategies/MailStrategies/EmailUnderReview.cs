using System.Collections.Generic;
using Ezbob.Database;
using Ezbob.Logger;

namespace EzBob.Backend.Strategies.MailStrategies {
	public class EmailUnderReview : AMailStrategyBase {
		#region constructor

		public EmailUnderReview(int customerId, AConnection oDB, ASafeLog oLog)
			: base(customerId, true, oDB, oLog) {
		} // constructor

		#endregion constructor

		public override string Name { get { return "Email Under Review"; } }

		#region method SetTemplateAndSubjectAndVariables

		protected override void SetTemplateAndSubjectAndVariables() {
			Subject = "Your completed application is currently under review";
			TemplateName = "Mandrill - Application completed under review";

			Variables = new Dictionary<string, string> {
				{"FirstName", CustomerData.FirstName}
			};
		} // SetTemplateAndSubjectAndVariables

		#endregion method SetTemplateAndSubjectAndVariables
	} // class EmailUnderReview
} // namespace

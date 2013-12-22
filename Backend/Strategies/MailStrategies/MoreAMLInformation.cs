using System.Collections.Generic;
using Ezbob.Database;
using Ezbob.Logger;

namespace EzBob.Backend.Strategies.MailStrategies {
	public class MoreAMLInformation : AMailStrategyBase {
		public MoreAMLInformation(int customerId, AConnection oDB, ASafeLog oLog) : base(customerId, true, oDB, oLog) {
		} // constructor

		public override string Name { get { return "More AML Information"; } } // Name

		#region method SetTemplateAndSubjectAndVariables

		protected override void SetTemplateAndSubjectAndVariables() {
			Subject = "Proof of ID required to make you a loan offer";
			TemplateName = "Mandrill - Application incompleted AML";

			Variables = new Dictionary<string, string> {
				{"FirstName", CustomerData.FirstName}
			};
		} // SetTemplateAndSubjectAndVariables

		#endregion method SetTemplateAndSubjectAndVariables
	} // class MoreAMLInformation
} // namespace

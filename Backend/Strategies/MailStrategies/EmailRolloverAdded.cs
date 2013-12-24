using System.Globalization;
using System.Collections.Generic;
using Ezbob.Database;
using Ezbob.Logger;

namespace EzBob.Backend.Strategies.MailStrategies {
	public class EmailRolloverAdded : AMailStrategyBase {
		#region constructor

		public EmailRolloverAdded(int customerId, decimal amount, AConnection oDB, ASafeLog oLog)
			: base(customerId, true, oDB, oLog)
		{
			this.amount = amount;
		} // constructor

		#endregion constructor

		public override string Name { get { return "Email Rollover Added"; } }

		#region method SetTemplateAndSubjectAndVariables

		protected override void SetTemplateAndSubjectAndVariables() {
			Subject = "Rollover added";
			TemplateName = "Mandrill - Rollover added";

			Variables = new Dictionary<string, string> {
				{"FirstName", CustomerData.FirstName},
				{"RolloverAmount", amount.ToString(CultureInfo.InvariantCulture)}
			};
		} // SetTemplateAndSubjectAndVariables

		#endregion method SetTemplateAndSubjectAndVariables

		private readonly decimal amount;
	} // class EmailRolloverAdded
} // namespace

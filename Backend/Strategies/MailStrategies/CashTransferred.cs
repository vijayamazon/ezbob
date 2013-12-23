using System.Globalization;
using System.Collections.Generic;
using Ezbob.Database;
using Ezbob.Logger;

namespace EzBob.Backend.Strategies.MailStrategies {
	public class CashTransferred : AMailStrategyBase {
		#region consturctor

		public CashTransferred(int customerId, int amount, AConnection oDB, ASafeLog oLog) : base(customerId, true, oDB, oLog) {
			this.amount = amount;
		} // constructor

		#endregion consturctor

		public override string Name { get { return "Cash Transferred"; } }

		#region method SetTemplateAndSubjectAndVariables

		protected override void SetTemplateAndSubjectAndVariables() {
			Variables = new Dictionary<string, string> {
				{"FirstName", CustomerData.FirstName},
				{"Amount", amount.ToString(CultureInfo.InvariantCulture)}
			};

			if (CustomerData.NumOfLoans == 1)
			{
				Subject = "Welcome to the ezbob family";
				TemplateName = CustomerData.IsOffline ? "Mandrill - Took Offline Loan (1st loan)" : "Mandrill - Took Loan (1st loan)";
			}
			else
			{
				Subject = "Thanks for choosing ezbob as your funding partner";
				TemplateName = CustomerData.IsOffline ? "Mandrill - Took Offline Loan (not 1st loan)" : "Mandrill - Took Loan (not 1st loan)";
			}
		} // SetTemplateAndSubjectAndVariables

		#endregion method SetTemplateAndSubjectAndVariables

		private readonly int amount;
	} // class CashTransferred
} // namespace

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

			Subject = (CustomerData.NumOfLoans == 1) ? "Welcome to the ezbob family" : "Thanks for choosing ezbob as your funding partner";

			SetTemplateName("Mandrill - Took __OFFLINE__ Loan __FIRST_TIME__", "1st loan", CustomerData.NumOfLoans == 1);
		} // SetTemplateAndSubjectAndVariables

		#endregion method SetTemplateAndSubjectAndVariables

		private readonly int amount;
	} // class CashTransferred
} // namespace

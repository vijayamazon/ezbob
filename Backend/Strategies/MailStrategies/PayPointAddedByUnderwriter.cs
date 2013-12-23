using System.Globalization;
using System.Collections.Generic;
using Ezbob.Database;
using Ezbob.Logger;

namespace EzBob.Backend.Strategies.MailStrategies {
	public class PayPointAddedByUnderwriter : AMailStrategyBase {
		#region constructor

		public PayPointAddedByUnderwriter(int customerId, string cardno, string underwriterName, int underwriterId, AConnection oDB, ASafeLog oLog) : base(customerId, false, oDB, oLog) {
			this.underwriterId = underwriterId;
			this.cardno = cardno;
			this.underwriterName = underwriterName;
		} // constructor

		#endregion constructor

		public override string Name { get { return "PayPoint Added By Underwriter"; } } // Name

		#region method SetTemplateAndSubjectAndVariables

		protected override void SetTemplateAndSubjectAndVariables() {
			Subject = "Underwriter has added a debit card to the client account";
			TemplateName = "Mandrill - Underwriter added a debit card";

			Variables = new Dictionary<string, string> {
				{"UWName", underwriterName},
				{"UWID", underwriterId.ToString(CultureInfo.InvariantCulture)},
				{"Email", CustomerData.Mail},
				{"ClientId", CustomerId.ToString(CultureInfo.InvariantCulture)},
				{"ClientName", CustomerData.FullName},
				{"CardNo", cardno}
			};
		} // SetTemplateAndSubjectAndVariables

		#endregion method SetTemplateAndSubjectAndVariables

		private readonly int underwriterId;
		private readonly string cardno;
		private readonly string underwriterName;
	} // class PayPointAddedByUnderwriter
} // namespace

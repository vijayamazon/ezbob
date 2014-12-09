namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Globalization;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class PayPointAddedByUnderwriter : AMailStrategyBase {

		public PayPointAddedByUnderwriter(int customerId, string cardno, string underwriterName, int underwriterId, AConnection oDb, ASafeLog oLog) : base(customerId, false, oDb, oLog) {
			this.underwriterId = underwriterId;
			this.cardno = cardno;
			this.underwriterName = underwriterName;
		} // constructor

		public override string Name { get { return "PayPoint Added By Underwriter"; } } // Name

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - Underwriter added a debit card";

			Variables = new Dictionary<string, string> {
				{"UWName", underwriterName},
				{"UWID", underwriterId.ToString(CultureInfo.InvariantCulture)},
				{"Email", CustomerData.Mail},
				{"ClientId", CustomerId.ToString(CultureInfo.InvariantCulture)},
				{"ClientName", CustomerData.FullName},
				{"CardNo", cardno}
			};
		} // SetTemplateAndVariables

		private readonly int underwriterId;
		private readonly string cardno;
		private readonly string underwriterName;
	} // class PayPointAddedByUnderwriter
} // namespace

namespace Ezbob.Backend.Strategies.MailStrategies {
	using System.Globalization;
	using System.Collections.Generic;

	public class PayPointAddedByUnderwriter : AMailStrategyBase {
		public PayPointAddedByUnderwriter(int customerId, string cardno, string underwriterName, int underwriterId) : base(customerId, false) {
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

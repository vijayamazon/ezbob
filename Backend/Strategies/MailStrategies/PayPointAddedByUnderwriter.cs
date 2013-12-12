namespace EzBob.Backend.Strategies.MailStrategies
{
	using System.Globalization;
	using System.Collections.Generic;

	public class PayPointAddedByUnderwriter : MailStrategyBase
	{
		private readonly int underwriterId;
		private readonly string cardno;
		private readonly string underwriterName;

		public PayPointAddedByUnderwriter(int customerId, string cardno, string underwriterName, int underwriterId)
			: base(customerId, true)
		{
			this.underwriterId = underwriterId;
			this.cardno = cardno;
			this.underwriterName = underwriterName;
		}

		public override void SetTemplateAndSubjectAndVariables()
		{
			Subject = "Underwriter has added a debit card to the client account";
			TemplateName = "Mandrill - Underwriter added a debit card";

			Variables = new Dictionary<string, string>
				{
					{"UWName", underwriterName},
					{"UWID", underwriterId.ToString(CultureInfo.InvariantCulture)},
					{"Email", CustomerData.Mail},
					{"ClientId", CustomerId.ToString(CultureInfo.InvariantCulture)},
					{"ClientName", CustomerData.FullName},
					{"CardNo", cardno}
				};
		}
	}
}

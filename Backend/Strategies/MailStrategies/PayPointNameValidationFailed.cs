namespace EzBob.Backend.Strategies.MailStrategies
{
	using System.Globalization;
	using System.Collections.Generic;

	public class PayPointNameValidationFailed : MailStrategyBase
	{
		private readonly string cardHodlerName;

		public PayPointNameValidationFailed(int customerId, string cardHodlerName)
			: base(customerId, false)
		{
			this.cardHodlerName = cardHodlerName;
		}

		public override void SetTemplateAndSubjectAndVariables()
		{
			Subject = "PayPoint personal data differs from EZBOB application";
			TemplateName = "Mandrill - PayPoint data differs";

			Variables = new Dictionary<string, string>
				{
					{"E-mail", CustomerData.Mail},
					{"UserId", CustomerId.ToString(CultureInfo.InvariantCulture)},
					{"Name", CustomerData.FirstName},
					{"Surname", CustomerData.Surname},
					{"PayPointName", cardHodlerName}
				};
		}
	}
}

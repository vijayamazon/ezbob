namespace EzBob.Backend.Strategies.MailStrategies
{
	using System.Globalization;
	using System.Collections.Generic;

	public class EmailRolloverAdded : MailStrategyBase
	{
		private readonly int amount;

		public EmailRolloverAdded(int customerId, int amount)
			: base(customerId, true)
		{
			this.amount = amount;
		}

		public override void SetTemplateAndSubjectAndVariables()
		{
			Subject = "Rollover added";
			TemplateName = "Mandrill - Rollover added";

			Variables = new Dictionary<string, string>
				{
					{"FirstName", CustomerData.FirstName},
					{"RolloverAmount", amount.ToString(CultureInfo.InvariantCulture)}
				};
		}
	}
}

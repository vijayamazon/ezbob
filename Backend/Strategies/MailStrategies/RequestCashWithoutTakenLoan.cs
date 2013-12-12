namespace EzBob.Backend.Strategies.MailStrategies
{
	using System.Collections.Generic;

	public class RequestCashWithoutTakenLoan : MailStrategyBase
	{
		public RequestCashWithoutTakenLoan(int customerId)
			: base(customerId, true)
		{
		}

		public override void SetTemplateAndSubjectAndVariables()
		{
			Subject = string.Format("{0}, we are currently re-analysing your business in order to make you a new funding offer.", CustomerData.FirstName);
			TemplateName = "Mandrill - Re-analyzing customer";

			Variables = new Dictionary<string, string>
				{
					{"FirstName", CustomerData.FirstName}
				};
		}
	}
}

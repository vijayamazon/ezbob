namespace EzBob.Backend.Strategies.MailStrategies
{
	using System.Collections.Generic;

	public class LoanFullyPaid : MailStrategyBase
	{
		private readonly string loanRefNum;

		public LoanFullyPaid(int customerId, string loanRefNum)
			: base(customerId, true)
		{
			this.loanRefNum = loanRefNum;
		}

		public override void SetTemplateAndSubjectAndVariables()
		{
			Subject = string.Format("{0}, we are currently re-analysing your business in order to make you a new funding offer.", CustomerData.FirstName);
			TemplateName = "Mandrill - Loan paid in full";

			Variables = new Dictionary<string, string>
				{
					{"FirstName", CustomerData.FirstName},
					{"RefNum", loanRefNum}
				};
		}
	}
}

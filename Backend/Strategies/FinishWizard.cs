namespace EzBob.Backend.Strategies
{
	using Ezbob.Database;
	using Ezbob.Logger;
	using MailStrategies;
	using global::FraudChecker;

	public class FinishWizard : AStrategy
	{
		private readonly int customerId;

		public FinishWizard(int customerId, AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			this.customerId = customerId;
		}
		
		public override string Name
		{
			get { return "Finish wizard"; }
		} // Name

		public override void Execute()
		{
			DB.ExecuteNonQuery("FinishWizard", new QueryParameter("CustomerId", customerId));
			
			var emailUnderReviewStratgey = new EmailUnderReview(customerId, DB, Log);
			emailUnderReviewStratgey.Execute();

			var mainStrategy = new MainStrategy(customerId, NewCreditLineOption.UpdateEverythingAndApplyAutoRules, 0, DB, Log);
			mainStrategy.Execute();

			var fraudCheckStrategy = new FraudChecker(customerId, FraudMode.FullCheck, DB, Log);
			fraudCheckStrategy.Execute();
		}
	}
}

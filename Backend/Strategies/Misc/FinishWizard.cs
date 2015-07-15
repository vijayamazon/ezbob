namespace Ezbob.Backend.Strategies.Misc {
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.MailStrategies;
	using Ezbob.Backend.Strategies.SalesForce;
	using Ezbob.Database;

	public class FinishWizard : AStrategy {
		public FinishWizard(FinishWizardArgs args) {
			this.strategyArgs = args;
		} // constructor

		public override string Name {
			get { return "Finish wizard"; }
		} // Name

		public override void Execute() {
			DB.ExecuteNonQuery(
				"FinishWizard",
				new QueryParameter("CustomerId", this.strategyArgs.CustomerID)
			);

			if (this.strategyArgs.DoSendEmail)
				new EmailUnderReview(this.strategyArgs.CustomerID).Execute();

			var salesForceUpdateLeadAccount = new AddUpdateLeadAccount(null, this.strategyArgs.CustomerID, false, false);
			salesForceUpdateLeadAccount.Execute();

			// if (strategyArgs.DoMain) - this one is handled in esi_wizard.cs
			// because when MainStrategy is run from here its times are not
			// written to EzServiceActionHistory.
		} // Execute

		private readonly FinishWizardArgs strategyArgs;
	} // class FinishWizard
} // namespace

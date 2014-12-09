namespace Ezbob.Backend.Strategies.Misc {
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.MailStrategies;
	using Ezbob.Backend.Strategies.MainStrategy;
	using Ezbob.Database;

	public class FinishWizard : AStrategy {
		public FinishWizard(FinishWizardArgs oArgs) {
			m_oArgs = oArgs;
		} // constructor

		public override string Name {
			get { return "Finish wizard"; }
		} // Name

		public override void Execute() {
			DB.ExecuteNonQuery(
				"FinishWizard",
				new QueryParameter("CustomerId", m_oArgs.CustomerID)
			);

			if (m_oArgs.DoSendEmail) {
				new EmailUnderReview(m_oArgs.CustomerID).Execute();
			} // if

			if (m_oArgs.DoMain) {
				new MainStrategy(
					m_oArgs.CustomerID,
					m_oArgs.NewCreditLineOption,
					m_oArgs.AvoidAutoDecision
				).SetOverrideApprovedRejected(false).Execute();
			} // if
		} // Execute

		private readonly FinishWizardArgs m_oArgs;
	} // class FinishWizard
} // namespace

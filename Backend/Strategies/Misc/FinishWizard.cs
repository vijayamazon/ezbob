namespace EzBob.Backend.Strategies.Misc {
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using MailStrategies;
	using MainStrategy;

	public class FinishWizard : AStrategy {
		public FinishWizard(FinishWizardArgs oArgs, AConnection oDb, ASafeLog oLog) : base(oDb, oLog) {
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
				new EmailUnderReview(
					m_oArgs.CustomerID,
					DB,
					Log
				).Execute();
			} // if

			if (m_oArgs.DoMain) {
				new MainStrategy(
					m_oArgs.CustomerID,
					m_oArgs.NewCreditLineOption,
					m_oArgs.AvoidAutoDecision,
					m_oArgs.IsUnderwriterForced,
					DB,
					Log
				).SetOverrideApprovedRejected(false).Execute();
			} // if

			if (m_oArgs.DoFraud) {
				new FraudChecker(
					m_oArgs.CustomerID,
					m_oArgs.FraudMode,
					DB,
					Log
				).Execute();
			} // if
		} // Execute

		private readonly FinishWizardArgs m_oArgs;
	} // class FinishWizard
} // namespace

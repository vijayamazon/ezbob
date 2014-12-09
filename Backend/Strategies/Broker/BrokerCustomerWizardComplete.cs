namespace Ezbob.Backend.Strategies.Broker {
	using MailStrategies;

	public class BrokerCustomerWizardComplete : AStrategy {
		public BrokerCustomerWizardComplete(int nCustomerID) {
			m_nCustomerID = nCustomerID;
		} // constructor

		public override string Name {
			get { return "Broker customer wizard complete"; } // get
		} // Name

		public override void Execute() {
			new EmailUnderReview(m_nCustomerID).Execute();

			new BrokerDeleteCustomerLead(DB, Log) {
				CustomerID = m_nCustomerID,
				ReasonCode = BrokerDeleteCustomerLead.DeleteReasonCode.SignedUp.ToString(),
			}.ExecuteNonQuery();
		} // Execute

		private readonly int m_nCustomerID;

	} // class BrokerCustomerWizardComplete
} // namespace Ezbob.Backend.Strategies.Broker

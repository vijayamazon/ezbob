namespace EzBob.Backend.Strategies.Broker {
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BrokerCustomerWizardComplete : AStrategy {
		#region public

		#region constructor

		public BrokerCustomerWizardComplete(int nCustomerID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_nCustomerID = nCustomerID;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Broker customer wizard complete"; } // get
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			var sp = new BrokerDeleteCustomerLead(DB, Log) {
				CustomerID = m_nCustomerID,
				ReasonCode = BrokerDeleteCustomerLead.DeleteReasonCode.SignedUp.ToString(),
			};

			sp.ExecuteNonQuery();

			// TODO: send email
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly int m_nCustomerID;

		#endregion private
	} // class BrokerCustomerWizardComplete
} // namespace EzBob.Backend.Strategies.Broker

namespace Ezbob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using API;
	using Ezbob.Database;
	using Ezbob.Logger;

	public abstract class ABrokerMailToo : AMailStrategyBase {
		protected ABrokerMailToo(
			int nCustomerID,
			bool bSendToCustomer,
			bool bSendWhenOneLoan = false
		) : base(nCustomerID, bSendToCustomer) {
			m_bSendWhenOneLoan = bSendWhenOneLoan;
		} // constructor

		protected override Addressee[] GetRecipients() {
			if (CustomerData.NumOfLoans > 1)
				return base.GetRecipients();

			if (!m_bSendWhenOneLoan && (CustomerData.NumOfLoans == 1))
				return base.GetRecipients();

			var aryAddresses = new List<Addressee>();

			aryAddresses.AddRange(base.GetRecipients());

			var sp = new BrokerLoadAddressForCustomerMailCC(DB, Log) { CustomerID = CustomerData.Id, };

			string sBrokerEmail = sp.ExecuteScalar<string>();

			if (!string.IsNullOrWhiteSpace(sBrokerEmail))
				aryAddresses.Add(new Addressee(sBrokerEmail, userID: CustomerData.BrokerID, addSalesforceActivity: false));

			return aryAddresses.ToArray();
		} // GetRecipients

		private readonly bool m_bSendWhenOneLoan;

		private class BrokerLoadAddressForCustomerMailCC : AStoredProcedure {
			public BrokerLoadAddressForCustomerMailCC(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) { } // constructor

			public override bool HasValidParameters() {
				return CustomerID > 0;
			} // HasValidParameters

			public int CustomerID { get; set; }
		} // BrokerLoadAddressForCustomerMailCC
	} // class ABrokerMailToo
} // namespace Ezbob.Backend.Strategies.MailStrategies

namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using API;
	using Ezbob.Database;
	using Ezbob.Logger;

	#region class ABrokerMailToo

	public abstract class ABrokerMailToo : AMailStrategyBase {
		#region constructor

		protected ABrokerMailToo(int nCustomerID, bool bSendToCustomer, AConnection oDB, ASafeLog oLog, bool bSendWhenOneLoan = false)
			: base(nCustomerID, bSendToCustomer, oDB, oLog)
		{
			m_bSendWhenOneLoan = bSendWhenOneLoan;
		} // constructor

		#endregion constructor

		#region method GetRecipients

		protected override Addressee[] GetRecipients()
		{
			if (CustomerData.NumOfLoans > 1)
				return base.GetRecipients();

			if (!m_bSendWhenOneLoan && (CustomerData.NumOfLoans == 1))
				return base.GetRecipients();

			var aryAddresses = new List<Addressee>(); 

			aryAddresses.AddRange(base.GetRecipients());

			var sp = new BrokerLoadAddressForCustomerMailCC(DB, Log) { CustomerID = CustomerData.Id, };

			string sBrokerEmail = sp.ExecuteScalar<string>();

			if (!string.IsNullOrWhiteSpace(sBrokerEmail))
				aryAddresses.Add(new Addressee(sBrokerEmail));

			return aryAddresses.ToArray();
		} // GetRecipients

		#endregion method GetRecipients

		#region private

		private readonly bool m_bSendWhenOneLoan;

		#region class BrokerLoadAddressForCustomerMailCC

		private class BrokerLoadAddressForCustomerMailCC : AStoredProcedure {
			public BrokerLoadAddressForCustomerMailCC(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			public override bool HasValidParameters() {
				return CustomerID > 0;
			} // HasValidParameters

			public int CustomerID { get; set; }
		} // BrokerLoadAddressForCustomerMailCC

		#endregion class BrokerLoadAddressForCustomerMailCC

		#endregion private
	} // class ABrokerMailToo

	#endregion class ABrokerMailToo
} // namespace EzBob.Backend.Strategies.MailStrategies

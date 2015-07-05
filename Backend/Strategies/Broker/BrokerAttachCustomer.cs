namespace Ezbob.Backend.Strategies.Broker {
	using System;
	using Ezbob.Backend.Strategies.SalesForce;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class BrokerAttachCustomer : AStrategy {

		public BrokerAttachCustomer(
			int nCustomerID,
			int? nBrokerID,
			int nUnderwriterID
		) {
			m_oSp = new AttachCustomerToBroker(DB, Log) {
				CustomerID = nCustomerID,
				ToBrokerID = nBrokerID,
				UnderwriterID = nUnderwriterID,
			};
		} // constructor

		public override string Name {
			get { return "BrokerAttachCustomer"; }
		} // Name

		public override void Execute() {
			this.m_oSp.ExecuteNonQuery();

			//update account in SF IsBroker flag can be changed.
			var sfAccountUpdate = new AddUpdateLeadAccount("", this.m_oSp.CustomerID, false, false);
			sfAccountUpdate.Execute();
		} // Execute

		private readonly AttachCustomerToBroker m_oSp;

		private class AttachCustomerToBroker : AStoredProcedure {
			public AttachCustomerToBroker(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			public override bool HasValidParameters() {
				if ((CustomerID <= 0) || (UnderwriterID <= 0))
					return false;

				if (ToBrokerID.HasValue && (ToBrokerID.Value <= 0))
					ToBrokerID = null;

				return true;
			} // HasValidParameters

			[UsedImplicitly]
			public int CustomerID { get; set; }

			[UsedImplicitly]
			public int? ToBrokerID { get; set; }

			[UsedImplicitly]
			public int UnderwriterID { get; set; }

			[UsedImplicitly]
			public DateTime Now {
				get { return DateTime.UtcNow; }
				// ReSharper disable ValueParameterNotUsed
				set { }
				// ReSharper restore ValueParameterNotUsed
			} // Now
		} // class AttachCustomerToBroker

	} // class BrokerAttachCustomer
} // namespace

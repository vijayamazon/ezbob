namespace EzBob.Backend.Strategies.Broker {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class BrokerAttachCustomer : AStrategy {
		#region public

		#region constructor

		public BrokerAttachCustomer(
			int nCustomerID,
			int? nBrokerID,
			int nUnderwriterID,
			AConnection oDB,
			ASafeLog oLog
		) : base(oDB, oLog) {
			m_oSp = new AttachCustomerToBroker(DB, Log) {
				CustomerID = nCustomerID,
				ToBrokerID = nBrokerID,
				UnderwriterID = nUnderwriterID,
			};
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "BrokerAttachCustomer"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			m_oSp.ExecuteNonQuery();
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly AttachCustomerToBroker m_oSp;

		#region class AttachCustomerToBroker

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

		#endregion class AttachCustomerToBroker

		#endregion private
	} // class BrokerAttachCustomer
} // namespace

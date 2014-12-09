namespace Ezbob.Backend.Strategies.Broker {
	using System;
	using System.ComponentModel;
	using Ezbob.Database;
	using Ezbob.Logger;

	internal class BrokerDeleteCustomerLead : AStoredProcedure {
		public enum DeleteReasonCode {
			SignedUp,
			Manual,
		} // enum DeleteReasonCode

		public BrokerDeleteCustomerLead(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
		} // constructor

		public override bool HasValidParameters() {
			return (CustomerID > 0) && !string.IsNullOrWhiteSpace(ReasonCode);
		} // HasValidParameters

		public int CustomerID { get; set; }

		public string ReasonCode {
			get { return m_sReasonCode; } // get
			set {
				DeleteReasonCode code;

				if (Enum.TryParse<DeleteReasonCode>(value, true, out code))
					m_sReasonCode = code.ToString().ToUpper();
				else
					throw new InvalidEnumArgumentException("Invalid reason code: " + value);
			} // set
		} // ReasonCode

		private string m_sReasonCode;

		public DateTime DateDeleted {
			get { return DateTime.UtcNow; } // get
			set {
				// Nothing here. It has to be here and be public because
				// property traverser looks for properties with setter.
			} // set
		} // DateDeleted

	} // class BrokerDeleteCustomerLead
} // namespace Ezbob.Backend.Strategies.Broker

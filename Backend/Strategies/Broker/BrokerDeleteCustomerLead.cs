namespace EzBob.Backend.Strategies.Broker {
	using System;
	using System.ComponentModel;
	using Ezbob.Database;
	using Ezbob.Logger;

	internal class BrokerDeleteCustomerLead : AStoredProcedure {
		public enum DeleteReasonCode {
			SignedUp,
			Manual,
		} // enum DeleteReasonCode

		#region constructor

		public BrokerDeleteCustomerLead(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
		} // constructor

		#endregion constructor

		#region method HasValidParameters

		public override bool HasValidParameters() {
			return (CustomerID > 0) && !string.IsNullOrWhiteSpace(ReasonCode);
		} // HasValidParameters

		#endregion method HasValidParameters

		#region property CustomerID

		public int CustomerID { get; set; }

		#endregion property CustomerID

		#region property ReasonCode

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

		#endregion property ReasonCode

		#region property DateDeleted

		public DateTime DateDeleted {
			get { return DateTime.UtcNow; } // get
			set {
				// Nothing here. It has to be here and be public because
				// property traverser looks for properties with setter.
			} // set
		} // DateDeleted

		#endregion property DateDeleted
	} // class BrokerDeleteCustomerLead
} // namespace EzBob.Backend.Strategies.Broker

namespace Ezbob.Backend.Strategies.Broker {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BrokerLeadCheckToken : BrokerLeadCanFillWizard {

		public BrokerLeadCheckToken(string sToken) : base(0, "", "") {
			m_sToken = sToken;
		} // constructor

		public override string Name {
			get { return "Broker check lead token"; }
		} // Name

		protected override void Init() {
			ResultRow = null;

			StoredProc = new SpBrokerLeadCheckToken(DB, Log) {
				Token = new Guid(m_sToken),
			};
		} // Init

		private readonly string m_sToken;

		private class SpBrokerLeadCheckToken : AStoredProcedure {

			public SpBrokerLeadCheckToken(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			public override bool HasValidParameters() {
				return (Token != null) && (Token != Guid.Empty);
			} // HasValidParameters

			public Guid Token { get; set; } // Token

			public DateTime DateDeleted //don't delete
			{
				get { return DateTime.UtcNow; }
				set { } // keep it here
			} // DateDeleted

			protected override string GetName() {
				return "BrokerLeadCheckToken";
			} // GetName

		} // class SpBrokerLeadCheckToken

	} // class BrokerLeadCheckToken
} // namespace Ezbob.Backend.Strategies.Broker

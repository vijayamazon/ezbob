namespace EzBob.Backend.Strategies.Broker {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BrokerLeadCheckToken : BrokerLeadCanFillWizard {
		#region public

		#region constructor

		public BrokerLeadCheckToken(string sToken, AConnection oDB, ASafeLog oLog) : base(0, "", "", oDB, oLog) {
			m_sToken = sToken;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Broker check lead token"; }
		} // Name

		#endregion property Name

		#endregion public

		#region protected

		#region method Init

		protected override void Init() {
			ResultRow = null;

			StoredProc = new SpBrokerLeadCheckToken(DB, Log) {
				Token = new Guid(m_sToken),
			};
		} // Init

		#endregion method Init

		#endregion protected

		#region private

		private readonly string m_sToken;

		#region class SpBrokerLeadCheckToken

		private class SpBrokerLeadCheckToken : AStoredProcedure {
			#region constructor

			public SpBrokerLeadCheckToken(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			#endregion constructor

			#region method HasValidParameters

			public override bool HasValidParameters() {
				return (Token != null) && (Token != Guid.Empty);
			} // HasValidParameters

			#endregion method HasValidParameters

			#region property Token

			public Guid Token { get; set; } // Token

			#endregion property Token

			#region method GetName

			protected override string GetName() {
				return "BrokerLeadCheckToken";
			} // GetName

			#endregion method GetName
		} // class SpBrokerLeadCheckToken

		#endregion class SpBrokerLeadCheckToken

		#endregion private
	} // class BrokerLeadCheckToken
} // namespace EzBob.Backend.Strategies.Broker

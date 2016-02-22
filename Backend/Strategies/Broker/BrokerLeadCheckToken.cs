namespace Ezbob.Backend.Strategies.Broker {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class BrokerLeadCheckToken : BrokerLeadCanFillWizard {
		public BrokerLeadCheckToken(string sToken) {
			this.token = sToken;
		} // constructor

		public override string Name {
			get { return "Broker check lead token"; }
		} // Name

		protected override void Init() {
			ResultRow = null;

			StoredProc = new SpBrokerLeadCheckToken(DB, Log) {
				Token = new Guid(this.token),
			};
		} // Init

		private readonly string token;

		private class SpBrokerLeadCheckToken : AStoredProc {
			public SpBrokerLeadCheckToken(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {}

			public override bool HasValidParameters() {
				return (Token != Guid.Empty);
			} // HasValidParameters

			[UsedImplicitly]
			public Guid Token { get; set; }

			[UsedImplicitly]
			public DateTime DateDeleted {
				get { return DateTime.UtcNow; }
				// ReSharper disable once ValueParameterNotUsed
				set { }
			} // DateDeleted
		} // class SpBrokerLeadCheckToken
	} // class BrokerLeadCheckToken
} // namespace Ezbob.Backend.Strategies.Broker

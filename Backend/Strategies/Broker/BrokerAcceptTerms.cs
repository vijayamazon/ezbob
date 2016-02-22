namespace Ezbob.Backend.Strategies.Broker {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database;
	using JetBrains.Annotations;

	public class BrokerAcceptTerms : AStrategy {
		public BrokerAcceptTerms(int nTermsID, string sContactEmail, CustomerOriginEnum origin) {
			m_oSp = new SpBrokerAcceptTerms(nTermsID, sContactEmail, origin, DB, Log);
		} // constructor

		public override string Name {
			get { return "BrokerAcceptTerms"; }
		} // Name

		public override void Execute() {
			m_oSp.ExecuteNonQuery();
		} // Execute

		private readonly SpBrokerAcceptTerms m_oSp;

		private class SpBrokerAcceptTerms : AStoredProc {
			public SpBrokerAcceptTerms(
				int nTermsID,
				string sContactEmail,
				CustomerOriginEnum origin,
				AConnection oDB,
				ASafeLog oLog
			) : base(oDB, oLog) {
				TermsID = nTermsID;
				OriginID = (int)origin;
				ContactEmail = sContactEmail;
			} // constructor

			public override bool HasValidParameters() {
				return (TermsID > 0) && !string.IsNullOrWhiteSpace(ContactEmail);
			} // HasValidParameters

			[UsedImplicitly]
			public int TermsID { get; set; }

			[UsedImplicitly]
			public string ContactEmail { get; set; }

			[UsedImplicitly]
			public int OriginID { get; set; }

			[UsedImplicitly]
			public DateTime Now {
				get { return DateTime.UtcNow; }
				// ReSharper disable ValueParameterNotUsed
				set { }
				// ReSharper restore ValueParameterNotUsed
			} // Now
		} // class SpBrokerAcceptTerms
	} // class BrokerAcceptTerms
} // namespace

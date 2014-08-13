namespace EzBob.Backend.Strategies.Broker {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class BrokerAcceptTerms : AStrategy {
		#region public

		#region constructor

		public BrokerAcceptTerms(int nTermsID, string sContactEmail, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_oSp = new SpBrokerAcceptTerms(nTermsID, sContactEmail, DB, Log);
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "BrokerAcceptTerms"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			m_oSp.ExecuteNonQuery();
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly SpBrokerAcceptTerms m_oSp;

		#region class SpBrokerAcceptTerms

		private class SpBrokerAcceptTerms : AStoredProc {
			public SpBrokerAcceptTerms(int nTermsID, string sContactEmail, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				TermsID = nTermsID;
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
			public DateTime Now {
				get { return DateTime.UtcNow; }
				// ReSharper disable ValueParameterNotUsed
				set { }
				// ReSharper restore ValueParameterNotUsed
			} // Now
		} // class SpBrokerAcceptTerms

		#endregion class SpBrokerAcceptTerms

		#endregion private
	} // class BrokerAcceptTerms
} // namespace

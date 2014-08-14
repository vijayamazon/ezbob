namespace EzBob.Backend.Strategies.Broker {
	using System;
	using System.Globalization;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BrokerLoadSignedTerms : AStrategy {
		#region public

		#region constructor

		public BrokerLoadSignedTerms(string sContactEmail, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			Terms = string.Empty;
			SignedTime = string.Empty;

			m_sContactEmail = sContactEmail;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "BrokerLoadSignedTerms"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			SafeReader sr = DB.GetFirst(
				"BrokerLoadSignedTerms",
				CommandSpecies.StoredProcedure,
				new QueryParameter("ContactEmail", m_sContactEmail)
			);

			if ((string)sr["ContactEmail"] == m_sContactEmail) {
				Terms = sr["Terms"];
				DateTime oTime = sr["SignedTime"];

				SignedTime = oTime.ToLocalTime().ToString("MMM d yyyy H:mm:ss", new CultureInfo("en-GB", false));
			} // if
		} // Execute

		#endregion method Execute

		#region property Terms

		public string Terms { get; private set; }

		#endregion property Terms

		#region property SignedTime

		public string SignedTime { get; private set; }

		#endregion property SignedTime

		#endregion public

		#region private

		private readonly string m_sContactEmail;

		#endregion private
	} // class BrokerLoadSignedTerms
} // namespace

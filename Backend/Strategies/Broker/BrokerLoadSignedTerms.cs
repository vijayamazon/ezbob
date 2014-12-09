namespace EzBob.Backend.Strategies.Broker {
	using System;
	using System.Globalization;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BrokerLoadSignedTerms : AStrategy {

		public BrokerLoadSignedTerms(string sContactEmail, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			Terms = string.Empty;
			SignedTime = string.Empty;

			m_sContactEmail = sContactEmail;
		} // constructor

		public override string Name {
			get { return "BrokerLoadSignedTerms"; }
		} // Name

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

		public string Terms { get; private set; }

		public string SignedTime { get; private set; }

		private readonly string m_sContactEmail;

	} // class BrokerLoadSignedTerms
} // namespace

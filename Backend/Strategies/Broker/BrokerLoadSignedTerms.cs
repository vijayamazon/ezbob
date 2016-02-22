namespace Ezbob.Backend.Strategies.Broker {
	using System;
	using System.Globalization;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database;

	public class BrokerLoadSignedTerms : AStrategy {
		public BrokerLoadSignedTerms(string sContactEmail, CustomerOriginEnum origin) {
			Terms = string.Empty;
			SignedTime = string.Empty;

			this.contactEmail = sContactEmail;
			this.origin = (int)origin;
		} // constructor

		public override string Name {
			get { return "BrokerLoadSignedTerms"; }
		} // Name

		public override void Execute() {
			SafeReader sr = DB.GetFirst(
				"BrokerLoadSignedTerms",
				CommandSpecies.StoredProcedure,
				new QueryParameter("ContactEmail", this.contactEmail),
				new QueryParameter("Origin", this.origin)
			);

			if ((string)sr["ContactEmail"] == this.contactEmail) {
				Terms = sr["Terms"];
				DateTime oTime = sr["SignedTime"];

				SignedTime = oTime.ToLocalTime().ToString("MMM d yyyy H:mm:ss", new CultureInfo("en-GB", false));
			} // if
		} // Execute

		public string Terms { get; private set; }

		public string SignedTime { get; private set; }

		private readonly string contactEmail;
		private readonly int origin;
	} // class BrokerLoadSignedTerms
} // namespace

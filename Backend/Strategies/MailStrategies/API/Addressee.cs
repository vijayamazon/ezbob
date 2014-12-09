namespace EzBob.Backend.Strategies.MailStrategies.API {
	public sealed class Addressee {

		public Addressee(string sRecipient = "", string sCarbonCopy = "", bool bShouldRegister = true) {
			Recipient = sRecipient;
			CarbonCopy = sCarbonCopy;
			ShouldRegister = bShouldRegister;
		} // constructor

		public string Recipient {
			get { return m_sRecipient; } // get
			set { m_sRecipient = Normalise(value); } // set
		} // Recipient

		private string m_sRecipient;

		public string CarbonCopy {
			get { return m_sCarbonCopy; } // get
			set { m_sCarbonCopy = Normalise(value); } // set
		} // CarbonCopy

		private string m_sCarbonCopy;

		public bool ShouldRegister { get; set; } // ShouldRegister

		public bool IsValid {
			get { return (Recipient != string.Empty) || (CarbonCopy != string.Empty); } // get
		} // IsValid

		private string Normalise(string v) {
			return string.IsNullOrWhiteSpace(v) ? string.Empty : v.Trim();
		} // Normalise

	} // class Addressee
} // namespace EzBob.Backend.Strategies.MailStrategies.API

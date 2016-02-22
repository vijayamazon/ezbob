namespace Ezbob.Backend.Strategies.MailStrategies.API {
	public sealed class Addressee {
		public Addressee(
			string sRecipient = "",
			string sCarbonCopy = "",
			bool bShouldRegister = true,
			int? userID = null,
			bool isBroker = false,
			string origin = "",
			bool addSalesforceActivity = true
		) {
			Recipient = sRecipient;
			CarbonCopy = sCarbonCopy;
			ShouldRegister = bShouldRegister;
			UserID = userID;
			IsBroker = isBroker;
			AddSalesforceActivity = addSalesforceActivity;
			Origin = origin;
		}// constructor

		public bool IsBroker { get; private set; }
		public bool AddSalesforceActivity { get; private set; }

		public string Origin {
			get { return m_sOrigin; } // get
			set { m_sOrigin = Normalise(value); } // set
		} // Origin

		private string m_sOrigin;

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
		public int? UserID { get; set; }

		public bool IsValid {
			get { return (Recipient != string.Empty) || (CarbonCopy != string.Empty); } // get
		} // IsValid

		private string Normalise(string v) {
			return string.IsNullOrWhiteSpace(v) ? string.Empty : v.Trim();
		} // Normalise
	} // class Addressee
} // namespace Ezbob.Backend.Strategies.MailStrategies.API

namespace EzBob.Backend.Strategies.MailStrategies.API {
	public sealed class Addressee {
		#region public

		#region constructor

		public Addressee(string sRecipient = "", string sCarbonCopy = "", bool bShouldRegister = true) {
			Recipient = sRecipient;
			CarbonCopy = sCarbonCopy;
			ShouldRegister = bShouldRegister;
		} // constructor

		#endregion constructor

		#region property Recipient

		public string Recipient {
			get { return m_sRecipient; } // get
			set { m_sRecipient = Normalise(value); } // set
		} // Recipient

		private string m_sRecipient;

		#endregion property Recipient

		#region property CarbonCopy

		public string CarbonCopy {
			get { return m_sCarbonCopy; } // get
			set { m_sCarbonCopy = Normalise(value); } // set
		} // CarbonCopy

		private string m_sCarbonCopy;

		#endregion property CarbonCopy

		#region property ShouldRegister

		public bool ShouldRegister { get; set; } // ShouldRegister

		#endregion property ShouldRegister

		#region property IsValid

		public bool IsValid {
			get { return (Recipient != string.Empty) || (CarbonCopy != string.Empty); } // get
		} // IsValid

		#endregion property IsValid

		#endregion public

		#region private

		private string Normalise(string v) {
			return string.IsNullOrWhiteSpace(v) ? string.Empty : v.Trim();
		} // Normalise

		#endregion private
	} // class Addressee
} // namespace EzBob.Backend.Strategies.MailStrategies.API

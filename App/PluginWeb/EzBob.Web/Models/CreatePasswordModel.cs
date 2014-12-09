namespace EzBob.Web.Models {
	using System;

	public class CreatePasswordModel : LogOnModel {

		public CreatePasswordModel() {
			Token = Guid.Empty;
			BrokerLeadStr = "no";
		} // constructor

		public bool IsBrokerLead {
			get { return BrokerLeadStr == "yes"; }
		} // IsBrokerLead

		public string BrokerLeadStr { get; set; } // BrokerLeadStr

		public string FirstName {
			get { return m_sFirstName; }
			set { m_sFirstName = (value ?? string.Empty).Trim(); }
		} // FirstName

		private string m_sFirstName;

		public string LastName {
			get { return m_sLastName; }
			set { m_sLastName = (value ?? string.Empty).Trim(); }
		} // LastName

		private string m_sLastName;

		public string FullName {
			get { return (FirstName + " " + LastName).Trim(); }
		} // FullName

		public string RawToken {
			get { return m_sRawToken; }
			set {
				m_sRawToken = value;

				try {
					Token = new Guid(m_sRawToken);
				}
				catch (Exception) {
					Token = Guid.Empty;
				} // try
			} // set
		} // RawToken

		private string m_sRawToken;

		public Guid Token { get; private set; }

		public bool IsTokenValid {
			get { return Token != Guid.Empty; }
		} // IsTokenValid

		public string signupPass2 { get; set; } // RawToken

	} // CreatePasswordModel
} // namespace

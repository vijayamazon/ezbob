namespace EzBob.Web.Models {
	using System;

	public class CreatePasswordModel : LogOnModel {
		#region constructor

		public CreatePasswordModel() {
			Token = Guid.Empty;
			BrokerLeadStr = "no";
		} // constructor

		#endregion constructor

		#region property IsBrokerLead

		public bool IsBrokerLead {
			get { return BrokerLeadStr == "yes"; }
		} // IsBrokerLead

		#endregion property IsBrokerLead

		#region property BrokerLeadStr

		public string BrokerLeadStr { get; set; } // BrokerLeadStr

		#endregion property BrokerLeadStr

		#region property FirstName

		public string FirstName {
			get { return m_sFirstName; }
			set { m_sFirstName = (value ?? string.Empty).Trim(); }
		} // FirstName

		private string m_sFirstName;

		#endregion property FirstName

		#region property LastName

		public string LastName {
			get { return m_sLastName; }
			set { m_sLastName = (value ?? string.Empty).Trim(); }
		} // LastName

		private string m_sLastName;

		#endregion property LastName

		#region property FullName

		public string FullName {
			get { return (FirstName + " " + LastName).Trim(); }
		} // FullName

		#endregion property FullName

		#region property RawToken

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

		#endregion property RawToken

		#region property Token

		public Guid Token { get; private set; }

		#endregion property Token

		#region property IsTokenValid

		public bool IsTokenValid {
			get { return Token != Guid.Empty; }
		} // IsTokenValid

		#endregion property IsTokenValid

		#region property signupPass2

		public string signupPass2 { get; set; } // RawToken

		#endregion property signupPass2
	} // CreatePasswordModel
} // namespace

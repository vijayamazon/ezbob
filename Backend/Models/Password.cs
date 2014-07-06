namespace Ezbob.Backend.Models {
	using System.Runtime.Serialization;
	using Utils.Security;

	[DataContract]
	public class Password {
		#region constructor

		public Password() : this(null, null) {
		} // constructor

		public Password(string sPrimary) : this(sPrimary, null) {
		} // constructor

		public Password(string sPrimary, string sConfirmation) {
			m_sPrimary = null;
			m_sConfirmation = null;

			if (!string.IsNullOrWhiteSpace(sPrimary))
				m_sPrimary =  new Encrypted(sPrimary);

			if (!string.IsNullOrWhiteSpace(sConfirmation))
				m_sConfirmation = new Encrypted(sConfirmation);
		} // constructor

		#endregion constructor

		#region property Primary

		public string Primary {
			get {
				return string.IsNullOrWhiteSpace(m_sPrimary) ? string.Empty : Encrypted.Decrypt(m_sPrimary);
			} // get
		} // Primary

		[DataMember]
		private string m_sPrimary;

		#endregion property Primary

		#region property Confirmation

		public string Confirmation {
			get {
				return string.IsNullOrWhiteSpace(m_sConfirmation) ? string.Empty : Encrypted.Decrypt(m_sConfirmation);
			} // get
		} // Confirmation

		[DataMember]
		private string m_sConfirmation;

		#endregion property Confirmation

		#region method ToString

		public override string ToString() {
			bool bHasPrimary = !string.IsNullOrWhiteSpace(Primary);
			bool bHasConfirm = !string.IsNullOrWhiteSpace(Confirmation);

			if (bHasConfirm && bHasPrimary)
				return "x*";

			if (bHasConfirm)
				return "x";

			return bHasPrimary ? "*" : "";
		} // ToString

		#endregion method ToString
	} // class Password
} // namespace

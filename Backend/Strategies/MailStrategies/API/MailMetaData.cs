namespace EzBob.Backend.Strategies.MailStrategies.API {
	using System.Collections;
	using System.Collections.Generic;

	public sealed class MailMetaData : IEnumerable {
		#region public

		#region operator cast to Dictionary

		public static implicit operator Dictionary<string, string>(MailMetaData oSrc) {
			return oSrc.m_oVariables;
		} // operator cast to Dictionary

		#endregion operator cast to Dictionary

		#region constructor

		public MailMetaData(string sTemplateName = "") {
			TemplateName = sTemplateName;
			m_oVariables = new Dictionary<string, string>();
			m_oAddressees = new List<Addressee>();
		} // constructor

		#endregion constructor

		#region method Add (variable)

		public void Add(string sVarName, string sVarValue) {
			string sKey = Normalise(sVarName);

			if (sKey != string.Empty)
				m_oVariables.Add(sKey, sVarValue ?? string.Empty);
		} // Add

		#endregion method Add (variable)

		#region method Add (addressee)

		public void Add(Addressee addr) {
			if (addr == null)
				return;

			if (!addr.IsValid)
				return;

			m_oAddressees.Add(addr);
		} // Add

		#endregion method Add (addressee)

		#region property TemplateName

		public string TemplateName {
			get { return m_sTemplateName; } // get
			set { m_sTemplateName = Normalise(value); } // set
		} // TemplateName

		private string m_sTemplateName;

		#endregion property TemplateName

		#region method GetEnumerator

		public IEnumerator GetEnumerator() {
			return m_oAddressees.GetEnumerator();
		} // GetEnumerator

		#endregion method GetEnumerator

		#endregion public

		#region private

		private readonly Dictionary<string, string> m_oVariables;
		private readonly List<Addressee> m_oAddressees;

		#region method Normalise

		private string Normalise(string v) {
			return string.IsNullOrWhiteSpace(v) ? string.Empty : v.Trim();
		} // Normalise

		#endregion method Normalise

		#endregion private
	} // class MailMetaData
} // namespace EzBob.Backend.Strategies.MailStrategies.API

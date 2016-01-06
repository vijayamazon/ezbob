namespace Ezbob.Backend.Strategies.MailStrategies.API {
	using System.Collections;
	using System.Collections.Generic;

	public sealed class MailMetaData : IEnumerable {
		public static implicit operator Dictionary<string, string>(MailMetaData oSrc) {
			return oSrc.m_oVariables;
		} // operator cast to Dictionary

		public MailMetaData(string sTemplateName = "") {
			TemplateName = sTemplateName;
			m_oVariables = new Dictionary<string, string>();
			m_oAddressees = new List<Addressee>();
		} // constructor

		public void Add(string sVarName, string sVarValue) {
			string sKey = Normalise(sVarName);

			if (sKey != string.Empty)
				m_oVariables.Add(sKey, sVarValue ?? string.Empty);
		} // Add

		public void Add(Addressee addr) {
			if (addr == null)
				return;

			if (!addr.IsValid)
				return;

			m_oAddressees.Add(addr);
		} // Add

		public string TemplateName {
			get { return m_sTemplateName; } // get
			set { m_sTemplateName = Normalise(value); } // set
		} // TemplateName

		private string m_sTemplateName;

		public IEnumerator GetEnumerator() {
			return m_oAddressees.GetEnumerator();
		} // GetEnumerator

		private readonly Dictionary<string, string> m_oVariables;
		private readonly List<Addressee> m_oAddressees;

		private string Normalise(string v) {
			return string.IsNullOrWhiteSpace(v) ? string.Empty : v.Trim();
		} // Normalise
	} // class MailMetaData
} // namespace Ezbob.Backend.Strategies.MailStrategies.API

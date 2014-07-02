namespace EzBob.Backend.Strategies.Experian {
	using System;
	using System.Collections.Generic;
	using System.Xml;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.ExperianParser;
	using Ezbob.Logger;

	public class UpdateExperianDirectors : AStrategy {
		#region public

		#region constructor

		public UpdateExperianDirectors(int nCustomerID, string sExperianXml, bool bIsLimited, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_nCustomerID = nCustomerID;
			m_sExperianXml = sExperianXml;
			m_bIsLimited = bIsLimited;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "UpdateExperianDirectors"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			if ((m_nCustomerID <= 0) || string.IsNullOrWhiteSpace(m_sExperianXml)) {
				Log.Warn(
					"Cannot update Experian directors from input parameters: " +
					"customer id = {0}, is limited = {1}, XML = {2}",
					m_nCustomerID, m_bIsLimited, m_sExperianXml
				);
				return;
			} // if

			Log.Debug("Updating Experian directors for customer {0}, is limited {1}...", m_nCustomerID, m_bIsLimited ? "yes" : "no");

			var doc = new XmlDocument();

			try {
				doc.LoadXml(m_sExperianXml);
			}
			catch (Exception e) {
				Log.Warn(e, "Updating Experian directors for customer {0}, is limited {1} failed.", m_nCustomerID, m_bIsLimited ? "yes" : "no");
				return;
			} // try

			var nVar = m_bIsLimited
				? Variables.DirectorDetailsParserConfiguration
				: Variables.DirectorDetailsNonLimitedParserConfiguration;

			var parser = new Parser(CurrentValues.Instance[nVar], Log);

			Dictionary<string, ParsedData> oParsed;

			try {
				oParsed = parser.NamedParse(doc);
			}
			catch (Exception e) {
				Log.Warn(e, "Updating Experian directors for customer {0}, is limited {1} failed.", m_nCustomerID, m_bIsLimited ? "yes" : "no");
				return;
			} // try

			Log.Debug("{0} parsed data - begin:", m_bIsLimited ? "Limited" : "Non-limited");

			foreach (KeyValuePair<string, ParsedData> pair in oParsed) {
				string sDataName = pair.Key;
				ParsedData oData = pair.Value;

				foreach (ParsedDataItem oItem in oData.Data) {
					Log.Debug("Parsed data item - begin:");

					foreach (KeyValuePair<string, string> x in oItem.Values)
						Log.Debug("d: {0} grp: {1} {2} = {3}", sDataName, oData.GroupName, x.Key, x.Value);

					Log.Debug("Parsed data item - end.");
				} // for each item
			} // for each

			Log.Debug("{0} parsed data - end.", m_bIsLimited ? "Limited" : "Non-limited");

			Log.Debug("Updating Experian directors for customer {0}, is limited {1} complete.", m_nCustomerID, m_bIsLimited ? "yes" : "no");
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly int m_nCustomerID;
		private readonly string m_sExperianXml;
		private readonly bool m_bIsLimited;

		#endregion private
	} // class UpdateExperianDirectors
} // namespace

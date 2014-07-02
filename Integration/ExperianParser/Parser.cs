namespace Ezbob.ExperianParser {
	using System.Collections.Generic;
	using System.IO;
	using System.Xml;
	using Ezbob.Logger;
	using Newtonsoft.Json;
	using ConfigManager;

	public class Parser : SafeLog {
		#region public

		#region constructor

		public Parser(string sConfiguration = "", ASafeLog log = null) : base(log) {
			m_oGroups = null;

			if (!string.IsNullOrWhiteSpace(sConfiguration))
				InitConfiguration(sConfiguration);
		} // constructor

		#endregion constructor

		#region method LoadConfiguration

		public void LoadConfiguration(string sFileName) {
			Info("Experian parser: request to load configuration from {0}", sFileName ?? "--null--");

			InitConfiguration(ReadFile(sFileName, "configuration file"));

			Info("Experian parser: loading configuration from {0} complete.", sFileName);
		} // LoadConfiguration

		#endregion method LoadConfiguration

		#region method NamedParse

		public Dictionary<string, ParsedData> NamedParse(string sFileName) {
			Info("Experian parser: request to read data from {0}", sFileName ?? "--null--");

			var doc = new XmlDocument();

			doc.LoadXml(ReadFile(sFileName, "data file"));

			return NamedParse(doc.DocumentElement);
		} // NamedParse

		public Dictionary<string, ParsedData> NamedParse(XmlDocument doc) {
			return NamedParse(doc.DocumentElement);
		} // NamedParse

		public Dictionary<string, ParsedData> NamedParse(XmlNode oRoot) {
			if (oRoot == null)
				throw new OwnException("XML root node is not specified.");
				
			var oResult = new Dictionary<string, ParsedData>();

			m_oGroups.ForEach(grp => {
				var oGroupData = new ParsedData(grp);
				oResult[grp.Name] = oGroupData;

				oGroupData.Parse(oRoot, this);
			});

			return oResult;
		} // NamedParse

		#endregion method NamedParse

		#endregion public

		#region private

		#region method InitConfiguration

		private void InitConfiguration(string sConfiguration) {
			if (string.IsNullOrWhiteSpace(sConfiguration))
				throw new OwnException("Configuration not specified.");

			Debug("Experian parser: parsing configuration...");

			if (m_oGroups != null) {
				m_oGroups.Clear();
				m_oGroups = null;
			} // if

			m_oGroups = JsonConvert.DeserializeObject<List<FieldGroup>>(sConfiguration); 

			m_oGroups.ForEach(g => g.Validate(this));

			if (CurrentValues.Instance.VerboseConfigurationLogging) {
				Debug("\n***\n*** Experian parser: configuration - begin\n***\n");

				m_oGroups.ForEach(g => g.Log(this));

				Debug("\n***\n*** Experian parser: configuration - end\n***\n");
			} // if

			Debug("Experian parser: parsing configuration complete.");
		} // InitConfiguration

		#endregion method InitConfiguration

		#region ReadFile

		private string ReadFile(string sFileName, string sFileTitle) {
			if (string.IsNullOrWhiteSpace(sFileName))
				throw new OwnException("Name of {0} not specified.", sFileTitle);

			if (!File.Exists(sFileName))
				throw new OwnException("Not found {1} {0}.", sFileName, sFileTitle);

			Info("Experian parser: reading from {0}", sFileName);

			string sOutput = File.ReadAllText(sFileName);

			Info("Experian parser: {0} characters read from {1}", sOutput.Length, sFileName);

			return sOutput;
		} // ReadFile

		#endregion ReadFile

		private List<FieldGroup> m_oGroups;

		#endregion private
	} // class Parser
} // namespace Ezbob.ExperianParser

using System.Collections.Generic;

namespace Ezbob.ExperianParser {
	using System.Xml;
	using Logger;

	public class ParsedData {

		public ParsedData(FieldGroup grp = null) {
			Data = new List<ParsedDataItem>();
			MetaData = new SortedDictionary<string, string>();

			m_oFieldGroup = grp;

			if (!ReferenceEquals(grp, null)) {
				GroupName = grp.Name;

				if (grp.MetaData != null)
					foreach (KeyValuePair<string, string> pair in grp.MetaData)
						MetaData[pair.Key] = pair.Value;
			} // if grp is specified
		} // constructor

		public string GroupName { get; set; }

		public SortedDictionary<string, string> MetaData { get; private set; }

		public List<ParsedDataItem> Data { get; private set; }

		public void Parse(XmlNode oRoot, ASafeLog log) {
			if (ReferenceEquals(m_oFieldGroup, null))
				throw new OwnException("Cannot parse: field group is not specified.");

			var oLog = new SafeLog(log);

			oLog.Debug("Parsing group {0}", m_oFieldGroup.Name);

			if (!string.IsNullOrWhiteSpace(m_oFieldGroup.PathToParent)) {
				XmlNodeList lst = oRoot.SelectNodes(m_oFieldGroup.PathToParent);

				if (lst != null) {
					oLog.Debug("{0} nodes found matching PathToParent", lst.Count);

					foreach (XmlNode oNode in lst)
						ParseOne(oNode, oLog);
				} // if nodes found
			} // if path to parent specified
			else
				ParseOne(oRoot, oLog);
		} // Parse

		private void ParseOne(XmlNode oNode, SafeLog oLog) {
			var oData = new ParsedDataItem();

			foreach (KeyValuePair<string, OutputFieldBuilder> pair in m_oFieldGroup.OutputFieldBuilders)
				pair.Value.Clear();

			foreach (Field fld in m_oFieldGroup.Fields) {
				XmlNode oValue = oNode.SelectSingleNode(fld.SourcePath);

				string sValue = (oValue == null) ? null : oValue.InnerText;

				fld.SetValue(sValue);
			} // for each field

			foreach (KeyValuePair<string, OutputFieldBuilder> pair in m_oFieldGroup.OutputFieldBuilders)
				oData[pair.Key] = pair.Value.Build();

			if (m_oFieldGroup.HasChildren()) {
				foreach (var grp in m_oFieldGroup.Children) {
					var oGroupData = new ParsedData(grp);
					oData.Children[grp.Name] = oGroupData;

					oGroupData.Parse(oNode, oLog);
				} // foreach child
			} // if

			Data.Add(oData);
		} // ParseOne

		private readonly FieldGroup m_oFieldGroup;

	} // class ParsedData

} // namespace Ezbob.ExperianParser

using System.Collections.Generic;
using System.Text;
using System.Xml;
using Ezbob.Logger;

namespace Ezbob.ExperianParser {
	#region class FieldGroup

	class FieldGroup {
		#region public

		#region constructor

		public FieldGroup() {
			m_oOutputFieldBuilders = new SortedDictionary<string, OutputFieldBuilder>();
		} // constructor

		#endregion constructor

		#region configuration

		public string Name { get; set; }
		public string PathToParent { get; set; }
		public List<Field> Fields { get; set; }

		#endregion configuration

		#region method Validate

		public void Validate(ASafeLog log) {
			if (string.IsNullOrWhiteSpace(Name))
				throw new OwnException("Field group name not specified.");

			m_oOutputFieldBuilders.Clear();

			PathToParent = (PathToParent ?? "").Trim();

			Fields.ForEach(f => f.Validate(this, log));
		} // Validate

		#endregion method Validate

		#region method Log

		public void Log(ASafeLog log) {
			if (log == null)
				return;

			var sb = new StringBuilder();

			sb.AppendFormat("\nGroup\n\tName: {0}\n\tPath to parent: {1}\n\tFields:\n", Name, PathToParent);

			Fields.ForEach(f => f.Log(sb, "\t\t"));

			log.Debug(sb.ToString());
		} // Log

		#endregion method Log

		#region method Parse

		public void Parse(XmlNode oRoot, List<SortedDictionary<string, string>> oOutput, ASafeLog log) {
			var oLog = new SafeLog(log);

			oLog.Debug("Parsing group {0}", Name);

			if (!string.IsNullOrWhiteSpace(PathToParent)) {
				XmlNodeList lst = oRoot.SelectNodes(PathToParent);

				if (lst != null) {
					oLog.Debug("{0} nodes found matching PathToParent", lst.Count);

					foreach (XmlNode oNode in lst)
						ParseOne(oNode, oOutput);
				} // if nodes found
			} // if path to parent specified
			else
				ParseOne(oRoot, oOutput);
		} // Parse

		#endregion method Parse

		#region method AddOutputFieldBuilder

		public void AddOutputFieldBuilder(Target oTarget) {
			if (m_oOutputFieldBuilders.ContainsKey(oTarget.Name))
				m_oOutputFieldBuilders[oTarget.Name].Add(oTarget);
			else
				m_oOutputFieldBuilders[oTarget.Name] = new OutputFieldBuilder(oTarget);
		} // AddOutputFieldBuilder

		#endregion method AddOutputFieldBuilder

		#endregion public

		#region private

		#region method ParseOne

		private void ParseOne(XmlNode oNode, List<SortedDictionary<string, string>> oOutput) {
			var oData = new SortedDictionary<string, string>();

			foreach (KeyValuePair<string, OutputFieldBuilder> pair in m_oOutputFieldBuilders)
				pair.Value.Clear();

			foreach (Field fld in Fields) {
				XmlNode oValue = oNode.SelectSingleNode(fld.SourcePath);

				string sValue = (oValue == null) ? null : oValue.InnerText;

				fld.SetValue(sValue);
			} // for each field

			foreach (KeyValuePair<string, OutputFieldBuilder> pair in m_oOutputFieldBuilders)
				oData[pair.Key] = pair.Value.Build();

			oOutput.Add(oData);
		} // ParseOne

		#endregion method ParseOne

		private readonly SortedDictionary<string, OutputFieldBuilder> m_oOutputFieldBuilders;

		#endregion private
	} // class FieldGroup

	#endregion class FieldGroup
} // namespace Ezbob.ExperianParser

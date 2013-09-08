using System;
using System.Collections.Generic;
using System.Text;
using Ezbob.Logger;

namespace Ezbob.ExperianParser {
	#region class Transformation

	class Transformation {
		#region public

		#region constructor

		public Transformation() {
			m_oMap = new SortedDictionary<string, string>();
		} // constructor

		#endregion constructor

		public List<string> Types { get; set; }
		public List<TransformationMapEntry> Map { get; set; }

		#region method Validate

		public void Validate(ASafeLog log) {
			if (Types == null)
				Types = new List<string>();

			var oTypes = new List<string>();

			foreach (string sType in Types) {
				if (string.IsNullOrWhiteSpace(sType))
					throw new OwnException("Transformation type not specified.");

				string t = sType.Trim().ToLower();

				switch (t) {
				case "map":
					break;

				default:
					throw new OwnException("Unsupported transformation type: {0}", t);
				} // switch

				oTypes.Add(t);
			} // for each type

			Types = oTypes;

			m_oMap.Clear();

			Map.ForEach(e => {
				e.Validate(log);

				if ((log != null) && m_oMap.ContainsKey(e.From)) {
					log.Warn(
						"Duplicate map key, the latter will prevail.\n\tThe key: {0}\n\tFormer: {1}\n\tLatter: {2}",
						e.From, m_oMap[e.From], e.To
					);
				} // if

				m_oMap[e.From] = e.To;
			});
		} // Validate

		#endregion method Validate

		#region method Log

		public void Log(StringBuilder sb, string sLinePrefix) {
			sb.AppendFormat("{0}Types: {1}\n", sLinePrefix, string.Join(", ", Types));

			if (m_oMap.Count > 0) {
				foreach (KeyValuePair<string, string> pair in m_oMap)
					sb.AppendFormat("{0}\t{1} --> {2}\n", sLinePrefix, pair.Key, pair.Value);
			} // if
		} // Log

		#endregion method Log

		#region method Apply

		public string Apply(string sValue) {
			string sResult = sValue;

			foreach (string sType in Types) {
				switch (sType) {
				case "map":
					if (m_oMap.ContainsKey(sResult))
						sResult = m_oMap[sResult];
					break;
				} // switch
			} // for each type
			return sResult;
		} // Apply

		#endregion method Apply

		#endregion public

		#region private

		private readonly SortedDictionary<string, string> m_oMap;

		#endregion private
	} // class Transformation

	#endregion class Transformation
} // namespace Ezbob.ExperianParser

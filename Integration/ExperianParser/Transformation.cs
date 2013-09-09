using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Ezbob.Logger;

namespace Ezbob.ExperianParser {
	#region enum TransformationType

	enum TransformationType {
		None,
		Map,
		MonthName,
	} // TransformationType

	#endregion enum TransformationType

	#region class Transformation

	class Transformation {
		#region public

		#region constructor

		public Transformation() {
			m_oTypes = new List<TransformationType>();
		} // constructor

		#endregion constructor

		public List<string> Types { get; set; }
		public SortedDictionary<string, string> Map { get; set; }

		#region method Validate

		public void Validate(ASafeLog log) {
			if (Types == null)
				Types = new List<string>();

			m_oTypes.Clear();

			foreach (string sType in Types) {
				if (string.IsNullOrWhiteSpace(sType))
					throw new OwnException("Transformation type not specified.");

				TransformationType nType = TransformationType.None;

				if (!TransformationType.TryParse(sType.Trim(), true, out nType))
					throw new OwnException("Unsupported transformation type: {0}", sType);

				m_oTypes.Add(nType);
			} // for each type
		} // Validate

		#endregion method Validate

		#region method Log

		public void Log(StringBuilder sb, string sLinePrefix) {
			sb.AppendFormat("{0}Types: {1}\n", sLinePrefix, string.Join(", ", Types));

			if (Map != null) {
				foreach (KeyValuePair<string, string> pair in Map)
					sb.AppendFormat("{0}\t{1} --> {2}\n", sLinePrefix, pair.Key, pair.Value);
			} // if
		} // Log

		#endregion method Log

		#region method Apply

		public string Apply(string sValue) {
			string sResult = sValue;

			foreach (TransformationType nType in m_oTypes) {
				switch (nType) {
				case TransformationType.Map:
					if ((Map != null) && Map.ContainsKey(sResult))
						sResult = Map[sResult];
					break;

				case TransformationType.MonthName:
					DateTime oDate = DateTime.Today;

					if (DateTime.TryParseExact("1976-" + sResult + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out oDate))
						sResult = oDate.ToString("MMMM", CultureInfo.InvariantCulture);

					break;
				} // switch
			} // for each type
			return sResult;
		} // Apply

		#endregion method Apply

		#endregion public

		#region private

		private readonly List<TransformationType> m_oTypes;

		#endregion private
	} // class Transformation

	#endregion class Transformation
} // namespace Ezbob.ExperianParser

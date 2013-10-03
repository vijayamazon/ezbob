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
		Money,
		MonthsAndYears,
		Shares
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
			string sResult = sValue ?? string.Empty;

			foreach (TransformationType nType in m_oTypes) {
				switch (nType) {
				case TransformationType.Map:
					if ((Map != null) && Map.ContainsKey(sResult))
						sResult = Map[sResult];
					break;

				case TransformationType.MonthName:
					DateTime oDate = DateTime.Today;

					if (DateTime.TryParseExact("1976-" + sResult + "-01", "yyyy-MM-dd", Culture, DateTimeStyles.None, out oDate))
						sResult = oDate.ToString("MMMM", Culture);

					break;

				case TransformationType.Money: {
					decimal nValue;

					if (decimal.TryParse(sResult, NumberStyles.Currency, Culture, out nValue))
						sResult = nValue.ToString("C2", Culture);

					} break;

				case TransformationType.MonthsAndYears: {
					decimal nValue;

					if (decimal.TryParse(sResult, NumberStyles.Any, Culture, out nValue)) {
						decimal nYears = Math.Floor(nValue / 12);
						decimal nMonths = nValue - nYears * 12;

						var os = new StringBuilder();
						if (nYears > 0)
							os.AppendFormat("{0} year{1}", nYears, nYears == 1 ? "" : "s");

						if (nMonths > 0)
							os.AppendFormat(" {0} month{1}", nMonths, nMonths == 1 ? "" : "s");

						sResult = os.ToString().Trim();
					} // if

					} break;

				case TransformationType.Shares: {
					decimal nValue = 0;
					bool bFound = false;

					foreach (char c in sResult) {
						bool bGood = false;

						switch (c) {
						case '0':
						case '1':
						case '2':
						case '3':
						case '4':
						case '5':
						case '6':
						case '7':
						case '8':
						case '9':
							bFound = true;
							nValue = nValue * 10m + (c - '0');
							goto case ' '; // !!! fall through !!!

						case ' ':
						case '\t':
							bGood = true;
							break;
						} // switch

						if (!bGood)
							break;
					} // for each char

					if (bFound)
						sResult = nValue.ToString();

					} break;
				} // switch

				if (sResult == null)
					sResult = string.Empty;
			} // for each type
			return sResult;
		} // Apply

		#endregion method Apply

		#endregion public

		#region private

		private readonly List<TransformationType> m_oTypes;

		#region property Culture

		private static CultureInfo Culture {
			get {
				if (ms_oCulture == null)
					ms_oCulture = new CultureInfo("en-GB", false);

				return ms_oCulture;
			} // get
		} // Culture

		private static CultureInfo ms_oCulture;

		#endregion property Culture

		#endregion private
	} // class Transformation

	#endregion class Transformation
} // namespace Ezbob.ExperianParser

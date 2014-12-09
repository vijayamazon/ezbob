using System;
using System.Data;
using System.Globalization;

namespace Reports {

	public abstract class Extractor {

		protected Extractor(IDataRecord oRow) {
			m_oRow = oRow;
		} // constructor

		protected string Retrieve(string sFieldName) {
			string sVal = Convert.ToString(Get(sFieldName)).Trim();

			if (string.IsNullOrEmpty(sVal))
				return null;

			return sVal;
		} // Retrieve

		protected T? Retrieve<T>(string sFieldName) where T: struct {
			object val = Get(sFieldName);

			if (val == null)
				return null;

			return (T)Convert.ChangeType(val, typeof (T));
		} // Retrieve

		protected static string Value<T>(T sVal) where T: class {
			return ReferenceEquals(sVal, null) ? "-- null --" : sVal.ToString();
		} // Value

		protected static string Value(DateTime? oVal) {
			return oVal.HasValue ? oVal.Value.ToString("MMM d yyyy", CultureInfo.InvariantCulture) : "-- null --";
		} // Value

		protected static string Value(int? oVal) {
			return oVal.HasValue ? oVal.Value.ToString() : "-- null --";
		} // Value

		protected static string Value(DateTime oVal) {
			return oVal.ToString("MMM d yyyy H:mm:ss", CultureInfo.InvariantCulture);
		} // Value

		private object Get(string sFieldName) {
			object val = m_oRow[sFieldName];

			if ((val == null) || Convert.IsDBNull(val))
				return null;

			return val;
		} // Get

		private readonly IDataRecord m_oRow;

	} // class Extractor

} // namespace Reports

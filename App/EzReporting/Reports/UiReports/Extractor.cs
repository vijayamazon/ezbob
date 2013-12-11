using System;
using System.Data;
using System.Globalization;

namespace Reports {
	#region class Extractor

	public abstract class Extractor {
		#region protected

		#region constructor

		protected Extractor(IDataRecord oRow) {
			m_oRow = oRow;
		} // constructor

		#endregion constructor

		#region method Retrieve

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

		#endregion method Retrieve

		#region method Value

		protected static string Value(string sVal) {
			return sVal ?? "-- null --";
		} // Value

		protected static string Value(DateTime? oVal) {
			return oVal.HasValue ? oVal.Value.ToString("MMM d yyyy", CultureInfo.InvariantCulture) : "-- null --";
		} // Value

		protected static string Value(DateTime oVal) {
			return oVal.ToString("MMM d yyyy H:mm:ss", CultureInfo.InvariantCulture);
		} // Value

		protected static string Value(int? oVal) {
			return oVal.HasValue ? oVal.Value.ToString() : "-- null --";
		} // Value

		#endregion method Value

		#endregion protected

		#region private

		#region method Get

		private object Get(string sFieldName) {
			object val = m_oRow[sFieldName];

			if ((val == null) || Convert.IsDBNull(val))
				return null;

			return val;
		} // Get

		#endregion method Get

		private readonly IDataRecord m_oRow;

		#endregion private
	} // class Extractor

	#endregion class Extractor
} // namespace Reports

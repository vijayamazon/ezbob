namespace Ezbob.Utils {
	using System;

	public class SafeParser {
		public int GetInt(object obj, int defaultValue) {
			if (ReferenceEquals(obj, null))
				return defaultValue;

			int result;
			if (!int.TryParse(obj.ToString(), out result))
				return defaultValue;

			return result;
		}

		public int GetInt(object obj) {
			return GetInt(obj, default(int));
		}

		public decimal GetDecimal(object obj, decimal defaultValue) {
			if (ReferenceEquals(obj, null))
				return defaultValue;

			decimal result;
			if (!decimal.TryParse(obj.ToString(), out result))
				return defaultValue;

			return result;
		}

		public decimal GetDecimal(object obj) {
			return GetDecimal(obj, default(decimal));
		}

		public bool GetBool(object obj, bool defaultValue) {
			if (ReferenceEquals(obj, null))
				return defaultValue;

			bool result;
			if (!bool.TryParse(obj.ToString(), out result))
				return defaultValue;

			return result;
		}

		public bool GetBool(object obj) {
			return GetBool(obj, default(bool));
		}

		public DateTime GetDateTime(object obj, DateTime defaultValue) {
			if (ReferenceEquals(obj, null))
				return defaultValue;

			DateTime result;
			if (!DateTime.TryParse(obj.ToString(), out result))
				return defaultValue;

			return result;
		}

		public DateTime GetDateTime(object obj) {
			return GetDateTime(obj, default(DateTime));
		}
	}
}

namespace IMailLib.Helpers {
	using System;

	public static class FormattingHelper {
		public static string ToLongDate(this DateTime? date) {
			return date.HasValue ? date.Value.ToString("dd MMM yyyy") : string.Empty;
		}

		public static string ToLongDate(this DateTime date) {
			return date.ToString("dd MMM yyyy");
		}

		public static string ToLongDateWithDayOfWeek(this DateTime date) {
			return date.ToString("dddd, dd MMM yyyy");
		}

		public static string ToNumeric2Decimals(this decimal dec) {
			return dec.ToString("N2");
		}

		public static string ToNumericNoDecimals(this decimal dec) {
			return dec.ToString("N0");
		}

		public static string ToNumericNoDecimals(this int integer) {
			return integer.ToString("N0");
		}
	}
}

namespace IMailLib.Helpers {
	using System;

	public static class FormattingHelper {
		public static string ToLongDate(this DateTime? date) {
			return date.HasValue ? date.Value.ToString("dd MMM yyyy") : string.Empty;
		}

		public static string ToLongDate(this DateTime date) {
			return date.ToString("dd MMM yyyy");
		}

		public static string ToLongUKDate(this DateTime date) {
			string suffix = "";
			switch (date.Day % 10) {
				case 1:
					suffix = "st";
					break;
				case 2:
					suffix = "nd";
					break;
				case 3:
					suffix = "rd";
					break;
				default:
					suffix = "th";
					break;
			}

			switch (date.Day) {
				case 11:
				case 12:
				case 13:
					suffix = "th";
					break;
			}

			return string.Format("{0}{1} {2}", date.Day, suffix, date.ToString("MMM yyyy"));
		}


		public static string ToLongDateWithDayOfWeek(this DateTime date) {
			return date.ToString("dddd, dd MMM yyyy");
		}

		public static string ToNumeric2Decimals(this decimal dec, bool isPound = false) {
			return (isPound ? "£" : "") + dec.ToString("N2");
		}

		public static string ToNumeric2Decimals(this decimal? dec, bool isPound = false) {
			if (!dec.HasValue) {
				return "";
			}

			return (isPound ? "£" : "") + dec.Value.ToString("N2");
		}

		public static string ToNumericNoDecimals(this decimal dec, bool isPound = false) {
			return (isPound ? "£" : "") + dec.ToString("N0");
		}

		public static string ToNumericNoDecimals(this int integer, bool isPound = false) {
			return (isPound ? "£" : "") + integer.ToString("N0");
		}
	}
}

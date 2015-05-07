namespace Ezbob.Backend.Extensions {
	using System;

	public static class DateTimeExt {
		public static string DateStr(this DateTime dt) {
			return dt.ToString("MMM dd yyyy", Library.Instance.Culture);
		} // DateStr

		public static string TimeStr(this DateTime dt) {
			return dt.ToString("HH:mm:ss", Library.Instance.Culture);
		} // TimeStr

		public static string MomentStr(this DateTime dt) {
			return dt.ToString("MMM dd yyyy H:mm:ss", Library.Instance.Culture);
		} // MomentStr

		public static string DateStr(this DateTime? dt) {
			return dt == null ? "UNSPECIFIED DATE" : dt.Value.DateStr();
		} // DateStr

		public static string TimeStr(this DateTime? dt) {
			return dt == null ? "UNSPECIFIED TIME" : dt.Value.TimeStr();
		} // TimeStr

		public static string MomentStr(this DateTime? dt) {
			return dt == null ? "UNSPECIFIED MOMENT" : dt.Value.MomentStr();
		} // MomentStr
	} // DateTimeExt
} // namespace

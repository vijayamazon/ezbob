namespace Ezbob.Utils {
	using System;
	using System.Globalization;

	#region class Utils

	public static class MiscUtils {
		#region method MD5

		public static string MD5(string input) {
			return Security.SecurityUtils.MD5(input);
		} // MD5

		#endregion method MD5

		#region method Validate

		public static string ValidateStringArg(string sValue, string sArgName, bool bThrow = true, int nMaxAllowedLength = 255) {
			sValue = (sValue ?? string.Empty).Trim();

			if (sValue.Length == 0) {
				if (bThrow)
					throw new ArgumentNullException(sArgName, sArgName + " not specified.");

				return sValue;
			} // if

			if (sValue.Length > nMaxAllowedLength) {
				if (bThrow)
					throw new Exception(sArgName + " is too long.");

				return sValue.Substring(0, nMaxAllowedLength);
			} // if

			return sValue;
		} // Validate

		#endregion method Validate

		#region method WeekStart

		public static DateTime WeekStart(DateTime oDate, CultureInfo oCulture = null) {
			if (oCulture == null)
				oCulture = CultureInfo.CurrentCulture;

			int nDiff = oDate.DayOfWeek - oCulture.DateTimeFormat.FirstDayOfWeek;

			if (nDiff < 0)
				nDiff += oCulture.DateTimeFormat.DayNames.Length;

			return oDate.AddDays(-1 * nDiff).Date;
		} // WeekStart

		public static DateTime FirstDayOfWeek(this DateTime oDate, CultureInfo oCulture = null) {
			return WeekStart(oDate, oCulture);
		} // FirstDayOfWeek

		public static DateTime FirstDayOfWeek(CultureInfo oCulture = null) {
			return WeekStart(DateTime.UtcNow, oCulture);
		} // FirstDayOfWeek

		#endregion method WeekStart

		public static int GetFullYears(DateTime date)
		{
			int years = DateTime.UtcNow.Year - date.Year;
			if (date > DateTime.UtcNow.AddYears(-years))
			{
				years--;
			}

			return years;
		}

		public static void GetFullYearsAndMonths(DateTime date, out int years, out int months)
		{
			years = GetFullYears(date);
			months = DateTime.UtcNow.Month - date.Month;

			if (months < 0)
			{
				months = 12 - date.Month + DateTime.UtcNow.Month;
			}
			else if (months > 0 && DateTime.UtcNow.Day < date.Day)
			{
				months--;
			}
			else if (months == 0 && DateTime.UtcNow.Day < date.Day)
			{
				months = 11;
			}
		}
	} // class MiscUtils

	#endregion class MiscUtils
} // namespace Ezbob.Utils

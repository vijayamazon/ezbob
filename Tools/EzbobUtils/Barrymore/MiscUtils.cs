namespace Ezbob.Utils {
	using System;
	using System.Globalization;

	public static class MiscUtils {

		public static string MD5(string input) {
			return Security.SecurityUtils.MD5(input);
		} // MD5

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

		public static int GetFullYears(DateTime date) {
			int years = DateTime.UtcNow.Year - date.Year;
			if (date > DateTime.UtcNow.AddYears(-years))
				years--;

			return years;
		} // GetFullYears

		public static void GetFullYearsAndMonths(DateTime? date, out int years, out int months) {
			if (!date.HasValue) {
				years = 0;
				months = 0;
				return;
			}

			years = GetFullYears(date.Value);
			months = DateTime.UtcNow.Month - date.Value.Month;

			if (months < 0)
				months = 12 - date.Value.Month + DateTime.UtcNow.Month;
			else if (months > 0 && DateTime.UtcNow.Day < date.Value.Day)
				months--;
			else if (months == 0 && DateTime.UtcNow.Day < date.Value.Day)
				months = 11;
		} // GetFullYearsAndMonths

		/// <summary>
		/// Returns number of full months between the dates a and b.
		/// If a > b the result is negative.
		/// </summary>
		/// <param name="a">First date.</param>
		/// <param name="b">Second date.</param>
		/// <returns>Number of full months between the dates a and b.</returns>
		public static int MonthDiff(DateTime a, DateTime b) {
			if (a == b)
				return 0;

			bool bReverse = a > b;

			if (bReverse) {
				DateTime t = a;
				a = b;
				b = t;
			} // if

			int nCount = 0;

			DateTime c = b;

			while (a <= c) {
				c = b.AddMonths(-nCount - 1);

				if (a <= c)
					nCount++;
				else
					break;
			} // while

			return bReverse ? -nCount : nCount;
		} // MonthDiff

		/// <summary>
		/// Calculates number of full calendar months between the months represented by arguments.
		/// If a > b the result is negative.
		/// </summary>
		/// <example>If a is November 15th 2014 and b is July 16 2014 then result is 3 (August, September, October).</example>
		/// <example>If a is November 15th 2012 and b is March 16 2014 then result is 15
		/// (December 2012, entire 2013, January 2014, and February).</example>
		/// <param name="a">First date.</param>
		/// <param name="b">Second date.</param>
		/// <returns>Number of months.</returns>
		public static int CountMonthsBetween(DateTime a, DateTime b) {
			if (a == b)
				return 0;

			int nReverse = a > b ? -1 : 1;

			if (a > b) {
				DateTime t = a;
				a = b;
				b = t;
			} // if

			if (a.Year == b.Year)
				return nReverse * Math.Max(0, b.Month - a.Month - 1);

			return nReverse * ((12 - a.Month) + (b.Month - 1) + 12 * (b.Year - a.Year - 1));
		} // CountMonthsBetween

	} // class MiscUtils
} // namespace

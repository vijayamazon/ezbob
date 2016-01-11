namespace Ezbob.Utils {
	using System;
	using System.Globalization;
	using Ezbob.Utils.Security;
	using Newtonsoft.Json;

	public static class MiscUtils {

		public static Tuple<DateTime?, DateTime?> NL_GetLateFeeDates(bool AutoLateFees,
														   DateTime? StopLateFeeFromDate,
														   DateTime? StopLateFeeToDate) {
			if (StopLateFeeFromDate != null)
				return new Tuple<DateTime?, DateTime?>(StopLateFeeFromDate, StopLateFeeToDate);
			if (AutoLateFees)
				return new Tuple<DateTime?, DateTime?>(DateTime.Now, null);
			return new Tuple<DateTime?, DateTime?>(null, null);
		}

		public static DateTime? NL_GetStopAutoChargeDate(bool AutoCharge, DateTime? StopAutoChargeDate) {
			if (StopAutoChargeDate != null)
				return StopAutoChargeDate;
			if (AutoCharge)
				return DateTime.Now;
			return null;
		}

		public static JsonSerializerSettings GetJsonDBFormat() {
			return new JsonSerializerSettings { Formatting = Formatting.None, ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
		}


		public static string MD5(string input) {
			return SecurityUtils.MD5(input);
		} // MD5



		public static string ValidateStringArg(
			string sValue,
			string sArgName,
			bool bThrow = true,
			int nMaxAllowedLength = 255
		) {
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
		/// <example>
		/// If a is November 15th 2014 and b is July 16 2014 then result is 3 (August, September, October).
		/// </example>
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

		/// <summary>
		/// <para>Calculates the first of 12 months for calculating marketplace annual turnover. First month calculation rule
		/// is: if calculation date is in month's tail and marketplace last update time is in the same month tail then
		/// requested year end at the month of calculation date. Otherwise requested year ends one month before the month of
		/// calculation date.</para>
		/// <para>There is special handling for HMRC marketplace.</para>
		/// <para>For HMRC there is one more step after applying standard procedure (described above): check
		/// what is the last reported month. HMRC is normally reported every three months so if requested year ends at
		/// May 2015 then it's all right NOT to have data for March'15, April'15, and May'15. In such cases
		/// we step back up to four months.</para>
		/// </summary>
		/// <example>
		/// Non-HMRC marketplace.
		/// Month tail is 28, 29, 30; calculation date is June 30 2015, marketplace last update time is June 29 =>
		/// returned value is July 2014, annual turnover is calculated based on July'14-June'15.
		/// </example>
		/// <example>
		/// Non-HMRC marketplace.
		/// Month tail is 28, 29, 30; calculation date is June 30 2015, marketplace last update time is June 19 =>
		/// returned value is June 2014, annual turnover is calculated based on June'14-May'15.
		/// </example>
		/// <example>
		/// Non-HMRC marketplace.
		/// Month tail is 28, 29, 30; calculation date is June 20 2015, marketplace last update time is not considered =>
		/// returned value is June 2014, annual turnover is calculated based on June'14-May'15.
		/// </example>
		/// <example>
		/// HMRC marketplace.
		/// Standard calculated month is June 2014. Last reported month is February 15 =>
		/// returned value is March 2014, annual turnover is calculated based on March'14-February'15.
		/// </example>
		/// <example>
		/// HMRC marketplace.
		/// Standard calculated month is June 2014. Last reported month is April 15 =>
		/// returned value is May 2014, annual turnover is calculated based on May'14-April'15.
		/// </example>
		/// <example>
		/// HMRC marketplace.
		/// Standard calculated month is June 2014. Last reported month is January 15 =>
		/// returned value is June 2014, annual turnover is calculated based on June'14-May'15.
		/// </example>
		/// <param name="calculationDate">A day when this calculation takes place.
		/// It can differ from DateTime.UtcNow in case of simulations or test executions.
		/// </param>
		/// <param name="lastUpdate">Marketplace last update time (i.e. maximum for marketplace from
		/// MP_CustomerMarketplaceUpdatingHistory.UpdatingEnd).</param>
		/// <param name="tailLength">Number of days in month tail. If month tail length is 3
		/// and the month is July then month tail is the 29th, the 30th, and the 31st.</param>
		/// <param name="lastExistingDataTime">Specified only for HMRC marketplaces: last reported month.</param>
		/// <returns>First month of the year for turnover calculation.</returns>
		public static DateTime GetPeriodAgo(
			DateTime calculationDate,
			DateTime lastUpdate,
			int tailLength,
			DateTime? lastExistingDataTime
		) {
			int daysInMonth = DateTime.DaysInMonth(calculationDate.Year, calculationDate.Month);

			bool inTheSameTail =
                ((daysInMonth - calculationDate.Date.Day) < tailLength) &&
				((daysInMonth - lastUpdate.Date.Day) < tailLength) &&
				(calculationDate.Month == lastUpdate.Month) &&
				(calculationDate.Year == lastUpdate.Year);

			DateTime months = calculationDate.AddMonths(inTheSameTail ? -11 : -12);

			var standardResult = new DateTime(months.Year, months.Month, 1, 0, 0, 0, DateTimeKind.Utc);

			if (lastExistingDataTime == null)
				return standardResult;

			DateTime standardYearEnd = standardResult.AddMonths(11);

			DateTime lastExisting = new DateTime(
				lastExistingDataTime.Value.Year,
				lastExistingDataTime.Value.Month,
				1,
				0,
				0,
				0,
				DateTimeKind.Utc
			);

			if ((lastExisting >= standardYearEnd) || (lastExisting.AddMonths(3) < standardYearEnd))
				return standardResult;

			// Note order of arguments: result should be negative.
			return standardResult.AddMonths(DateDiffInMonths(standardYearEnd, lastExisting));
		} // GetPeriodAgo

		/// <summary>
		/// 
		/// </summary>
		/// <param name="raw"></param>
		/// <returns></returns>
		public static string ByteArr2Hex(byte[] raw) {
			string hex = null;

			if (raw != null) {
				hex = BitConverter.ToString(raw);
				hex = (hex.Replace("-", "")).ToLower();
			} // if

			return hex;
		} // ByteArr2Hex

		/// <summary>
		/// Calculates difference between to DatTime dates in months
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <returns></returns>
		public static int DateDiffInMonths(DateTime start, DateTime end) {
			return (end.Month + end.Year * 12) - (start.Month + start.Year * 12);
		} // DateDiffInMonths


		/// <summary>
		/// Calculates difference between to DateTime dates in weeks
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <returns></returns>
		public static int DateDiffInWeeks(DateTime start, DateTime end) {
			return (int)Math.Ceiling(end.Subtract(start).Days / 7d);
		} // DateDiffInWeeks

		public static int DaysInMonth(DateTime date) {
			return DateTime.DaysInMonth(date.Year, date.Month);
		}

	} // class MiscUtils
} // namespace

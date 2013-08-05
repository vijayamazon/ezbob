using System;

namespace EzBob.CommonLib
{
	using System.ComponentModel;

	public static class Extensions
	{
		public static DateTime GetLastMonthDate(this DateTime date)
		{
			return date.GetNextMonthFirstDate().AddSeconds( -1 );
		}

		public static DateTime GetFirstMonthDate( this DateTime date )
		{
			return new DateTime( date.Year, date.Month, 1, 0, 0, 0 );
		}

		public static DateTime GetNextMonthFirstDate( this DateTime date )
		{
			return date.GetFirstMonthDate().AddMonths( 1 );
		}

		public static int GetCountMonthsToByEntire( this DateTime from, DateTime to )
		{
			var fromDate = from.GetFirstMonthDate();
			var toDate = to.GetFirstMonthDate();

			if ( fromDate >= toDate )
			{
				return 0;
			}

			var months = 0;

			while ( fromDate < toDate )
			{
				fromDate = fromDate.GetNextMonthFirstDate();
				months++;
			}

			return months;
		}

		public static int GetCountIncludedMonthsWithByEntire( this DateTime fromDate, DateTime toDate )
		{
			fromDate = fromDate.GetFirstMonthDate();

			if ( fromDate > toDate )
			{
				var temp = fromDate;
				fromDate = toDate;
				toDate = temp;
			}

			var months = 0;
			while ( fromDate <= toDate ) // at least one time
			{
				fromDate = fromDate.GetNextMonthFirstDate();
				months++;
			}

			return months;
		}

		public static int GetCountIncludedMonthsWithByStep( this DateTime fromDate, DateTime toDate )
		{
			var months = 0;

			if ( fromDate > toDate )
			{
				var temp = fromDate;
				fromDate = toDate;
				toDate = temp;
			}

			while ( fromDate <= toDate ) // at least one time
			{
				fromDate = fromDate.AddMonths( 1 );
				++months;
			}

			return months;
		}

		public static string Description(this Enum value)
		{
			var enumType = value.GetType();
			var field = enumType.GetField(value.ToString());
			var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute),
													   false);
			return attributes.Length == 0
				? value.ToString()
				: ((DescriptionAttribute)attributes[0]).Description;
		}
	}
}

using System;
using System.Globalization;

namespace EzBob.TeraPeakServiceLib.Requests.SellerResearch
{
	internal class DateTimeCorrectionHelper
	{
		private static readonly TimeZoneInfo _PacificStandardTimeZone = TimeZoneInfo.FindSystemTimeZoneById( "Pacific Standard Time" );

		/// <summary>
		/// 
		/// </summary>
		/// <param name="date">DateTime in UTC format</param>
		/// <returns></returns>
		public static string TransformToSting( DateTime? date )
		{
			if(!date.HasValue)
			{
				return null;
			}
			var newDate = TimeZoneInfo.ConvertTimeFromUtc( date.Value, _PacificStandardTimeZone ); 
			return newDate.ToString( CultureInfo.InvariantCulture );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns>DateTime in UTC format</returns>
		public static DateTime? TransformDateValue( string value )
		{
			DateTime rez;
			if ( DateTime.TryParse( value, CultureInfo.InvariantCulture, DateTimeStyles.None, out rez ) )
			{
				return TimeZoneInfo.ConvertTimeToUtc( rez, _PacificStandardTimeZone );
			}
			else
			{
				return null;
			}
		}
	}
}

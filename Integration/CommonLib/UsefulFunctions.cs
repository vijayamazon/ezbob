using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EzBob.CommonLib
{
	public static class UsefulFunctions
	{
		public static List<Tuple<DateTime, DateTime>> SplitDateRanges(DateTime startDate, DateTime endDate, int maxMonths)
		{
			var rez = new List<Tuple<DateTime, DateTime>>();

			DateTime fromDate = startDate;
			DateTime toDate = fromDate.AddMonths( maxMonths );
			while ( true )
			{
				if ( toDate >= endDate || ( toDate < endDate && toDate.AddDays( 1 ) > endDate ) )
				{
					rez.Add( new Tuple<DateTime, DateTime>( fromDate, endDate ) );
					break;
				}

				rez.Add( new Tuple<DateTime, DateTime>( fromDate, toDate ) );
				fromDate = toDate;
				toDate = toDate.AddMonths( maxMonths );
				fromDate = fromDate.AddSeconds( 1 );
			}

			return rez;
		}
	}
}

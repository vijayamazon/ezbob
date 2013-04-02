using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NHibernate.Linq;

namespace CustomSchedulers
{
	internal class CurrencyRateHistoryContainer : ConcurrentBag<CurrencyHistoryItemData>
	{
		public CurrencyRateHistoryContainer(IEnumerable<CurrencyHistoryItemData> data)
		{
			data.AsParallel().ForEach( Add );
		}

		public static CurrencyRateHistoryContainer Union( CurrencyRateHistoryContainer msnData, CurrencyRateHistoryContainer yahooData )
		{
			CurrencyRateHistoryContainer mainData = null;
			CurrencyRateHistoryContainer minorData = null;
			CurrencyRateHistoryContainer rez = null;
			if ( msnData != null && msnData.Count > 0 )
			{
				mainData = msnData;
				if ( yahooData != null && yahooData.Count > 0 )
				{
					minorData = yahooData;
				}
			}
			else if ( yahooData != null && yahooData.Count > 0 )
			{
				mainData = yahooData;
			}
			else
			{
				// NO DATA!
				return null;
			}

			rez = new CurrencyRateHistoryContainer(mainData);

			if ( minorData != null )
			{
				Parallel.ForEach( minorData, itemData =>
				                             	{
				                             		var existsItem = mainData.FirstOrDefault( d => d.Date.Date.Equals( itemData.Date.Date ) );
				                             		if ( existsItem == null )
				                             		{
				                             			rez.Add( itemData );
				                             		}

				                             	} );
			}

			return rez;
		}
	}
}
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using log4net;

namespace CustomSchedulers.Providers
{
	abstract class CurrencyRateProviderBase : ICurrencyRateProvider
	{
		private static readonly ILog Log = LogManager.GetLogger( typeof( CurrencyRateProviderBase ) );
		
		public static readonly string GbpLabel = "GBP";

		public abstract CurrencyRateProviderType ServiceType { get; }

		public CurrencyRateHistoryContainer RetriveData( string currencyName, DateTime startDate, DateTime endDate )
		{
			return InternalRetriveData( currencyName, startDate, endDate, false );
		}

		private CurrencyRateHistoryContainer InternalRetriveData( string currencyName, DateTime startDate, DateTime endDate, bool isNeedToReversePrice )
		{
			CurrencyRateHistoryContainer result = null;

			var symbolName = CreateSymbolName( currencyName, isNeedToReversePrice );

			var url = CreateServiceConnectionString( symbolName, startDate, endDate, isNeedToReversePrice );

			Log.Info( string.Format( "{1}: Retrieve historical price data started for Symbol {0}...", symbolName, ServiceType ) );
			try
			{
				var webClient = new WebClient();
				var stream = webClient.OpenRead( url );
				if ( stream == null )
				{
					throw new Exception( string.Format( "Can`t read {1} Service, history update not possible for Symbol {0}", symbolName, ServiceType ) );
				}

				var reader = new StreamReader( stream );
				var response = reader.ReadToEnd();

				result = ParceData( response, isNeedToReversePrice );

				if ( result == null )
				{
					Log.Info( string.Format( "{1}: NO DATA for Symbol: {0}!", symbolName, ServiceType ) );
				}

				if ( result == null && !isNeedToReversePrice )
				{
					return InternalRetriveData( currencyName, startDate, endDate, true );
				}
			}
			catch ( WebException ex )
			{
				if ( ex.Message.Contains( "404" ) )
				{
					Log.Error( string.Format( "{1}: for Symbol [{0}] Update: {1}", symbolName, ex.Message, ServiceType ) );
					if ( !isNeedToReversePrice )
					{
						return InternalRetriveData( currencyName, startDate, endDate, true );
					}
				}
				else
				{
					Log.Error( ex );
					throw;
				}
			}
			catch ( Exception ex )
			{
				Log.Error( ex.Message );
				Log.Error( ex );
				throw;
			}

			Log.Info( string.Format( "{1}: Retrieve historical price data successfully completed for Symbol {0}!", symbolName, ServiceType ) );
			return result;
		}

		protected abstract string CreateSymbolName(string currencyName, bool isNeedToReversePrice);

		protected abstract string CreateServiceConnectionString( string currencyName, DateTime startDate, DateTime endDate, bool isNeedToReversePrice );

		private CurrencyRateHistoryContainer ParceData( string response, bool isNeedToReversePrice )
		{
			#region test response from MSN
			/* 
				Copyright © 2012 Thomson Reuters.					
				Quotes supplied by Interactive Data Real-Time Services.					
				US Dollar/British Pound (GBPUSD)					
				Daily prices					
				DATE	OPEN	HIGH	LOW	CLOSE	VOLUME
				5/2/2012	1.6216	1.6238	1.6158	1.6199	0
				5/1/2012	1.6234	1.6247	1.6184	1.6218	0
				4/30/2012	1.6271	1.6301	1.6219	1.6231	0
				4/27/2012	1.6176	1.6279	1.615	1.6265	0
				4/26/2012	1.6164	1.6206	1.6156	1.6174	0				
				 */
			#endregion

			#region test response from YAHOO
			/* 
				Date,Open,High,Low,Close,Volume,Adj Close
				2012-04-26,1843.71,1845.31,1831.23,1837.21,000,1837.21
				2012-04-25,1841.64,1845.24,1832.59,1841.15,000,1841.15
				2012-04-24,1837.06,1844.62,1834.58,1837.15,000,1837.15
				2011-05-31,1782.95,1785.86,1771.70,1775.49,000,1775.49
				2011-05-30,1784.99,1785.53,1777.27,1778.36,000,1778.36
				2011-05-27,1785.26,1789.38,1772.51,1785.25,000,1785.25
				
				 */
			#endregion

			var list = response.Split( new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries ).ToList();

			while ( true )
			{
				if ( list.Count == 0 )
				{
					return null;
				}

				if ( list[0].ToUpper().Contains( "DATE" ) )
				{
					list.RemoveAt( 0 );
					break;
				}
				else
				{
					list.RemoveAt( 0 );
				}
			}

            var formats = new string[] { "M/d/yyyy", "yyyy-MM-dd" };

			var historyItemDatas = list.AsParallel().Select( i =>
			    {
			        var dataList = i.Split( new[] { ',' } );

			        var dateStr = dataList[0];
                    
			        DateTime date = DateTime.ParseExact(dateStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None);

			        var priceStr = dataList[4];
			        decimal price = 0;
                    var rez = decimal.TryParse(priceStr, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out price);
					//Debug.Assert( rez );
			        return new CurrencyHistoryItemData
			                {
			                    Date = date,
			                    Price = isNeedToReversePrice && price != 0 ? 1 / price : price
			                };
			    } );

			var result = historyItemDatas.Where( i => i.Price > 0 ).ToList();

			return !result.Any() ? null : new CurrencyRateHistoryContainer( result );
		}
	}
}
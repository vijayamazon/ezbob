namespace CustomSchedulers.Currency
{
	using System;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Net;
	using log4net;

	class CurrencyRateProviderYahoo
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(CurrencyRateProviderYahoo));

		public static readonly string GbpLabel = "GBP";

		public CurrencyRateHistoryContainer RetriveData(string currencyName, DateTime startDate, DateTime endDate)
		{
			return InternalRetriveData(currencyName, startDate, endDate, false);
		}

		private CurrencyRateHistoryContainer InternalRetriveData(string currencyName, DateTime startDate, DateTime endDate, bool isNeedToReversePrice)
		{
			CurrencyRateHistoryContainer result = null;

			var symbolName = CreateSymbolName(currencyName, isNeedToReversePrice);

			var url = CreateServiceConnectionString(symbolName, startDate, endDate, isNeedToReversePrice);

			Log.Info(string.Format("Yahoo: Retrieve historical price data started for Symbol {0}...", symbolName));
			try
			{
				var webClient = new WebClient();
				var stream = webClient.OpenRead(url);
				if (stream == null)
				{
					throw new Exception(string.Format("Can`t read Yahoo Service, history update not possible for Symbol {0}", symbolName));
				}

				var reader = new StreamReader(stream);
				var response = reader.ReadToEnd();

				result = ParceData(response, isNeedToReversePrice);

				if (result == null)
				{
					Log.Info(string.Format("Yahoo: NO DATA for Symbol: {0}!", symbolName));
				}

				if (result == null && !isNeedToReversePrice)
				{
					return InternalRetriveData(currencyName, startDate, endDate, true);
				}
			}
			catch (WebException ex)
			{
				if (ex.Message.Contains("404"))
				{
					if (symbolName == "USD" || symbolName == "GBP" || symbolName == "EUR" || symbolName == "CAD" || symbolName == "AUD" || symbolName == "ILS")
					{
						Log.Error(string.Format("Yahoo: for Symbol [{0}] Update: {1}", symbolName, ex.Message));
					}
					else
					{
						Log.Warn(string.Format("Yahoo: for Symbol [{0}] Update: {1}", symbolName, ex.Message));
					}
					if (!isNeedToReversePrice)
					{
						return InternalRetriveData(currencyName, startDate, endDate, true);
					}
				}
				else
				{
					Log.Error(ex);
					return null;
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex.Message);
				Log.Error(ex);
				throw;
			}

			Log.Info(string.Format("Yahoo: Retrieve historical price data successfully completed for Symbol {0}!", symbolName));
			return result;
		}
		protected string CreateSymbolName(string currencyName, bool isNeedToReversePrice)
		{
			var format = isNeedToReversePrice ? "{0}{1}=X" : "{1}{0}=X";
			return string.Format( format, currencyName, GbpLabel );
		}

		protected string CreateServiceConnectionString( string symbolName, DateTime startDate, DateTime endDate, bool isNeedToReversePrice )
		{

			/*info: http://etraderzone.com/free-scripts/47-historical-quotes-yahoo.html
			 s - This is where you can specify your stock quote, if you want to download stock quote for Microsoft, 
				just enter it as 's=MSFT'

			a - This parameter is to get the input for the start month. '00' is for January, '01' is for February 
				and so on.

			b - This parameter is to get the input for the start day, this one quite straight forward, '1' is 
				for day one of the month, '2' is for second day of the month and so on.

			c - This parameter is to get the input for the start year

			d - This parameter is to get the input for end month, and again '00' is for January, '02' is for February 
				and so on.

			e - This parameter is to get the input for the end day

			f - This parameter is to get the input for the end year

			g - This parameter is to specify the interval of the data you want to download. 'd' is for daily, 'w' 
			 *is for weekly and 'm' is for monthly prices. The default is 'daily' if you ignore this parameter.

			example: http://ichart.finance.yahoo.com/table.csv?s=GBP&a=4&b=1&c=2012&d=4&e=27&f=2012&g=d
			*/

			var startMonth = startDate.Month;
			var startDay = 1;
			var startYear = startDate.Year;
			var endMonth = endDate.Month;
			var endDay = endDate.Day;
			var endYear = endDate.Year;

			return string.Format( "http://ichart.finance.yahoo.com/table.csv?s={0}&a={1}&b={2}&c={3}&d={4}&e={5}&f={6}&g=d", symbolName, startMonth, startDay, startYear, endMonth, endDay, endYear );
		}

		private CurrencyRateHistoryContainer ParceData(string response, bool isNeedToReversePrice)
		{

			/* 
				Date,Open,High,Low,Close,Volume,Adj Close
				2012-04-26,1843.71,1845.31,1831.23,1837.21,000,1837.21
				2012-04-25,1841.64,1845.24,1832.59,1841.15,000,1841.15
				2012-04-24,1837.06,1844.62,1834.58,1837.15,000,1837.15
				2011-05-31,1782.95,1785.86,1771.70,1775.49,000,1775.49
				2011-05-30,1784.99,1785.53,1777.27,1778.36,000,1778.36
				2011-05-27,1785.26,1789.38,1772.51,1785.25,000,1785.25

				 */

			var list = response.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList();

			while (true)
			{
				if (list.Count == 0)
				{
					return null;
				}

				if (list[0].ToUpper().Contains("DATE"))
				{
					list.RemoveAt(0);
					break;
				}
				else
				{
					list.RemoveAt(0);
				}
			}

			var formats = new string[] { "M/d/yyyy", "yyyy-MM-dd" };

			var historyItemDatas = list.AsParallel().Select(i =>
			{
				var dataList = i.Split(new[] { ',' });

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
			});

			var result = historyItemDatas.Where(i => i.Price > 0).ToList();

			return !result.Any() ? null : new CurrencyRateHistoryContainer(result);
		}
	}
}

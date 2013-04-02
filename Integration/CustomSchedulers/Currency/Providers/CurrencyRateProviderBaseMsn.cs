using System;

namespace CustomSchedulers.Providers
{
	class CurrencyRateProviderMsn : CurrencyRateProviderBase
	{
		public override CurrencyRateProviderType ServiceType
		{
			get { return CurrencyRateProviderType.MSN; }
		}

		protected override string CreateSymbolName(string currencyName, bool isNeedToReversePrice)
		{
			var format = isNeedToReversePrice ? "{0}{1}" : "{1}{0}";
			return string.Format( format, currencyName, GbpLabel );
		}

		protected override string CreateServiceConnectionString( string symbolName, DateTime startDate, DateTime endDate, bool isNeedToReversePrice )
		{
			#region Info
			/* description: http://stefan-radstrom.se/index.php?option=com_content&view=article&id=33%3Adownloading-historical-stock-quotes&catid=17%3Aapplication-to-investment&Itemid=40&lang=en
			 * 
			 * example: http://data.moneycentral.msn.com/scripts/chrtsrv.dll?symbol=USD&filedownload=&c1=2&c2=&c5=4&c6=2011&c7=4&c8=2012&c9=1&ce=0&cf=0&d0=1&d3=0&d4=1&d5=0&d9=1
			 * 
			 * c1 - Appears to be used for the time interval when downloading: 
			 *		'0' and '1' is selected when downloading the full history, minimum 1m, see below; 
			 *		'2' is used when selecting a start and end date. {'0','1','2'}:['0']
			 * 
			 * c5 - Start month; '1' is January, '2' is February, etc. {'1','2',...,'11','12'}
			 * 
			 * c5D - Start day; '1' ... '31'. {'1','2',...,'30','31'}
			 * 
			 * c6 - Start year; in the form 'YYYY'.
			 * 
			 * c7 -	End month; '1' is January, '2' is February, etc. {'1','2',...,'11','12'}
			 *  
			 * c7D- End day; '1' ... '31'. {'1','2',...,'30','31'}
			 * 
			 * c8 - End year; in the form 'YYYY'.
			 * 
			 * C9 -	This parameter is to specify the interval of the data you want to download. '-1' is for "auto" 
			 *		(default?), 
			 *		'0' is for daily 
			 *		and '1' is for weekly 
			 *		'2' is for monthly, 
			 *		and '3' is for yearly prices. {'-1','0','1','2','3'}
			 *		
			 * CF - Secondary chart: '0' is None, '2' is MACD, '16' is RSI, etc. 
			 *		Stochastic oscillator: '4' is Fast, '8' is Slow. {'0','2','16',...,'4','8'}:['0']
			 * 
			 * D9 -	Moving average envelope. The value is '1' if used.
			 * 
			 */
			#endregion

			var startMonth = startDate.Month;
			var startDay = 1;
			var startYear = startDate.Year;
			var endMonth = endDate.Month;
			var endDay = endDate.Day;
			var endYear = endDate.Year;

			return string.Format( "http://data.moneycentral.msn.com/scripts/chrtsrv.dll?symbol={0}&filedownload=&c1=2&c5={1}&c5D={2}&c6={3}&c7={4}&c7D={5}&c8={6}&c9=0&ce=0&cf=0&d0=1&d3=0&d4=1&d5=0&d9=0", symbolName, startMonth, startDay, startYear, endMonth, endDay, endYear );

		}
	}
}
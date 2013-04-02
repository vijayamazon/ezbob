using System;

namespace CustomSchedulers.Providers
{
	class CurrencyRateProviderYahoo : CurrencyRateProviderBase
	{
		public override CurrencyRateProviderType ServiceType
		{
			get { return CurrencyRateProviderType.Yahoo; }
		}

		protected override string CreateSymbolName(string currencyName, bool isNeedToReversePrice)
		{
			var format = isNeedToReversePrice ? "{0}{1}=X" : "{1}{0}=X";
			return string.Format( format, currencyName, GbpLabel );
		}

		protected override string CreateServiceConnectionString( string symbolName, DateTime startDate, DateTime endDate, bool isNeedToReversePrice )
		{
			#region Info
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
			#endregion
			
			var startMonth = startDate.Month;
			var startDay = 1;
			var startYear = startDate.Year;
			var endMonth = endDate.Month;
			var endDay = endDate.Day;
			var endYear = endDate.Year;

			return string.Format( "http://ichart.finance.yahoo.com/table.csv?s={0}&a={1}&b={2}&c={3}&d={4}&e={5}&f={6}&g=d", symbolName, startMonth, startDay, startYear, endMonth, endDay, endYear );

		}

		
	}
}
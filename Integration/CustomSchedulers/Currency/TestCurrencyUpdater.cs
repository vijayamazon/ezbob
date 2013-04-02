using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CustomSchedulers.Providers;
using NUnit.Framework;

namespace CustomSchedulers
{
	class TestCurrencyUpdater
	{
		[Test]
		public void TestParceHistory()
		{
			string symbolName = "KWL";
			string response =
@"Date,Open,High,Low,Close,Volume,Adj Close
2012-04-26,1843.71,1845.31,1831.23,1837.21,000,1837.21
2012-04-25,1841.64,1845.24,1832.59,1841.15,000,1841.15
2012-04-24,1837.06,1844.62,1834.58,1837.15,000,1837.15
2011-05-31,1782.95,1785.86,1771.70,1775.49,000,1775.49
2011-05-30,1784.99,1785.53,1777.27,1778.36,000,1778.36
2011-05-27,1785.26,1789.38,1772.51,1785.25,000,1785.25
";
			var list = response.Split(new[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries).ToList();
			Assert.IsTrue( list[0].Contains( "Open" ) );
			Assert.AreEqual( list.Count, 7 );
			
			list.RemoveAt( 0 );
			
			Assert.IsFalse( list[0].Contains( "Open" ) );
			Assert.AreEqual( list.Count, 6 );

			var data = new CurrencyHistoryData( symbolName );

			data.History = list.Select( i =>
				{
					var dataList = i.Split( new[] { ',' } );
					Assert.AreEqual( dataList.Length, 7 );
					var dateStr = dataList[0];
					DateTime date = DateTime.Parse(dateStr);

					var priceStr = dataList[4];
					var price = decimal.Parse(priceStr);
					return new CurrencyHistoryItemData
						{
							Date = date,
							Price = price
						};
				} ).ToList();

			Assert.AreEqual( symbolName, data.CurrencyName );
			Assert.AreEqual( data.History.Count, list.Count );
		}

		[Test]
		public void TestRetriveCurrencyHistoryFromWeb()
		{
			var controller = new CurrencyUpdateController();

			string currencyName = "VND";
			var now = DateTime.Now;
			var fromDate = now.AddYears(-1);
			var toDate = now;
			var data = controller.RetriveCurrencyHistoryFromWeb( currencyName, fromDate, toDate );
		}

		[Test]
		public void TestRetriveCurrencyHistoryFromYahoo()
		{
			string currencyName = "USD";
			var data = RetrieveData( currencyName, CurrencyRateProviderType.Yahoo );
		}

		[Test]
		public void TestRetriveCurrencyHistoryFromMsn()
		{
			string currencyName = "VND";
			var data = RetrieveData( currencyName, CurrencyRateProviderType.MSN );
		}

		private CurrencyRateHistoryContainer RetrieveData(string currencyName, CurrencyRateProviderType serviceType )
		{
			var now = DateTime.Now;
			var fromDate = now.AddYears( -1 );
			var toDate = now;
			var service = CurrencyRateProviderFactory.Create( serviceType );
			return service.RetriveData( currencyName, fromDate, toDate );
		}

	}
}

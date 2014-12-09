namespace CustomSchedulers.Currency
{
	using System;
	using System.Collections.Generic;

	public class CurrencyHistoryData
	{
		public CurrencyHistoryData()
		{
			History = new List<CurrencyHistoryItemData>();
		}

		public CurrencyHistoryData( string currencyName )
			:this()
		{
			CurrencyName = currencyName;
		}

		public string CurrencyName { get; set; }
		public List<CurrencyHistoryItemData> History { get; set; }

		internal void AddHistory(CurrencyRateHistoryContainer data)
		{
			if ( data == null || data.Count == 0 )
			{
				return;
			}

			History.AddRange( data );
		}
	}

	public class CurrencyHistoryItemData
	{
		public CurrencyHistoryItemData()
		{
		}

		public CurrencyHistoryItemData(DateTime date, decimal price)
		{
			Date = date;
			Price = price;
		}

		public DateTime Date { get; set; }
		public decimal Price { get; set; }

		public override string ToString()
		{
			return string.Format( "[{0:yyyy-MM-dd}] {1}", Date, Price );
		}
	}

}

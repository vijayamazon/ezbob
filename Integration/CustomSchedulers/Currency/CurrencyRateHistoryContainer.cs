namespace CustomSchedulers.Currency
{
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Linq;
	using NHibernate.Linq;

	internal class CurrencyRateHistoryContainer : ConcurrentBag<CurrencyHistoryItemData>
	{
		public CurrencyRateHistoryContainer(IEnumerable<CurrencyHistoryItemData> data)
		{
			if (data != null)
			{
				data.AsParallel().ForEach(Add);
			}
		}
	}
}
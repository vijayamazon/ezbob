namespace EZBob.DatabaseLib.Repository {
	using System;
	using System.Linq;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database;
	using NHibernate;
	using NHibernate.Linq;
	using log4net;

	public interface ICurrencyRateRepository {
		double GetCurrencyHistoricalRate(DateTime? purchaseDate, string currencyCode);
		CurrencyData GetCurrencyOrCreate(string currencyName);
	} // interface ICurrencyRateRepository

	public class CurrencyRateRepository : NHibernateRepositoryBase<CurrencyData>, ICurrencyRateRepository {
		private static readonly ILog _Log = LogManager.GetLogger(typeof(CurrencyRateRepository));

		public CurrencyRateRepository(ISession session) : base(session) {} // constructor

		public bool HasCurrencyList() {
			return GetAll().Any();
		} // HasCurrencyList

		public CurrencyData GetCurrencyOrCreate(string currencyName) {
			if (string.IsNullOrEmpty(currencyName))
				return null;

			var currency = GetAll().FirstOrDefault(c => c.Name.ToUpper() == currencyName.ToUpper());

			if (currency == null) {
				currency = new CurrencyData(currencyName);
				Save(currency);
			} // if

			return currency;
		} // GetCurrencyOrCreate

		public bool IsContainsHistory(string currencyName) {
			return Session.Query<CurrencyRateHistory>().Any(rh => rh.CurrencyData.Name == currencyName);
		} // IsContainsHistory

		public double GetCurrencyHistoricalRate(DateTime? purchaseDate, string currencyCode) {
			return EnsureTransaction(() => {
				_Log.DebugFormat("Loading a currency {0} rate on {1}.",
					currencyCode,
					purchaseDate.HasValue ? purchaseDate.ToString() : "no date"
				);

				IFutureValue<CurrencyData> currency = Session
					.Query<CurrencyData>()
					.Where(c => c.Name == currencyCode)
					.Take(1)
					.Cacheable().CacheRegion("Longterm")
					.ToFutureValue();

				if (currency.Value == null) {
					_Log.ErrorFormat("Currency: {0} not found!", currencyCode);
					return 1;
				} // if

				IFutureValue<CurrencyRateHistory> data = Session
					.Query<CurrencyRateHistory>()
					.Where(rh => ((rh.CurrencyData.Name == currencyCode) && (rh.Updated <= purchaseDate)))
					.OrderByDescending(rh => rh.Updated)
					.Take(1)
					.Cacheable().CacheRegion("Longterm")
					.ToFutureValue();

				if (data.Value != null && data.Value.Price.HasValue)
					return (double)data.Value.Price.Value;

				return (double)currency.Value.Price;
			});
		} // GetCurrencyHistoricalRate

		public void SaveHistoryItem(CurrencyRateHistory historyItem) {
			EnsureTransaction(() => Session.Save(historyItem));
		} // SaveHistoryItem
	}

}
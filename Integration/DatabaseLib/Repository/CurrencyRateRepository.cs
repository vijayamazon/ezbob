using System;
using System.Linq;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using NHibernate;
using NHibernate.Linq;
using log4net;

namespace Scorto.NHibernate.Repository
{
	public interface ICurrencyRateRepository
	{
		double GetCurrencyHistoricalRate(DateTime? purchaseDate, string currencyCode);
		CurrencyData GetCurrencyOrCreate(string currencyName);
	}

	public class CurrencyRateRepository : NHibernateRepositoryBase<CurrencyData>, ICurrencyRateRepository
    {
		private static readonly ILog _Log = LogManager.GetLogger( typeof( CurrencyRateRepository ) );

        public CurrencyRateRepository(ISession session) : base(session)
        {
        }

    	public bool HasCurrencyList()
    	{
			return GetAll().Any();
    	}

    	public CurrencyData GetCurrencyOrCreate(string currencyName)
    	{
			if (string.IsNullOrEmpty(currencyName))
			{
				return null;
			}

			var currency = GetAll().FirstOrDefault( c => c.Name.ToUpper() == currencyName.ToUpper() );
			
			if ( currency == null )
			{
				currency = new CurrencyData( currencyName );

				Save( currency );
			}

			return currency;
    	}

    	public bool IsContainsHistory(string currencyName)
    	{
    	    return _session.Query<CurrencyRateHistory>().Any(rh => rh.CurrencyData.Name == currencyName);
    	}

	    public double GetCurrencyHistoricalRate(DateTime? purchaseDate, string currencyCode)
		{
            return EnsureTransaction(() =>
                {
                    var currency = _session
                                    .Query<CurrencyData>()
                                    .Where(c => c.Name == currencyCode)
                                    .Take(1)
                                    .Cacheable().CacheRegion("Longterm")
                                    .ToFutureValue();
                    var data = _session
                                .Query<CurrencyRateHistory>()
                                .Where(rh => rh.CurrencyData.Name == currencyCode)
                                .Where(rh => rh.Updated <= purchaseDate)
                                .OrderByDescending(rh => rh.Updated)
                                .Take(1)
                                .Cacheable().CacheRegion("Longterm")
                                .ToFutureValue();

                    if (currency.Value == null)
                    {
                        _Log.ErrorFormat("Currency: {0} not found!", currencyCode);
                        return 1;
                    }

                    if (data.Value != null && data.Value.Price.HasValue)
                    {
                        return (double)data.Value.Price.Value;
                    }

                    return (double)currency.Value.Price;
                });
		}

	    public void SaveHistoryItem(CurrencyRateHistory historyItem)
	    {
            EnsureTransaction(()=> _session.Save(historyItem));
	    }
    }

}
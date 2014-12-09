namespace CustomSchedulers.Currency
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Net;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Repository;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	using StructureMap;
	using log4net;

	public class CurrencyUpdateController
	{		
		private static readonly ILog Log = LogManager.GetLogger( typeof( CurrencyUpdateController ) );

		private Dictionary<string, decimal> _LastCurrencyRates;
		private CurrencyRateRepository _CurrencyRateRepository;

		internal CurrencyUpdateController()
		{
		}

		private CurrencyRateRepository CurrencyRateRepository
		{
			get { return _CurrencyRateRepository ?? ( _CurrencyRateRepository = ObjectFactory.GetInstance<CurrencyRateRepository>() ); }
		}

		private List<string> GetCurrencyListFromWeb( out DateTime latestUpdated )
		{
			List<string> list = null;
			Log.Info( "Currency list retrieve started..." );
			try
			{
				_LastCurrencyRates = GetLastCurrencyRates(out latestUpdated);
				list = _LastCurrencyRates.Select( r => r.Key ).ToList();
			}
			catch(Exception ex)
			{
				Log.Error( "Currency list retrieve failed, please review error:" );
				Log.Error( ex );
				throw;
			}

			Log.Info( "Currency list retrieve successfully completed!" );

			return list;
		}

		private Dictionary<string, decimal> GetLastCurrencyRates(out DateTime latestUpdated)
		{
			try
			{
				latestUpdated = DateTime.Now.ToUniversalTime();

				Log.Info("Currency update latest rates started...");

				var webClient = new WebClient();
				var stream =
					webClient.OpenRead("http://finance.yahoo.com/webservice/v1/symbols/allcurrencies/quote;currency=true?format=json");
				if (stream == null)
				{
					throw new Exception("Can`t read Yahoo Financial Services, currency update not possible");

				}
				var reader = new StreamReader(stream);
				var request = reader.ReadToEnd();
				var data = JsonConvert.DeserializeObject(request) as JObject;
				if (data == null)
				{
					throw new InvalidDataException();
				}

				var dict = (from d in data["list"]["resources"].ToArray()
				            select new
				                   	{
				                   		price = d["resource"]["fields"]["price"].Value<decimal>(),
				                   		name =
				            	d["resource"]["fields"]["symbol"].Value<string>().Replace("=X", String.Empty).ToUpperInvariant()
				                   	}).ToDictionary(x => x.name, x => x.price);

				//translate from USD base to GPB
				string gbpLabel = CurrencyRateProviderYahoo.GbpLabel;

				if (!dict.ContainsKey(gbpLabel))
				{
					throw new Exception("Can`t get USD -> GBP currency rate, recalculations are not possible, will be terminate.");
				}

				var gbpRate = dict[gbpLabel];
				foreach (var rate in dict.ToArray())
				{
					dict[rate.Key] /= gbpRate;
				}
				dict.Remove(gbpLabel);
				if (dict.ContainsKey("USD"))
				{
					dict["USD"] = 1 / gbpRate;
				}
				else
				{
					dict.Add("USD", 1 / gbpRate);
				}
				dict.Add(gbpLabel, 1);

				Log.Info( "Currency update latest rates successfully completed!" );
				return dict;

			}
			catch(Exception ex)
			{
				Log.Error( "Currency update latest rates failed, please review error:" );
				Log.Error( ex );
				throw;
			}
		}

		private bool DatabaseHasStoredCurrencyList
		{
			get
			{
				return CurrencyRateRepository.HasCurrencyList();
			}
		}

		private void StoreToDatabaseCurrencyList( IEnumerable<string> data )
		{
			CurrencyRateRepository.EnsureTransaction(()=> data.ToList().ForEach( currencyName => CurrencyRateRepository.Save( new CurrencyData( currencyName) )));
		}

		private void StoreToDatabaseCurrencyHistory(IEnumerable<CurrencyHistoryData> history)
		{
            foreach(var historyData in history)
            {
				var currencyName = historyData.CurrencyName;
				var currency = CurrencyRateRepository.GetCurrencyOrCreate( currencyName );

				foreach(var hdi in historyData.History.OrderBy(i => i.Date))
				{
                    var historyItem = new CurrencyRateHistory
                    {
                        CurrencyData = currency,
                        Price = hdi.Price,
                        Updated = hdi.Date
                    };
                    currency.LastUpdated = hdi.Date;
                    currency.Price = hdi.Price;
                    CurrencyRateRepository.SaveHistoryItem(historyItem);
				}
            }
		}

		private List<CurrencyHistoryData> RetriveCurrencyHistoriesFromWeb( List<string> currencyList, DateTime startDate, DateTime endDate )
		{
			return currencyList.Where( IsDatabaseNotContainsHistory).Select( currencyName => RetriveCurrencyHistoryFromWeb( currencyName, startDate, endDate) ).ToList();
		}

		private bool IsDatabaseNotContainsHistory( string currencyName )
		{
			return !CurrencyRateRepository.IsContainsHistory( currencyName );
		}

		internal CurrencyHistoryData RetriveCurrencyHistoryFromWeb(string currencyName, DateTime startDate, DateTime endDate)
		{
			Log.Info(string.Format("Retrieve historical price data started for Currency {0}...", currencyName));
			var data = new CurrencyHistoryData(currencyName);

			if (currencyName.Equals(CurrencyRateProviderYahoo.GbpLabel, StringComparison.InvariantCultureIgnoreCase))
			{
				data.History.Add(new CurrencyHistoryItemData(startDate, 1));
				return data;
			}

			var yahooService = new CurrencyRateProviderYahoo();
			var yahooData = yahooService.RetriveData(currencyName, startDate, endDate);
			data.AddHistory(new CurrencyRateHistoryContainer(yahooData));

			Log.Info(string.Format("Retrieve historical price data successfully completed for Currency {0}!", currencyName));
			return data;
		}

		private void InternalRun()
		{
			DateTime latestUpdated;
			var currencyList = GetCurrencyListFromWeb(out latestUpdated);
			Log.Info( string.Format( "Currency list contains {0} items", currencyList.Count() ) );
			if ( !DatabaseHasStoredCurrencyList )
			{
				Log.Info( "Currency list store to DB started..." );
				StoreToDatabaseCurrencyList( currencyList );
				Log.Info( "Currency list store to DB successfully completed!" );
			}

			var now = DateTime.Now.Date;

			var history = RetriveCurrencyHistoriesFromWeb( currencyList, now.AddYears( -1 ), now );

            CurrencyRateRepository.EnsureTransaction(() =>
            {
			    Log.Info( string.Format( "Currency list with history data {0} items", history.Count() ) );
			    if ( history != null )
			    {
				    Log.Info( "Store currency historical price started" );
				    StoreToDatabaseCurrencyHistory( history );
				    Log.Info( "Store currency historical price successfully completed!" );
			    }

                var latestData = _LastCurrencyRates.Select(pair =>
                    {
                        var currencyName = pair.Key;
                        var price = pair.Value;
                        var data = new CurrencyHistoryData(currencyName);

                        data.History.Add(new CurrencyHistoryItemData(latestUpdated, price));

                        return data;
                    }
                );

                Log.Info("Store latest currency rates started");
                StoreToDatabaseCurrencyHistory(latestData);
                Log.Info("Store latest currency rates successfully completed!");                    
            });
		}

		public static void Run()
		{
			new CurrencyUpdateController().InternalRun();
		}

	}
}

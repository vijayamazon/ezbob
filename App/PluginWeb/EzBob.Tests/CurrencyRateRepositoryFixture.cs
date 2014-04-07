using System;
using ApplicationMng.Model;
using EZBob.DatabaseLib.Model.Database;
using NHibernate;
using NUnit.Framework;
using EZBob.DatabaseLib.Repository;

namespace EzBob.Tests
{

	[TestFixture]
	public class CurrencyRateRepositoryFixture : InMemoryDbTestFixtureBase
	{
		private ISession _session;
		private CurrencyRateRepository _currency;

		[TestFixtureSetUp]
		public void FixtureSetUp()
		{
			InitialiseNHibernate(typeof(Customer).Assembly, typeof(User).Assembly);
		}

		[SetUp]
		public void SetUp()
		{
			_session = CreateSession();
			_currency = new CurrencyRateRepository(_session);
		}

		[Test]
		public void history_rate_returns_1_when_no_currency()
		{
			var rate = _currency.GetCurrencyHistoricalRate(new DateTime(2012, 1, 1), "USD");
			Assert.That(rate, Is.EqualTo(1));
		}

		[Test]
		public void history_rate_returns_curreyncy_price_if_no_history_data()
		{
			var usd = new CurrencyData() { Name = "USD", Price = 5 };
			_currency.Save(usd);

			var rate = _currency.GetCurrencyHistoricalRate(new DateTime(2012, 1, 1), "USD");

			Assert.That(rate, Is.EqualTo(5));
		}

		[Test]
		public void history_rate_returns_curreyncy_history_rate_if_it_present()
		{
			var usd = new CurrencyData() { Name = "USD", Price = 5 };
			var history = new CurrencyRateHistory { CurrencyData = usd, Price = 8, Updated = new DateTime(2010, 1, 1) };

			_currency.Save(usd);
			_currency.SaveHistoryItem(history);

			var rate = _currency.GetCurrencyHistoricalRate(new DateTime(2012, 1, 1), "USD");

			Assert.That(rate, Is.EqualTo(8));
		}

		[Test]
		public void history_rate_returns_curreyncy_price_if_history_data_only_for_later_dates()
		{
			var usd = new CurrencyData() { Name = "USD", Price = 5 };
			var history = new CurrencyRateHistory { CurrencyData = usd, Price = 8, Updated = new DateTime(2014, 1, 1) };

			_currency.Save(usd);
			_currency.SaveHistoryItem(history);

			var rate = _currency.GetCurrencyHistoricalRate(new DateTime(2012, 1, 1), "USD");

			Assert.That(rate, Is.EqualTo(5));
		}

	}
}
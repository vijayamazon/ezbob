namespace EzBob.Web.Code.Bank
{
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database;
	using EzBob.Models;
	using YodleeLib.connector;
	using log4net;

	public interface IYodleeAccountChecker {
		void Check(Customer customer, string accountNumber, string sortcode, string bankAccountType);
	}

	public class YodleeAccount
	{
		public string AccountNumber { get; set; }
		public string RoutingNumber { get; set; }
	}

	public class YodleeAccountChecker : IYodleeAccountChecker
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(YodleeAccountChecker));

		public void Check(Customer customer, string accountNumber, string sortcode, string bankAccountType)
		{
			if (customer.IsOffline.HasValue && !customer.IsOffline.Value)
			{
				return;
			}

			if (customer.IsTest)
			{
				return;
			}

			if (!customer.GetYodleeAccounts().ToList().Any())
			{
				return;
			}

			var yodleeServiceInfo = new YodleeServiceInfo();
			var yodleeAcounts = customer.CustomerMarketPlaces
				.Where(m => m.Marketplace.InternalId == yodleeServiceInfo.InternalId && m.DisplayName != "ParsedBank")
				.SelectMany(x => x.YodleeOrders)
				.SelectMany(o => o.OrderItems)
				.Select(i => new YodleeAccount {
					AccountNumber = i.accountNumber,
					RoutingNumber = i.routingNumber
				});

			foreach (var account in yodleeAcounts)
			{
				Log.DebugFormat("AccountNumber {0} SortCode {1} Yodlee Account {2} Yodlee Routing Number {3}", 
					accountNumber, sortcode, account.AccountNumber, account.RoutingNumber);
				
				if (string.IsNullOrEmpty(account.AccountNumber)) continue;

				var number = account.AccountNumber.Replace("x", "").Replace(" ", "");
				//var routing = account.RoutingNumber.Replace("-", "").Replace(" ", "");

				if (accountNumber.Contains(number))
				{
					return;
				}
			}

			throw new YodleeAccountNotFoundException();
		}
	}
}
namespace EzBob.Web.Areas.Customer.Models
{
	using FreeAgent;
	using PayPoint;
	using EzBob.Models;
    using System.Collections.Generic;
    using System.Linq;
    using EKM;
    using EZBob.DatabaseLib.Model.Database;
    using AmazonLib;
    using PayPal;
    using YodleeLib.connector;
    using eBayLib;
    using Integration.Volusion;
    using Integration.Play;

    public static class CustomerExtensions
    {
        public static IEnumerable<SimpleMarketPlaceModel> GetEbayMarketPlaces(this Customer customer)
        {
            var marketplaces = GetEbayCustomerMarketPlaces(customer).Select(m => new SimpleMarketPlaceModel { displayName = m.DisplayName });
            return marketplaces;
        }

        public static IEnumerable<MP_CustomerMarketPlace> GetEbayCustomerMarketPlaces(this Customer customer)
        {
            var ebay = new eBayDatabaseMarketPlace();
            return customer.CustomerMarketPlaces.Where(m => m.Marketplace.InternalId == ebay.InternalId);
        }

        public static IEnumerable<MP_CustomerMarketPlace> GetPayPalCustomerMarketPlaces(this Customer customer)
        {
            var paypal = new PayPalDatabaseMarketPlace();
            return customer.CustomerMarketPlaces.Where(m => m.Marketplace.InternalId == paypal.InternalId);
        }

        public static IEnumerable<SimpleMarketPlaceModel> GetPayPalAccountsSimple(this Customer customer)
        {
            return GetPayPalCustomerMarketPlaces(customer).ToList().Select(m => new SimpleMarketPlaceModel { displayName = m.DisplayName });
        }

        public static IEnumerable<PaymentAccountsModel> GetPayPalAccounts(this Customer customer)
        {
            var marketpalces = GetPayPalCustomerMarketPlaces(customer);
            return marketpalces.Select(m => PayPalModelBuilder.CreatePayPalAccountModelModel(m)).ToList();
        }

        public static IEnumerable<SimpleMarketPlaceModel> GetAmazonMarketPlaces(this Customer customer)
        {
            var marketplaces = GetAmazonMP(customer);

            var simpleMarketPlaceModels = marketplaces.Select(m => new SimpleMarketPlaceModel { displayName = m.DisplayName });
            return simpleMarketPlaceModels;
        }

		public static IEnumerable<SimpleMarketPlaceModel> GetEkmShops(this Customer customer)
		{
			var oEsi = new EkmServiceInfo();
			var marketplaces = customer.CustomerMarketPlaces.Where(m => m.Marketplace.InternalId == oEsi.InternalId);
			var simpleMarketPlaceModels = marketplaces.Select(m => new SimpleMarketPlaceModel { displayName = m.DisplayName });
			return simpleMarketPlaceModels;
		}

		public static IEnumerable<SimpleMarketPlaceModel> GetFreeAgentAccounts(this Customer customer)
		{
			var oEsi = new FreeAgentServiceInfo();
			var marketplaces = customer.CustomerMarketPlaces.Where(m => m.Marketplace.InternalId == oEsi.InternalId);
			var simpleMarketPlaceModels = marketplaces.Select(m => new SimpleMarketPlaceModel { displayName = m.DisplayName });
			return simpleMarketPlaceModels;
		}

        public static IEnumerable<SimpleMarketPlaceModel> GetVolusionShops(this Customer customer) {
	        var oVsi = new VolusionServiceInfo();
            var marketplaces = customer.CustomerMarketPlaces.Where(m => m.Marketplace.InternalId == oVsi.InternalId);
            var simpleMarketPlaceModels = marketplaces.Select(m => new SimpleMarketPlaceModel { displayName = m.DisplayName });
            return simpleMarketPlaceModels;
        } // GetVolusionShops

        public static IEnumerable<SimpleMarketPlaceModel> GetPlayShops(this Customer customer) {
	        var oVsi = new PlayServiceInfo();
            var marketplaces = customer.CustomerMarketPlaces.Where(m => m.Marketplace.InternalId == oVsi.InternalId);
            var simpleMarketPlaceModels = marketplaces.Select(m => new SimpleMarketPlaceModel { displayName = m.DisplayName });
            return simpleMarketPlaceModels;
        } // GetPlayShops

        public static IEnumerable<SimpleMarketPlaceModel> GetPayPointAccounts(this Customer customer)
        {
            var payPointServiceInfo = new PayPointServiceInfo();
            var marketplaces = customer.CustomerMarketPlaces.Where(m => m.Marketplace.InternalId == payPointServiceInfo.InternalId);
            var simpleMarketPlaceModels = marketplaces.Select(m => new SimpleMarketPlaceModel { displayName = m.DisplayName });
            return simpleMarketPlaceModels;
        }

		public static IEnumerable<SimpleMarketPlaceModel> GetYodleeAccounts(this Customer customer)
		{
			var yodleeServiceInfo = new YodleeServiceInfo();
			var marketplaces = customer.CustomerMarketPlaces.Where(m => m.Marketplace.InternalId == yodleeServiceInfo.InternalId);
			var simpleMarketPlaceModels = marketplaces.Select(m => new SimpleMarketPlaceModel { displayName = m.DisplayName });
			return simpleMarketPlaceModels;
		}

        public static List<MP_CustomerMarketPlace> GetAmazonMP(this Customer customer)
        {
            var amazon = new AmazonDatabaseMarketPlace();

            var marketplaces = customer.CustomerMarketPlaces
                .Where(mp => mp.Marketplace.InternalId == amazon.InternalId)
                .ToList();
            return marketplaces;
        }
    }
}
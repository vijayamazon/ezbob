namespace EzBob.Models {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database;
	using EzBob.Models.Marketplaces.Builders;
	using EzBob.PayPal;
	using StructureMap;
	using YodleeLib.connector;
	using EzBob.eBayLib;

	public static class CustomerExtensions {
		public static IEnumerable<MP_CustomerMarketPlace> GetEbayCustomerMarketPlaces(this Customer customer) {
			var ebay = new eBayDatabaseMarketPlace();
			return customer.CustomerMarketPlaces.Where(m => m.Marketplace.InternalId == ebay.InternalId);
		}

		public static IEnumerable<MP_CustomerMarketPlace> GetPayPalCustomerMarketPlaces(this Customer customer) {
			var paypal = new PayPalDatabaseMarketPlace();
			return customer.CustomerMarketPlaces.Where(m => m.Marketplace.InternalId == paypal.InternalId);
		}
		
		public static IEnumerable<SimpleMarketPlaceModel> GetYodleeAccounts(this Customer customer) {
			var yodleeServiceInfo = new YodleeServiceInfo();
			var marketplaces = customer.CustomerMarketPlaces.Where(m => m.Marketplace.InternalId == yodleeServiceInfo.InternalId);
			var simpleMarketPlaceModels = marketplaces.Select(m => new SimpleMarketPlaceModel { displayName = m.DisplayName, MpId = m.Marketplace.Id, MpName = m.Marketplace.Name });
			return simpleMarketPlaceModels;
		}

		public static IEnumerable<SimpleMarketPlaceModel> GetMarketPlaces(this Customer customer) {
			return
				customer.CustomerMarketPlaces.Where(m => m.Disabled == false).Select(
					m =>
					new SimpleMarketPlaceModel {
						displayName = m.DisplayName,
						MpId = m.Marketplace.Id,
						MpName = m.Marketplace.Name == "Pay Pal" ? "paypal" : m.Marketplace.Name
					}).ToList();
		}
	}
}

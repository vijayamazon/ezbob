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

		public static DateTime GetMarketplaceOriginationDate(this Customer customer) {
			DateTime now = DateTime.UtcNow;

			if (customer == null)
				return now;

			IEnumerable<DateTime> dates = customer.CustomerMarketPlaces
				.Where(m =>
					!m.Disabled &&
					m.Marketplace.InternalId != CompanyFiles && (
						!m.Marketplace.IsPaymentAccount ||
						m.Marketplace.InternalId == PayPal ||
						m.Marketplace.InternalId == Hmrc
					)
				)
				.Select(mp => {
					IMarketplaceModelBuilder builder =
						ObjectFactory.TryGetInstance<IMarketplaceModelBuilder>(mp.Marketplace.GetType().ToString()) ??
						ObjectFactory.GetNamedInstance<IMarketplaceModelBuilder>("DEFAULT");

					builder.UpdateOriginationDate(mp);
					return mp.OriginationDate ?? now;
				});

			try {
				return dates.Min();
			}
			catch (ArgumentNullException) {
				return now;
			}
			catch (InvalidOperationException) {
				return now;
			} // try
		} // GetMarketplaceOriginationDate

		private static readonly Guid Hmrc = new Guid("AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA");
		private static readonly Guid PayPal = new Guid("3FA5E327-FCFD-483B-BA5A-DC1815747A28");
		private static readonly Guid CompanyFiles = new Guid("1C077670-6D6C-4CE9-BEBC-C1F9A9723908");

	}
}

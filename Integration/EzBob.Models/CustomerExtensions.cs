namespace EzBob.Models {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database;
	using EzBob.Models.Marketplaces;
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


		public static List<PaymentAccountsModel> GetPaymentAccounts(this Customer customer) {
			return
				customer.CustomerMarketPlaces.Where(m => m.Marketplace.IsPaymentAccount)
					.Select(m => PayPalModelBuilder.CreatePayPalAccountModel(m))
					.ToList();
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

		public static DateTime GetMarketplaceOriginationDate(
			this Customer customer,
			bool? isPaymentAccount = null,
			Func<MP_CustomerMarketPlace, bool> oIncludeMp = null
		) {
			DateTime now = DateTime.UtcNow;

			if (customer == null)
				return now;

			IEnumerable<DateTime> dates = customer.CustomerMarketPlaces
				.Where(m =>
					!m.Disabled && (
						isPaymentAccount == null || m.Marketplace.IsPaymentAccount == isPaymentAccount.Value
					) && (
						oIncludeMp == null || oIncludeMp(m)
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
	}
}
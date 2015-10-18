namespace EzBob.Models {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using CompanyFiles;
	using EZBob.DatabaseLib.Model.Database;
	using EzBob.Models.Marketplaces.Builders;
	using EzBob.PayPal;
	using StructureMap;
	using YodleeLib.connector;
	using EzBob.eBayLib;
	using Integration.ChannelGrabberConfig;

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
			var yodlee = new YodleeServiceInfo();
			var financialDocuments = new CompanyFilesServiceInfo();
			var hmrc = Configuration.Instance.GetVendorInfo("HMRC");

			var mps = customer.CustomerMarketPlaces.Where(m => m.Disabled == false);
			var simpleMps = new List<SimpleMarketPlaceModel>();
			foreach (var m in mps) {
				if (m.Marketplace.InternalId == yodlee.InternalId) {
					simpleMps.Add(new SimpleMarketPlaceModel {
						displayName = m.DisplayName,
						MpId = m.Marketplace.Id,
						MpName = m.DisplayName == "ParsedBank" ? m.Marketplace.Name + "Upload" : m.Marketplace.Name
					});
				}

				else if (m.Marketplace.InternalId == hmrc.Guid()) {
					simpleMps.Add(new SimpleMarketPlaceModel {
						displayName = m.DisplayName,
						MpId = m.Marketplace.Id,
						MpName = m.DisplayName.Contains("@") ? m.Marketplace.Name + "Upload" : m.Marketplace.Name
					});
				} 
				
				else if (m.Marketplace.InternalId == financialDocuments.InternalId) {
					var hasBankStatements = customer.CompanyFiles.Any(x => x.IsBankStatement.HasValue && x.IsBankStatement.Value);
					var hasCompanyFiles = customer.CompanyFiles.Any(x => !x.IsBankStatement.HasValue || !x.IsBankStatement.Value);

					if (hasCompanyFiles) {
						simpleMps.Add(new SimpleMarketPlaceModel {
							displayName = m.DisplayName,
							MpId = m.Marketplace.Id,
							MpName = m.Marketplace.Name
						});
					}

					if (hasBankStatements) {
						simpleMps.Add(new SimpleMarketPlaceModel {
							displayName = m.DisplayName,
							MpId = m.Marketplace.Id,
							MpName = yodlee.DisplayName + "Upload"
						});
					}
				} 
				
				else {
					simpleMps.Add(new SimpleMarketPlaceModel {
						displayName = m.DisplayName,
						MpId = m.Marketplace.Id,
						MpName = m.Marketplace.Name == "Pay Pal" ? "paypal" : m.Marketplace.Name
					});
				}
			}

			return simpleMps;
		}
	}
}

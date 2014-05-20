﻿namespace EZBob.DatabaseLib {
	using System;
	using EzBob.CommonLib;
	using Common;
	using DatabaseWrapper;
	using Model.Database;
	using Ezbob.Utils.Security;

	public partial class DatabaseDataHelper : IDatabaseDataHelper {
		public IDatabaseCustomerMarketPlace SaveOrUpdateCustomerMarketplace(
			string displayName,
			IMarketplaceType marketplaceType,
			IMarketPlaceSecurityInfo securityData,
			Customer customer,
			string amazonMarketPlaceId = null
		) {
			return SaveOrUpdateCustomerMarketplace(
				displayName,
				marketplaceType,
				SerializeDataHelper.Serialize(securityData),
				customer,
				amazonMarketPlaceId
			);
		} // SaveOrUpdateCustomerMarketplace

		public IDatabaseCustomerMarketPlace SaveOrUpdateCustomerMarketplace(
			string displayName,
			IMarketplaceType marketplaceType,
			string password,
			Customer customer
		) {
			return SaveOrUpdateCustomerMarketplace(
				displayName,
				marketplaceType,
				SecurityUtils.EncryptBytes(password),
				customer
			);
		} // SaveOrUpdateCustomerMarketplace

		public IDatabaseCustomerMarketPlace SaveOrUpdateCustomerMarketplace(
			string displayName,
			IMarketplaceType marketplaceType,
			byte[] serializedSecurityData,
			Customer customer,
			string amazonMarketPlaceId = null
		) {
			int customerMarketPlaceId;
			var now = DateTime.UtcNow;

			var customerMarketPlace = _CustomerMarketplaceRepository.Get(customer.Id, marketplaceType.InternalId, displayName);

			if (customerMarketPlace != null) {
				customerMarketPlaceId = customerMarketPlace.Id;
				customerMarketPlace.SecurityData = serializedSecurityData;
				_CustomerMarketplaceRepository.Update(customerMarketPlace);
			}
			else {
				customerMarketPlace = new MP_CustomerMarketPlace {
					SecurityData = serializedSecurityData,
					Customer = customer,
					Marketplace = _MarketPlaceRepository.Get(marketplaceType.InternalId),
					DisplayName = displayName,
					Created = now,
					AmazonMarketPlace = amazonMarketPlaceId != null ? _amazonMarketPlaceTypeRepository.GetByMarketPlaceId(amazonMarketPlaceId) : null
				};

				customerMarketPlaceId = (int)_CustomerMarketplaceRepository.Save(customerMarketPlace);
			} // if

			customerMarketPlace.Updated = now;

			return CreateDatabaseCustomerMarketPlace(customer, marketplaceType, customerMarketPlace, customerMarketPlaceId);
		} // SaveOrUpdateCustomerMarketplace
	} // class DatabaseDataHelper
} // namespace EZBob.DatabaseLib.DatabaseDataHelper

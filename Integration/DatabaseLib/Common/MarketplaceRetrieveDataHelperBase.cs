namespace EZBob.DatabaseLib.Common {
	using System;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Utils;
	using Ezbob.Utils.Serialization;

	public abstract class MarketplaceRetrieveDataHelperBase : IMarketplaceRetrieveDataHelper {
		public IMarketplaceType Marketplace {
			get { return _Marketplace; }
		}

		public void CustomerMarketplaceUpdateAction(int nCustomerMarketplaceID) {
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace = GetDatabaseCustomerMarketPlace(nCustomerMarketplaceID);

			MP_CustomerMarketplaceUpdatingHistory historyRecord = Helper.GetHistoryItem(databaseCustomerMarketPlace);

			var action = new Func<IUpdateActionResultInfo>(() => new UpdateActionResultInfo {
				ElapsedTime = RetrieveAndAggregate(databaseCustomerMarketPlace, historyRecord),
			});

			Helper.CustomerMarketplaceUpdateAction(
				CustomerMarketplaceUpdateActionType.UpdateOrdersInfo,
				databaseCustomerMarketPlace,
				historyRecord,
				action
			);
		}

		public abstract IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(int customerMarketPlaceId);

		public void StoreOrUpdateCustomerSecurityInfo(Customer databaseCustomer, IMarketPlaceSecurityInfo securityData, string marketPlaceName) {
			Helper.SaveOrUpdateCustomerMarketplace(marketPlaceName, _Marketplace, securityData, databaseCustomer);
		}

		public virtual void Update(int nCustomerMarketplaceID) {
			CustomerMarketplaceUpdateAction(nCustomerMarketplaceID);
		}

		public void UpdateCustomerMarketplaceFirst(int customerMarketPlaceId) {
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace = GetDatabaseCustomerMarketPlace(customerMarketPlaceId);

			if (databaseCustomerMarketPlace.Disabled)
				return;

			Helper.UpdateCustomerMarketplaceData(
				databaseCustomerMarketPlace,
				historyRecord => InternalUpdateInfoFirst(databaseCustomerMarketPlace, historyRecord)
			);
		}

		public void UpdateCustomerMarketplaceRegular(int customerMarketPlaceId) {
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace = GetDatabaseCustomerMarketPlace(customerMarketPlaceId);

			if (databaseCustomerMarketPlace.Disabled)
				return;

			Helper.UpdateCustomerMarketplaceData(
				databaseCustomerMarketPlace,
				historyRecord => InternalUpdateInfo(databaseCustomerMarketPlace, historyRecord)
			);
		}

		protected DatabaseDataHelper Helper { get; private set; }

		protected MarketplaceRetrieveDataHelperBase(DatabaseDataHelper helper, DatabaseMarketplaceBaseBase marketplace) {
			Helper = helper;
			_Marketplace = marketplace;
		}

		protected IDatabaseCustomerMarketPlace GetDatabaseCustomerMarketPlace(int customerMarketPlaceId) {
			return Helper.GetDatabaseCustomerMarketPlace(_Marketplace, customerMarketPlaceId);
		}

		protected virtual void InternalUpdateInfo(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		) {
			RetrieveAndAggregate(databaseCustomerMarketPlace, historyRecord);
		}

		protected virtual void InternalUpdateInfoFirst(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		) {
			InternalUpdateInfo(databaseCustomerMarketPlace, historyRecord);
		}

		protected abstract ElapsedTimeInfo RetrieveAndAggregate(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		);

		protected TSecurityData RetrieveCustomerSecurityInfo<TSecurityData>(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
			where TSecurityData : IMarketPlaceSecurityInfo {
			return Serialized.Deserialize<TSecurityData>(databaseCustomerMarketPlace.SecurityData);
		}

		private readonly DatabaseMarketplaceBaseBase _Marketplace;
	}
}

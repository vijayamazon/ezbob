namespace EZBob.DatabaseLib.Common {
	using System;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Utils;
	using Ezbob.Utils.Serialization;

	public abstract class MarketplaceRetrieveDataHelperBase<TEnum> : IMarketplaceRetrieveDataHelper {
		protected DatabaseDataHelper Helper { get; private set; }

		private readonly DatabaseMarketplaceBase<TEnum> _Marketplace;

		protected MarketplaceRetrieveDataHelperBase(DatabaseDataHelper helper, DatabaseMarketplaceBase<TEnum> marketplace) {
			Helper = helper;
			_Marketplace = marketplace;
		}

		protected virtual void InternalUpdateInfo(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		) {
			RetrieveAndAggregate(databaseCustomerMarketPlace, historyRecord);
		}

		protected abstract ElapsedTimeInfo RetrieveAndAggregate(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		);

		protected abstract void AddAnalysisValues(
			IDatabaseCustomerMarketPlace marketPlace,
			AnalysisDataInfo value
		);

		private IAnalysisDataInfo GetAnalysisValuesByCustomerMarketPlace(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace) {
			var rez = new AnalysisDataInfo(databaseCustomerMarketPlace, Helper.GetAnalyisisFunctions(databaseCustomerMarketPlace));

			AddAnalysisValues(databaseCustomerMarketPlace, rez);

			return rez;
		}

		protected virtual void InternalUpdateInfoFirst(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		) {
			InternalUpdateInfo(databaseCustomerMarketPlace, historyRecord);
		}

		public virtual void Update(int nCustomerMarketplaceID) {
			CustomerMarketplaceUpdateAction(nCustomerMarketplaceID);
		} // Update

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
		} // CustomerMarketplaceUpdateAction

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

		protected IDatabaseCustomerMarketPlace GetDatabaseCustomerMarketPlace(int customerMarketPlaceId) {
			return Helper.GetDatabaseCustomerMarketPlace(_Marketplace, customerMarketPlaceId);
		}

		public IMarketplaceType Marketplace {
			get { return _Marketplace; }
		}

		public IAnalysisDataInfo GetAnalysisValuesByCustomerMarketPlace(int customerMarketPlaceId) {
			MP_CustomerMarketPlace marketPlace = Helper.GetCustomerMarketPlace(customerMarketPlaceId);
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace = Helper.CreateDatabaseCustomerMarketPlace(marketPlace.DisplayName, _Marketplace, marketPlace.Customer);

			return GetAnalysisValuesByCustomerMarketPlace(databaseCustomerMarketPlace);
		}

		public void StoreOrUpdateCustomerSecurityInfo(Customer databaseCustomer, IMarketPlaceSecurityInfo securityData, string marketPlaceName) {
			Helper.SaveOrUpdateCustomerMarketplace(marketPlaceName, _Marketplace, securityData, databaseCustomer);
		}

		public abstract IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(int customerMarketPlaceId);

		protected TSecurityData RetrieveCustomerSecurityInfo<TSecurityData>(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
			where TSecurityData : IMarketPlaceSecurityInfo {
			return Serialized.Deserialize<TSecurityData>(databaseCustomerMarketPlace.SecurityData);
		}
	}
}

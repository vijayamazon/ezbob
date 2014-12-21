namespace EKM {
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using Ezbob.Utils.Security;
	using EzBob.CommonLib;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using EZBob.DatabaseLib.Model.Database;

	public class EkmRetriveDataHelper : MarketplaceRetrieveDataHelperBase<EkmDatabaseFunctionType> {
		private static readonly ASafeLog log = new SafeILog(typeof (EkmRetriveDataHelper));

		public EkmRetriveDataHelper(
			DatabaseDataHelper helper,
			DatabaseMarketplaceBase<EkmDatabaseFunctionType> marketplace
		) : base(helper, marketplace) {} // constructor

		protected override ElapsedTimeInfo RetrieveAndAggregate(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		) {
			// Retrieve data from EKM API
			var ordersList = EkmConnector.GetOrders(
				databaseCustomerMarketPlace.DisplayName,
				Encrypted.Decrypt(databaseCustomerMarketPlace.SecurityData),
				Helper.GetEkmDeltaPeriod(databaseCustomerMarketPlace)
				);

			var ekmOrderList = new List<EkmOrderItem>();
			foreach (var order in ordersList) {
				try {
					ekmOrderList.Add(order.ToEkmOrderItem());
				} catch (Exception e) {
					log.Error(e, "Failed to create EKMOrderItem from the original order {0}", order);
					throw;
				}
			} // for

			var elapsedTimeInfo = new ElapsedTimeInfo();

			var newOrders = new EkmOrdersList(DateTime.UtcNow, ekmOrderList);

			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				databaseCustomerMarketPlace.Id,
				ElapsedDataMemberType.StoreDataToDatabase,
				() => Helper.StoreEkmOrdersData(databaseCustomerMarketPlace, newOrders, historyRecord)
			);

			DbConnectionGenerator.Get().ExecuteNonQuery(
				"UpdateMpTotalsEkm",
				CommandSpecies.StoredProcedure,
				new QueryParameter("MpID", databaseCustomerMarketPlace.Id),
				new QueryParameter("HistoryID", historyRecord.Id)
			);

			return elapsedTimeInfo;
		} // RetrieveAndAggregate

		protected override void AddAnalysisValues(IDatabaseCustomerMarketPlace marketPlace, AnalysisDataInfo data) {
			// Nothing here.
		} // AddAnalysisValues

		public override IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(int customerMarketPlaceId) {
			return null;
		} // RetrieveCustomerSecurityInfo
	} // class EkmRetriveDataHelper
} // namespace

using EzBob.CommonLib;
using EzBob.CommonLib.Security;
using EzBob.CommonLib.TimePeriodLogic.DependencyChain;
using EzBob.CommonLib.TimePeriodLogic.DependencyChain.Factories;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using EZBob.DatabaseLib.DatabaseWrapper.FunctionValues;
using EZBob.DatabaseLib.DatabaseWrapper.Order;
using EZBob.DatabaseLib.Model.Database;
using System;
using System.Collections.Generic;
using Integration.ChannelGrabberAPI;
using log4net;

namespace Integration.Play {
	public class PlayRetriveDataHelper : MarketplaceRetrieveDataHelperBase<PlayDatabaseFunctionType> {
		private static readonly ILog ms_oLog = LogManager.GetLogger(typeof(PlayRetriveDataHelper));

		public PlayRetriveDataHelper(
			DatabaseDataHelper helper,
			DatabaseMarketplaceBase<PlayDatabaseFunctionType> marketplace
		) : base(helper, marketplace)
		{} // constructor

		protected override void InternalUpdateInfo(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		) {
			var securityInfo = (PlaySecurityInfo)RetrieveCustomerSecurityInfo(
				databaseCustomerMarketPlace.Id
			);

			// Store orders
			UpdateClientOrdersInfo(
				databaseCustomerMarketPlace,
				securityInfo,
				ActionAccessType.Full,
				historyRecord
			);
		} // InternalUpdateInfo

		private void UpdateClientOrdersInfo(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			PlaySecurityInfo securityInfo,
			ActionAccessType actionAccessType,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		) {
			List<ChannelGrabberOrder> oRawOrders = PlayConnector.GetOrders(
				ms_oLog,
				databaseCustomerMarketPlace.Customer,
				securityInfo.Name,
				securityInfo.Login
			);

			var oPlayOrders = new List<PlayOrderItem>();

			foreach (var oRaw in oRawOrders) {
				oPlayOrders.Add(new PlayOrderItem {
					CurrencyCode  = oRaw.CurrencyCode,
					OrderStatus   = oRaw.OrderStatus,
					NativeOrderId = oRaw.NativeOrderId,
					PaymentDate   = oRaw.PaymentDate,
					PurchaseDate  = oRaw.PurchaseDate,
					TotalCost     = oRaw.TotalCost
				});
			} // foreach

			var elapsedTimeInfo = new ElapsedTimeInfo();

			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.StoreDataToDatabase,
				() => Helper.StorePlayOrdersData(
					databaseCustomerMarketPlace,
					new PlayOrdersList(DateTime.UtcNow, oPlayOrders),
					historyRecord
				)
			);

			// retrieve orders
			var allOrders = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
									ElapsedDataMemberType.RetrieveDataFromDatabase,
									() => Helper.GetAllPlayOrdersData(DateTime.UtcNow, databaseCustomerMarketPlace));

			// calculate aggregated
			IEnumerable<IWriteDataInfo<PlayDatabaseFunctionType>> aggregatedData =
				ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
					elapsedTimeInfo,
					ElapsedDataMemberType.AggregateData,
					() => CreateOrdersAggregationInfo(allOrders, Helper.CurrencyConverter)
				);

			// store aggregated
			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.StoreAggregatedData,
				() => Helper.StoreToDatabaseAggregatedData(databaseCustomerMarketPlace, aggregatedData, historyRecord)
			);
		} // UpdateClientOrdersInfo

		protected override void AddAnalysisValues(
			IDatabaseCustomerMarketPlace marketPlace,
			AnalysisDataInfo data
		) {} // AddAnalysisValues

		public override IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(
			int customerMarketPlaceId
		) {
			return SerializeDataHelper.DeserializeType<PlaySecurityInfo>(
				GetDatabaseCustomerMarketPlace(customerMarketPlaceId).SecurityData
			);
		} // RetrieveSecurityInfo

		private IEnumerable<IWriteDataInfo<PlayDatabaseFunctionType>> CreateOrdersAggregationInfo(
			PlayOrdersList orders,
			ICurrencyConvertor currencyConverter
		) {
			var aggregateFunctionArray = new[] {
				PlayDatabaseFunctionType.AverageSumOfOrder,
				PlayDatabaseFunctionType.NumOfOrders, 
				PlayDatabaseFunctionType.TotalSumOfOrders
			};

			var updated = orders.SubmittedDate;

			var nodesCreationFactory = TimePeriodNodesCreationTreeFactoryFactory.CreateHardCodeTimeBoundaryCalculationStrategy();

			TimePeriodChainWithData<PlayOrderItem> timeChain =
				TimePeriodChainContructor.CreateDataChain(
					new TimePeriodNodeWithDataFactory<PlayOrderItem>(),
					orders,
					nodesCreationFactory
				);

			if (timeChain.HasNoData)
				return null;

			var timePeriodData = TimePeriodChainContructor.ExtractDataWithCorrectTimePeriod(timeChain, updated);

			var factory = new PlayOrdersAggregatorFactory();

			return DataAggregatorHelper.AggregateData(
				factory,
				timePeriodData,
				aggregateFunctionArray,
				updated,
				currencyConverter
			);
		} // CreateOrdersAggreationInfo
	} // class PlayRetrieveDataHelper
} // namespace
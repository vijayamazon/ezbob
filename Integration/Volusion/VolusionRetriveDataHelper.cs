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

namespace Integration.Volusion {
	public class VolusionRetriveDataHelper : MarketplaceRetrieveDataHelperBase<VolusionDatabaseFunctionType> {
		private static readonly ILog ms_oLog = LogManager.GetLogger(typeof(VolusionRetriveDataHelper));

		public VolusionRetriveDataHelper(
			DatabaseDataHelper helper,
			DatabaseMarketplaceBase<VolusionDatabaseFunctionType> marketplace
		) : base(helper, marketplace)
		{} // constructor

		protected override void InternalUpdateInfo(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		) {
			var securityInfo = (VolusionSecurityInfo)RetrieveCustomerSecurityInfo(
				databaseCustomerMarketPlace.Id
			);

			var ad = new VolusionAccountData {
				endpoint = securityInfo.Url,
				username = securityInfo.Login
			};

			var ctr = new Connector(ad, ms_oLog, databaseCustomerMarketPlace.Customer);

			List<ChannelGrabberOrder> oRawOrders = ctr.GetOrders();

			var oVolusionOrders = new List<VolusionOrderItem>();

			foreach (var oRaw in oRawOrders) {
				oVolusionOrders.Add(new VolusionOrderItem {
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
				() => Helper.StoreVolusionOrdersData(
					databaseCustomerMarketPlace,
					new VolusionOrdersList(DateTime.UtcNow, oVolusionOrders),
					historyRecord
				)
			);

			// retrieve orders
			var allOrders = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
									ElapsedDataMemberType.RetrieveDataFromDatabase,
									() => Helper.GetAllVolusionOrdersData(DateTime.UtcNow, databaseCustomerMarketPlace));

			// calculate aggregated
			IEnumerable<IWriteDataInfo<VolusionDatabaseFunctionType>> aggregatedData =
				ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
					elapsedTimeInfo,
					ElapsedDataMemberType.AggregateData,
					() => CreateOrdersAggregationInfo(allOrders, Helper.CurrencyConverter)
				);

			// Store aggregated
			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.StoreAggregatedData,
				() => Helper.StoreToDatabaseAggregatedData(databaseCustomerMarketPlace, aggregatedData, historyRecord)
			);
		} // InternalUpdateInfo

		protected override void AddAnalysisValues(
			IDatabaseCustomerMarketPlace marketPlace,
			AnalysisDataInfo data
		) {} // AddAnalysisValues

		public override IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(
			int customerMarketPlaceId
		) {
			return SerializeDataHelper.DeserializeType<VolusionSecurityInfo>(
				GetDatabaseCustomerMarketPlace(customerMarketPlaceId).SecurityData
			);
		} // RetrieveSecurityInfo

		private IEnumerable<IWriteDataInfo<VolusionDatabaseFunctionType>> CreateOrdersAggregationInfo(
			VolusionOrdersList orders,
			ICurrencyConvertor currencyConverter
		) {
			var aggregateFunctionArray = new[] {
				VolusionDatabaseFunctionType.AverageSumOfOrder,
				VolusionDatabaseFunctionType.NumOfOrders, 
				VolusionDatabaseFunctionType.TotalSumOfOrders
			};

			var updated = orders.SubmittedDate;

			var nodesCreationFactory = TimePeriodNodesCreationTreeFactoryFactory.CreateHardCodeTimeBoundaryCalculationStrategy();

			TimePeriodChainWithData<VolusionOrderItem> timeChain =
				TimePeriodChainContructor.CreateDataChain(
					new TimePeriodNodeWithDataFactory<VolusionOrderItem>(),
					orders,
					nodesCreationFactory
				);

			if (timeChain.HasNoData)
				return null;

			var timePeriodData = TimePeriodChainContructor.ExtractDataWithCorrectTimePeriod(timeChain, updated);

			var factory = new VolusionOrdersAggregatorFactory();

			return DataAggregatorHelper.AggregateData(
				factory,
				timePeriodData,
				aggregateFunctionArray,
				updated,
				currencyConverter
			);
		} // CreateOrdersAggreationInfo
	} // class VolusionRetrieveDataHelper
} // namespace
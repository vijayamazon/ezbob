using EzBob.CommonLib;
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
using Integration.ChannelGrabberConfig;
using log4net;

namespace Integration.ChannelGrabberFrontend {
	public class RetriveDataHelper : MarketplaceRetrieveDataHelperBase<FunctionType> {
		public RetriveDataHelper(
			DatabaseDataHelper helper,
			DatabaseMarketplaceBase<FunctionType> marketplace,
			VendorInfo oVendorInfo
		) : base(helper, marketplace) {
			m_oVendorInfo = oVendorInfo;
		} // constructor

		protected override void InternalUpdateInfo(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		) {
			var secInfo = SerializeDataHelper.DeserializeType<SecurityInfo>(
				GetDatabaseCustomerMarketPlace(databaseCustomerMarketPlace.Id).SecurityData
			);

			var ctr = new Connector(secInfo.AccountData, ms_oLog, databaseCustomerMarketPlace.Customer);

			var oRawOrders = ctr.GetOrders();

			var oChaGraOrders = new List<ChannelGrabberOrderItem>();

			foreach (var oRaw in oRawOrders) {
				oChaGraOrders.Add(new ChannelGrabberOrderItem {
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
				() => Helper.StoreChannelGrabberOrdersData(
					databaseCustomerMarketPlace,
					new ChannelGrabberOrdersList(DateTime.UtcNow, oChaGraOrders),
					historyRecord
				)
			);

			// retrieve orders
			var allOrders = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.RetrieveDataFromDatabase,
				() => Helper.GetAllChannelGrabberOrdersData(DateTime.UtcNow, databaseCustomerMarketPlace)
			);

			// calculate aggregated
			IEnumerable<IWriteDataInfo<FunctionType>> aggregatedData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
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
			return SerializeDataHelper.DeserializeType<SecurityInfo>(
				GetDatabaseCustomerMarketPlace(customerMarketPlaceId).SecurityData
			);
		} // RetrieveSecurityInfo

		private IEnumerable<IWriteDataInfo<FunctionType>> CreateOrdersAggregationInfo(
			ChannelGrabberOrdersList orders,
			ICurrencyConvertor currencyConverter
		) {
			var oFunctionTypes = new List<FunctionType>();
			m_oVendorInfo.Aggregators.ForEach(a => oFunctionTypes.Add(a.FunctionType()));

			var updated = orders.SubmittedDate;

			var nodesCreationFactory = TimePeriodNodesCreationTreeFactoryFactory.CreateHardCodeTimeBoundaryCalculationStrategy();

			TimePeriodChainWithData<ChannelGrabberOrderItem> timeChain =
				TimePeriodChainContructor.CreateDataChain(
					new TimePeriodNodeWithDataFactory<ChannelGrabberOrderItem>(),
					orders,
					nodesCreationFactory
				);

			if (timeChain.HasNoData)
				return null;

			var timePeriodData = TimePeriodChainContructor.ExtractDataWithCorrectTimePeriod(timeChain, updated);

			var factory = new OrdersAggregatorFactory();

			return DataAggregatorHelper.AggregateData(
				factory,
				timePeriodData,
				oFunctionTypes.ToArray(),
				updated,
				currencyConverter
			);
		} // CreateOrdersAggreationInfo

		private static readonly ILog ms_oLog = LogManager.GetLogger(typeof(RetriveDataHelper));

		private VendorInfo m_oVendorInfo;
	} // class RetrieveDataHelper
} // namespace
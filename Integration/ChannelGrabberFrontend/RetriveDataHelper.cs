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
			ms_oLog.DebugFormat("RetrieveDataHelper constructed for {0}", m_oVendorInfo.Name);
		} // constructor

		protected override void InternalUpdateInfo(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		) {
			ms_oLog.DebugFormat("start {0}", m_oVendorInfo.Name);

			AccountModel oSecInfo = null;

			try {
				oSecInfo = SerializeDataHelper.DeserializeType<AccountModel>(databaseCustomerMarketPlace.SecurityData);
			}
			catch (Exception e) {
				throw new ApiException(string.Format("Failed to deserialise security data for marketplace {0} ({1})",
					databaseCustomerMarketPlace.DisplayName, databaseCustomerMarketPlace.Id), e);
			}

			var ctr = new Connector(oSecInfo.Fill(), ms_oLog, databaseCustomerMarketPlace.Customer);

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

			ms_oLog.DebugFormat("end {0}", m_oVendorInfo.Name);
		} // InternalUpdateInfo

		protected override void AddAnalysisValues(
			IDatabaseCustomerMarketPlace marketPlace,
			AnalysisDataInfo data
		) {
			ms_oLog.DebugFormat("start and end {0}", m_oVendorInfo.Name);
		} // AddAnalysisValues

		public override IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(
			int customerMarketPlaceId
		) {
			ms_oLog.DebugFormat("start {0}", m_oVendorInfo.Name);

			var account = GetDatabaseCustomerMarketPlace(customerMarketPlaceId);

			try {
				var am = SerializeDataHelper.DeserializeType<AccountModel>(account.SecurityData);

				ms_oLog.DebugFormat("end {0}", m_oVendorInfo.Name);

				return am;
			}
			catch (Exception e) {
				ms_oLog.DebugFormat("exception {0}", m_oVendorInfo.Name);

				throw new ApiException(string.Format("Failed to deserialise security data for marketplace {0} ({1})",
					account.DisplayName, account.Id), e);
			}
		} // RetrieveSecurityInfo

		private IEnumerable<IWriteDataInfo<FunctionType>> CreateOrdersAggregationInfo(
			ChannelGrabberOrdersList orders,
			ICurrencyConvertor currencyConverter
		) {
			ms_oLog.DebugFormat("start {0}", m_oVendorInfo.Name);

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

			var aggData = DataAggregatorHelper.AggregateData(
				factory,
				timePeriodData,
				oFunctionTypes.ToArray(),
				updated,
				currencyConverter
			);

			ms_oLog.DebugFormat("end {0}", m_oVendorInfo.Name);

			return aggData;
		} // CreateOrdersAggreationInfo

		private static readonly ILog ms_oLog = LogManager.GetLogger(typeof(RetriveDataHelper));

		private readonly VendorInfo m_oVendorInfo;
	} // class RetrieveDataHelper
} // namespace
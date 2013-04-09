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

namespace Integration.Volusion {
	public class VolusionRetriveDataHelper : MarketplaceRetrieveDataHelperBase<VolusionDatabaseFunctionType> {
		public VolusionRetriveDataHelper(
			DatabaseDataHelper helper,
			DatabaseMarketplaceBase<VolusionDatabaseFunctionType> marketplace
		) : base(helper, marketplace)
		{} // constructor

		protected override void InternalUpdateInfo(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		) {
			var securityInfo = (VolusionSecurityInfo)this.RetrieveCustomerSecurityInfo(
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
			VolusionSecurityInfo securityInfo,
			ActionAccessType actionAccessType,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		) {
			// TODO: implement this.

			/*
			//retreive data from Volusion api
			var ordersList = VolusionConnector.GetOrders(securityInfo.Name, securityInfo.Password);

			var Iwant = new List<VolusionOrderItem>();
			foreach (Volusion.API.Order order in ordersList) {
				Iwant.Add(new VolusionOrderItem {
					VolusionOrderId = order.OrderID,
					OrderNumber = order.OrderNumber,
					CustomerID = order.CustomerID,
					CompanyName = order.CompanyName,
					FirstName = order.FirstName,
					LastName = order.LastName,
					EmailAddress = order.EmailAddress,
					TotalCost = order.TotalCost,
					OrderDate = DateTime.Parse(order.OrderDate),
					OrderStatus = order.OrderStatus,
					OrderDateIso = DateTime.Parse(order.OrderDateISO),
					OrderStatusColour = order.OrderStatusColour,
				});
			} // foreach

			var elapsedTimeInfo = new ElapsedTimeInfo();
			VolusionOrdersList allOrders = new VolusionOrdersList(DateTime.UtcNow, Iwant);
			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.StoreDataToDatabase,
				() => Helper.StoreVolusionOrdersData(
					databaseCustomerMarketPlace,
					allOrders,
					historyRecord
				)
			);

			// store agregated
			var aggregatedData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.AggregateData,
				() => CreateOrdersAggregationInfo(allOrders, Helper.CurrencyConverter)
			);

			// Save
			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.StoreAggregatedData,
				() => Helper.StoreToDatabaseAggregatedData(
					databaseCustomerMarketPlace,
					aggregatedData,
					historyRecord
				)
			);
			*/
		} // UpdateClientOrdersInfo

		protected override void AddAnalysisValues(
			IDatabaseCustomerMarketPlace marketPlace,
			AnalysisDataInfo data
		) {} // AddAnalysisValues

		public override IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(
			int customerMarketPlaceId
		) {
			var VolusionSecurityInfo = new VolusionSecurityInfo();
			IDatabaseCustomerMarketPlace customerMarketPlace = GetDatabaseCustomerMarketPlace(customerMarketPlaceId);
			VolusionSecurityInfo.Password = Encryptor.Decrypt(customerMarketPlace.SecurityData);
			VolusionSecurityInfo.Name = customerMarketPlace.DisplayName;
			VolusionSecurityInfo.MarketplaceId = customerMarketPlace.Id;
			return VolusionSecurityInfo;
		} // RetrieveSecurityInfo

		private IEnumerable<IWriteDataInfo<VolusionDatabaseFunctionType>> CreateOrdersAggregationInfo(
			VolusionOrdersList orders,
			ICurrencyConvertor currencyConverter
		) {
			var aggregateFunctionArray = new[] {
				VolusionDatabaseFunctionType.AverageSumOfOrder, 
				VolusionDatabaseFunctionType.NumOfOrders, 						
				VolusionDatabaseFunctionType.TotalSumOfOrders, 
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

			var timePeriodData = TimePeriodChainContructor.ExtractDataWithCorrectTimePeriod(
				timeChain,
				updated
			);

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
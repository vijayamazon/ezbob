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
using log4net;

namespace EKM
{
    public class EkmRetriveDataHelper : MarketplaceRetrieveDataHelperBase<EkmDatabaseFunctionType>
    {

        private static ILog log = LogManager.GetLogger(typeof(EkmRetriveDataHelper));

        public EkmRetriveDataHelper(DatabaseDataHelper helper, DatabaseMarketplaceBase<EkmDatabaseFunctionType> marketplace)
            : base(helper, marketplace)
        {

        }

        protected override void InternalUpdateInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
                                                   MP_CustomerMarketplaceUpdatingHistory historyRecord)
        {
            // Retreive data from ekm api
            var ordersList = EkmConnector.GetOrders(
				databaseCustomerMarketPlace.DisplayName,
				Encryptor.Decrypt(databaseCustomerMarketPlace.SecurityData),
				Helper.GetEkmDeltaPeriod(databaseCustomerMarketPlace)
			);

            var ekmOrderList = new List<EkmOrderItem>();
            foreach (var order in ordersList)
            {
                try
                {
                    ekmOrderList.Add(order.ToEkmOrderItem());
                }
                catch (Exception e)
                {
                    log.Error("Failed to create EKMOrderItem", e);
                    log.DebugFormat("Original order is: {0}", order);
                    throw;
                }
            }

            var elapsedTimeInfo = new ElapsedTimeInfo();

            var newOrders = new EkmOrdersList(DateTime.UtcNow, ekmOrderList);
            //store orders
            ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
                                    ElapsedDataMemberType.StoreDataToDatabase,
                                    () => Helper.StoreEkmOrdersData(databaseCustomerMarketPlace, newOrders, historyRecord));

            //retrieve orders
            var allOrders = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
                                    ElapsedDataMemberType.RetrieveDataFromDatabase,
                                    () => Helper.GetAllEkmOrdersData(DateTime.UtcNow, databaseCustomerMarketPlace));

            //calculate aggregated
            var aggregatedData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
                                    ElapsedDataMemberType.AggregateData,
                                    () => CreateOrdersAggregationInfo(allOrders, Helper.CurrencyConverter));
            
            // store aggregated
            ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
                            ElapsedDataMemberType.StoreAggregatedData,
                            () => Helper.StoreToDatabaseAggregatedData(databaseCustomerMarketPlace, aggregatedData, historyRecord));
        }

        protected override void AddAnalysisValues(IDatabaseCustomerMarketPlace marketPlace, AnalysisDataInfo data)
        {

        }

        public override IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(int customerMarketPlaceId)
        {
	        return null;
        }

        private IEnumerable<IWriteDataInfo<EkmDatabaseFunctionType>> CreateOrdersAggregationInfo(EkmOrdersList orders, ICurrencyConvertor currencyConverter)
        {
            var aggregateFunctionArray = new[]
                {
                    EkmDatabaseFunctionType.AverageSumOfOrder,
                    EkmDatabaseFunctionType.NumOfOrders,
                    EkmDatabaseFunctionType.TotalSumOfOrders,
                    EkmDatabaseFunctionType.AverageSumOfCancelledOrder,
                    EkmDatabaseFunctionType.NumOfCancelledOrders,
                    EkmDatabaseFunctionType.TotalSumOfCancelledOrders,
                    EkmDatabaseFunctionType.AverageSumOfOtherOrder,
                    EkmDatabaseFunctionType.NumOfOtherOrders,
                    EkmDatabaseFunctionType.TotalSumOfOtherOrders,
                    EkmDatabaseFunctionType.CancellationRate, 
                };

            var updated = orders.SubmittedDate;
            var nodesCreationFactory = TimePeriodNodesCreationTreeFactoryFactory.CreateHardCodeTimeBoundaryCalculationStrategy();
            TimePeriodChainWithData<EkmOrderItem> timeChain = TimePeriodChainContructor.CreateDataChain(new TimePeriodNodeWithDataFactory<EkmOrderItem>(), orders, nodesCreationFactory);

            if (timeChain.HasNoData)
            {
                return null;
            }

            var timePeriodData = TimePeriodChainContructor.ExtractDataWithCorrectTimePeriod(timeChain, updated);
            var factory = new EkmOrdersAggregatorFactory();
            return DataAggregatorHelper.AggregateData(factory, timePeriodData, aggregateFunctionArray, updated, currencyConverter);
        }

    }
}
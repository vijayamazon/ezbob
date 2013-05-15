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
            var securityInfo = (EkmSecurityInfo)RetrieveCustomerSecurityInfo(databaseCustomerMarketPlace.Id);

            //store orders
            UpdateClientOrdersInfo(databaseCustomerMarketPlace, securityInfo, ActionAccessType.Full, historyRecord);
        }

        private void UpdateClientOrdersInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, EkmSecurityInfo securityInfo, ActionAccessType actionAccessType, MP_CustomerMarketplaceUpdatingHistory historyRecord)
        {
            //retreive data from ekm api
            var ordersList = EkmConnector.GetOrders(securityInfo.Name, securityInfo.Password);

            var ekmOrderItem = new List<EkmOrderItem>();
            foreach (var order in ordersList)
            {
                try
                {
                    ekmOrderItem.Add(new EkmOrderItem
                                         {
                                             EkmOrderId = order.OrderID,
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
                }
                catch (Exception e)
                {
                    log.Error("Failed to create EKMOrderItem", e);
                    log.DebugFormat("Original order is: {0}", order);
                }
            }

            var elapsedTimeInfo = new ElapsedTimeInfo();
            var allOrders = new EkmOrdersList(DateTime.UtcNow, ekmOrderItem);
            ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
                                    ElapsedDataMemberType.StoreDataToDatabase,
                                    () => Helper.StoreEkmOrdersData(databaseCustomerMarketPlace, allOrders, historyRecord));


            //store agregated
            var aggregatedData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
                                    ElapsedDataMemberType.AggregateData,
                                    () => { return CreateOrdersAggregationInfo(allOrders, Helper.CurrencyConverter); });
            // Save
            ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
                            ElapsedDataMemberType.StoreAggregatedData,
                            () => Helper.StoreToDatabaseAggregatedData(databaseCustomerMarketPlace, aggregatedData, historyRecord));
        }

        protected override void AddAnalysisValues(IDatabaseCustomerMarketPlace marketPlace, AnalysisDataInfo data)
        {

        }

        public override IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(int customerMarketPlaceId)
        {
            var ekmSecurityInfo = new EkmSecurityInfo();
            IDatabaseCustomerMarketPlace customerMarketPlace = GetDatabaseCustomerMarketPlace(customerMarketPlaceId);
            ekmSecurityInfo.Password = Encryptor.Decrypt(customerMarketPlace.SecurityData);
            ekmSecurityInfo.Name = customerMarketPlace.DisplayName;
            ekmSecurityInfo.MarketplaceId = customerMarketPlace.Id;
            return ekmSecurityInfo;
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
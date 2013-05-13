﻿namespace YodleeLib.connector
{
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
	using System.Collections.Generic;

    public class YodleeRetriveDataHelper : MarketplaceRetrieveDataHelperBase<YodleeDatabaseFunctionType>
    {
        public YodleeRetriveDataHelper(DatabaseDataHelper helper, DatabaseMarketplaceBase<YodleeDatabaseFunctionType> marketplace)
            : base(helper, marketplace)
        {

        }

        protected override void InternalUpdateInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
                                                   MP_CustomerMarketplaceUpdatingHistory historyRecord)
        {
            var securityInfo = (YodleeSecurityInfo)RetrieveCustomerSecurityInfo(databaseCustomerMarketPlace.Id);

            //store orders
            UpdateClientOrdersInfo(databaseCustomerMarketPlace, securityInfo, ActionAccessType.Full, historyRecord);
        }

        private void UpdateClientOrdersInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, YodleeSecurityInfo securityInfo, ActionAccessType actionAccessType, MP_CustomerMarketplaceUpdatingHistory historyRecord)
        {
            //retreive data from Yodlee api
            var ordersList = YodleeConnector.GetOrders(securityInfo.Name, securityInfo.Password);
            //TODO: implement
            //var YodleeOrderItem = new List<YodleeOrderItem>();
            //foreach (var order in ordersList)
            //{
            //    YodleeOrderItem.Add(new YodleeOrderItem
            //    {
            //        YodleeOrderId = order.OrderID,
            //        OrderNumber = order.OrderNumber,
            //        CustomerID = order.CustomerID,
            //        CompanyName = order.CompanyName,
            //        FirstName = order.FirstName,
            //        LastName = order.LastName,
            //        EmailAddress = order.EmailAddress,
            //        TotalCost = order.TotalCost,
            //        OrderDate = DateTime.Parse(order.OrderDate),
            //        OrderStatus = order.OrderStatus,
            //        OrderDateIso = DateTime.Parse(order.OrderDateISO),
            //        OrderStatusColour = order.OrderStatusColour,
            //    });
            //}

            //var elapsedTimeInfo = new ElapsedTimeInfo();
            //var allOrders = new YodleeOrdersList(DateTime.UtcNow, YodleeOrderItem);
            //ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
            //                        ElapsedDataMemberType.StoreDataToDatabase,
            //                        () => Helper.StoreYodleeOrdersData(databaseCustomerMarketPlace, allOrders, historyRecord));


            ////store agregated
            //var aggregatedData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
            //                        ElapsedDataMemberType.AggregateData,
            //                        () => { return CreateOrdersAggregationInfo(allOrders, Helper.CurrencyConverter); });
            //// Save
            //ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
            //                ElapsedDataMemberType.StoreAggregatedData,
            //                () => Helper.StoreToDatabaseAggregatedData(databaseCustomerMarketPlace, aggregatedData, historyRecord));
        }

        protected override void AddAnalysisValues(IDatabaseCustomerMarketPlace marketPlace, AnalysisDataInfo data)
        {

        }

        public override IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(int customerMarketPlaceId)
        {
            var yodleeSecurityInfo = new YodleeSecurityInfo();
            IDatabaseCustomerMarketPlace customerMarketPlace = GetDatabaseCustomerMarketPlace(customerMarketPlaceId);
            yodleeSecurityInfo.Password = Encryptor.Decrypt(customerMarketPlace.SecurityData);
            yodleeSecurityInfo.Name = customerMarketPlace.DisplayName;
            yodleeSecurityInfo.MarketplaceId = customerMarketPlace.Id;
            return yodleeSecurityInfo;
        }
        
        
        private IEnumerable<IWriteDataInfo<YodleeDatabaseFunctionType>> CreateOrdersAggregationInfo(YodleeOrdersList orders, ICurrencyConvertor currencyConverter)
        {
            var aggregateFunctionArray = new[]
                {
                    YodleeDatabaseFunctionType.TotalExpense,
                    YodleeDatabaseFunctionType.TotlaIncome,
                    YodleeDatabaseFunctionType.CurrentBalance,
                };

            var updated = orders.SubmittedDate;
            var nodesCreationFactory = TimePeriodNodesCreationTreeFactoryFactory.CreateHardCodeTimeBoundaryCalculationStrategy();
            TimePeriodChainWithData<YodleeOrderItem> timeChain = TimePeriodChainContructor.CreateDataChain(new TimePeriodNodeWithDataFactory<YodleeOrderItem>(), orders, nodesCreationFactory);

            if (timeChain.HasNoData)
            {
                return null;
            }

            var timePeriodData = TimePeriodChainContructor.ExtractDataWithCorrectTimePeriod(timeChain, updated);
            var factory = new YodleeOrdersAggregatorFactory();
            return DataAggregatorHelper.AggregateData(factory, timePeriodData, aggregateFunctionArray, updated, currencyConverter);
        }

    }
}
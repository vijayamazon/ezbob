namespace YodleeLib.connector
{
    using System;
    using EzBob.CommonLib;
    using EzBob.CommonLib.ReceivedDataListLogic;
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
        public YodleeRetriveDataHelper(DatabaseDataHelper helper,
                                       DatabaseMarketplaceBase<YodleeDatabaseFunctionType> marketplace)
            : base(helper, marketplace)
        {
        }

        protected override void InternalUpdateInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
                                                   MP_CustomerMarketplaceUpdatingHistory historyRecord)
        {
            var securityInfo = (YodleeSecurityInfo)RetrieveCustomerSecurityInfo(databaseCustomerMarketPlace.Id);

            UpdateClientOrdersInfo(databaseCustomerMarketPlace, securityInfo, ActionAccessType.Full, historyRecord);
        }

        private void UpdateClientOrdersInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, YodleeSecurityInfo securityInfo, ActionAccessType actionAccessType, MP_CustomerMarketplaceUpdatingHistory historyRecord)
        {
            //retreive data from Yodlee api
            var ordersList = YodleeConnector.GetOrders(securityInfo.Name, securityInfo.Password, securityInfo.ItemId);
            var yodleeOrderItem = new YodleeOrderDictionary();
            yodleeOrderItem.Data = ordersList;

            var elapsedTimeInfo = new ElapsedTimeInfo();

            //save orders data
            ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
                                    ElapsedDataMemberType.StoreDataToDatabase,
                                    () => Helper.StoreYodleeOrdersData(databaseCustomerMarketPlace, yodleeOrderItem, historyRecord));


            //TODO: retrive all data from db
            //var allOrders = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
            //                        ElapsedDataMemberType.RetrieveDataFromDatabase,
            //                        () => Helper.GetAllAmazonOrdersData(submittedDate, databaseCustomerMarketPlace));

            //calculate transactions aggregated data
            var transactionsAggregatedData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
                                    ElapsedDataMemberType.AggregateData,
                                    () => CreateTransactionsAggregationInfo(yodleeOrderItem, Helper.CurrencyConverter));
            // store transactions aggregated data
            ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
                            ElapsedDataMemberType.StoreAggregatedData,
                            () => Helper.StoreToDatabaseAggregatedData(databaseCustomerMarketPlace, transactionsAggregatedData, historyRecord));

            //calculate accounts aggregated data
            var accountsAggregatedData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
                                    ElapsedDataMemberType.AggregateData,
                                    () => CreateAccountsAggregationInfo(yodleeOrderItem, Helper.CurrencyConverter));
            // store accounts aggregated data
            ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
                            ElapsedDataMemberType.StoreAggregatedData,
                            () => Helper.StoreToDatabaseAggregatedData(databaseCustomerMarketPlace, accountsAggregatedData, historyRecord));
        }

        public override IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(int customerMarketPlaceId)
        {
            return SerializeDataHelper.DeserializeType<YodleeSecurityInfo>(
                GetDatabaseCustomerMarketPlace(customerMarketPlaceId).SecurityData);

        }

        protected override void AddAnalysisValues(IDatabaseCustomerMarketPlace marketPlace, AnalysisDataInfo data)
        {

        }

        private IEnumerable<IWriteDataInfo<YodleeDatabaseFunctionType>> CreateTransactionsAggregationInfo(YodleeOrderDictionary orders, ICurrencyConvertor currencyConverter)
        {
            var aggregateTransactionsArray = new[]
                {
                    YodleeDatabaseFunctionType.TotalExpense,
                    YodleeDatabaseFunctionType.TotlaIncome,
                };

            var updated = DateTime.UtcNow;//todo:use time from server
            var nodesCreationFactory = TimePeriodNodesCreationTreeFactoryFactory.CreateHardCodeTimeBoundaryCalculationStrategy();

            ReceivedDataListTimeDependentBase<YodleeTransactionItem> yodleeTransactionList = YodleeTransactionList.Create(updated, orders);
            var timeChain = TimePeriodChainContructor.CreateDataChain(new TimePeriodNodeWithDataFactory<YodleeTransactionItem>(), yodleeTransactionList, nodesCreationFactory);

            if (timeChain.HasNoData)
            {
                return null;
            }

            var timePeriodChain = TimePeriodChainContructor.ExtractDataWithCorrectTimePeriod(timeChain, updated);

            var transactionsFactory = new YodleeTransactionsAggregatorFactory();

            return DataAggregatorHelper.AggregateData(transactionsFactory, timePeriodChain, aggregateTransactionsArray, updated, currencyConverter);
        }

        private IEnumerable<IWriteDataInfo<YodleeDatabaseFunctionType>> CreateAccountsAggregationInfo(YodleeOrderDictionary orders, ICurrencyConvertor currencyConverter)
        {
            var aggregateAccountsArray = new[]
                {
                    YodleeDatabaseFunctionType.CurrentBalance,
                    YodleeDatabaseFunctionType.AvailableBalance,
                };

            var updated = DateTime.UtcNow;//todo:use time from server
            var nodesCreationFactory = TimePeriodNodesCreationTreeFactoryFactory.CreateHardCodeTimeBoundaryCalculationStrategy();

            ReceivedDataListTimeDependentBase<YodleeAccountItem> yodleeAccountsList = YodleeAccountList.Create(updated, orders);
            var timeChain = TimePeriodChainContructor.CreateDataChain(new TimePeriodNodeWithDataFactory<YodleeAccountItem>(), yodleeAccountsList, nodesCreationFactory);

            if (timeChain.HasNoData)
            {
                return null;
            }

            var timePeriodChain = TimePeriodChainContructor.ExtractDataWithCorrectTimePeriod(timeChain, updated);
            var accountsFactory = new YodleeAccountsAggregatorFactory();
            return DataAggregatorHelper.AggregateData(accountsFactory, timePeriodChain, aggregateAccountsArray, updated, currencyConverter);
        }
    }
}
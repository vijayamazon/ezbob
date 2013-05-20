namespace YodleeLib.connector
{
    using System;
    using System.Linq;
    using EZBob.DatabaseLib.DatabaseWrapper.ValueType;
    using EzBob.CommonLib;
    using EzBob.CommonLib.ReceivedDataListLogic;
    using EzBob.CommonLib.Security;
    using EzBob.CommonLib.TimePeriodLogic;
    using EzBob.CommonLib.TimePeriodLogic.DependencyChain;
    using EzBob.CommonLib.TimePeriodLogic.DependencyChain.Factories;
    using EZBob.DatabaseLib;
    using EZBob.DatabaseLib.Common;
    using EZBob.DatabaseLib.DatabaseWrapper;
    using EZBob.DatabaseLib.DatabaseWrapper.FunctionValues;
    using EZBob.DatabaseLib.DatabaseWrapper.Order;
    using EZBob.DatabaseLib.Model.Database;
    using System.Collections.Generic;
    using StructureMap;
    using config;

    public class YodleeRetriveDataHelper : MarketplaceRetrieveDataHelperBase<YodleeDatabaseFunctionType>
    {
        // private readonly IYodleeMarketPlaceConfig _Config;
        public YodleeRetriveDataHelper(DatabaseDataHelper helper,
                                       DatabaseMarketplaceBase<YodleeDatabaseFunctionType> marketplace)
            : base(helper, marketplace)
        {
            // _Config = ObjectFactory.GetInstance<IYodleeMarketPlaceConfig>();  
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
            var ordersList = YodleeConnector.GetOrders(securityInfo.Name, securityInfo.Password, securityInfo.ItemId);
            var yodleeOrderItem = new YodleeOrderItem();
            yodleeOrderItem.Data = ordersList;

            var elapsedTimeInfo = new ElapsedTimeInfo();

            ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
                                    ElapsedDataMemberType.StoreDataToDatabase,
                                    () => Helper.StoreYodleeOrdersData(databaseCustomerMarketPlace, yodleeOrderItem, historyRecord));


            ////store agregated
            var aggregatedData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
                                    ElapsedDataMemberType.AggregateData,
                                    () => { return CreateOrdersAggregationInfo(yodleeOrderItem, Helper.CurrencyConverter); });
            // Save
            //ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
            //                ElapsedDataMemberType.StoreAggregatedData,
            //                () => Helper.StoreToDatabaseAggregatedData(databaseCustomerMarketPlace, aggregatedData, historyRecord));
        }

        public override IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(int customerMarketPlaceId)
        {
            return SerializeDataHelper.DeserializeType<YodleeSecurityInfo>(
                GetDatabaseCustomerMarketPlace(customerMarketPlaceId).SecurityData);

        }

        protected override void AddAnalysisValues(IDatabaseCustomerMarketPlace marketPlace, AnalysisDataInfo data)
        {
            /*
            var items = Helper.GetYodleeOrderItems()
                .Where(f => f.Order.CustomerMarketPlace.Id == marketPlace.Id && f.Order.HistoryRecord.UpdatingStart != null && f.Order.HistoryRecord.UpdatingEnd != null).ToList();

            if (items.Any())
            {
                var itemsParams = new List<IAnalysisDataParameterInfo>();

                items.ForEach(af =>
                {
                    {
                        if (af.Order.HistoryRecord != null)
                        {
                            DateTime? afDate = af.Order.HistoryRecord.UpdatingStart;
                            var timePeriod = TimePeriodFactory.CreateById(afp.TimePeriod.InternalId);
                            var c = new AnalysisDataParameterInfo("Number of reviews", timePeriod, DatabaseValueType.Integer, afp.Count);
                            var g = new AnalysisDataParameterInfo("Negative Feedback rate", timePeriod, DatabaseValueType.Integer, afp.Negative);
                            var n = new AnalysisDataParameterInfo("Neutral Feedback rate", timePeriod, DatabaseValueType.Integer, afp.Neutral);
                            var p = new AnalysisDataParameterInfo("Positive Feedback Rate", timePeriod, DatabaseValueType.Integer, afp.Positive);


                            itemsParams.AddRange(new[] { c, n, g, p, });


                            if (itemsParams.Count > 0)
                            {
                                data.AddData(afDate.Value, itemsParams);
                            }
                        }
                    }

                });
            }
          */
        }



        private IEnumerable<IWriteDataInfo<YodleeDatabaseFunctionType>> CreateOrdersAggregationInfo(YodleeOrderItem orders, ICurrencyConvertor currencyConverter)
        {
            var aggregateFunctionArray = new[]
                {
                    YodleeDatabaseFunctionType.TotalExpense,
                    YodleeDatabaseFunctionType.TotlaIncome,
                    YodleeDatabaseFunctionType.CurrentBalance,
                    YodleeDatabaseFunctionType.AvailableBalance,
                };


            var updated = DateTime.UtcNow;//todo:use time from server
            var nodesCreationFactory = TimePeriodNodesCreationTreeFactoryFactory.CreateHardCodeTimeBoundaryCalculationStrategy();

            ReceivedDataListTimeDependentBase<YodleeTransactionItem> tr = YodleeTransactionList.Create(orders);
            var timeChain = TimePeriodChainContructor.CreateDataChain(new TimePeriodNodeWithDataFactory<YodleeTransactionItem>(), tr, nodesCreationFactory);

            if (timeChain.HasNoData)
            {
                return null;
            }

            var timePeriodChain = TimePeriodChainContructor.ExtractDataWithCorrectTimePeriod(timeChain, updated);
            
            var factory = new YodleeOrdersAggregatorFactory();

            return DataAggregatorHelper.AggregateData(factory, timePeriodChain, aggregateFunctionArray, updated, currencyConverter);
        }

    }
}
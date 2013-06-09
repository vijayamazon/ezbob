namespace FreeAgent
{
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
	using log4net;

	public class FreeAgentRetrieveDataHelper : MarketplaceRetrieveDataHelperBase<FreeAgentDatabaseFunctionType>
    {
		private static ILog log = LogManager.GetLogger(typeof(FreeAgentRetrieveDataHelper));

		public FreeAgentRetrieveDataHelper(DatabaseDataHelper helper, DatabaseMarketplaceBase<FreeAgentDatabaseFunctionType> marketplace)
            : base(helper, marketplace)
        {
        }

        protected override void InternalUpdateInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
                                                   MP_CustomerMarketplaceUpdatingHistory historyRecord)
        {
            // Retreive data from free agent api
			var freeAgentOrderList = FreeAgentConnector.GetOrders(
				databaseCustomerMarketPlace.DisplayName,
				SerializeDataHelper.DeserializeType<FreeAgentSecurityInfo>(databaseCustomerMarketPlace.SecurityData), 
				Helper.GetFreeAgentDeltaPeriod(databaseCustomerMarketPlace)
			);

            var elapsedTimeInfo = new ElapsedTimeInfo();
			
			//store orders
            ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
                                    ElapsedDataMemberType.StoreDataToDatabase,
									() => Helper.StoreFreeAgentOrdersData(databaseCustomerMarketPlace, freeAgentOrderList, historyRecord));

            //retrieve orders
            var allOrders = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
                                    ElapsedDataMemberType.RetrieveDataFromDatabase,
									() => Helper.GetAllFreeAgentOrdersData(DateTime.UtcNow, databaseCustomerMarketPlace));

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

		private IEnumerable<IWriteDataInfo<FreeAgentDatabaseFunctionType>> CreateOrdersAggregationInfo(FreeAgentOrdersList orders, ICurrencyConvertor currencyConverter)
        {
            var aggregateFunctionArray = new[]
                {
                    FreeAgentDatabaseFunctionType.NumOfOrders,
                    FreeAgentDatabaseFunctionType.TotalSumOfOrders
                };

            var updated = orders.SubmittedDate;
            var nodesCreationFactory = TimePeriodNodesCreationTreeFactoryFactory.CreateHardCodeTimeBoundaryCalculationStrategy();
			TimePeriodChainWithData<FreeAgentOrderItem> timeChain = TimePeriodChainContructor.CreateDataChain(new TimePeriodNodeWithDataFactory<FreeAgentOrderItem>(), orders, nodesCreationFactory);

            if (timeChain.HasNoData)
            {
                return null;
            }

            var timePeriodData = TimePeriodChainContructor.ExtractDataWithCorrectTimePeriod(timeChain, updated);
			var factory = new FreeAgentOrdersAggregatorFactory();
            return DataAggregatorHelper.AggregateData(factory, timePeriodData, aggregateFunctionArray, updated, currencyConverter);
        }
    }
}
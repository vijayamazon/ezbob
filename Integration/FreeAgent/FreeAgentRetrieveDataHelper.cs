namespace FreeAgent
{
	using EZBob.DatabaseLib.Model.Marketplaces.FreeAgent;
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
	        string accessToken =
		        (SerializeDataHelper.DeserializeType<FreeAgentSecurityInfo>(databaseCustomerMarketPlace.SecurityData))
			        .AccessToken;
			var freeAgentOrderList = FreeAgentConnector.GetOrders(
				accessToken, 
				Helper.GetFreeAgentDeltaPeriod(databaseCustomerMarketPlace)
			);

			FreeAgentCompany freeAgentCompany = FreeAgentConnector.GetCompany(accessToken);
			FreeAgentUsersList freeAgentUsers = FreeAgentConnector.GetUsers(accessToken);
			
            var elapsedTimeInfo = new ElapsedTimeInfo();
			
			// Store orders
			var mpOrder = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.StoreDataToDatabase,
				() => Helper.StoreFreeAgentOrdersData(databaseCustomerMarketPlace, freeAgentOrderList, historyRecord));

            //retrieve orders
            var allOrders = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
                ElapsedDataMemberType.RetrieveDataFromDatabase,
				() => Helper.GetAllFreeAgentOrdersData(DateTime.UtcNow, databaseCustomerMarketPlace));
			
            //calculate aggregated
            var aggregatedData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
                ElapsedDataMemberType.AggregateData,
                () => CreateOrdersAggregationInfo(allOrders, Helper.CurrencyConverter));
            
            // store aggregated
            ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
                ElapsedDataMemberType.StoreAggregatedData,
                () => Helper.StoreToDatabaseAggregatedData(databaseCustomerMarketPlace, aggregatedData, historyRecord));

	        if (mpOrder != null)
	        {
		        // Store company data
		        var mpFreeAgentCompany = new MP_FreeAgentCompany
			        {
				        Order = mpOrder,
				        url = freeAgentCompany.url,
				        name = freeAgentCompany.name,
				        subdomain = freeAgentCompany.subdomain,
				        type = freeAgentCompany.type,
				        currency = freeAgentCompany.currency,
				        mileage_units = freeAgentCompany.mileage_units,
				        company_start_date = freeAgentCompany.company_start_date,
				        freeagent_start_date = freeAgentCompany.freeagent_start_date,
				        first_accounting_year_end = freeAgentCompany.first_accounting_year_end,
				        company_registration_number = freeAgentCompany.company_registration_number,
				        sales_tax_registration_status = freeAgentCompany.sales_tax_registration_status,
				        sales_tax_registration_number = freeAgentCompany.sales_tax_registration_number
			        };

		        ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
			        elapsedTimeInfo,
			        ElapsedDataMemberType.StoreDataToDatabase,
			        () => Helper.StoreFreeAgentCompanyData(mpFreeAgentCompany));

		        // Store users data
		        var mpFreeAgentUsersList = new List<MP_FreeAgentUsers>();
		        foreach (FreeAgentUsers user in freeAgentUsers.Users)
		        {
			        var mpFreeAgentUsers = new MP_FreeAgentUsers
				        {
					        Order = mpOrder,
					        first_name = user.first_name,
					        last_name = user.last_name,
					        email = user.email,
					        role = user.role,
					        permission_level = user.permission_level,
					        opening_mileage = user.opening_mileage,
					        updated_at = user.updated_at,
					        created_at = user.created_at
				        };

			        mpFreeAgentUsersList.Add(mpFreeAgentUsers);
		        }

		        ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
			        elapsedTimeInfo,
			        ElapsedDataMemberType.StoreDataToDatabase,
			        () => Helper.StoreFreeAgentUsersData(mpFreeAgentUsersList));
	        }
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
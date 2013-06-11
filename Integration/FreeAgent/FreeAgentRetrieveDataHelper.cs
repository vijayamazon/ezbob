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
			string accessToken = (SerializeDataHelper.DeserializeType<FreeAgentSecurityInfo>(databaseCustomerMarketPlace.SecurityData)).AccessToken;
			var freeAgentInvoices = FreeAgentConnector.GetInvoices(
				accessToken,
				Helper.GetFreeAgentInvoiceDeltaPeriod(databaseCustomerMarketPlace));
			var freeAgentExpenses = FreeAgentConnector.GetExpenses(
				accessToken,
				Helper.GetFreeAgentExpenseDeltaPeriod(databaseCustomerMarketPlace));

			FreeAgentCompany freeAgentCompany = FreeAgentConnector.GetCompany(accessToken);
			FreeAgentUsersList freeAgentUsers = FreeAgentConnector.GetUsers(accessToken);
			
            var elapsedTimeInfo = new ElapsedTimeInfo();
			
			// Store request and invoices
			var mpRequest = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.StoreDataToDatabase,
				() => Helper.StoreFreeAgentRequestAndInvoicesAndExpensesData(databaseCustomerMarketPlace, freeAgentInvoices, freeAgentExpenses, historyRecord));

            // Retrieve all distinct invoices
            var allInvoices = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
                ElapsedDataMemberType.RetrieveDataFromDatabase,
				() => Helper.GetAllFreeAgentInvoicesData(DateTime.UtcNow, databaseCustomerMarketPlace));
			
            // Calculate aggregated
            var aggregatedData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
                ElapsedDataMemberType.AggregateData,
				() => CreateInvoicesAggregationInfo(allInvoices, Helper.CurrencyConverter));
            
            // Store aggregated
            ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
                ElapsedDataMemberType.StoreAggregatedData,
                () => Helper.StoreToDatabaseAggregatedData(databaseCustomerMarketPlace, aggregatedData, historyRecord));

			if (mpRequest != null)
	        {
		        // Store company data
		        var mpFreeAgentCompany = new MP_FreeAgentCompany
			        {
						Request = mpRequest,
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
							Request = mpRequest,
							url = user.url,
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

		private IEnumerable<IWriteDataInfo<FreeAgentDatabaseFunctionType>> CreateInvoicesAggregationInfo(FreeAgentInvoicesList invoices, ICurrencyConvertor currencyConverter)
        {
            var aggregateFunctionArray = new[]
                {
                    FreeAgentDatabaseFunctionType.NumOfOrders,
                    FreeAgentDatabaseFunctionType.TotalSumOfOrders
                };

			var updated = invoices.SubmittedDate;
            var nodesCreationFactory = TimePeriodNodesCreationTreeFactoryFactory.CreateHardCodeTimeBoundaryCalculationStrategy();
			TimePeriodChainWithData<FreeAgentInvoice> timeChain = TimePeriodChainContructor.CreateDataChain(new TimePeriodNodeWithDataFactory<FreeAgentInvoice>(), invoices, nodesCreationFactory);

            if (timeChain.HasNoData)
            {
                return null;
            }

            var timePeriodData = TimePeriodChainContructor.ExtractDataWithCorrectTimePeriod(timeChain, updated);
			var factory = new FreeAgentInvoicesAggregatorFactory();
            return DataAggregatorHelper.AggregateData(factory, timePeriodData, aggregateFunctionArray, updated, currencyConverter);
        }
    }
}
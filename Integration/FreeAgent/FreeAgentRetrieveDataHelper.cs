namespace FreeAgent
{
	using EZBob.DatabaseLib.Model.Marketplaces.FreeAgent;
	using EzBob.CommonLib;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.DatabaseWrapper.FunctionValues;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using EZBob.DatabaseLib.Model.Database;
	using System;
	using System.Collections.Generic;
	using Ezbob.Utils;
	using Ezbob.Utils.Serialization;
	using log4net;

	public class FreeAgentRetrieveDataHelper : MarketplaceRetrieveDataHelperBase<FreeAgentDatabaseFunctionType>
    {
		private static readonly ILog log = LogManager.GetLogger(typeof(FreeAgentRetrieveDataHelper));

		private readonly Dictionary<string, FreeAgentExpenseCategory> expenseCategories;

		public FreeAgentRetrieveDataHelper(DatabaseDataHelper helper, DatabaseMarketplaceBase<FreeAgentDatabaseFunctionType> marketplace)
            : base(helper, marketplace)
        {
			expenseCategories = Helper.GetExpenseCategories();
        }

        protected override ElapsedTimeInfo RetrieveAndAggregate(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
                                                   MP_CustomerMarketplaceUpdatingHistory historyRecord)
        {
			log.InfoFormat("Starting to update FreeAgent marketplace. Id:{0} Name:{1}", databaseCustomerMarketPlace.Id, databaseCustomerMarketPlace.DisplayName);
	        var freeAgentSecurityInfo = (Serialized.Deserialize<FreeAgentSecurityInfo>(databaseCustomerMarketPlace.SecurityData));
			string accessToken = freeAgentSecurityInfo.AccessToken;

			if (DateTime.UtcNow > freeAgentSecurityInfo.ValidUntil)
			{
				log.Info("Starting to refresh access token");
				var tokenContainer = FreeAgentConnector.RefreshToken(freeAgentSecurityInfo.RefreshToken);

				if (tokenContainer == null)
				{
					throw new Exception("Failed refreshing the access token");
				}

				log.Info("Received new access token, will save it to DB");
				var securityData = new FreeAgentSecurityInfo
				{
					ApprovalToken = freeAgentSecurityInfo.ApprovalToken,
					AccessToken = tokenContainer.access_token,
					ExpiresIn = tokenContainer.expires_in,
					TokenType = tokenContainer.token_type,
					RefreshToken = freeAgentSecurityInfo.RefreshToken,
					MarketplaceId = freeAgentSecurityInfo.MarketplaceId,
					Name = freeAgentSecurityInfo.Name,
					ValidUntil = DateTime.UtcNow.AddSeconds(tokenContainer.expires_in - 60)
				};
				var serializedSecurityData = new Serialized(securityData);
				Helper.SaveOrUpdateCustomerMarketplace(
					databaseCustomerMarketPlace.DisplayName,
					new FreeAgentDatabaseMarketPlace(), 
					serializedSecurityData, 
					databaseCustomerMarketPlace.Customer);

				log.Info("New access token was saved in DB");
			}

			log.Info("Getting invoices...");
			var freeAgentInvoices = FreeAgentConnector.GetInvoices(
				accessToken,
				Helper.GetFreeAgentInvoiceDeltaPeriod(databaseCustomerMarketPlace));

			log.Info("Getting expenses...");
			var freeAgentExpenses = FreeAgentConnector.GetExpenses(
				accessToken,
				Helper.GetFreeAgentExpenseDeltaPeriod(databaseCustomerMarketPlace));

			log.Info("Filling expenses category...");
	        FillExpensesCategory(freeAgentExpenses, accessToken);

	        log.Info("Getting company...");
			FreeAgentCompany freeAgentCompany = FreeAgentConnector.GetCompany(accessToken);

			log.Info("Getting users...");
			List<FreeAgentUsers> freeAgentUsers = FreeAgentConnector.GetUsers(accessToken);
			
            var elapsedTimeInfo = new ElapsedTimeInfo();

			log.InfoFormat("Saving request, {0} invoices & {1} expenses in DB...", freeAgentInvoices.Count, freeAgentExpenses.Count);
			var mpRequest = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				databaseCustomerMarketPlace.Id,
				ElapsedDataMemberType.StoreDataToDatabase,
				() => Helper.StoreFreeAgentRequestAndInvoicesAndExpensesData(databaseCustomerMarketPlace, freeAgentInvoices, freeAgentExpenses, historyRecord));

			StoreCompanyData(mpRequest, freeAgentCompany, elapsedTimeInfo, databaseCustomerMarketPlace.Id);

			StoreUsersData(mpRequest, freeAgentUsers, elapsedTimeInfo, databaseCustomerMarketPlace.Id);

			CalculateAndStoreAggregatedInvoiceData(databaseCustomerMarketPlace, historyRecord, elapsedTimeInfo);
			CalculateAndStoreAggregatedExpenseData(databaseCustomerMarketPlace, historyRecord, elapsedTimeInfo, accessToken);

	        return elapsedTimeInfo;
        }

		private void FillExpensesCategory(IEnumerable<FreeAgentExpense> freeAgentExpenses, string accessToken)
		{
			foreach (var expense in freeAgentExpenses)
			{
				if (expenseCategories.ContainsKey(expense.category))
				{
					expense.categoryItem = expenseCategories[expense.category];
				}
				else
				{
					log.InfoFormat("Getting expenses category:{0}", expense.category);
					expense.categoryItem = FreeAgentConnector.GetExpenseCategory(accessToken, expense.category);
					expense.categoryItem.Id = Helper.AddExpenseCategory(expense.categoryItem);
					if (!expenseCategories.ContainsKey(expense.categoryItem.url))
					{
						expenseCategories.Add(expense.categoryItem.url, expense.categoryItem);
					}
				}
			}
		}

		private void CalculateAndStoreAggregatedExpenseData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
		                                                    MP_CustomerMarketplaceUpdatingHistory historyRecord,
		                                                    ElapsedTimeInfo elapsedTimeInfo,
		                                                    string accessToken)
		{
			log.Info("Fetching all distinct expenses");
			var allExpenses = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				databaseCustomerMarketPlace.Id,
				ElapsedDataMemberType.RetrieveDataFromDatabase,
				() => Helper.GetAllFreeAgentExpensesData(DateTime.UtcNow, databaseCustomerMarketPlace));
			
			log.Info("Filling expenses category for aggregations...");
			FillExpensesCategory(allExpenses, accessToken);

			log.InfoFormat("Creating aggregated data for {0} expenses", allExpenses);
			var expensesAggregatedData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				databaseCustomerMarketPlace.Id,
				ElapsedDataMemberType.AggregateData,
				() => CreateExpensesAggregationInfo(allExpenses, Helper.CurrencyConverter));

			log.Info("Saving aggragated expenses data");
			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				databaseCustomerMarketPlace.Id,
				ElapsedDataMemberType.StoreAggregatedData,
				() => Helper.StoreToDatabaseAggregatedData(databaseCustomerMarketPlace, expensesAggregatedData, historyRecord));
		}

		private void CalculateAndStoreAggregatedInvoiceData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
		                                                    MP_CustomerMarketplaceUpdatingHistory historyRecord,
		                                                    ElapsedTimeInfo elapsedTimeInfo)
		{
			log.Info("Fetching all distinct invoices");
			var allInvoices = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				databaseCustomerMarketPlace.Id,
				ElapsedDataMemberType.RetrieveDataFromDatabase,
				() => Helper.GetAllFreeAgentInvoicesData(DateTime.UtcNow, databaseCustomerMarketPlace));

			log.InfoFormat("Creating aggregated data for {0} invoices", allInvoices);
			var invoicesAggregatedData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				databaseCustomerMarketPlace.Id,
				ElapsedDataMemberType.AggregateData,
				() => CreateInvoicesAggregationInfo(allInvoices, Helper.CurrencyConverter));

			log.Info("Saving aggragated invoices data");
			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				databaseCustomerMarketPlace.Id,
				ElapsedDataMemberType.StoreAggregatedData,
				() => Helper.StoreToDatabaseAggregatedData(databaseCustomerMarketPlace, invoicesAggregatedData, historyRecord));
		}

		private void StoreCompanyData(MP_FreeAgentRequest mpRequest, FreeAgentCompany freeAgentCompany, ElapsedTimeInfo elapsedTimeInfo, int mpId)
		{
			if (mpRequest == null) return;

			log.Info("Saving company in DB...");

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
				mpId,
				ElapsedDataMemberType.StoreDataToDatabase,
				() => Helper.StoreFreeAgentCompanyData(mpFreeAgentCompany));
		}

		private void StoreUsersData(MP_FreeAgentRequest mpRequest, List<FreeAgentUsers> freeAgentUsers, ElapsedTimeInfo elapsedTimeInfo, int mpId)
		{
			if (mpRequest == null) return;

			log.InfoFormat("Saving {0} user(s) in DB...", freeAgentUsers.Count);

			var mpFreeAgentUsersList = new List<MP_FreeAgentUsers>();
			foreach (FreeAgentUsers user in freeAgentUsers)
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
				mpId,
				ElapsedDataMemberType.StoreDataToDatabase,
				() => Helper.StoreFreeAgentUsersData(mpFreeAgentUsersList));
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
                    FreeAgentDatabaseFunctionType.TotalSumOfOrders,
                    FreeAgentDatabaseFunctionType.SumOfPaidInvoices,
                    FreeAgentDatabaseFunctionType.SumOfOverdueInvoices,
                    FreeAgentDatabaseFunctionType.SumOfOpenInvoices,
                    FreeAgentDatabaseFunctionType.SumOfDraftInvoices,
                    FreeAgentDatabaseFunctionType.TotalSumOfOrdersAnnualized
                };

			var updated = invoices.SubmittedDate;

			var timePeriodData = DataAggregatorHelper.GetOrdersForPeriods(invoices, (submittedDate, orders) => new FreeAgentInvoicesList(submittedDate, orders));
			var factory = new FreeAgentInvoiceAggregatorFactory();
			return DataAggregatorHelper.AggregateData(factory, timePeriodData, aggregateFunctionArray, updated, currencyConverter);
		}

		private IEnumerable<IWriteDataInfo<FreeAgentDatabaseFunctionType>> CreateExpensesAggregationInfo(FreeAgentExpensesList expenses, ICurrencyConvertor currencyConverter)
		{
			var aggregateFunctionArray = new[]
				{
					FreeAgentDatabaseFunctionType.NumOfExpenses,
					FreeAgentDatabaseFunctionType.TotalSumOfExpenses,
					FreeAgentDatabaseFunctionType.SumOfAdminExpensesCategory,
					FreeAgentDatabaseFunctionType.SumOfCostOfSalesExpensesCategory,
					FreeAgentDatabaseFunctionType.SumOfGeneralExpensesCategory
				};

			var updated = expenses.SubmittedDate;
			var timePeriodData = DataAggregatorHelper.GetOrdersForPeriods(expenses, (submittedDate, orders) => new FreeAgentExpensesList(submittedDate, orders));
			var factory = new FreeAgentExpenseAggregatorFactory();
			return DataAggregatorHelper.AggregateData(factory, timePeriodData, aggregateFunctionArray, updated, currencyConverter);
		}
    }
}
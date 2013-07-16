namespace Sage
{
	using EZBob.DatabaseLib.Model.Marketplaces.Sage;
	using EzBob.CommonLib;
	using EzBob.CommonLib.ReceivedDataListLogic;
	using EzBob.CommonLib.TimePeriodLogic;
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

	public class SageRetrieveDataHelper : MarketplaceRetrieveDataHelperBase<SageDatabaseFunctionType>
    {
		private static readonly ILog log = LogManager.GetLogger(typeof(SageRetrieveDataHelper));
		
		public SageRetrieveDataHelper(DatabaseDataHelper helper, DatabaseMarketplaceBase<SageDatabaseFunctionType> marketplace)
            : base(helper, marketplace)
		{
		}

        protected override void InternalUpdateInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
                                                   MP_CustomerMarketplaceUpdatingHistory historyRecord)
        {
			log.InfoFormat("Starting to update Sage marketplace. Id:{0} Name:{1}", databaseCustomerMarketPlace.Id, databaseCustomerMarketPlace.DisplayName);
	        var sageSecurityInfo = (SerializeDataHelper.DeserializeType<SageSecurityInfo>(databaseCustomerMarketPlace.SecurityData));
			string accessToken = sageSecurityInfo.AccessToken;

			log.Info("Getting sales invoices...");
			var salesInvoices = SageConnector.GetSalesInvoices(
				accessToken,
				Helper.GetSageDeltaPeriod(databaseCustomerMarketPlace));

			log.Info("Getting incomes...");
			var incomes = SageConnector.GetIncomes(
				accessToken,
				Helper.GetSageDeltaPeriod(databaseCustomerMarketPlace));

			log.Info("Getting purchase invoices...");
			var purchaseInvoices = SageConnector.GetPurchaseInvoices(
				accessToken,
				Helper.GetSageDeltaPeriod(databaseCustomerMarketPlace));

			log.Info("Getting expenditures...");
			var expenditures = SageConnector.GetExpenditures(
				accessToken,
				Helper.GetSageDeltaPeriod(databaseCustomerMarketPlace));

            var elapsedTimeInfo = new ElapsedTimeInfo();

			log.InfoFormat("Saving request, {0} sales invoices, {1} purchase invoices, {2} incomes, {3} expenditures in DB...", salesInvoices.Count, purchaseInvoices.Count, incomes.Count, expenditures);
			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.StoreDataToDatabase,
				() => Helper.StoreSageData(databaseCustomerMarketPlace, salesInvoices, purchaseInvoices, incomes, expenditures, historyRecord));

			log.Info("Getting payment statuses...");
	        var paymentStatuses = SageConnector.GetPaymentStatuses(accessToken);
			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.StoreDataToDatabase,
				() => Helper.StoreSagePaymentStatuses(paymentStatuses));

			CalculateAndStoreAggregatedSalesInvoiceData(databaseCustomerMarketPlace, historyRecord, elapsedTimeInfo);
			CalculateAndStoreAggregatedPurchaseInvoiceData(databaseCustomerMarketPlace, historyRecord, elapsedTimeInfo);
			CalculateAndStoreAggregatedIncomeData(databaseCustomerMarketPlace, historyRecord, elapsedTimeInfo);
			CalculateAndStoreAggregatedExpenditureData(databaseCustomerMarketPlace, historyRecord, elapsedTimeInfo);
        }

		private void CalculateAndStoreAggregatedSalesInvoiceData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
															MP_CustomerMarketplaceUpdatingHistory historyRecord,
															ElapsedTimeInfo elapsedTimeInfo)
		{
			log.Info("Fetching all distinct sales invoices");
			var allSalesInvoices = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.RetrieveDataFromDatabase,
				() => Helper.GetAllSageSalesInvoicesData(DateTime.UtcNow, databaseCustomerMarketPlace));

			log.InfoFormat("Creating aggregated data for {0} sales invoices", allSalesInvoices);
			var aggregateFunctions = new[]
                {
                    SageDatabaseFunctionType.NumOfOrders,
                    SageDatabaseFunctionType.TotalSumOfOrders,
					SageDatabaseFunctionType.TotalSumOfPaidSalesInvoices,
					SageDatabaseFunctionType.TotalSumOfUnpaidSalesInvoices,
					SageDatabaseFunctionType.TotalSumOfPartiallyPaidSalesInvoices
                };
			var salesInvoicesAggregatedData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.AggregateData,
				() => CreateAggregationInfo(allSalesInvoices, Helper.CurrencyConverter, new SageSalesInvoiceAggregatorFactory { SagePaymentStatuses = Helper.GetSagePaymentStatuses() }, aggregateFunctions));

			log.Info("Saving aggragated sales invoices data");
			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.StoreAggregatedData,
				() => Helper.StoreToDatabaseAggregatedData(databaseCustomerMarketPlace, salesInvoicesAggregatedData, historyRecord));
		}

		private void CalculateAndStoreAggregatedPurchaseInvoiceData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
															MP_CustomerMarketplaceUpdatingHistory historyRecord,
															ElapsedTimeInfo elapsedTimeInfo)
		{
			log.Info("Fetching all distinct purchase invoices");
			var allPurchaseInvoices = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.RetrieveDataFromDatabase,
				() => Helper.GetAllSagePurchaseInvoicesData(DateTime.UtcNow, databaseCustomerMarketPlace));

			log.InfoFormat("Creating aggregated data for {0} purchase invoices", allPurchaseInvoices); 
			var aggregateFunctions = new[]
                {
                    SageDatabaseFunctionType.NumOfPurchaseInvoices,
                    SageDatabaseFunctionType.TotalSumOfPurchaseInvoices,
					SageDatabaseFunctionType.TotalSumOfPaidPurchaseInvoices,
					SageDatabaseFunctionType.TotalSumOfUnpaidPurchaseInvoices,
					SageDatabaseFunctionType.TotalSumOfPartiallyPaidPurchaseInvoices
                };
			var purchaseInvoicesAggregatedData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.AggregateData,
				() => CreateAggregationInfo(allPurchaseInvoices, Helper.CurrencyConverter, new SagePurchaseInvoiceAggregatorFactory { SagePaymentStatuses = Helper.GetSagePaymentStatuses() }, aggregateFunctions));

			log.Info("Saving aggragated purchase invoices data");
			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.StoreAggregatedData,
				() => Helper.StoreToDatabaseAggregatedData(databaseCustomerMarketPlace, purchaseInvoicesAggregatedData, historyRecord));
		}

		private void CalculateAndStoreAggregatedIncomeData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
															MP_CustomerMarketplaceUpdatingHistory historyRecord,
															ElapsedTimeInfo elapsedTimeInfo)
		{
			log.Info("Fetching all distinct incomes");
			var allIncomes = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.RetrieveDataFromDatabase,
				() => Helper.GetAllSageIncomesData(DateTime.UtcNow, databaseCustomerMarketPlace));

			log.InfoFormat("Creating aggregated data for {0} incomes", allIncomes);
			var aggregateFunctions = new[]
                {
                    SageDatabaseFunctionType.NumOfIncomes,
                    SageDatabaseFunctionType.TotalSumOfIncomes
                };
			var incomesAggregatedData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.AggregateData,
				() => CreateAggregationInfo(allIncomes, Helper.CurrencyConverter, new SageIncomeAggregatorFactory(), aggregateFunctions));

			log.Info("Saving aggragated incomes data");
			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.StoreAggregatedData,
				() => Helper.StoreToDatabaseAggregatedData(databaseCustomerMarketPlace, incomesAggregatedData, historyRecord));
		}

		private void CalculateAndStoreAggregatedExpenditureData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
															MP_CustomerMarketplaceUpdatingHistory historyRecord,
															ElapsedTimeInfo elapsedTimeInfo)
		{
			log.Info("Fetching all distinct expenditure");
			var allExpenditures = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.RetrieveDataFromDatabase,
				() => Helper.GetAllSageExpendituresData(DateTime.UtcNow, databaseCustomerMarketPlace));

			log.InfoFormat("Creating aggregated data for {0} expenditures", allExpenditures);
			var aggregateFunctions = new[]
                {
                    SageDatabaseFunctionType.NumOfExpenditures,
                    SageDatabaseFunctionType.TotalSumOfExpenditures
                };
			var expendituresAggregatedData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.AggregateData,
				() => CreateAggregationInfo(allExpenditures, Helper.CurrencyConverter, new SageExpenditureAggregatorFactory(), aggregateFunctions));

			log.Info("Saving aggragated expenditures data");
			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.StoreAggregatedData,
				() => Helper.StoreToDatabaseAggregatedData(databaseCustomerMarketPlace, expendituresAggregatedData, historyRecord));
		}

		protected override void AddAnalysisValues(IDatabaseCustomerMarketPlace marketPlace, AnalysisDataInfo data)
        {
        }

        public override IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(int customerMarketPlaceId)
        {
	        return null;
        }

		private IEnumerable<IWriteDataInfo<SageDatabaseFunctionType>> CreateAggregationInfo<T>(ReceivedDataListTimeMarketTimeDependentBase<T> list, ICurrencyConvertor currencyConverter, DataAggregatorFactoryBase<ReceivedDataListTimeDependentInfo<T>, T, SageDatabaseFunctionType> factory, SageDatabaseFunctionType[] aggregateFunctionArray)
			where T : TimeDependentRangedDataBase
		{
			var updated = list.SubmittedDate;

			var nodesCreationFactory = TimePeriodNodesCreationTreeFactoryFactory.CreateHardCodeTimeBoundaryCalculationStrategy();
			var timeChain = TimePeriodChainContructor.CreateDataChain(new TimePeriodNodeWithDataFactory<T>(), list, nodesCreationFactory);

			if (timeChain.HasNoData)
			{
				return null;
			}

			var timePeriodData = TimePeriodChainContructor.ExtractDataWithCorrectTimePeriod(timeChain, updated);
			return DataAggregatorHelper.AggregateData(factory, timePeriodData, aggregateFunctionArray, updated, currencyConverter);
		}
    }
}
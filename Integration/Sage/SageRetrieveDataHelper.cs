namespace Sage
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
			var salesInvoicesAggregatedData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.AggregateData,
				() => CreateSalesInvoicesAggregationInfo(allSalesInvoices, Helper.CurrencyConverter));

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
			var purchaseInvoicesAggregatedData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.AggregateData,
				() => CreatePurchaseInvoicesAggregationInfo(allPurchaseInvoices, Helper.CurrencyConverter));

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
			var incomesAggregatedData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.AggregateData,
				() => CreateIncomesAggregationInfo(allIncomes, Helper.CurrencyConverter));

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
			var expendituresAggregatedData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.AggregateData,
				() => CreateExpendituresAggregationInfo(allExpenditures, Helper.CurrencyConverter));

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

		private IEnumerable<IWriteDataInfo<SageDatabaseFunctionType>> CreateSalesInvoicesAggregationInfo(SageSalesInvoicesList salesInvoices, ICurrencyConvertor currencyConverter)
		{
			var aggregateFunctionArray = new[]
                {
                    SageDatabaseFunctionType.NumOfOrders,
                    SageDatabaseFunctionType.TotalSumOfOrders
                };

			var updated = salesInvoices.SubmittedDate;

			var nodesCreationFactory = TimePeriodNodesCreationTreeFactoryFactory.CreateHardCodeTimeBoundaryCalculationStrategy();
			var timeChain = TimePeriodChainContructor.CreateDataChain(new TimePeriodNodeWithDataFactory<SageSalesInvoice>(), salesInvoices, nodesCreationFactory);

			if (timeChain.HasNoData)
			{
				return null;
			}

			var timePeriodData = TimePeriodChainContructor.ExtractDataWithCorrectTimePeriod(timeChain, updated);
			var factory = new SageSalesInvoiceAggregatorFactory();
			return DataAggregatorHelper.AggregateData(factory, timePeriodData, aggregateFunctionArray, updated, currencyConverter);
		}

		private IEnumerable<IWriteDataInfo<SageDatabaseFunctionType>> CreatePurchaseInvoicesAggregationInfo(SagePurchaseInvoicesList purchaseInvoices, ICurrencyConvertor currencyConverter)
		{
			var aggregateFunctionArray = new[]
                {
                    SageDatabaseFunctionType.NumOfPurchaseInvoices,
                    SageDatabaseFunctionType.TotalSumOfPurchaseInvoices
                };

			var updated = purchaseInvoices.SubmittedDate;

			var nodesCreationFactory = TimePeriodNodesCreationTreeFactoryFactory.CreateHardCodeTimeBoundaryCalculationStrategy();
			var timeChain = TimePeriodChainContructor.CreateDataChain(new TimePeriodNodeWithDataFactory<SagePurchaseInvoice>(), purchaseInvoices, nodesCreationFactory);

			if (timeChain.HasNoData)
			{
				return null;
			}

			var timePeriodData = TimePeriodChainContructor.ExtractDataWithCorrectTimePeriod(timeChain, updated);
			var factory = new SagePurchaseInvoiceAggregatorFactory();
			return DataAggregatorHelper.AggregateData(factory, timePeriodData, aggregateFunctionArray, updated, currencyConverter);
		}

		private IEnumerable<IWriteDataInfo<SageDatabaseFunctionType>> CreateIncomesAggregationInfo(SageIncomesList incomes, ICurrencyConvertor currencyConverter)
		{
			var aggregateFunctionArray = new[]
                {
                    SageDatabaseFunctionType.NumOfIncomes,
                    SageDatabaseFunctionType.TotalSumOfIncomes
                };

			var updated = incomes.SubmittedDate;

			var nodesCreationFactory = TimePeriodNodesCreationTreeFactoryFactory.CreateHardCodeTimeBoundaryCalculationStrategy();
			var timeChain = TimePeriodChainContructor.CreateDataChain(new TimePeriodNodeWithDataFactory<SageIncome>(), incomes, nodesCreationFactory);

			if (timeChain.HasNoData)
			{
				return null;
			}

			var timePeriodData = TimePeriodChainContructor.ExtractDataWithCorrectTimePeriod(timeChain, updated);
			var factory = new SageIncomeAggregatorFactory();
			return DataAggregatorHelper.AggregateData(factory, timePeriodData, aggregateFunctionArray, updated, currencyConverter);
		}

		private IEnumerable<IWriteDataInfo<SageDatabaseFunctionType>> CreateExpendituresAggregationInfo(SageExpendituresList expenditures, ICurrencyConvertor currencyConverter)
		{
			var aggregateFunctionArray = new[]
                {
                    SageDatabaseFunctionType.NumOfExpenditures,
                    SageDatabaseFunctionType.TotalSumOfExpenditures
                };

			var updated = expenditures.SubmittedDate;

			var nodesCreationFactory = TimePeriodNodesCreationTreeFactoryFactory.CreateHardCodeTimeBoundaryCalculationStrategy();
			var timeChain = TimePeriodChainContructor.CreateDataChain(new TimePeriodNodeWithDataFactory<SageExpenditure>(), expenditures, nodesCreationFactory);

			if (timeChain.HasNoData)
			{
				return null;
			}

			var timePeriodData = TimePeriodChainContructor.ExtractDataWithCorrectTimePeriod(timeChain, updated);
			var factory = new SageExpenditureAggregatorFactory();
			return DataAggregatorHelper.AggregateData(factory, timePeriodData, aggregateFunctionArray, updated, currencyConverter);
		}
    }
}
namespace Sage
{
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using EzBob.CommonLib;
	using EzBob.CommonLib.ReceivedDataListLogic;
	using EzBob.CommonLib.TimePeriodLogic;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.DatabaseWrapper.FunctionValues;
	using EZBob.DatabaseLib.Model.Database;
	using System;
	using System.Collections.Generic;
	using Ezbob.Utils;
	using Ezbob.Utils.Serialization;
	using log4net;

	public class SageRetrieveDataHelper : MarketplaceRetrieveDataHelperBase<SageDatabaseFunctionType>
    {
		private static readonly ILog log = LogManager.GetLogger(typeof(SageRetrieveDataHelper));
		
		public SageRetrieveDataHelper(DatabaseDataHelper helper, DatabaseMarketplaceBase<SageDatabaseFunctionType> marketplace)
            : base(helper, marketplace)
		{
		}

        protected override ElapsedTimeInfo RetrieveAndAggregate(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
                                                   MP_CustomerMarketplaceUpdatingHistory historyRecord)
        {
			log.InfoFormat("Starting to update Sage marketplace. Id:{0} Name:{1}", databaseCustomerMarketPlace.Id, databaseCustomerMarketPlace.DisplayName);
	        var sageSecurityInfo = (Serialized.Deserialize<SageSecurityInfo>(databaseCustomerMarketPlace.SecurityData));
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

			log.InfoFormat("Saving request, {0} sales invoices, {1} purchase invoices, {2} incomes, {3} expenditures in DB...", salesInvoices.Count, purchaseInvoices.Count, incomes.Count, expenditures.Count);
			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				databaseCustomerMarketPlace.Id,
				ElapsedDataMemberType.StoreDataToDatabase,
				() => Helper.StoreSageData(databaseCustomerMarketPlace, salesInvoices, purchaseInvoices, incomes, expenditures, historyRecord));

			log.Info("Getting payment statuses...");
	        var paymentStatuses = SageConnector.GetPaymentStatuses(accessToken);
			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				databaseCustomerMarketPlace.Id,
				ElapsedDataMemberType.StoreDataToDatabase,
				() => Helper.StoreSagePaymentStatuses(paymentStatuses));

			CalculateAndStoreAggregatedData(databaseCustomerMarketPlace, historyRecord, elapsedTimeInfo);

	        return elapsedTimeInfo;
        }

		private void CalculateAndStoreAggregatedData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
													 MP_CustomerMarketplaceUpdatingHistory historyRecord,
													 ElapsedTimeInfo elapsedTimeInfo)
		{
			var salesInvoicesAggregateFunctions = new[]
                {
                    SageDatabaseFunctionType.NumOfOrders,
                    SageDatabaseFunctionType.TotalSumOfOrders,
					SageDatabaseFunctionType.TotalSumOfPaidSalesInvoices,
					SageDatabaseFunctionType.TotalSumOfUnpaidSalesInvoices,
					SageDatabaseFunctionType.TotalSumOfPartiallyPaidSalesInvoices,
                    SageDatabaseFunctionType.TotalSumOfOrdersAnnualized
                };
			CalculateAndStoreAggregatedData(databaseCustomerMarketPlace, historyRecord, elapsedTimeInfo, "sales invoices",
											Helper.GetAllSageSalesInvoicesData, new SageSalesInvoiceAggregatorFactory { SagePaymentStatuses = Helper.GetSagePaymentStatuses() },
											salesInvoicesAggregateFunctions,
											(submittedDate, o) => new SageSalesInvoicesList(submittedDate, o));

			var purchaseInvoicesAggregateFunctions = new[]
				{
					SageDatabaseFunctionType.NumOfPurchaseInvoices,
					SageDatabaseFunctionType.TotalSumOfPurchaseInvoices,
					SageDatabaseFunctionType.TotalSumOfPaidPurchaseInvoices,
					SageDatabaseFunctionType.TotalSumOfUnpaidPurchaseInvoices,
					SageDatabaseFunctionType.TotalSumOfPartiallyPaidPurchaseInvoices
				};
			CalculateAndStoreAggregatedData(databaseCustomerMarketPlace, historyRecord, elapsedTimeInfo, "purchase invoices",
											Helper.GetAllSagePurchaseInvoicesData, new SagePurchaseInvoiceAggregatorFactory { SagePaymentStatuses = Helper.GetSagePaymentStatuses() },
											purchaseInvoicesAggregateFunctions,
											(submittedDate, o) => new SagePurchaseInvoicesList(submittedDate, o));

			var incomeAggregateFunctions = new[]
                {
                    SageDatabaseFunctionType.NumOfIncomes,
                    SageDatabaseFunctionType.TotalSumOfIncomes
                };
			CalculateAndStoreAggregatedData(databaseCustomerMarketPlace, historyRecord, elapsedTimeInfo, "incomes",
											Helper.GetAllSageIncomesData, new SageIncomeAggregatorFactory(),
											incomeAggregateFunctions,
											(submittedDate, o) => new SageIncomesList(submittedDate, o));

			var expendituresAggregateFunctions = new[]
                {
                    SageDatabaseFunctionType.NumOfExpenditures,
                    SageDatabaseFunctionType.TotalSumOfExpenditures
                };
			CalculateAndStoreAggregatedData(databaseCustomerMarketPlace, historyRecord, elapsedTimeInfo, "expenditures",
											Helper.GetAllSageExpendituresData, new SageExpenditureAggregatorFactory(),
											expendituresAggregateFunctions,
											(submittedDate, o) => new SageExpendituresList(submittedDate, o));
		}
		
		private void CalculateAndStoreAggregatedData<T>(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
		                                                MP_CustomerMarketplaceUpdatingHistory historyRecord,
		                                                ElapsedTimeInfo elapsedTimeInfo, 
														string typeName,
		                                                Func<DateTime, IDatabaseCustomerMarketPlace, ReceivedDataListTimeMarketTimeDependentBase<T>> getAllFunc,
		                                                DataAggregatorFactoryBase<ReceivedDataListTimeDependentInfo<T>, T, SageDatabaseFunctionType> factory, 
														SageDatabaseFunctionType[] aggregateFunctions,
														Func<DateTime, List<T>, ReceivedDataListTimeMarketTimeDependentBase<T>> createListFunc)
			where T : TimeDependentRangedDataBase
		{
			log.InfoFormat("Fetching all distinct {0}", typeName);
			var allItems = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				databaseCustomerMarketPlace.Id,
				ElapsedDataMemberType.RetrieveDataFromDatabase,
				() => getAllFunc(DateTime.UtcNow, databaseCustomerMarketPlace));

			log.InfoFormat("Creating aggregated data for {0} {1}", allItems, typeName);
			var aggregatedData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				databaseCustomerMarketPlace.Id,
				ElapsedDataMemberType.AggregateData,
				() => CreateAggregationInfo(allItems, Helper.CurrencyConverter, factory, aggregateFunctions, createListFunc));

			log.InfoFormat("Saving aggragated {0} data", typeName);
			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				databaseCustomerMarketPlace.Id,
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

		private IEnumerable<IWriteDataInfo<SageDatabaseFunctionType>> CreateAggregationInfo<T>(ReceivedDataListTimeMarketTimeDependentBase<T> list, ICurrencyConvertor currencyConverter, DataAggregatorFactoryBase<ReceivedDataListTimeDependentInfo<T>, T, SageDatabaseFunctionType> factory, SageDatabaseFunctionType[] aggregateFunctions, Func<DateTime, List<T>, ReceivedDataListTimeMarketTimeDependentBase<T>> createListFunc)
			where T : TimeDependentRangedDataBase
		{
			var updated = list.SubmittedDate;
			var timePeriodData = DataAggregatorHelper.GetOrdersForPeriods(list, createListFunc);
			return DataAggregatorHelper.AggregateData(factory, timePeriodData, aggregateFunctions, updated, currencyConverter);
		}
    }
}
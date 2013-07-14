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
				Helper.GetSageInvoiceDeltaPeriod(databaseCustomerMarketPlace));

            var elapsedTimeInfo = new ElapsedTimeInfo();

			log.InfoFormat("Saving request, {0} sales invoices in DB...", salesInvoices.Count);
			var mpRequest = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.StoreDataToDatabase,
				() => Helper.StoreSageRequestAndInvoicesData(databaseCustomerMarketPlace, salesInvoices, historyRecord));
			
			CalculateAndStoreAggregatedInvoiceData(databaseCustomerMarketPlace, historyRecord, elapsedTimeInfo);
        }
		
		private void CalculateAndStoreAggregatedInvoiceData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
		                                                    MP_CustomerMarketplaceUpdatingHistory historyRecord,
		                                                    ElapsedTimeInfo elapsedTimeInfo)
		{
			log.Info("Fetching all distinct sales invoices");
			var allInvoices = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.RetrieveDataFromDatabase,
				() => Helper.GetAllSageInvoicesData(DateTime.UtcNow, databaseCustomerMarketPlace));
			
			log.InfoFormat("Creating aggregated data for {0} sales invoices", allInvoices);
			var invoicesAggregatedData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.AggregateData,
				() => CreateSalesInvoicesAggregationInfo(allInvoices, Helper.CurrencyConverter));

			log.Info("Saving aggragated sales invoices data");
			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.StoreAggregatedData,
				() => Helper.StoreToDatabaseAggregatedData(databaseCustomerMarketPlace, invoicesAggregatedData, historyRecord));
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
    }
}
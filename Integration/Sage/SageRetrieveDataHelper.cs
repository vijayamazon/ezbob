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
			
			log.Info("Getting invoices...");
			var sageInvoices = SageConnector.GetInvoices(
				accessToken,
				Helper.GetSageInvoiceDeltaPeriod(databaseCustomerMarketPlace));

            var elapsedTimeInfo = new ElapsedTimeInfo();

			log.InfoFormat("Saving request, {0} invoices in DB...", sageInvoices.Count);
			var mpRequest = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.StoreDataToDatabase,
				() => Helper.StoreSageRequestAndInvoicesData(databaseCustomerMarketPlace, sageInvoices, historyRecord));
			
			CalculateAndStoreAggregatedInvoiceData(databaseCustomerMarketPlace, historyRecord, elapsedTimeInfo);
        }
		
		private void CalculateAndStoreAggregatedInvoiceData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
		                                                    MP_CustomerMarketplaceUpdatingHistory historyRecord,
		                                                    ElapsedTimeInfo elapsedTimeInfo)
		{
			log.Info("Fetching all distinct invoices");
			var allInvoices = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.RetrieveDataFromDatabase,
				() => Helper.GetAllSageInvoicesData(DateTime.UtcNow, databaseCustomerMarketPlace));
			
			log.InfoFormat("Creating aggregated data for {0} invoices", allInvoices);
			var invoicesAggregatedData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.AggregateData,
				() => CreateInvoicesAggregationInfo(allInvoices, Helper.CurrencyConverter));

			log.Info("Saving aggragated invoices data");
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

		private IEnumerable<IWriteDataInfo<SageDatabaseFunctionType>> CreateInvoicesAggregationInfo(SageInvoicesList invoices, ICurrencyConvertor currencyConverter)
		{
			var aggregateFunctionArray = new[]
                {
                    SageDatabaseFunctionType.NumOfOrders,
                    SageDatabaseFunctionType.TotalSumOfOrders
                };

			var updated = invoices.SubmittedDate;

			var nodesCreationFactory = TimePeriodNodesCreationTreeFactoryFactory.CreateHardCodeTimeBoundaryCalculationStrategy();
			var timeChain = TimePeriodChainContructor.CreateDataChain(new TimePeriodNodeWithDataFactory<SageInvoice>(), invoices, nodesCreationFactory);

			if (timeChain.HasNoData)
			{
				return null;
			}

			var timePeriodData = TimePeriodChainContructor.ExtractDataWithCorrectTimePeriod(timeChain, updated);
			var factory = new SageInvoiceAggregatorFactory();
			return DataAggregatorHelper.AggregateData(factory, timePeriodData, aggregateFunctionArray, updated, currencyConverter);
		}
    }
}
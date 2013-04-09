using EzBob.CommonLib;
using EzBob.CommonLib.Security;
using EzBob.CommonLib.TimePeriodLogic.DependencyChain;
using EzBob.CommonLib.TimePeriodLogic.DependencyChain.Factories;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using EZBob.DatabaseLib.DatabaseWrapper.FunctionValues;
using EZBob.DatabaseLib.DatabaseWrapper.Order;
using EZBob.DatabaseLib.Model.Database;
using PaymentServices.Web_References.PayPoint;
using System;
using System.Collections.Generic;
using System.IO;

namespace PayPoint
{
    public static class StringToStreamExtension
    {
        public static Stream ToStream(this string str)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }

    public class PayPointRetrieveDataHelper : MarketplaceRetrieveDataHelperBase<PayPointDatabaseFunctionType>
    {
        public PayPointRetrieveDataHelper(DatabaseDataHelper helper, DatabaseMarketplaceBase<PayPointDatabaseFunctionType> marketplace)
            : base(helper, marketplace)
        {

        }

        protected override void InternalUpdateInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
                                                   MP_CustomerMarketplaceUpdatingHistory historyRecord)
        {
            PayPointSecurityInfo securityInfo = (PayPointSecurityInfo)this.RetrieveCustomerSecurityInfo(databaseCustomerMarketPlace.Id);

            UpdateClientOrdersInfo(databaseCustomerMarketPlace, securityInfo, ActionAccessType.Full, historyRecord);
        }

        private void UpdateClientOrdersInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, PayPointSecurityInfo securityInfo, ActionAccessType actionAccessType, MP_CustomerMarketplaceUpdatingHistory historyRecord)
        {
            var secVpnService = new SECVPNService();
            string condition = DateTime.Today.AddYears(-1).Year + DateTime.Today.Month.ToString("00") + "-" + DateTime.Today.Year + DateTime.Today.Month.ToString("00");
            string reportXmlString = secVpnService.getReport(securityInfo.Mid, securityInfo.VpnPassword, securityInfo.RemotePassword, "XML-Report", "Date", condition, "GBP", string.Empty, false, true);

            var payPointDataSet = new PayPointDataSet();
            using (Stream xmlStream = reportXmlString.ToStream())
            {
                payPointDataSet.ReadXml(xmlStream);
            }

            var Iwant = new List<PayPointOrderItem>();
            foreach (var x in payPointDataSet.Transaction)
            {
                int h = 9;
                int hh = h + 9; // qqq - debug this line and init object properly
                Iwant.Add(new PayPointOrderItem()
                {
                });
            }

            var elapsedTimeInfo = new ElapsedTimeInfo();
            PayPointOrdersList allOrders = new PayPointOrdersList(DateTime.UtcNow, Iwant);
            ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
                                    ElapsedDataMemberType.StoreDataToDatabase,
                                    () => Helper.StorePayPointOrdersData(databaseCustomerMarketPlace, allOrders, historyRecord));


            //store agregated
            var aggregatedData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
                                    ElapsedDataMemberType.AggregateData,
                                    () => CreateOrdersAggregationInfo(allOrders, Helper.CurrencyConverter));
            // Save
            ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
                            ElapsedDataMemberType.StoreAggregatedData,
                            () => Helper.StoreToDatabaseAggregatedData(databaseCustomerMarketPlace, aggregatedData, historyRecord));
        }

        protected override void AddAnalysisValues(IDatabaseCustomerMarketPlace marketPlace, AnalysisDataInfo data)
        {
          

        }

        public override IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(int customerMarketPlaceId)
        {
            PayPointSecurityInfo payPointSecurityInfo = new PayPointSecurityInfo();
            IDatabaseCustomerMarketPlace customerMarketPlace = GetDatabaseCustomerMarketPlace(customerMarketPlaceId);
            payPointSecurityInfo.VpnPassword = Encryptor.Decrypt(customerMarketPlace.SecurityData);
            payPointSecurityInfo.RemotePassword = Encryptor.Decrypt(customerMarketPlace.SecurityData); // qqq - should be something else - add column to MP_CustomerMarketPlace
            payPointSecurityInfo.Mid = customerMarketPlace.DisplayName;
            payPointSecurityInfo.MarketplaceId = customerMarketPlace.Id;
            return payPointSecurityInfo;
        }

        private IEnumerable<IWriteDataInfo<PayPointDatabaseFunctionType>> CreateOrdersAggregationInfo(PayPointOrdersList orders, ICurrencyConvertor currencyConverter)
        {
            var aggregateFunctionArray = new[]
					{
						PayPointDatabaseFunctionType.NumOfOrders, 
					};

            var updated = orders.SubmittedDate;

            var nodesCreationFactory = TimePeriodNodesCreationTreeFactoryFactory.CreateHardCodeTimeBoundaryCalculationStrategy();
            TimePeriodChainWithData<PayPointOrderItem> timeChain = TimePeriodChainContructor.CreateDataChain(new TimePeriodNodeWithDataFactory<PayPointOrderItem>(), orders, nodesCreationFactory);

            if (timeChain.HasNoData)
            {
                return null;
            }

            var timePeriodData = TimePeriodChainContructor.ExtractDataWithCorrectTimePeriod(timeChain, updated);

            var factory = new PayPointOrdersAgregatorFactory();

            return DataAggregatorHelper.AggregateData(factory, timePeriodData, aggregateFunctionArray, updated, currencyConverter);
        }
    }
}
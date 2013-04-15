namespace PayPoint
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
    using PaymentServices.Web_References.PayPoint;
    using System;
    using System.Collections.Generic;
    using System.IO;

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
            var securityInfo = (PayPointSecurityInfo)RetrieveCustomerSecurityInfo(databaseCustomerMarketPlace.Id);

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

            var payPointOrders = new List<PayPointOrderItem>();
            foreach (PayPointDataSet.TransactionRow x in payPointDataSet.Transaction)
            {
                var order = new PayPointOrderItem
                    {
                        acquirer = x.acquirer,
                        amount = x.amount,
                        auth_code = x.auth_code,
                        authorised = x.authorised,
                        card_type = x.card_type,
                        cid = x.cid,
                        classType = x._class,
                        company_no = x.company_no,
                        country = x.country,
                        currency = x.currency,
                        cv2avs = x.cv2avs,
                        deferred = x.deferred,
                        emvValue = x.emvValue,
                        fraud_code = x.fraud_code,
                        FraudScore = x.FraudScore,
                        ip = x.ip,
                        lastfive = x.lastfive,
                        merchant_no = x.merchant_no,
                        message = x.message,
                        MessageType = x.MessageType,
                        mid = x.mid,
                        name = x.name,
                        options = x.options,
                        status = x.status,
                        tid = x.tid,
                        trans_id = x.trans_id
                    };

                DateTime result;
                order.date = !DateTime.TryParse(x.date, out result) ? (DateTime?)null : result;
                order.ExpiryDate = !DateTime.TryParse(x.ExpiryDate, out result) ? (DateTime?)null : result;
                order.start_date = !DateTime.TryParse(x.start_date, out result) ? (DateTime?)null : result;
                payPointOrders.Add(order);
                
            }

            var elapsedTimeInfo = new ElapsedTimeInfo();
            var allOrders = new PayPointOrdersList(DateTime.UtcNow, payPointOrders);
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
            IDatabaseCustomerMarketPlace customerMarketPlace = GetDatabaseCustomerMarketPlace(customerMarketPlaceId);
            return SerializeDataHelper.DeserializeType<PayPointSecurityInfo>(customerMarketPlace.SecurityData);
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
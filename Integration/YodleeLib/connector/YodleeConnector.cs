namespace YodleeLib.connector
{
    using System.Collections.Generic;
    using config;
    using log4net;

    public class YodleeConnector
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(YodleeConnector));

        public bool Validate(string userName, string password, int itemId, out string errMsg)
        {
            //TODO: implement
            errMsg = string.Empty;
            return true;

        }

        public static Dictionary<BankData, List<BankTransactionData>> GetOrders(string userName, string password, int itemId, IYodleeMarketPlaceConfig config)
        {
            //TODO: implement
            var yodlee = new YodleeMain(config.soapServer);
            yodlee.loginUser(userName, password);
            var lu = new LoginUser();
            var DisplayBankData = new DisplayBankData(config);
            string ItemSummaryInfo;
            string error;
            Dictionary<BankData, List<BankTransactionData>> orders;
            DisplayBankData.displayBankDataForItem(yodlee.userContext, itemId, out ItemSummaryInfo, out error, out orders);

            if (!string.IsNullOrEmpty(error))
            {
                _Log.ErrorFormat("Yodlee GetOrders error: {0}", error);
            }
            // var orders = new Dictionary<BankData, List<BankTransactionData>>();
            return orders;
        }
    }
}
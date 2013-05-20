﻿namespace YodleeLib.connector
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

        public static Dictionary<BankData, List<BankTransactionData>> GetOrders(string userName, string password, long itemId)
        {
            //TODO: implement
            var yodlee = new YodleeMain();
            yodlee.loginUser(userName, password);
            var lu = new LoginUser();
            var displayBankData = new DisplayBankData();
            string itemSummaryInfo;
            string error;
            Dictionary<BankData, List<BankTransactionData>> orders;
            displayBankData.displayBankDataForItem(yodlee.userContext, itemId, out itemSummaryInfo, out error, out orders);

            if (!string.IsNullOrEmpty(error))
            {
                _Log.ErrorFormat("Yodlee GetOrders error: {0}", error);
            }
            // var orders = new Dictionary<BankData, List<BankTransactionData>>();
            return orders;
        }
    }
}
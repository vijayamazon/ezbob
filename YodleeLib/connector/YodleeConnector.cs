namespace YodleeLib.connector
{
    using System.Collections.Generic;
    using Scorto.Configuration;
    using log4net;

    public class YodleeConnector
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(YodleeConnector));

        public bool Validate(string userName, string password, out string errMsg)
        {
            //TODO: implement
            errMsg = string.Empty;
            return true;

        }

        public static Dictionary<BankData, List<BankTransactionData>> GetOrders(string userName, string password)
        {
            //TODO: implement
            var orders = new Dictionary<BankData, List<BankTransactionData>>();
            return orders;
        }
    }
}
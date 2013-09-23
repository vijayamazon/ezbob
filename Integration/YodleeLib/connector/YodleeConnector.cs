namespace YodleeLib.connector
{
	using System.Collections.Generic;
	using log4net;

	public class YodleeConnector
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(YodleeConnector));

		public bool Validate(string userName, string password, int itemId, out string errMsg)
		{
			//TODO: implement
			errMsg = string.Empty;
			return true;

		}

		public static Dictionary<BankData, List<BankTransactionData>> GetOrders(string userName, string password, long itemId)
		{
			Log.Debug("Begin retrieve yodlee orders");
			var yodlee = new YodleeMain();
			var lu = yodlee.LoginUser(userName, password);
			var displayBankData = new DisplayBankData();
			string itemSummaryInfo;
			string error;
			Dictionary<BankData, List<BankTransactionData>> orders;
			displayBankData.displayBankDataForItem(yodlee.UserContext, itemId, out itemSummaryInfo, out error, out orders);

			if (!string.IsNullOrEmpty(error))
			{
				Log.ErrorFormat("Yodlee GetOrders error: {0}", error);
			}

			return orders;
		}
	}
}
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
			if (lu == null)
			{
				Log.Error("Login To Yodlee Account Failed No Data Can Be Retrieved");
				return null;
			}
			var displayBankData = new GetBankData();
			string itemSummaryInfo;
			string error;
			Dictionary<BankData, List<BankTransactionData>> orders;
			displayBankData.GetBankDataForItem(yodlee.UserContext, itemId, out itemSummaryInfo, out error, out orders);
			lu.logoutUser(yodlee.UserContext);
			return orders;
		}
	}
}
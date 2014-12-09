namespace YodleeLib
{
	using System;
	using System.Collections.Generic;
	using ConfigManager;
	using log4net;

	/// <summary>
	/// Displays a user's Bank item data in the Yodlee software platform..
	/// </summary>
	public class GetBankData : ApplicationSuper
	{
		DataServiceService dataService;
		private static readonly ILog Log = LogManager.GetLogger(typeof(GetBankData));
		public GetBankData()
		{
			string soapServer = CurrentValues.Instance.YodleeSoapServer;
			dataService = new DataServiceService { Url = soapServer + "/" + "DataService" };
		}

		/// <summary>
		/// Convert UTC to DateTime
		/// </summary>
		/// <param name="utc">time in utc</param>
		public DateTime UtcToDateTime(long utc)
		{
			//calculate UTC format for Now
			int seconds = Convert.ToInt32(utc);
			//convert it back to current DateTime
			var dateTime = new DateTime(1970, 1, 1).AddSeconds(seconds);
			return dateTime;
		}

		/// <summary>
		/// Displays the item information and item data information
		/// for the given bank itemSummary.
		/// </summary>
		/// <param name="userContext"></param>
		/// <param name="itemId"></param>
		/// <param name="itemSummaryInfo"></param>
		/// <param name="error"></param>
		/// <param name="bankTransactionDataList"></param>
		public void GetBankDataForItem(UserContext userContext, long itemId, out string itemSummaryInfo, out string error, out Dictionary<BankData, List<BankTransactionData>> bankTransactionDataList)
		{
			Log.Debug(string.Format("GetBankDataForItem: userContex valid {0}, itemId {1}", userContext.valid, itemId));
			var dataExtent = new DataExtent
				{
					startLevel = 0,
					endLevel = int.MaxValue,
					startLevelSpecified = true,
					endLevelSpecified = true
				};

			ItemSummary itemSummary = dataService.getItemSummaryForItem1(userContext, itemId, true, dataExtent);

			if (itemSummary == null)
			{
				throw new Exception(string.Format("Item for item id {0} not found", itemId));
			}

			error = "";
			bankTransactionDataList = new Dictionary<BankData, List<BankTransactionData>>();

			String containerType = itemSummary.contentServiceInfo.containerInfo.containerName;
			if (!containerType.Equals("bank"))
			{
				throw new Exception("displayBankDataForItem called with invalid container type" + containerType);
			}

			var displayItemInfo = new DisplayItemInfo();
			itemSummaryInfo = displayItemInfo.getItemSummaryInfo(itemSummary);
			Log.Debug(itemSummaryInfo);
			// Get ItemData
			ItemData1 itemData = itemSummary.itemData;
			if (itemData == null)
			{
				error += "\tItemData is null";
			}
			else
			{
				object[] accounts = itemData.accounts;
				if (accounts == null || accounts.Length == 0)
				{
					Log.Warn("Yodlee get bank data: No accounts");
				}
				else
				{
					foreach (object account in accounts)
					{
						var bankData = (BankData)account;
						bankTransactionDataList.Add(bankData, new List<BankTransactionData>());
						object[] bankTransactions = bankData.bankTransactions;
						if (bankTransactions == null || bankTransactions.Length == 0)
						{
							Log.Warn("Yodlee get bank data: No bank transactions");
						}
						else
						{
							foreach (object bankTransaction in bankTransactions)
							{
								var transactionData =
									(BankTransactionData)bankTransaction;
								bankTransactionDataList[bankData].Add(transactionData);
							}
						}
					}
				}
			}
			// Get AccountHistory

			if (itemData != null)
			{
				object[] acctHistories = itemData.accountHistory;
				if (acctHistories == null || acctHistories.Length == 0)
				{
					//error += "\tNo Account History";
				}
				else
				{
					foreach (object accountHistory in acctHistories)
					{
						var acctHistory = (AccountHistory)accountHistory;

						Log.Debug(string.Format("\tHistory For Account ID: {0}", acctHistory.accountId));

						// Get History
						object[] histories = acctHistory.history;
						if (histories == null || histories.Length == 0)
						{
							Log.Warn("Yodlee get bank data: No History");
						}
						else
						{
							foreach (object history in histories)
							{
								var bankData = (BankData)history;
								Log.Debug(
									string.Format(
										"Account History: Bank Account Name: {0}, Bank Account Cust Description: {1} Bank Account Identifier: {2} Bank Account Balance: {3} Bank Current Balance: {4} Bank Current Acct Type: {5} Bank Current As of Date: {6} Last Updated: {7}",
										bankData.accountName, bankData.customDescription, bankData.bankAccountId, bankData.availableBalance.amount,
										bankData.currentBalance.amount, bankData.acctType, bankData.asOfDate.date,
										UtcToDateTime(bankData.lastUpdated.HasValue ? bankData.lastUpdated.Value : 0)));
							}
						}
					}
				}
			}

			if (!string.IsNullOrEmpty(error))
			{
				Log.Warn(error);
			}
		}
	}

}

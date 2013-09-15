using System;
using System.Collections.Generic;

namespace YodleeLib
{
	using EzBob.Configuration;
	using StructureMap;
    using config;

    /// <summary>
    /// Displays a user's Bank item data in the Yodlee software platform..
    /// </summary>
    public class DisplayBankData : ApplicationSuper
    {
        DataServiceService dataService;
		private static YodleeEnvConnectionConfig _config;
        public DisplayBankData()
        {
			_config = YodleeConfig._Config;
            dataService = new DataServiceService();
            dataService.Url = _config.soapServer + "/" + "DataService";
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
        public void displayBankDataForItem(UserContext userContext, long itemId, out string itemSummaryInfo, out string error, out Dictionary<BankData, List<BankTransactionData>> bankTransactionDataList)
        {
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
            //String containerName = itemSummary.contentServiceInfo.containerInfo.containerName;

            error = "";
            bankTransactionDataList = new Dictionary<BankData, List<BankTransactionData>>();

            String containerType = itemSummary.contentServiceInfo.containerInfo.containerName;
            if (!containerType.Equals("bank"))
            {
                throw new Exception("displayBankDataForItem called with invalid container type" + containerType);
            }

            var displayItemInfo = new DisplayItemInfo();
            itemSummaryInfo = displayItemInfo.getItemSummaryInfo(itemSummary);

            // Get ItemData
            ItemData1 itemData = itemSummary.itemData;
            if (itemData == null)
            {
                error = "\tItemData is null";
            }
            else
            {
                object[] accounts = itemData.accounts;
                if (accounts == null || accounts.Length == 0)
                {
                    error = "\tNo accounts";
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
                            error = "\t\tNo bank transactions";
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
                    Console.WriteLine("\tNo Account History");
                }
                else
                {
                    Console.WriteLine("\n\t**Account History**");
                    foreach (object accountHistory in acctHistories)
                    {
                        var acctHistory = (AccountHistory)accountHistory;

                        Console.WriteLine("\tAccount ID: {0}", acctHistory.accountId);

                        // Get History
                        object[] histories = acctHistory.history;
                        if (histories == null || histories.Length == 0)
                        {
                            Console.WriteLine("\t\tNo History");
                        }
                        else
                        {
                            Console.WriteLine("\t\t**History**");
                            foreach (object history in histories)
                            {
                                var bankData = (BankData)history;
                                Console.WriteLine("\t\tBank Account Name: {0}",
                                                  bankData.accountName);
                                Console.WriteLine("\t\tBank Account Cust Description: {0}",
                                                  bankData.customDescription);
                                Console.WriteLine("\t\tBank Account Identifier: {0}",
                                                  bankData.bankAccountId);
                                Console.WriteLine("\t\tBank Account Balance: {0}",
                                                  bankData.availableBalance.amount);
                                Console.WriteLine("\t\tBank Current Balance: {0}",
                                                  bankData.currentBalance.amount);
                                Console.WriteLine("\t\tBank Current Acct Type: {0}",
                                                  bankData.acctType);
                                Console.WriteLine("\t\tBank Current As of Date: {0}",
                                                  bankData.asOfDate.date);
                                Console.WriteLine("\t\tLast Updated: {0}\n",
                                                  UtcToDateTime(bankData.lastUpdated.HasValue ? bankData.lastUpdated.Value : 0));
                            }
                        }
                    }
                }
            }
        }
    }


}

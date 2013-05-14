using System;
using YodleeLib.datatypes;
using System.Xml;
using System.Collections.Generic;

namespace YodleeLib
{
    using config;

    /// <summary>
    /// Displays a user's Bank item data in the Yodlee software platform..
    /// </summary>
    public class DisplayBankData : ApplicationSuper
    {
        DataServiceService dataService;
        private readonly IYodleeMarketPlaceConfig _Config;
        public DisplayBankData(IYodleeMarketPlaceConfig config)
        {
            _Config = config;
            dataService = new DataServiceService();
            dataService.Url = config.soapServer + "/" + "DataService";
        }



        /// <summary>
        /// Convert UTC to DateTime
        /// </summary>
        /// <param name="utc">time in utc</param>
        public DateTime UtcToDateTime(long utc)
        {
            //calculate UTC format for Now
            DateTime then = new DateTime(1970, 1, 1);
            TimeSpan unixnow = DateTime.Now.Subtract(then);
            int seconds = Convert.ToInt32(utc);

            //convert it back to current DateTime
            DateTime dateTime = new DateTime(1970, 1, 1).AddSeconds(seconds);

            return dateTime;
        }

        /// <summary>
        /// Displays the item information and item data information
        /// for the given bank itemSummary.
        /// </summary>
        /// <param name="itemSummary">an itemSummary whose containerType is 'bank'</param>
        public void displayBankDataForItem(UserContext userContext, long itemId, out string ItemSummaryInfo, out string error, out Dictionary<BankData, List<BankTransactionData>> BankTransactionDataList)
        {
            DataExtent dataExtent = new DataExtent();
			dataExtent.startLevel = 0;
			dataExtent.endLevel = int.MaxValue;

            dataExtent.startLevelSpecified = true;
            dataExtent.endLevelSpecified = true;

			DataExtent da = new DataExtent();
			ItemSummary itemSummary = dataService.getItemSummaryForItem1(userContext, itemId, true, dataExtent );
			//String containerName = itemSummary.contentServiceInfo.containerInfo.containerName;			

            error = "";
            BankTransactionDataList = new Dictionary<BankData, List<BankTransactionData>>();

            String containerType = itemSummary.contentServiceInfo.containerInfo.containerName;
            if (!containerType.Equals("bank"))
            {
                throw new Exception("displayBankDataForItem called with invalid container type" + containerType);
            }

            DisplayItemInfo displayItemInfo = new DisplayItemInfo(_Config);
            ItemSummaryInfo = displayItemInfo.getItemSummaryInfo(itemSummary);

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
                    for (int i = 0; i < accounts.Length; i++)
                    {
                        BankData bankData = (BankData)accounts[i];
                        BankTransactionDataList.Add(bankData, new List<BankTransactionData>());
                        object[] bankTransactions = bankData.bankTransactions;
                        if (bankTransactions == null || bankTransactions.Length == 0)
                        {
                            error = "\t\tNo bank transactions";
                        }
                        else
                        {
                            for (int j = 0; j < bankTransactions.Length; j++)
                            {
                                BankTransactionData transactionData =
                                    (BankTransactionData)bankTransactions[j];
                                BankTransactionDataList[bankData].Add(transactionData);
                            }
                        }
                        error = "";
                    }
                }
            }

            // Get AccountHistory

            object[] acctHistories = itemData.accountHistory;
            if (acctHistories == null || acctHistories.Length == 0)
            {
                System.Console.WriteLine("\tNo Account History");
            }
            else
            {
                System.Console.WriteLine("\n\t**Account History**");
                for (int i = 0; i < acctHistories.Length; i++)
                {
                    AccountHistory acctHistory = (AccountHistory)acctHistories[i];

                    System.Console.WriteLine("\tAccount ID: {0}", acctHistory.accountId);

                    // Get History
                    object[] histories = acctHistory.history;
                    if (histories == null || histories.Length == 0)
                    {
                        System.Console.WriteLine("\t\tNo History");
                    }
                    else
                    {
                        System.Console.WriteLine("\t\t**History**");
                        for (int j = 0; j < histories.Length; j++)
                        {
                            BankData bankData = (BankData)histories[j];
                            System.Console.WriteLine("\t\tBank Account Name: {0}",
                                bankData.accountName);
                            System.Console.WriteLine("\t\tBank Account Cust Description: {0}",
                                bankData.customDescription);
                            System.Console.WriteLine("\t\tBank Account Identifier: {0}",
                                bankData.bankAccountId);
                            System.Console.WriteLine("\t\tBank Account Balance: {0}",
                                bankData.availableBalance.amount);
                            System.Console.WriteLine("\t\tBank Current Balance: {0}",
                                bankData.currentBalance.amount);
                            System.Console.WriteLine("\t\tBank Current Acct Type: {0}",
                                bankData.acctType);
                            System.Console.WriteLine("\t\tBank Current As of Date: {0}",
                                bankData.asOfDate.date);
                            System.Console.WriteLine("\t\tLast Updated: {0}\n",
                                UtcToDateTime(bankData.lastUpdated.HasValue ? bankData.lastUpdated.Value: 0));
                        }
                    }
                }
            }
        }
    }


}

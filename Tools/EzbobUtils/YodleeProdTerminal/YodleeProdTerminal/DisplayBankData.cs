using System;
using com.yodlee.sampleapps.datatypes;
using System.Xml;

namespace com.yodlee.sampleapps
{
	/// <summary>
	/// Displays a user's Bank item data in the Yodlee software platform..
	/// </summary>
	public class DisplayBankData : ApplicationSuper
	{
		DataServiceService dataService;

		public DisplayBankData()
		{
			dataService = new DataServiceService();
            dataService.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + "DataService";
		}

		/// <summary>
		/// Displays all the item summaries of banking items of the user.
		/// </summary>
		/// <param name="userContext"></param>
		public void displayBankData(UserContext userContext, bool isHistoryNeeded)
		{	
			// Create Data Extent
			DataExtent dataExtent = new DataExtent();
			dataExtent.startLevel = 0;
			dataExtent.endLevel = int.MaxValue;

			// Create Container Criteria
			ContainerCriteria cc = new ContainerCriteria();
			cc.dataExtent = dataExtent;
			cc.containerType = ContainerTypes.BANK;

			// Create a list of Container Criteria
			object[] list = {cc};

			// Create Summary request and add Container Criteria
			SummaryRequest sr = new SummaryRequest();
			sr.containerCriteria = list;
			sr.deletedItemAccountsNeeded = true;
			sr.historyNeeded = isHistoryNeeded;

			object[] itemSummaries = dataService.getItemSummaries1(userContext, sr);

			if(itemSummaries == null || itemSummaries.Length == 0) 
			{
				System.Console.WriteLine("No bank data available");
				return;
			}

			for(int i = 0; i < itemSummaries.Length; i++)
			{
				ItemSummary itemSummary = (ItemSummary)itemSummaries[i];
				displayBankDataForItem(itemSummary);
			}
		}

		/// <summary>
		/// Convert UTC to DateTime
		/// </summary>
		/// <param name="utc">time in utc</param>
		public DateTime UtcToDateTime(long utc)
		{
			//calculate UTC format for Now
			DateTime then = new DateTime(1970,1,1);
			TimeSpan unixnow = DateTime.Now.Subtract(then);
			int seconds = Convert.ToInt32(utc);

			//convert it back to current DateTime
			DateTime dateTime = new DateTime(1970,1,1).AddSeconds(seconds);

			return dateTime;
		}

		/// <summary>
		/// Displays the item information and item data information
		/// for the given bank itemSummary.
		/// </summary>
		/// <param name="itemSummary">an itemSummary whose containerType is 'bank'</param>
		public void displayBankDataForItem(ItemSummary itemSummary)
		{
			System.Console.WriteLine("");
			String containerType = itemSummary.contentServiceInfo.containerInfo.containerName;
			if (!containerType.Equals("bank")) 
			{
				throw new Exception("displayBankDataForItem called with invalid container type" +	containerType);
			}

			DisplayItemInfo displayItemInfo = new DisplayItemInfo();
			displayItemInfo.displayItemSummaryInfo(itemSummary);

			// Get ItemData
			ItemData1 itemData = itemSummary.itemData;
			if( itemData == null )
			{
				System.Console.WriteLine("\tItemData is null");
			}
			else
			{
				object[] accounts = itemData.accounts;
				if(accounts == null || accounts.Length == 0) 
				{
					System.Console.WriteLine("\tNo accounts");
				}
				else 
				{					
					for(int i = 0; i < accounts.Length; i++)
					{
                        System.Console.WriteLine("\n\t\t**BankData**");
                        BankData bankData = (BankData) accounts[i];
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
							bankData.asOfDate.date );

						// BankTransactionData
						object[] bankTransactions = bankData.bankTransactions;
						if (bankTransactions == null || bankTransactions.Length == 0) 
						{
							System.Console.WriteLine("\t\tNo bank transactions");
						}
						else 
						{
                            System.Console.WriteLine("\n\t\t**BankTransactionData**");
							for (int j = 0; j < bankTransactions.Length; j++)
							{
								BankTransactionData transactionData =
									(BankTransactionData) bankTransactions[j];
								System.Console.WriteLine("\t\t\tTransaction Id: {0}",
									transactionData.bankTransactionId);
								System.Console.WriteLine("\t\t\tTransaction Description: {0}",
									transactionData.description);
								System.Console.WriteLine("\t\t\tTransaction Amount: {0}",
									transactionData.transactionAmount.amount);
                                System.Console.WriteLine("\t\t\tTransaction Base Type: {0}",
                                    transactionData.transactionBaseType);
                                System.Console.WriteLine("\t\t\tCategory: {0}",
                                   transactionData.category);
                                System.Console.WriteLine("");
							}
						}
                        System.Console.WriteLine("");
					}
				}
			}

			// Get AccountHistory

			object[] acctHistories = itemData.accountHistory;
			if(acctHistories == null || acctHistories.Length == 0)
			{
				System.Console.WriteLine("\tNo Account History");
			}
			else
			{
				System.Console.WriteLine("\n\t**Account History**");
				for(int i = 0; i < acctHistories.Length; i++)
				{
					AccountHistory acctHistory = (AccountHistory)acctHistories[i];

					System.Console.WriteLine("\tAccount ID: {0}", acctHistory.accountId );

					// Get History
					object[] histories = acctHistory.history;
					if(histories == null || histories.Length == 0)
					{
						System.Console.WriteLine("\t\tNo History");
					}
					else
					{
						System.Console.WriteLine("\t\t**History**");
						for(int j = 0; j < histories.Length; j++)
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
								UtcToDateTime(bankData.lastUpdated.Value) );							
						}						
					}
				}
			}
		}
	}

}

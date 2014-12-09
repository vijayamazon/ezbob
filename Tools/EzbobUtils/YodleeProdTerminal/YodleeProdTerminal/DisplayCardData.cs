using System;

using com.yodlee.sampleapps.datatypes;

namespace com.yodlee.sampleapps
{
	/// <summary>
	/// Displays a user's Credit Card item data in the Yodlee software platform..
	/// </summary>
	public class DisplayCardData : ApplicationSuper
	{
		DataServiceService dataService;

		public DisplayCardData()
		{
			dataService = new DataServiceService();
            dataService.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + "DataService";
		}

		/// <summary>
		/// Displays all the item summaries of credit card items of the user.
		/// </summary>
		/// <param name="userContext"></param>
		public void displayCardData(UserContext userContext, bool isHistoryNeeded)
		{
			// Create Data Extent
			DataExtent dataExtent = new DataExtent();
			dataExtent.startLevel = 0;
			dataExtent.endLevel = int.MaxValue;

			// Create Container Criteria
			ContainerCriteria cc = new ContainerCriteria();
			cc.dataExtent = dataExtent;
			cc.containerType = ContainerTypes.CREDIT_CARD;

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
				DisplayCardDataForItem(itemSummary);
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
		public void DisplayCardDataForItem(ItemSummary itemSummary)
		{
			System.Console.WriteLine("");
			String containerType = itemSummary.contentServiceInfo.containerInfo.containerName;
			if (!containerType.Equals(ContainerTypes.CREDIT_CARD)) 
			{
				throw new Exception("DisplayCardDataForItem called with invalid container type" +
					containerType);
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
				// CardData
				object[] accounts = itemData.accounts;
				if(accounts == null || accounts.Length == 0) 
				{
					System.Console.WriteLine("\tNo accounts");
				}
				else 
				{
					System.Console.WriteLine("\n\t\t**CardData**");
					for(int i = 0; i < accounts.Length; i++)
					{
						CardData cardData = (CardData) accounts[i];
						System.Console.WriteLine("\t\taccountNumber : " + cardData.accountNumber );
						System.Console.WriteLine("\t\taccountId : " + cardData.accountId );
						System.Console.WriteLine("\t\tamountDue : " + cardData.amountDue.amount );
						System.Console.WriteLine("\t\tlastPayment : " + cardData.lastPayment.amount );
						System.Console.WriteLine("\t\tminPayment : " + cardData.minPayment );
						System.Console.WriteLine("\t\trunningBalance : " + cardData.runningBalance.amount );
						System.Console.WriteLine("\t\tavailableCredit : " + cardData.availableCredit.amount );
						System.Console.WriteLine("\t\ttotalCreditLine : " + cardData.totalCreditLine.amount );
						System.Console.WriteLine("\t\tlastUpdated: " + UtcToDateTime(cardData.lastUpdated.Value) );

						// CardStatementData
						object[] cardStatements = cardData.cardStatements;
						if (cardStatements == null || cardStatements.Length == 0) 
						{
							System.Console.WriteLine("\t\tNo card statements");
						}
						else 
						{
							System.Console.WriteLine("\t\t\t**CardStatementData**");
							for (int j = 0; j < cardStatements.Length; j++)
							{
								CardStatementData csd = (CardStatementData) cardStatements[j];
								System.Console.WriteLine("\t\t\tCardStatementData availableCredit: " + csd.availableCredit.amount );
								System.Console.WriteLine("\t\t\tCardStatementData availableCash: " + csd.availableCash.amount );
								System.Console.WriteLine("\t\t\tCardStatementData credit: " + csd.credit.amount );
								System.Console.WriteLine("\t\t\tCardStatementData payments: " + csd.payments.amount );

								// CardTransactionData
								object[] cardStatementTransactions = csd.cardTransactions;
								if (cardStatementTransactions == null || cardStatementTransactions.Length == 0) 
								{
									System.Console.WriteLine("\t\t\tNo card transactions");
								}
								else 
								{
									System.Console.WriteLine("\t\t\t\t**CardTransactionData**");
									for (int u = 0; u < cardStatementTransactions.Length; u++)
									{
										CardTransactionData ctd =
											(CardTransactionData) cardStatementTransactions[u];
										System.Console.WriteLine("\t\t\t\tTransaction transactionType: " + ctd.transactionType );
										System.Console.WriteLine("\t\t\t\tTransaction description: " + ctd.description );
										System.Console.WriteLine("\t\t\t\tTransaction postDate: " + ctd.postDate.date );
										System.Console.WriteLine("\t\t\t\tTransaction transAmount: " + ctd.transAmount.amount);
										System.Console.WriteLine("\t\t\t\tTransaction transDate: " + ctd.transDate.date );
									}
								}
								// End CardTransactionData
							}
						}
						// End CardStatementData

						// CardTransactionData
						object[] cardTransactions = cardData.cardTransactions;
						if (cardTransactions == null || cardTransactions.Length == 0) 
						{
							System.Console.WriteLine("\t\tNo card transactions");
						}
						else 
						{
							System.Console.WriteLine("\t\t\t**CardTransactionData**");
							for (int j = 0; j < cardTransactions.Length; j++)
							{
								CardTransactionData ctd =
									(CardTransactionData) cardTransactions[j];
								System.Console.WriteLine("\t\t\tTransaction transactionType: " + ctd.transactionType );
								System.Console.WriteLine("\t\t\tTransaction description: " + ctd.description );
								System.Console.WriteLine("\t\t\tTransaction postDate: " + ctd.postDate.date );
								System.Console.WriteLine("\t\t\tTransaction transAmount: " + ctd.transAmount.amount );
								System.Console.WriteLine("\t\t\tTransaction transDate: " + ctd.transDate.date );
							}
						}
						// End CardTransactionData

					}
				}
				// End CardData
			}

			/*// Get AccountHistory
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
							//BankData bankData = (BankData)histories[j];
							CardData cardData = (CardData) histories[j];
							System.Console.WriteLine("\t\taccountNumber : " + cardData.accountNumber );
							System.Console.WriteLine("\t\taccountId : " + cardData.accountId );
							System.Console.WriteLine("\t\tamountDue : " + cardData.amountDue.amount );
							System.Console.WriteLine("\t\tlastPayment : " + cardData.lastPayment.amount );
							System.Console.WriteLine("\t\tminPayment : " + cardData.minPayment );
							System.Console.WriteLine("\t\trunningBalance : " + cardData.runningBalance.amount );
							System.Console.WriteLine("\t\tavailableCredit : " + cardData.availableCredit.amount );
							System.Console.WriteLine("\t\ttotalCreditLine : " + cardData.totalCreditLine.amount );
							System.Console.WriteLine("\t\tlastUpdated: " + UtcToDateTime(cardData.lastUpdated.Value) );	
						}	
					}
				}
			}
			// AccountHistory*/
		}
	}

}

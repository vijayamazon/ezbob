using System;

using com.yodlee.sampleapps.datatypes;

namespace com.yodlee.sampleapps
{
	/// <summary>
	/// Displays a user's investment item data in the Yodlee software platform..
	/// </summary>
	public class DisplayInvestmentData : ApplicationSuper
	{
		DataServiceService dataService;

		public DisplayInvestmentData()
		{
			dataService = new DataServiceService();
            dataService.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + "DataService";
		}

		/// <summary>
		/// Displays all the item summaries of investment items of the user.
		/// </summary>
		/// <param name="userContext"></param>
		/// <param name="isHistoryNeeded"></param>
		public void displayInvestmentData(UserContext userContext, bool isHistoryNeeded)
		{
			// Create Data Extent
			DataExtent dataExtent = new DataExtent();
			dataExtent.startLevel = 0;
			dataExtent.endLevel = int.MaxValue;

			// Create Container Criteria
			ContainerCriteria cc = new ContainerCriteria();
			cc.dataExtent = dataExtent;
			cc.containerType = ContainerTypes.INVESTMENT;

			// Create a list of Container Criteria
			object[] list = {cc};

			// Create Summary request and add Container Criteria
			SummaryRequest sr = new SummaryRequest();
			sr.containerCriteria = list;
			sr.deletedItemAccountsNeeded = true;
			sr.historyNeeded = isHistoryNeeded;

			// Get ItemSummary
			object[] itemSummaries = dataService.getItemSummaries1(userContext, sr);

			// Verify that there is an ItemSummary
			if(itemSummaries == null || itemSummaries.Length == 0) 
			{
				System.Console.WriteLine("No bank data available");
				return;
			}

			for(int i = 0; i < itemSummaries.Length; i++)
			{
				ItemSummary itemSummary = (ItemSummary)itemSummaries[i];
				displayInvestmentDataForItem(itemSummary);
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
		/// for the given investment itemSummary.
		/// </summary>
		/// <param name="itemSummary">an itemSummary whose containerType is 'stocks'</param>
		public void displayInvestmentDataForItem(ItemSummary itemSummary)
		{
			System.Console.WriteLine("");
			String containerType = itemSummary.contentServiceInfo.containerInfo.containerName;
			if (!containerType.Equals(ContainerTypes.INVESTMENT)) 
			{
				throw new Exception("DisplayInvestmentDataForItem called with invalid container type" +
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
				// InvestmentData
				object[] accounts = itemData.accounts;
				if(accounts == null || accounts.Length == 0) 
				{
					System.Console.WriteLine("\tNo accounts");
				}
				else 
				{
					System.Console.WriteLine("\n\t\t**InvestmentData**");
					for(int i = 0; i < accounts.Length; i++)
					{
						InvestmentData investment = (InvestmentData) accounts[i];                        
						System.Console.WriteLine("\t\tAccount Name: "+ investment.accountName );
						System.Console.WriteLine("\t\tAccount Number: "+ investment.accountNumber );
						System.Console.WriteLine("\t\tAccount Holder: "+ investment.accountHolder );
						System.Console.WriteLine("\t\tAccount Type: "+ investment.acctType );
						System.Console.WriteLine("\t\tCash: "+ investment.cash );
						System.Console.WriteLine("\t\tTotal Balance: "+ investment.totalBalance.amount );
						System.Console.WriteLine("\t\tTotal Vested Balance: "+ investment.totalVestedBalance.amount );
						System.Console.WriteLine("\t\tTotal Unvested Balance: "+ investment.totalUnvestedBalance.amount );
						System.Console.WriteLine("\t\tMargin Balance: "+ investment.marginBalance.amount );
						System.Console.WriteLine("\t\tlastUpdated: " + UtcToDateTime(investment.lastUpdated.Value) );

						// HoldingData
						object[] holdings = investment.holdings;
						if (holdings == null || holdings.Length == 0) 
						{
							System.Console.WriteLine("\t\tNo investment holdings.");
						}
						else 
						{
							System.Console.WriteLine("\t\t\t**HoldingData**");
							for (int j = 0; j < holdings.Length; j++)
							{
								HoldingData holding = (HoldingData) holdings[j];
								System.Console.WriteLine("\t\t\tHoldingData Symbol: " + holding.symbol );
								System.Console.WriteLine("\t\t\tHoldingData Quantity: " + holding.quantity );
								System.Console.WriteLine("\t\t\tHoldingData Value: " + holding.value.amount );
								System.Console.WriteLine("\t\t\tHoldingData Description: " + holding.description );
								System.Console.WriteLine("\t\t\tHoldingData Price: " + holding.price.amount );
								System.Console.WriteLine("\t\t\tHoldingData Link: " + holding.link );
								System.Console.WriteLine("\t\t\tHoldingData HoldingType: " + holding.holdingType );
								System.Console.WriteLine("\t\t\tHoldingData HoldingTypeId: " + holding.holdingTypeId );
								System.Console.WriteLine("\t\t\tHoldingData Percentage Allocaton: " + holding.percentAllocation );
								System.Console.WriteLine("\t\t\tHoldingData Percantage Change: " + holding.percentageChange );
								System.Console.WriteLine("\t\t\tHoldingData Employee Contribution: " + holding.employeeContribution );
								System.Console.WriteLine("\t\t\tHoldingData Employeer Contribution: " + holding.employerContribution );
								System.Console.WriteLine("\t\t\tHoldingData Cusip Number: " + holding.cusipNumber );
								System.Console.WriteLine("\t\t\tHoldingData Daily Change: " + holding.dailyChange );
								System.Console.WriteLine("\t\t\tHoldingData Cost Basis: " + holding.costBasis.amount + "\n");

								// TaxLots
								object[] taxLots = holding.taxLots;
								if (taxLots == null || taxLots.Length == 0) 
								{
									System.Console.WriteLine("\t\t\tNo holdings taxLots");
								}
								else 
								{
									System.Console.WriteLine("\t\t\t\t**TaxLot**");
									for (int u = 0; u < taxLots.Length; u++)
									{
										TaxLot taxLot = (TaxLot) taxLots[u];
										System.Console.WriteLine("\t\t\t\tTaxLot Symbol: " + taxLot.symbol );
										System.Console.WriteLine("\t\t\t\tTaxLot Description: " + taxLot.description );
										System.Console.WriteLine("\t\t\t\tTaxLot Quantity: " + taxLot.quantity );
										System.Console.WriteLine("\t\t\t\tTaxLot Amount: " + taxLot.amount.amount );
										System.Console.WriteLine("\t\t\t\tTaxLot Price: " + taxLot.price.amount );
										System.Console.WriteLine("\t\t\t\tTaxLot Link: " + taxLot.link );
										System.Console.WriteLine("\t\t\t\tTaxLot Cusip Number: " + taxLot.cusipNumber + "\n");
									}
								}
								// End TaxLot
							}
						}
						// End HoldingData

						// InvestmentTransactionData
						object[] investTransactions = investment.investmentTransactions;
						if (investTransactions == null || investTransactions.Length == 0) 
						{
							System.Console.WriteLine("\t\tNo investment tranactions");
						}
						else 
						{
							System.Console.WriteLine("\t\t\t**InvestmentTransactionsData**");
							for (int j = 0; j < investTransactions.Length; j++)
							{
								InvestmentTransactionsData trans =
									(InvestmentTransactionsData) investTransactions[j];
								System.Console.WriteLine("\t\t\tTranaction.symbol: " + trans.symbol );
								System.Console.WriteLine("\t\t\tTranaction.amount: " + trans.amount.amount);
								System.Console.WriteLine("\t\t\tTranaction.price : " + trans.price.amount );
								System.Console.WriteLine("\t\t\tTranaction.quantity : " + trans.quantity );
								System.Console.WriteLine("\t\t\tTranaction.transDate : " + trans.transDate.date );
								System.Console.WriteLine("\t\t\tTransaction.description: " + trans.description );
								System.Console.WriteLine("\t\t\tTranaction.link : " + trans.link );
								System.Console.WriteLine("\t\t\tTranaction.transactionType : " + trans.transactionType );
								System.Console.WriteLine("\t\t\tTranaction.confirmantionNumber : " + trans.confirmationNumber + "\n" );
							}
						}
						// End InvestmentTransactionData

					}
				}
				// End InvestmentData
			}

/*
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
							InvestmentData investment = (InvestmentData) histories[j];
							System.Console.WriteLine("\t\taccountName: "+ investment.accountName );
							System.Console.WriteLine("\t\taccountNumber: "+ investment.accountNumber );
							System.Console.WriteLine("\t\taccountHolder: "+ investment.accountHolder );
							System.Console.WriteLine("\t\tacctType: "+ investment.acctType );
							System.Console.WriteLine("\t\tcash: "+ investment.cash );
							System.Console.WriteLine("\t\ttotalBalance: "+ investment.totalBalance.amount );
							System.Console.WriteLine("\t\ttotalVestedBalance: "+ investment.totalVestedBalance.amount );
							System.Console.WriteLine("\t\ttotalUnvestedBalance: "+ investment.totalUnvestedBalance.amount );
							System.Console.WriteLine("\t\tmarginBalance: "+ investment.marginBalance.amount );
							System.Console.WriteLine("\t\tlastUpdated: " + UtcToDateTime(investment.lastUpdated.Value) + "\n");	
						}	
					}
				}
			}
			// AccountHistory*/
		}		
	}
}

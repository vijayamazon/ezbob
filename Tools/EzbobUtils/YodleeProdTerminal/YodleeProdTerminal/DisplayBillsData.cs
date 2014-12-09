using System;

using com.yodlee.sampleapps.datatypes;

namespace com.yodlee.sampleapps
{
	/// <summary>
	/// Displays a user's bills item data in the Yodlee software platform..
	/// </summary>
	public class DisplayBillsData : ApplicationSuper
	{
		DataServiceService dataService;

		public DisplayBillsData()
		{
			dataService = new DataServiceService();
            dataService.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + "DataService";
		}

		/// <summary>
		/// Displays all the item summaries of bills items of the user.
		/// </summary>
		/// <param name="userContext"></param>
		/// <param name="isHistoryNeeded"></param>
		public void displayBillsData(UserContext userContext, bool isHistoryNeeded)
		{
			// Create Data Extent
			DataExtent dataExtent = new DataExtent();
			dataExtent.startLevel = 0;
			dataExtent.endLevel = int.MaxValue;

			// Create Container Criteria
			ContainerCriteria cc = new ContainerCriteria();
			cc.dataExtent = dataExtent;
			cc.containerType = ContainerTypes.BILL;

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
				System.Console.WriteLine("No bill data available");
				return;
			}

			for(int i = 0; i < itemSummaries.Length; i++)
			{
				ItemSummary itemSummary = (ItemSummary)itemSummaries[i];
				displayBillsDataForItem(itemSummary);
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
		/// for the given bill itemSummary.
		/// </summary>
		/// <param name="itemSummary">an itemSummary whose containerType is 'bills'</param>
		public void displayBillsDataForItem(ItemSummary itemSummary)
		{
			System.Console.WriteLine("");
			String containerType = itemSummary.contentServiceInfo.containerInfo.containerName;
			if (!containerType.Equals(ContainerTypes.BILL)) 
			{
				throw new Exception("DisplayBillsDataForItem called with invalid container type" +
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
				// BillsData
				object[] accounts = itemData.accounts;
				if(accounts == null || accounts.Length == 0) 
				{
					System.Console.WriteLine("\tNo accounts");
				}
				else 
				{
					System.Console.WriteLine("\n\t\t**BillsData**");
					for(int i = 0; i < accounts.Length; i++)
					{
						BillsData billsData = (BillsData) accounts[i];
						System.Console.WriteLine("\t\tBillsData.accountHolder: " + billsData.accountHolder );
						System.Console.WriteLine("\t\tBillsData.accountId: " + billsData.accountId );
						System.Console.WriteLine("\t\tBillsData.lastUpdated: " + UtcToDateTime(billsData.lastUpdated.Value) );

						// Bills
						object[] bills = billsData.bills;
						if (bills == null || bills.Length == 0) 
						{
							System.Console.WriteLine("\t\tNo bills holdings.");
						}
						else 
						{
							System.Console.WriteLine("\t\t\t**HoldingData**");
							for (int j = 0; j < bills.Length; j++)
							{
								Bill bill = (Bill) bills[j];
								System.Console.WriteLine("\t\t\tBill.accountNumber: " + bill.accountNumber );
								System.Console.WriteLine("\t\t\tBill.acctType: " + bill.acctType );
								System.Console.WriteLine("\t\t\tBill.pastDue: " + bill.pastDue.amount );
								System.Console.WriteLine("\t\t\tBill.lastpayment: " + bill.lastPayment );
								System.Console.WriteLine("\t\t\tBill.amountDue: " + bill.amountDue );
								System.Console.WriteLine("\t\t\tBill.minPayment: " + bill.minPayment );

								// AccountUsageData
								object[] accountUsages = bill.accountUsages;
								if (accountUsages == null || accountUsages.Length == 0) 
								{
									System.Console.WriteLine("\t\t\tNo accountUsages");
								}
								else 
								{
									System.Console.WriteLine("\t\t\t\t**AccountUsageData**");
									for (int u = 0; u < accountUsages.Length; u++)
									{
										AccountUsageData aud = (AccountUsageData) accountUsages[u];
										System.Console.WriteLine("\t\t\t\tAccountUsageData.billId: " + aud.billId );
										System.Console.WriteLine("\t\t\t\tAccountUsageData.unitsUsed: " + aud.unitsUsed );
									}
								}
								// End AccountUsageData
							}
						}
						// End Bills

					}
				}
				// End BillsData
			}

		}

	}

}

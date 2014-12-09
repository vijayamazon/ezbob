using System;

using com.yodlee.sampleapps.datatypes;

namespace com.yodlee.sampleapps
{
	/// <summary>
	/// Displays a user's Insurance item data in the Yodlee software platform..
	/// </summary>
	public class DisplayInsuranceData : ApplicationSuper
	{
		DataServiceService dataService;

		public DisplayInsuranceData()
		{
			dataService = new DataServiceService();
            dataService.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + "DataService";
		}

		/// <summary>
		/// Displays all the item summaries of Insurance items of the user.
		/// </summary>
		/// <param name="userContext"></param>
		/// <param name="isHistoryNeeded"></param>
		public void displayInsuranceData(UserContext userContext, bool isHistoryNeeded)
		{
			// Create Data Extent
			DataExtent dataExtent = new DataExtent();
			dataExtent.startLevel = 0;
			dataExtent.endLevel = int.MaxValue;

			// Create Container Criteria
			ContainerCriteria cc = new ContainerCriteria();
			cc.dataExtent = dataExtent;
			cc.containerType = ContainerTypes.INSURANCE;

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
				displayInsuranceDataForItem(itemSummary);
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
		/// for the given Insurance itemSummary.
		/// </summary>
		/// <param name="itemSummary">an itemSummary whose containerType is 'insurance'</param>
		public void displayInsuranceDataForItem(ItemSummary itemSummary)
		{
			System.Console.WriteLine("");
			String containerType = itemSummary.contentServiceInfo.containerInfo.containerName;
			if (!containerType.Equals(ContainerTypes.INSURANCE)) 
			{
				throw new Exception("DisplayInsuranceDataForItem called with invalid container type" +
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
				// InsuranceLoginAccountData
				object[] accounts = itemData.accounts;
				if(accounts == null || accounts.Length == 0) 
				{
					System.Console.WriteLine("\tNo accounts");
				}
				else 
				{
					System.Console.WriteLine("\n\t\t**InsuranceLoginAccountData**");
					for(int i = 0; i < accounts.Length; i++)
					{
						InsuranceLoginAccountData ilad = (InsuranceLoginAccountData) accounts[i];
						System.Console.WriteLine("\t\tInsuranceLoginAccountData.lastUpdated: " + UtcToDateTime(ilad.lastUpdated.Value) );

						// InsuranceData
						object[] insurancePolicys = ilad.insurancePolicys;
						if (insurancePolicys == null || insurancePolicys.Length == 0) 
						{
							System.Console.WriteLine("\t\tNo InsuranceData.");
						}
						else 
						{
							System.Console.WriteLine("\t\t\t**InsuranceData**");
							for (int j = 0; j < insurancePolicys.Length; j++)
							{
								InsuranceData insData = (InsuranceData) insurancePolicys[j];
								System.Console.WriteLine("\t\t\tInsuranceData.accountNumber: " + insData.accountNumber );
								System.Console.WriteLine("\t\t\tInsuranceData.cashValue: " + insData.cashValue.amount );
								System.Console.WriteLine("\t\t\tInsuranceData.insuranceType: " + insData.insuranceType );
							}
						}
						// End InsuranceData					
					}
				}
				// End InsuranceLoginAccountData
			}

		}

	}
}

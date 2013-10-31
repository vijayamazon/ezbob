using System;
using System.Collections;
using System.Web.Services.Protocols;

namespace com.yodlee.sampleapps
{
	/// <summary>
	/// Remove an Item
	/// </summary>
	public class RemoveItem : ApplicationSuper
	{
		DataServiceService dataService;
		ItemManagementService itemManagement;
		
		public RemoveItem()
		{
			dataService = new DataServiceService();
            dataService.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + "DataService";
			itemManagement = new ItemManagementService();
            itemManagement.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + "ItemManagementService";
		}

		/// <summary>
		/// Remove an Item
		/// </summary>
		/// <param name="userContext"></param>
		/// <param name="itemId"></param>
		public void removeItem(UserContext userContext, long itemId)
		{
			//System.Console.WriteLine("Removing itemId: " + itemId );
            try
            {
                itemManagement.removeItem(userContext, itemId, true);
                System.Console.WriteLine("Successfully removed itemId: " + itemId + "\n");
            }
            catch (SoapException soapExc)
            {
                System.Console.WriteLine("Error removing the item " + itemId + ": " + soapExc.Message + "\n");
            }
		}
		
		/// <summary>
		/// Remove an array of itemIds
		/// </summary>
		/// <param name="userContext"></param>
		/// <param name="itemIds"></param>
		public void removeItems(UserContext userContext, long[] itemIds)
		{
			for(int i=0; i<itemIds.Length; i++)
			{
				System.Console.WriteLine("Removing itemId: " + itemIds[i]);
				itemManagement.removeItem(userContext, itemIds[i], true);
			}
		}
		
		/// <summary>
		/// List all the accounts a user has.  The user is prompted to pick
		/// one to remove.  It returns the itemId of the item to remove.
		/// </summary>
		public long listAccounts(UserContext userContext)
		{
			// Get ItemSummary
			object[] itemSummaries = dataService.getItemSummariesWithoutItemData(userContext);
			
			// Verify that there is an ItemSummary
			if(itemSummaries == null || itemSummaries.Length == 0) 
			{
				System.Console.WriteLine("No bank data available");
				return 0;
			}

			System.Console.WriteLine("Please an account:");
			int count = 1;
			Hashtable map = new Hashtable();
			for(int i = 0; i < itemSummaries.Length; i++)
			{
				ItemSummary itemSummary = (ItemSummary)itemSummaries[i];
				System.Console.WriteLine(count + ". " +
					itemSummary.contentServiceInfo.contentServiceDisplayName );
				map.Add((long)count, (long)itemSummary.itemId);
				
				count++;

			}

			System.Console.Write("> ");
			// Read User Input
			String readStr = System.Console.ReadLine();
			
			// Convert input to a long
			long sel = long.Parse(readStr);
			long itemId = 0;
			// Get ItemId from Hashtable
			if( sel >= 1 && sel <= count )
			{				 
				IDictionaryEnumerator en = map.GetEnumerator();			
				while (en.MoveNext())
				{
					System.Console.WriteLine(en.Key + " : " + en.Value);
					long key = (long)en.Key;
					if(key == sel )
					{
						itemId = (long)en.Value;	
					}
				}
				return itemId;
			}
			else
			{
				System.Console.WriteLine("Error! Invalid Entry");
			}
			return 0;
		}	
	}	
}

using System;
using System.Text;

namespace YodleeLib
{
	using EzBob.Configuration;
	using StructureMap;
    using config;

    /// <summary>
    /// Encapsulates DataService functionality of the Yodlee software platform.
    /// </summary>
    public class DisplayItemInfo
    {
        readonly DataServiceService dataService;
		private static YodleeEnvConnectionConfig _config;
        public DisplayItemInfo()
        {
			_config = YodleeConfig._Config;
            dataService = new DataServiceService {Url = _config.soapServer + "/" + "DataService"};
        }

        /// <summary>
        /// Displays basic information about all items belonging to the user.
        /// </summary>
        /// <param name="userContext"></param>
		public object[] displayItemSummariesWithoutItemData(UserContext userContext)
        {
            object[] itemSummaries =
                dataService.getItemSummariesWithoutItemData(userContext);
            //_printItemSummaries(itemSummaries);
	        return itemSummaries;
        }

        /// <summary>
        /// Displays all information about all items belonging to the user.
        /// </summary>
        /// <param name="userContext"></param>
        public void displayItemSummaries(UserContext userContext)
        {
            object[] itemSummaries =
                dataService.getItemSummariesWithoutItemData(userContext);
            _printItemSummaries(itemSummaries);
        }

        /// <summary>
        /// Displays all information about items belonging to the user of a particular
        /// container type.
        /// </summary>
        /// <param name="userContext"></param>
        /// <param name="containerName"></param>
        public void displayItemSummariesForContainer(UserContext userContext,
                                                     String containerName)
        {
            object[] itemSummaries =
                    dataService.getItemSummariesForContainer(userContext, containerName);
            _printItemSummaries(itemSummaries);
        }

        /// <summary>
        /// Displays all information about the specified item.
        /// </summary>
        /// <param name="userContext"></param>
        /// <param name="itemId"></param>
        public void displayItemSummariesForItem(UserContext userContext, long itemId)
        {
            ItemSummary itemSummary = dataService.getItemSummaryForItem(userContext, itemId, true);
            var itemSummaries = new object[1];
            itemSummaries[0] = itemSummary;
            _printItemSummaries(itemSummaries);
        }

        /// <summary>
        /// Displays all information about the specified item.
        /// </summary>
        /// <param name="userContext"></param>
        /// <param name="itemIds"></param>
        public void displayItemSummariesForItems(UserContext userContext, long?[] itemIds)
        {
            object[] itemSummaries =
                    dataService.getItemSummaries3(userContext, itemIds);
            _printItemSummaries(itemSummaries);
        }

        private void _printItemSummaries(object[] itemSummaries)
        {
            if (itemSummaries == null)
            {
                Console.WriteLine("No items were found for the user.");
                return;
            }
            
            if (itemSummaries.Length == 0)
            {
                Console.WriteLine("No items were found for the user.");
                return;
            }
            /*
            foreach (object t in itemSummaries)
            {
                Console.WriteLine("Item Summary Information:");
                var itemSummary = (ItemSummary)t;
                //displayItemSummaryInfo(itemSummary);
                Console.WriteLine("");
            }
            */
        }

        /// <summary>
        /// Displays the item information for the given item summary.
        /// </summary>
        /// <param name="itemSummary">an item summary instance whose information
        /// is to be displayed.</param>
        public string getItemSummaryInfo(ItemSummary itemSummary)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("\tItem identifier: {0}",
                       itemSummary.itemId);
            sb.AppendFormat("\tItem display name: {0}",
                       itemSummary.itemDisplayName);
            sb.AppendFormat("\tContainer type: {0}",
                       itemSummary.contentServiceInfo.containerInfo.containerName);
            sb.AppendFormat("\tContent Service Name: {0}",
                       itemSummary.contentServiceInfo.siteDisplayName);
            sb.AppendFormat("\tLast updated time (U1TC seconds): {0}",
                       itemSummary.refreshInfo.lastUpdatedTime);
            sb.AppendFormat("\tLast update attempt time (UTC seconds): {0}",
                       itemSummary.refreshInfo.lastUpdateAttemptTime);
            sb.AppendFormat("\tRefresh status code: {0}",
                       itemSummary.refreshInfo.statusCode);
            return sb.ToString();
            /*ItemData1 itemData = itemSummary.itemData;
            if (itemData != null) 
            {
                object[] itemAccounts = itemData.accounts;
                System.Console.WriteLine("\t\tItem Account count: {0}", itemAccounts.Length);
                for(int i = 0; i < itemAccounts.Length; i++)  
                {
                    BaseTagData dataType = (BaseTagData) itemAccounts[i];
                    System.Console.WriteLine("\t\tData Type      : {0}", dataType.GetType().FullName);
                    System.Console.WriteLine("\t\tData (tostring): {0}", dataType.ToString());
                }
            }*/
        }

        public void viewItems(UserContext userContext)
        {
            var itemSummaries = (ItemSummary[])dataService.getItemSummaries(userContext);
            if (itemSummaries == null || itemSummaries.Length == 0)
            {
                Console.WriteLine("You have no Items Added.");
            }
            else
            {
                foreach (ItemSummary itemSummary in itemSummaries)
                {
                    String displayName = itemSummary.contentServiceInfo.contentServiceDisplayName;
                    Console.WriteLine("ItemId: " + itemSummary.itemId + " DisplayName: "
                                      + displayName + " errorCode: " + itemSummary.refreshInfo.statusCode +
                                      " refreshInfo time: " /**new Date(itemSummary.refreshInfo.lastUpdatedTime * 1000)*/);
                    ItemData1 id = itemSummary.itemData;
                    if (id != null)
                    {
                        var itemAccounts = (ItemAccountData[])id.accounts;
                        foreach (var iad in itemAccounts)
                        {
                            Console.WriteLine("\tItemAccountId: " + iad.itemAccountInfo.itemAccountId);
                        }
                    }
                }
            }

        }
    }
}

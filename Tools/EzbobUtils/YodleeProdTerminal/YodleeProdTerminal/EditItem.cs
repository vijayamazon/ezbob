using System;
using System.Collections;
using System.Web.Services.Protocols;
using com.yodlee.sampleapps.util;

namespace com.yodlee.sampleapps
{
    /// <summary>
    /// Edit an Item
    /// </summary>
    public class EditItem : ApplicationSuper
    {
        DataServiceService dataService;
        ItemManagementService itemManagement;
        String SEPARATOR = "***************************";
        ArrayList fieldInfoList = new ArrayList();

        public EditItem()
        {
            dataService = new DataServiceService();
            itemManagement = new ItemManagementService();
            dataService.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + "DataService";
            itemManagement.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + "ItemManagementService";
        }

        /// <summary>
        /// Get the current credentials of an item.
        /// </summary>
        /*public void getItemInfo(UserContext userContext, long itemId)
        {

            System.Console.WriteLine("Getting Credentials for item: " + itemId);
            try
            {
                Form form = itemManagement.getLoginFormCredentialsForItem(userContext, itemId, true);

                System.Console.WriteLine("\n" + SEPARATOR);
                FormUtil.PrintFormStructureAsText(form);
                System.Console.WriteLine(SEPARATOR + "\n");
                fieldInfoList = new ArrayList();
                //fieldInfoList = FormUtil.getUserInputFieldInfoList(userContext,
                //    itemManagement.getLoginFormCredentialsForItem(userContext, itemId,true));
                fieldInfoList = FormUtil.getUserInputFieldInfoList(userContext,
                    itemManagement.getLoginFormForContentService(userContext, csId, true)); 
               // FormUtil.getUserInputFieldInfoList(form, fieldInfoList);
            }
            catch (SoapException se)
            {
                System.Console.WriteLine(se.StackTrace);
            }
        }*/

        public void getItemInfo(UserContext userContext, long itemId)
        {
            System.Console.WriteLine("Getting Credentials for item: " + itemId);
            try
            {
                ItemSummary itemSummary = dataService.getItemSummaryForItem(userContext, itemId,true);
                Form form = itemManagement.getLoginFormForContentService(getCobrandContext(),itemSummary.contentServiceId, true);
                System.Console.WriteLine("\n" + SEPARATOR);
                FormUtil.PrintFormStructureAsText(form);
                System.Console.WriteLine(SEPARATOR + "\n");
                fieldInfoList = new ArrayList();
                FormUtil.getUserInputFieldInfoList(form, fieldInfoList);
            }
            catch (SoapException se)
            {
                System.Console.WriteLine(se.StackTrace);
            }
        }

        /// <summary>
        /// Update the item with the new credentials
        /// </summary>
        public void updateItem(UserContext userContext, long itemId)
        {
            try
            {
                itemManagement.updateCredentialsForItem1(userContext, itemId,true, fieldInfoList.ToArray(),true);

            }
            catch (SoapException se)
            {
                System.Console.WriteLine(se.Message);
                System.Console.WriteLine(se.StackTrace);
                System.Console.WriteLine("Unable to add item for content service!");
            }

        }

        /// <summary>
        /// List all the accounts a user has.  The user is prompted to pick
        /// one to edit.  It returns the itemId of the item to edit.
        /// </summary>
        public long listAccounts(UserContext userContext)
        {
            // Get ItemSummary
            object[] itemSummaries = dataService.getItemSummariesWithoutItemData(userContext);

            // Verify that there is an ItemSummary
            if (itemSummaries == null || itemSummaries.Length == 0)
            {
                System.Console.WriteLine("No bank data available");
                return 0;
            }

            System.Console.WriteLine("Please an account:");
            int count = 1;
            Hashtable map = new Hashtable();
            for (int i = 0; i < itemSummaries.Length; i++)
            {
                ItemSummary itemSummary = (ItemSummary)itemSummaries[i];
                System.Console.WriteLine(count + ". " +
                    itemSummary.contentServiceInfo.contentServiceDisplayName);
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
            if (sel >= 1 && sel <= count)
            {
                IDictionaryEnumerator en = map.GetEnumerator();
                while (en.MoveNext())
                {
                    //System.Console.WriteLine(en.Key + " : " + en.Value);
                    long key = (long)en.Key;
                    if (key == sel)
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

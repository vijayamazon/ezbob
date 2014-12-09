using System;
using System.Collections;
using System.IO;
using System.Web.Services.Protocols;
using com.yodlee.sampleapps.util;
using com.yodlee.sampleapps.datatypes;

namespace com.yodlee.sampleapps
{
    /// <summary>
    /// Displays all the Content Services in the Yodlee software platform and allows
    /// a user to add an item to any one of those services.
    /// </summary>
    public class AddItem : ApplicationSuper
    {
        const String SEPARATOR = "*****************************************************************";

        ContentServiceTraversalService cst;
        ItemManagementService itemManagement;        

        /// <summary>
        /// Constructs an instance of the AddItem class that
        //  provides the functionality to display all content.
        /// </summary>
        public AddItem()
        {
            cst = new ContentServiceTraversalService();
            cst.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + "ContentServiceTraversalService";
            itemManagement = new ItemManagementService();
            itemManagement.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + "ItemManagementService";
        }

        /// <summary>
        /// Adds the desired item for the user.
        /// </summary>
        public long addItem(UserContext userContext, long? csId, ArrayList fieldInfoList)
        {

            object[] fieldInfoArray = fieldInfoList.ToArray();

            System.Console.WriteLine("Adding Item...");
            long? itemId = 0;
            try
            {                
                bool itemIdSpecified = true;
                bool? shareCredentialsWithinSite = false;
                bool? startRefreshItemOnAddition = false;
                itemManagement.addItemForContentService1(userContext, csId, true, fieldInfoArray, shareCredentialsWithinSite, startRefreshItemOnAddition, out itemId, out itemIdSpecified);
            }
            catch (SoapException soapEx)
            {
                System.Console.WriteLine(soapEx.StackTrace);
                throw new Exception("Unable to add item for content service!");
            }
            System.Console.WriteLine("Successfully created itemId: " + itemId.Value);
            return itemId.Value;

        }

        public Form getLoginForm(long csId)
        {
            return itemManagement.getLoginFormForContentService(getCobrandContext(), csId, true);
        }

        /**
         * Get Login Form For Conent Service
         * @param userContext
         * @param csId
         * @return Form
         */
        public Form getLoginFormForContentService(UserContext userContext, long csId)
        {
            return itemManagement.getLoginFormForContentService(userContext, csId, true);
        }

        /**
         * Add the item
         * This primarily uses the FormUtil class to prompt the user
         * for their usernames and passwords and create the appropriate fieldInfoList.
         *
         * As a convenience it will also write out the populated HTML to disk
         */
        public long doAddItem(UserContext userContext, long contentServiceId)
        {
            // Prompt user to enter credentials
            ArrayList fieldInfoList = FormUtil.getUserInputFieldInfoList(userContext,
                    getLoginFormForContentService(userContext, contentServiceId));
            long itemId = addItem (userContext, contentServiceId, fieldInfoList);
            return itemId;
        }

        public ArrayList inputLoginForm(long csId)
        {
            ArrayList fieldInfoList = new ArrayList();
            Form form = getLoginForm(csId);
            FormUtil.PrintFormStructureAsText(form);
            FormUtil.getUserInputFieldInfoList(form, fieldInfoList);

            return fieldInfoList;
        }

        /// <summary>
        /// Returns item summaries without the item data.
        /// </summary>
        public void displayAllContentServices()
        {
            ContentServiceInfo[] csi =
                cst.getAllContentServices(getCobrandContext());

            if (csi.Length == 0)
            {
                System.Console.WriteLine("No content services!");
                return;
            }

            System.Console.WriteLine("Content Services Available:");
            for (int i = 0; i < csi.Length; i++)
            {
                System.Console.WriteLine("\tContent Service: {0}",
                        csi[i].contentServiceDisplayName);
                System.Console.WriteLine("\tSite name: {0}",
                        csi[i].siteDisplayName);
                System.Console.WriteLine("\tContent Service ID: {0}",
                        csi[i].contentServiceId);
                System.Console.WriteLine("");
            }
        }

        /// <summary>
        /// Prompts the user to enter a content service identifier
        /// </summary>
        public long chooseItem()
        {
            System.Console.WriteLine("Choose the Content Service Identifier for the Item you want to add");
            System.Console.Write("> ");

            // Read User Input
            String readStr = System.Console.ReadLine();

            // Convert input to a long
            long csId = long.Parse(readStr);
            // todo: must handle a number format error here

            return csId;

        }

        public ArrayList inputFieldInfos(long csId)
        {
            System.Console.WriteLine("You will be prompted for the values to enter for each FieldInfo. \n" +
                    "Type <RETURN> to proceed to the next FieldInfo. The values you \n" +
                    "enter must correspond to the FieldInfo grouping and validity \n" +
                    "constraints displayed above.");
            System.Console.WriteLine(SEPARATOR);

            ArrayList fieldInfoList = new ArrayList();
            Form form = itemManagement.getLoginFormForContentService(getCobrandContext(), csId, true);
            FormUtil.getUserInputFieldInfoList(form, fieldInfoList);

            return fieldInfoList;
        }

        public void printForm(long csId)
        {
            Form form = itemManagement.getLoginFormForContentService(getCobrandContext(), csId, true);
            // todo: must handle a contentservicenotfoundexception here

            System.Console.WriteLine("Chosen identifier: {0}", csId);
            System.Console.WriteLine("The following are the fields you need to enter to add an item\nto this Content Service:");
            System.Console.WriteLine(SEPARATOR);
            FormUtil.PrintFormStructureAsText(form);
        }

        //constructs ItemData for bank container
        private ItemData getBankItemData()
        {
            ItemData itemData = new ItemData();
            ArrayList accounts = new ArrayList();

            BankData bankData = new BankData();            
            System.Console.WriteLine("\nEnter account holder name");
            String acctHolder = IOUtils.readStr();
            bankData.accountHolder = acctHolder;

            System.Console.WriteLine("Enter accountNo.");
            String accNo = IOUtils.readStr();
            bankData.accountNumber = accNo;

            System.Console.WriteLine("Enter balance");
            long bal = IOUtils.readLong();
            YMoney balance = new YMoney();
            balance.amount = bal;
            balance.currencyCode = Currency.USD;
            bankData.availableBalance = balance;
            bankData.currentBalance = balance;

            bankData.accountName = "Custom Account";
            bankData.customDescription = "Test";
            bankData.includeInNetworth = 1;
            bankData.hasDetails = 1;
            bankData.isItemAccountDeleted = 0;
            bankData.isDeleted = 0;
            bankData.lastUpdated = 999000;
            bankData.isSeidMod = 0;
            bankData.acctType = AccountType.CHECKING;
            YMoney intYtd = new YMoney();
            intYtd.amount = 12.5;
            intYtd.currencyCode = Currency.USD;
            bankData.interestEarnedYtd = intYtd;

            YMoney od = new YMoney();
            od.amount = 25;
            od.currencyCode = Currency.USD;
            bankData.overdraftProtection = od;

            YDate mat = new YDate();
            mat.date = DateTime.Now;
            bankData.maturityDate = mat;
            YDate asOf = new YDate();
            asOf.date = DateTime.Now;
            bankData.asOfDate = asOf;
            bankData.shortNickName = "";
            bankData.nickName = "";

            accounts.Add(bankData);
            //itemData.accounts = accounts.ToArray();

            return itemData;
        }
    }
}

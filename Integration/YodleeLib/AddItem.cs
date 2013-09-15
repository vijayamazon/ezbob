using System;
using System.Collections;
using System.Web.Services.Protocols;
using YodleeLib.util;
using YodleeLib.datatypes;

namespace YodleeLib
{
	using EzBob.Configuration;
	using StructureMap;
    using config;

    /// <summary>
    /// Displays all the Content Services in the Yodlee software platform and allows
    /// a user to add an item to any one of those services.
    /// </summary>
    public class AddItem : ApplicationSuper
    {
        const String Separator = "*****************************************************************";

        readonly ContentServiceTraversalService _cst;
        readonly ItemManagementService _itemManagement;
		private static YodleeEnvConnectionConfig _config;
        /// <summary>
        /// Constructs an instance of the AddItem class that
        //  provides the functionality to display all content.
        /// </summary>
        public AddItem()
        {
			_config = YodleeConfig._Config;
            _cst = new ContentServiceTraversalService {Url = _config.soapServer + "/" + "ContentServiceTraversalService"};
            _itemManagement = new ItemManagementService {Url = _config.soapServer + "/" + "ItemManagementService"};
        }

        /// <summary>
        /// Adds the desired item for the user.
        /// </summary>
        public long addItem(UserContext userContext, long? csId, ArrayList fieldInfoList)
        {

            object[] fieldInfoArray = fieldInfoList.ToArray();

            Console.WriteLine("Adding Item...");
            long? itemId = 0;
            try
            {                
                bool itemIdSpecified = true;
                bool? shareCredentialsWithinSite = false;
                bool? startRefreshItemOnAddition = false;
                _itemManagement.addItemForContentService1(userContext, csId, true, fieldInfoArray, shareCredentialsWithinSite, startRefreshItemOnAddition, out itemId, out itemIdSpecified);
            }
            catch (SoapException soapEx)
            {
                Console.WriteLine(soapEx.StackTrace);
                throw new Exception("Unable to add item for content service!");
            }
            Console.WriteLine("Successfully created itemId: " + itemId.Value);
            return itemId.Value;

        }

        public Form getLoginForm(long csId)
        {
            return _itemManagement.getLoginFormForContentService(GetCobrandContext(), csId, true);
        }

        /**
         * Get Login Form For Conent Service
         * @param userContext
         * @param csId
         * @return Form
         */
        public Form getLoginFormForContentService(UserContext userContext, long csId)
        {
            return _itemManagement.getLoginFormForContentService(userContext, csId, true);
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
            var fieldInfoList = new ArrayList();
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
                _cst.getAllContentServices(GetCobrandContext());

            if (csi.Length == 0)
            {
                Console.WriteLine("No content services!");
                return;
            }

            Console.WriteLine("Content Services Available:");
            foreach (ContentServiceInfo contentService in csi)
            {
                Console.WriteLine("\tContent Service: {0}",
                                  contentService.contentServiceDisplayName);
                Console.WriteLine("\tSite name: {0}",
                                  contentService.siteDisplayName);
                Console.WriteLine("\tContent Service ID: {0}",
                                  contentService.contentServiceId);
                Console.WriteLine("");
            }
        }

        /// <summary>
        /// Prompts the user to enter a content service identifier
        /// </summary>
        public long chooseItem()
        {
            Console.WriteLine("Choose the Content Service Identifier for the Item you want to add");
            Console.Write("> ");

            // Read User Input
            String readStr = Console.ReadLine();

            // Convert input to a long
            long csId = long.Parse(readStr);
            // todo: must handle a number format error here

            return csId;

        }

        public ArrayList inputFieldInfos(long csId)
        {
            Console.WriteLine("You will be prompted for the values to enter for each FieldInfo. \n" +
                    "Type <RETURN> to proceed to the next FieldInfo. The values you \n" +
                    "enter must correspond to the FieldInfo grouping and validity \n" +
                    "constraints displayed above.");
            Console.WriteLine(Separator);

            var fieldInfoList = new ArrayList();
            Form form = _itemManagement.getLoginFormForContentService(GetCobrandContext(), csId, true);
            FormUtil.getUserInputFieldInfoList(form, fieldInfoList);

            return fieldInfoList;
        }

        public void printForm(long csId)
        {
            Form form = _itemManagement.getLoginFormForContentService(GetCobrandContext(), csId, true);
            // todo: must handle a contentservicenotfoundexception here

            Console.WriteLine("Chosen identifier: {0}", csId);
            Console.WriteLine("The following are the fields you need to enter to add an item\nto this Content Service:");
            Console.WriteLine(Separator);
            FormUtil.PrintFormStructureAsText(form);
        }
        
        //constructs ItemData for bank container
        private ItemData getBankItemData()
        {
            ItemData itemData = new ItemData();
            ArrayList accounts = new ArrayList();
            
            BankData bankData = new BankData();            
            Console.WriteLine("\nEnter account holder name");
            String acctHolder = IOUtils.readStr();
            bankData.accountHolder = acctHolder;

            Console.WriteLine("Enter accountNo.");
            String accNo = IOUtils.readStr();
            bankData.accountNumber = accNo;

            Console.WriteLine("Enter balance");
            long bal = IOUtils.readLong();
            var balance = new YMoney();
            balance.amount = bal;
            balance.currencyCode = Currency.GBP;
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
            var intYtd = new YMoney();
            intYtd.amount = 12.5;
            intYtd.currencyCode = Currency.GBP;
            bankData.interestEarnedYtd = intYtd;

            var od = new YMoney();
            od.amount = 25;
            od.currencyCode = Currency.GBP;
            bankData.overdraftProtection = od;

            var mat = new YDate();
            mat.date = DateTime.Now;
            bankData.maturityDate = mat;
            var asOf = new YDate();
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

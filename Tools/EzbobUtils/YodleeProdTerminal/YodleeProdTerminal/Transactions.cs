using System;
using System.Collections;
using System.Text;

using System.Web.Services.Protocols;
using com.yodlee.sampleapps.util;
using com.yodlee.sampleapps.datatypes;

namespace com.yodlee.sampleapps
{
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Serialization;

    public class Transactions : ApplicationSuper
    {
        protected TransactionSearchServiceService transactionSearchService;
        protected TransactionCategorizationServiceService tcService;
        protected TransactionManagementService transactionManagementService;

        private static int OPTION_CNT = 1;
        private static int NAV_QUIT = 0;
        private static int NAV_VIEW_ITEM_ACCOUNT_TRANSACTIONS = OPTION_CNT++;
        private static int NAV_VIEW_ALL_TRANSACTIONS = OPTION_CNT++;
        private static int NAV_SEARCH_TRANSACTIONS = OPTION_CNT++;
        private static int NAV_CATEGORIZE_TRANSACTION = OPTION_CNT++;
        private static int NAV_ADD_SPLIT_TRANSACTION = OPTION_CNT++;
        private static int NAV_DELETE_SPLIT_TRANSACTION = OPTION_CNT++;
        private static int NAV_ADD_SUB_CATEGORY = OPTION_CNT++;
        private static int NAV_DISPLAY_SUB_CATEGORY = OPTION_CNT++;
        private static int NAV_DELETE_SUB_CATEGORY = OPTION_CNT++;
        private static int NAV_EDIT_TRANSACTION_DESCRIPTION = OPTION_CNT++;
        private static int NAV_ADD_MANUAL_TRANSACTION = OPTION_CNT++;
        private static int NAV_VIEW_MANUAL_TRANSACTIONS = OPTION_CNT++;

        public static String InvalidEnterPrompt = "Invalid Entry.. Enter ";
        public static String EXIT_VALUE = "-1";
        public static String EXIT_STRING = " or " + EXIT_VALUE + " to Quit : ";
        public static String EnterPrompt = "Enter ";
        public static String ItemIdPrompt = EnterPrompt + "ItemId : ";
        public static String ReItemIdPrompt = InvalidEnterPrompt + ItemIdPrompt 
    		    + EXIT_STRING;

        public static String ItemAccountIdPrompt = EnterPrompt + "ItemAccountId : ";
        public static String ReItemAcountIdPrompt = InvalidEnterPrompt + ItemAccountIdPrompt
                + EXIT_STRING;
        public static String searchStringPrompt = EnterPrompt + "Search String : ";
        public static String ReSarchStringPrompt = InvalidEnterPrompt + searchStringPrompt
                + EXIT_STRING;
        public static String TransactionIdPrompt = EnterPrompt + "TransactionId : ";
        public static String ReTransactionIdPrompt = InvalidEnterPrompt + TransactionIdPrompt
    		    + EXIT_STRING;
        public static String CategoryIdPrompt = EnterPrompt + "CategoryId : ";
        public static String ReCategoryIdPrompt = InvalidEnterPrompt + CategoryIdPrompt
    		    + EXIT_STRING;
        public static String ContainerTypePrompt = EnterPrompt + "ContainerType : ";
        public static String ReContainerTypePrompt = InvalidEnterPrompt + ContainerTypePrompt
    		    + EXIT_STRING;

        public static long SPLIT_TXN_OP_ADD = 1;
        public static long SPLIT_TXN_OP_DELETE = 2;

        //CATEGORY_LEVEL
        public static long SUPER_CATEGORY = 2;
        public static long CATEGORY = 3;
        public static long SUB_CATEGORY = 4;

        //SPLIT_TRANSACTION
        public static long BANK_TRANSACTION = 11;
        public static long CARD_TRANSACTION = 20;
        public static long INSURANCE_TRANSACTION = 45;
        public static long INVESTMENT_TRANSACTION = 48;
        public static long LOAN_TRANSACTION = 61;
        public static long MANUAL_TRANSACTION = 101;

        public Transactions ()
        {
            transactionSearchService = new TransactionSearchServiceService();
            transactionSearchService.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + "TransactionSearchService";
            tcService = new TransactionCategorizationServiceService();
           // tcService.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + tcService.GetType().FullName;
            tcService.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + "TransactionCategorizationService";
            transactionManagementService = new TransactionManagementService();
            //transactionManagementService.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + transactionManagementService.GetType().FullName;
            transactionManagementService.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + "TransactionManagementService";
        }

        public void doMenu(UserContext userContext) {
            Boolean loop = true;
            int choice = 0;
            while (loop) {
                try {
                    System.Console.WriteLine("Transactions Menu");
                    System.Console.WriteLine(NAV_VIEW_ITEM_ACCOUNT_TRANSACTIONS + ". View Transactions for ItemAccount");
                    System.Console.WriteLine(NAV_VIEW_ALL_TRANSACTIONS + ". View all transactions");
                    System.Console.WriteLine(NAV_SEARCH_TRANSACTIONS + ". Search Transactions");
                    System.Console.WriteLine(NAV_CATEGORIZE_TRANSACTION + ". Categorize Transaction");
                    System.Console.WriteLine(NAV_ADD_SPLIT_TRANSACTION + ". Split Transaction");
                    System.Console.WriteLine(NAV_DELETE_SPLIT_TRANSACTION + ". Delete Split Transation");
                    System.Console.WriteLine(NAV_ADD_SUB_CATEGORY + ". Add Sub-Category");
                    System.Console.WriteLine(NAV_DISPLAY_SUB_CATEGORY + ". Display Sub-Category");
                    System.Console.WriteLine(NAV_DELETE_SUB_CATEGORY + ". Delete Sub-Category");
                    System.Console.WriteLine(NAV_EDIT_TRANSACTION_DESCRIPTION + ". Edit Transaction description");
                    System.Console.WriteLine(NAV_VIEW_MANUAL_TRANSACTIONS + ". View Manual Transaction");
                    System.Console.WriteLine(NAV_ADD_MANUAL_TRANSACTION + ". Add Manual Transaction");
                    System.Console.WriteLine(NAV_QUIT + ". Exit");
                    System.Console.WriteLine("\n");
                    System.Console.WriteLine("Enter Choice : ");
                    choice = IOUtils.readInt();

                    if (choice == NAV_VIEW_ITEM_ACCOUNT_TRANSACTIONS)
                        viewTransactionsForItemAccount(userContext);
                    if (choice == NAV_VIEW_ALL_TRANSACTIONS)
                        viewAllTransactions(userContext);
                    if (choice == NAV_SEARCH_TRANSACTIONS)
                        searchTransactions(userContext);
                    if (choice == NAV_CATEGORIZE_TRANSACTION)
                        categorizeTransaction(userContext);
                    if (choice == NAV_ADD_SPLIT_TRANSACTION)
                        manageSplitTransaction(userContext, Transactions.SPLIT_TXN_OP_ADD);
                    if (choice == NAV_DELETE_SPLIT_TRANSACTION)
                        manageSplitTransaction(userContext, Transactions.SPLIT_TXN_OP_DELETE);
                    if (choice == NAV_ADD_SUB_CATEGORY)
                        addSubCategory(userContext);
                    if (choice == NAV_DISPLAY_SUB_CATEGORY)
                        displaySubCategory(userContext);
                    if (choice == NAV_DELETE_SUB_CATEGORY)
                        deleteSubCategories(userContext);
                    if (choice == NAV_EDIT_TRANSACTION_DESCRIPTION)
                        editTransactionDescription(userContext);
                    if (choice == NAV_VIEW_MANUAL_TRANSACTIONS)
                        viewManualTransactions(userContext);
                    if (choice == NAV_ADD_MANUAL_TRANSACTION)
                        addManualTransaction(userContext);
                    if (choice == NAV_QUIT)
                        loop = false;
                }
                catch (SoapException soapEx)
                {
                    System.Console.WriteLine(soapEx.Message);
                    System.Console.WriteLine(soapEx.StackTrace);
                    /*CoreException coreEx = ExceptionHandler.processException(soapEx);
                    if (coreEx != null)
                    {
                        System.Console.WriteLine(coreEx.message);
                        System.Console.WriteLine(coreEx.trace);
                        System.Console.WriteLine(soapEx.StackTrace);
                    }*/
                }
                catch (Exception e) {
                    System.Console.WriteLine("Exception:::" + e.Message);
                    System.Console.WriteLine(e.StackTrace);
                }
            }
        }

        public void viewTransactionsForItemAccount(UserContext userContext){

            // Display Item Accounts
            displayItemAccounts(userContext);

            //Prompt for the Item to be Searched.
            String itemId = IOUtils.promptInput(
                    ItemIdPrompt,
                    ReItemIdPrompt);            

            // Prompt for the Item Account to be Searched.
            String itemAccountId = IOUtils.promptInput(
                ItemAccountIdPrompt,
                ReItemAcountIdPrompt);           

            System.Console.WriteLine("Retrieving transactions for ItemId="
        		    + itemId + ", ItemAccountId=" + itemAccountId + " ...");             
            /** 
             * The Transaction Search API will only calculate a running balance
             * if the search is in the bank container and the current balance
             * is fed to the search request.  This is a check to see if the
             * itemAccountObject is a bank itemAccountObject and to get back the current balance
             * of the bank itemAccountObject so the API can be properly populated.
             */
            String containerName = null;
            double? runningBalance = -1;
            try {
	            DataServiceService dataService = new DataServiceService();
                dataService.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + "DataService";
	            long?[] itemIds = new long?[] {long.Parse(itemId)};
                object[] itemSummaries = dataService.getItemSummaries3(
                        userContext, itemIds);

	            if(itemSummaries != null) {
	                for(int i=0; i<itemSummaries.Length; i++){
			            ItemSummary itemSummary = (ItemSummary)itemSummaries[i];
			            containerName = itemSummary.contentServiceInfo.containerInfo.containerName;
                        object[] accounts = itemSummary.itemData.accounts;
                        if(containerName.Equals(ContainerTypes.BANK) 
                    		    && accounts.Length > 0) {

                        }                            
                            for (int j = 0; j < accounts.Length; j++) 
                            {
	                            BankData bankData = (BankData) accounts[j];
                                long? tempItemAccountId = bankData.itemAccountId;
                                if(tempItemAccountId == long.Parse(itemAccountId)) {
                            	    runningBalance = bankData.currentBalance.amount;
                                }
                            }

	                }
	            }
            } catch (Exception e) {
        	    System.Console.WriteLine("Failed to get container: " + e.Message);
            }
            // End the check for container and running balance.

            long startRange = 1;
            long endRange = 200;

            // Create Results Range
            TransactionSearchResultRange txSearchResultRange = new TransactionSearchResultRange();
            txSearchResultRange.startNumber = startRange;
            txSearchResultRange.startNumberSpecified = true;
            txSearchResultRange.endNumber = endRange;
            txSearchResultRange.endNumberSpecified = true;

            // Create  TransactionSearchFilter
            TransactionSearchFilter txSearchFilter = new TransactionSearchFilter();             
            ItemAccountId itemAccountObject = new ItemAccountId();
            itemAccountObject.identifier = long.Parse(itemAccountId);
            itemAccountObject.identifierSpecified = true;
            txSearchFilter.itemAccountId = itemAccountObject;
            txSearchFilter.transactionSplitType = TransactionSplitType.ALL_TRANSACTION;
            txSearchFilter.transactionSplitTypeSpecified = true;
            // Create TransactionSearchRequest
            TransactionSearchRequest txSearchRequest = new TransactionSearchRequest();
            txSearchRequest.searchFilter = txSearchFilter;
            txSearchRequest.containerType = "all";
            txSearchRequest.ignorePaymentTransactions = false;
            txSearchRequest.resultRange = txSearchResultRange;

            txSearchRequest.ignoreUserInput = true;
            txSearchRequest.userInput = "";
            //txSearchRequest.ignoreManualTransactions = true;
            txSearchRequest.includeAggregatedTransactions = true;
            txSearchRequest.isSharedAccountTransactionReq = true;

            /**
             * Calculating a running balance requires setting the container
             * restriction to bank, setting a current balance, and
             * turning on the flag
             */               
            if(ContainerTypes.BANK.Equals(containerName)){
                txSearchRequest.containerType = "bank";
                if(runningBalance > -1) {
            	    txSearchRequest.currentBalance = runningBalance;
                    txSearchRequest.calculateTransactionBalance = true;
                }
            }            
            txSearchRequest.searchClients = TransactionSearchClients.DEFAULT_SERVICE_CLIENT;
            txSearchRequest.searchClientsSpecified = true;
            /*
              txSearchRequest.setUserInput(searchString);
              Date sysDate = new Date(System.currentTimeMillis());
              Date fromDate = new Date(System.currentTimeMillis());
              fromDate.setDate(1);
              DateRange dateRange = new DateRange();
              dateRange.setFromDate(fromDate);
              dateRange.setToDate(sysDate);
              searchFilter.setPostDateRange(dateRange);
              searchResultRange.setStartNumber(startRange);
              searchResultRange.setEndNumber(endRange);
              txSearchRequest.setResultRange(searchResultRange);
              */
            try
            {
                TransactionSearchExecInfo txSearchExecInfo =
                        transactionSearchService.executeUserSearchRequest(userContext, txSearchRequest);
                if (txSearchExecInfo != null)
                {
                    displayTransactionSearchExecInfo(txSearchExecInfo);
                }

                viewTransactions(userContext, txSearchExecInfo);
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Msg:" + e.Message);
                System.Console.WriteLine("Exc:" + e.StackTrace);
            }
        }

        public void displayTransactionSearchExecInfo(TransactionSearchExecInfo transactionSearchExecInfo) {
            System.Console.WriteLine("TransactionSearchExecInfo Details: ");
            System.Console.WriteLine("\tCount of All Transaction: " + transactionSearchExecInfo.countOfAllTransaction);
            System.Console.WriteLine("\tNumber of Hit: " + transactionSearchExecInfo.numberOfHits);
            System.Console.WriteLine("\tNumber of Hits for Projected: " + transactionSearchExecInfo.numberOfHitsForProjected);
            TransactionSearchIdentifier transactionSearchIdentifier = transactionSearchExecInfo.searchIdentifier;
            if(transactionSearchIdentifier == null) {
        	    System.Console.WriteLine("\ttransactionSearchIdentifier is null");
            } else {
        	    System.Console.WriteLine("\ttransactionSearchIdentifier details: ");

        	    String[] identifiers = transactionSearchIdentifier.identifiers;
        	    if(identifiers != null) {
	        	    for(int i=0; i<identifiers.Length; i++) {
	        		    System.Console.WriteLine("\t\tidentifiers[" + i + "] = " + identifiers[i]);
	        	    }
        	    } else {
        		    System.Console.WriteLine("\t\tidentifiers is null");
        	    }

        	    String identifier = transactionSearchIdentifier.identifier;
        	    if(identifier != null) {
        		    System.Console.WriteLine("\t\tidentifier = " + identifier);
        	    } else {
                    System.Console.WriteLine("\t\tidentifier is null");
        	    }
            }                	
        }

        public void viewTransactions(UserContext userContext, TransactionSearchExecInfo txSearchExecInfo){
            long startRange = 1;
            long endRange = 10;
            TransactionSearchIdentifier txSearchId = txSearchExecInfo.searchIdentifier;
            long? searchHit = txSearchExecInfo.numberOfHits;
            int pageId = 1;

            String description;
            TransactionSearchResult txSearchResult = txSearchExecInfo.searchResult;
            if (txSearchResult != null && txSearchResult.transactions.Length > 0) {
        	    TransactionView [ ] txView = txSearchResult.transactions;        	
        	    while ( searchHit > 0 ) {
        		    if (txView.Length < 10 || startRange <= txView.Length) {  		
        			    System.Console.WriteLine("\n#### Page "+ pageId + " transaction " + startRange + " - "+endRange+ " ####\n");
        		    }
                    pageId ++;
                    for (int i = (int) (startRange - 1); i < (int) endRange && i < txView.Length; i++) {
                        // This logic is to print user set description for the txn if it is present
                        if (txView[i].description != null && txView[i].description.viewPref
                            && txView[i].description.userDescription != null)
                        {
                            description = txView[i].description.userDescription;
                        } else {
                            description = txView[i].description.description;
                        }
                	    System.Console.WriteLine(
                                "PostDate=" + Formatter.formatDate(txView[i].postDate, Formatter.DATE_SHORT_FORMAT )+ " " +
                                        "TransDate=" + Formatter.formatDate(txView[i].transactionDate, Formatter.DATE_SHORT_FORMAT )+ "\n " +
                                        "ItemAccountId=" +txView[i].account.itemAccountId + "\n " +
                                        "TransactionId=" +txView[i].viewKey.transactionId + "\n " +
                                        "ContainerType=" +txView[i].viewKey.containerType + "\n " +
                                        "Desc=" + description + "\n " +
                                        "AccountName=" +txView[i].account.accountName +  "\n " +
                                        "Mem=" +txView[i].memo.memo + "\n " +
                                        "CategoryName=" +txView[i].category.categoryName+ "\n " +
                                        "Status=" +txView[i].status.description + "\n " +
                                        "Price=" +Formatter.formatMoney(txView[i].price) + "\n " +
                                        "Quantity=" +txView[i].quantity + "\n " +
                                        "CatKeyword=" +txView[i].categorizationKeyword + "\n " +
                                        "RunningBalance=" +txView[i].runningBalance + "\n " +
                                        "Amount=" +Formatter.formatMoney(txView[i].amount) + " "
                        );
                        if (txView[i].viewKey.splitType != null) {
                            TransactionKeyData txnKeyData = new TransactionKeyData();
                            txnKeyData.transactionId = txView[i].viewKey.transactionId;
                            txnKeyData.transactionIdSpecified = true;
                            txnKeyData.itemDataTableId = fetchItemDataTableId(txView[i].viewKey.containerType);
                            txnKeyData.itemDataTableIdSpecified = true;
                            txnKeyData.containerType = txView[i].viewKey.containerType;
                            txnKeyData.splitType = txView[i].viewKey.splitType;
                            try
                            {
                                SplitTransaction[] splittxns = transactionManagementService.getAllSplitTransactions(userContext, txnKeyData);
                                if (splittxns != null && splittxns.Length > 0)
                                {
                                    System.Console.WriteLine("\n\t**Split Transaction data**\n ");
                                    for (int s = 0; s < splittxns.Length; s++)
                                    {                                        
                                        SplitTransaction splitTxn = splittxns[s];
                                        System.Console.WriteLine(                                        
                                        "\tTransactionId=" + splitTxn.transactionId + "\n " +
                                        "\tSplitTransactionId=" + splitTxn.splitTransactionId + "\n " +
                                        "\tDesc=" + splitTxn.description + "\n " +
                                        "\tMem=" + splitTxn.memo + "\n " +
                                        "\tCategoryName=" + splitTxn.category.categoryName + "\n " +
                                        "\tAmount=" + splitTxn.yMoney.amount + "\n"
                                        );
                                    }
                                } else {
                                    System.Console.WriteLine("Could not fetching split transaction");
                                }
                            } catch (Exception e) {
                                System.Console.WriteLine("Error fetching split transaction" + e.Message);
                            }
                        }
                        System.Console.WriteLine("\n");
                    }

	        	    searchHit = searchHit - 10;
                    startRange = endRange + 1;
                    endRange = endRange + 10;
                }

				if (txSearchResult.transactions.Any())
				{
					Console.WriteLine("first trn date {1}, last trn date {0}",
									  txSearchResult.transactions.First().postDate ??
									  txSearchResult.transactions.First().transactionDate,
									  txSearchResult.transactions.Last().postDate ??
									  txSearchResult.transactions.Last().transactionDate);
				}
            } else {
                System.Console.WriteLine("ERROR: Unable to get user transaction\n");
            }
        }

        public void viewAllTransactions(UserContext userContext)
        {

            long startRange = 100;
            long endRange = 200;
            //long cateogoryId = 23;

            // Create Results Range
            TransactionSearchResultRange txSearchResultRange = new TransactionSearchResultRange();
            txSearchResultRange.startNumber = startRange;
            txSearchResultRange.startNumberSpecified = true;
            txSearchResultRange.endNumber = endRange;
            txSearchResultRange.endNumberSpecified = true;

            // Create  TransactionSearchFilter
            TransactionSearchFilter txSearchFilter = new TransactionSearchFilter();
            txSearchFilter.transactionSplitType = TransactionSplitType.ALL_TRANSACTION;
            //txSearchFilter.transactionCategory.categoryId = cateogoryId;

            // Create TransactionSearchRequest
            TransactionSearchRequest txSearchRequest = new TransactionSearchRequest();
            txSearchRequest.searchFilter = txSearchFilter;
            txSearchRequest.containerType = "all";
            txSearchRequest.ignoreUserInput = true;
            txSearchRequest.userInput = "";
            txSearchRequest.higherFetchLimit = 0;
            txSearchRequest.includeAggregatedTransactions = true;
            txSearchRequest.isSharedAccountTransactionReq = true;

            txSearchRequest.resultRange = txSearchResultRange;
            txSearchRequest.ignoreUserInput = true;            
            txSearchRequest.searchClients = TransactionSearchClients.DEFAULT_SERVICE_CLIENT;

            try
            {
                TransactionSearchExecInfo txSearchExecInfo =
                        transactionSearchService.executeUserSearchRequest(userContext, txSearchRequest);
                if (txSearchExecInfo != null)
                {
                    displayTransactionSearchExecInfo(txSearchExecInfo);
                }
                viewTransactions(userContext, txSearchExecInfo);
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Msg:" + e.Message);
                System.Console.WriteLine("Exc:"+e.StackTrace);
            }
        }

        public void searchTransactions(UserContext userContext){
            String searchString = IOUtils.promptInput(
                    searchStringPrompt,
                    ReSarchStringPrompt);

            System.Console.WriteLine("Searching for \"" + searchString + "\"...");

            long startRange = 1;
            long endRange = 10;

            // Create Results Range
            TransactionSearchResultRange txSearchResultRange = new TransactionSearchResultRange();
            txSearchResultRange.startNumber = startRange;
            txSearchResultRange.startNumberSpecified = true;
            txSearchResultRange.endNumber = endRange;
            txSearchResultRange.endNumberSpecified = true;

            // Create  TransactionSearchFilter
            TransactionSearchFilter txSearchFilter = new TransactionSearchFilter();
            /*
             *  splitType  - should be "P" or "C" or "A" or "PC"
             * where,
             *   P -   PARENT_TRANSACTION
             *   C -   SPLIT_TRANSACTION
             *   A -   ALL_TRANSACTION
             *   PC -  PARENT_SPLIT_TRANSACTION
             */
            txSearchFilter.transactionSplitType = TransactionSplitType.ALL_TRANSACTION;
            txSearchFilter.transactionSplitTypeSpecified = true;
            // Create TransactionSearchRequest
            TransactionSearchRequest txSearchRequest = new TransactionSearchRequest();
            txSearchRequest.searchFilter = txSearchFilter;
            txSearchRequest.containerType = "all";
            txSearchRequest.ignorePaymentTransactions = false;
            txSearchRequest.ignoreManualTransactions = true;
            txSearchRequest.includeAggregatedTransactions = true;
            txSearchRequest.isSharedAccountTransactionReq = true;
            txSearchRequest.resultRange = txSearchResultRange;
            //txSearchRequest.setIgnoreUserInput(true);
            txSearchRequest.ignoreUserInput =false;
            txSearchRequest.userInput = searchString;
            txSearchRequest.searchClients = TransactionSearchClients.DEFAULT_SERVICE_CLIENT;
            txSearchRequest.searchClientsSpecified = true;
            TransactionSearchExecInfo txSearchExecInfo =
                    transactionSearchService.executeUserSearchRequest(userContext, txSearchRequest);
            if ( txSearchExecInfo != null ) {
        	    displayTransactionSearchExecInfo(txSearchExecInfo);
            }

            viewTransactions(userContext, txSearchExecInfo);
        }

         /**
         * Display all item accounts
         *
         * @param userContext
         */
        public static void displayItemAccounts(UserContext userContext){

            DataServiceService dataService = new DataServiceService();
            dataService.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + "DataService";

            object[]  itemSummaries = dataService.getItemSummaries(userContext);
            if(itemSummaries != null){
                for (int i = 0; i < itemSummaries.Length; i++)
                {
                    ItemSummary itemSummary = (ItemSummary)itemSummaries[i];
                    String containerName = itemSummary.contentServiceInfo.containerInfo.containerName;

                    if(itemSummary.isCustom) {
                        System.Console.WriteLine( " (custom)");
                        //continue;
                    }

                    if(itemSummary != null){
                        if(itemSummary.itemData != null){
                            System.Console.WriteLine("from itemsummary" + itemSummary.itemData.accounts);
                            object[] accounts = itemSummary.itemData.accounts;
                            if(accounts == null || accounts.Length == 0){
                                // No Accounts
                            }else{                               
                                for (int j = 0; j < accounts.Length; j++)
                                {
                                    String actName = "";
                                    String nickname = null;
                                    long? itemAccountId=0;
                                    long itemId = itemSummary.itemId;
                                    if(containerName.Equals(ContainerTypes.BANK)){
                                        BankData bankData = (BankData)accounts[j];
                                        actName = getItemAccountName(bankData.accountName,
                                                bankData.accountNumber) ;
                                        nickname = bankData.nickName;
                                        if(bankData.itemAccountId != null) {
                                            itemAccountId = bankData.itemAccountId;
                                        }
                                    } else if(containerName.Equals(ContainerTypes.CREDIT_CARD)){
                                        CardData cardData = (CardData)accounts[j];
                                        actName = getItemAccountName(cardData.accountName,
                                                cardData.accountNumber) ;
                                        nickname = cardData.nickName;
                                        if(cardData.itemAccountId != null) {
                                            itemAccountId = cardData.itemAccountId;
                                        }
                                    } else if(containerName.Equals(ContainerTypes.INVESTMENT)){
                                        InvestmentData investmentData = (InvestmentData)accounts[j];
                                        actName = getItemAccountName(investmentData.accountName,
                                                investmentData.accountNumber) ;
                                        nickname = investmentData.nickName;
                                        if(investmentData.itemAccountId != null) {
                                            itemAccountId = investmentData.itemAccountId;
                                        }

                                    } else if(containerName.Equals(ContainerTypes.REWARD_PROGRAM)){
                                        RewardPgm rewardData = (RewardPgm)accounts[j];
                                        actName = getItemAccountName(null,
                                                rewardData.accountNumber) ;
                                        nickname = rewardData.nickName;
                                        if(rewardData.itemAccountId != null) {
                                            itemAccountId = rewardData.itemAccountId;
                                        }

                                    } else if(containerName.Equals(ContainerTypes.LOAN) || containerName.Equals(ContainerTypes.MORTGAGE)){
                                        LoanLoginAccountData loanLoginAccountData = (LoanLoginAccountData)accounts[j];
                                        if(loanLoginAccountData != null){
                                            object[] loans = loanLoginAccountData.loans;
                                            if(loans == null || loans.Length == 0 ){
                                                // System.out.println("\tNo Mortgage Accounts");
                                            }else{                                                
                                                for (int k = 0; k < loans.Length; k++) 
                                                {
                                                    Loan loan = (Loan)loans[k];
                                                    actName = getItemAccountName(loan.accountName,
                                                            loan.accountNumber) ;
                                                    nickname = loan.nickName;
                                                    if(loan.itemAccountId != null) {
                                                        itemAccountId = loan.itemAccountId;
                                                    }
                                                }
                                            }
                                        }
                                    } else if(containerName.Equals(ContainerTypes.INSURANCE)){
                                        InsuranceLoginAccountData insuranceLoginAccountData = (InsuranceLoginAccountData)accounts[j];
                                        if(insuranceLoginAccountData != null){
                                            object[] insurancePolicyList = insuranceLoginAccountData.insurancePolicys;
                                            if (null != insurancePolicyList){                                                
                                                for (int k = 0; k < insurancePolicyList.Length; k++)
                                                {
                                                    InsuranceData insuranceData = (InsuranceData)insurancePolicyList[k];
                                                    actName = getItemAccountName(insuranceData.accountName,
                                                            insuranceData.accountNumber) ;
                                                    nickname = insuranceData.nickName;
                                                    if(insuranceData.itemAccountId != null) {
                                                        itemAccountId = insuranceData.itemAccountId;
                                                    }

                                                }
                                            }

                                        }
                                    } else if(containerName.Equals(ContainerTypes.BILL) ||
                                            containerName.Equals(ContainerTypes.MINUTES) ||
                                            containerName.Equals(ContainerTypes.TELEPHONE)){
                                                System.Console.WriteLine("Account name:" + accounts[j].GetType());
                                                Object obj = (Object)accounts[j];
                                                System.Console.WriteLine("Object::" + obj);
                                                //BillsData billsData = (BillsData)accounts[j];
                                                BillsData billsData = (BillsData)obj;
                                        actName = getItemAccountName(billsData.accountName,
                                                billsData.accountNumber) ;
                                        nickname = billsData.nickName;
                                        if(billsData.itemAccountId != null) {
                                            itemAccountId = billsData.itemAccountId;
                                        }
                                    } else if(containerName.Equals(ContainerTypes.BILL_PAY_SERVICE)){
                                        BillPayServiceData billPayData = (BillPayServiceData)accounts[j];
                                        actName = getItemAccountName(billPayData.accountName,
                                                billPayData.accountNumber) ;
                                        nickname = billPayData.nickName;
                                        if(billPayData.itemAccountId != null) {
                                            itemAccountId = billPayData.itemAccountId;
                                        }
                                    } else if(containerName.Equals(ContainerTypes.MAIL)){
                                        MailData mailData = (MailData)accounts[j];
                                        actName = "";
                                        nickname = mailData.customName;
                                        // Mail has no ItemAccountId, using ItemId
                                        itemAccountId=itemSummary.itemId;
                                    } else {                                        
                                        continue;
                                    }

                                    // Get Display Name
                                    String displayName = getItemAccountDisplayName(
                                            actName,
                                            itemSummary.contentServiceInfo.contentServiceDisplayName,
                                            nickname) ;

                                    System.Console.WriteLine(displayName + " itemId=" + itemId
                                		    + " itemAccountId=" + itemAccountId);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static String getItemAccountName(String actName, String acountNumber)
        {
            if (actName == null)
            {
                actName = acountNumber;
            }
            if (actName != null)
            {
                actName = " - " + actName;
            }
            else
            {
                actName = "";
            }
            return actName;
        }

        public static String getItemAccountDisplayName(String actName,
                                                  String siteDisplayName,
                                                  String nickName
        )
        {
            String displayName = siteDisplayName + "" + actName;

            if (nickName != null)
            {
                displayName = nickName;
            }

            return displayName;
        }

        public void categorizeTransaction(UserContext userContext)
        {
            Hashtable subCategoryMap = new Hashtable();

            // Prompt for the TransactionId of transaction to be categorized.
            //String transactionId = IOUtils.promptInput(
            //        TransactionIdPrompt,
            //        ReItemAcountIdPrompt);

            //// Prompt for the container of the transaction to be categorized.
            //String containerType = IOUtils.promptInput(
            //        ContainerTypePrompt,
            //        ReContainerTypePrompt);

            // Get the list of supported transaction categories to display
            Category1[] category = tcService.getUserTransactionCategories(userContext);

            var xsSubmit = new XmlSerializer(typeof(Category1[]));
            var sww = new StringWriter();
            XmlWriter writer = XmlWriter.Create(sww);
            xsSubmit.Serialize(writer, category);
            var xml = sww.ToString(); // Your xml

            using (var fileWriter = new StreamWriter("categories.xml", true))
            {
                fileWriter.WriteLine(xml);
            }
            //if(category != null)
            //{
            //    Category1[] childCategory;
            //    System.Console.WriteLine("\n");
            //    System.Console.WriteLine("category\t\t\tcategoryId");
            //    System.Console.WriteLine("--------\t\t\t----------");
            //    String categoryName;                
            //    for ( int i = 0; i < category.Length; ++i) {
            //        childCategory = category[i].childCategory;

            //        categoryName = category[i].categoryName != null ? category[i].categoryName : category[i].localizedCategoryName;
            //        System.Console.Write(categoryName);
            //        for (int j = categoryName.Length; j < 32; j++)
            //        {
            //            System.Console.Write(".");
            //        }
            //        System.Console.Write("   " + category[i].categoryId);
            //        System.Console.Write("\n");
            //        childCategory = category[i].childCategory;
            //        if (childCategory != null) {
            //            for (int c = 0; c < childCategory.Length; c++) {
            //                Category1 child = childCategory[c];
            //                System.Console.Write(" +" + child.categoryName);
            //                for (int j = child.categoryName.Length; j < 30; j++)
            //                {
            //                    System.Console.Write(".");
            //                }
            //                subCategoryMap.Add(child.categoryId, child.categoryName);
            //                System.Console.Write("   " + child.categoryId);
            //                System.Console.Write("\n");
            //            }
            //        }
            //    }
            //    System.Console.Write("\n");
            //}

            //// Prompt for the categoryId.
            //String categoryId = IOUtils.promptInput(
            //        CategoryIdPrompt,
            //        ReCategoryIdPrompt);            

            //System.Console.WriteLine("\n");

            //UserCategorizationObject [] userCategorizationObjects = new UserCategorizationObject[1];

            //UserCategorizationObject ucObject = new UserCategorizationObject();

            //ucObject.containerTransactionId = transactionId;
            //ucObject.targetTransactionCategoryId = categoryId;
            //ucObject.container = containerType;
            //if (subCategoryMap[long.Parse(categoryId)] != null)            
            //    ucObject.categoryLevelId = SUB_CATEGORY;
            //userCategorizationObjects[0] = ucObject;

            //// Call TransactionCategorizationService to categorize the transaction
            //try
            //{
            //    tcService.categorizeTransactions(userContext, userCategorizationObjects);
            //    System.Console.WriteLine("CategoryId updated for transactionId=" + transactionId + ".\n");
            //}
            //catch (SoapException soapEx)
            //{
            //    System.Console.WriteLine(soapEx.StackTrace);
            //    /*CoreExceptionFault coreEx = ExceptionHandler.processException(soapEx);
            //    if (coreEx != null)
            //    {
            //        //System.Console.WriteLine(coreEx.message);
            //        //System.Console.WriteLine(coreEx.trace);
            //        if (coreEx is InvalidUserCategorizationObjectException)
            //        {
            //            System.Console.WriteLine("\nInvalidUserCategorizationObjectException -- One of these arguments in invalid: " +
            //            "\n\tTransactionID=" + transactionId +
            //            "\n\tContainerType=" + containerType +
            //            "\n\tCategoryId=" + categoryId + "\n");
            //        }
            //        return;
            //    }*/
            //}            
        }

        /**
         * This methid adds or deletes a split transaction to  
         * an existing transaction identified by transactionId
         */ 
        public void manageSplitTransaction(UserContext userContext, long splittransactionoperation)
        {
            System.Console.WriteLine("\nEnter TransactionId:");
            long transactionId = IOUtils.readLong();
            TransactionSearchExecInfo txSearchExecInfo = 
                fetchTransactionDataForAGivenTransactionId(userContext, transactionId);            
            if (txSearchExecInfo != null)
            {
                TransactionSearchResult txnSearchResult = txSearchExecInfo.searchResult;
                TransactionView[] txnView = txnSearchResult.transactions;
                if (splittransactionoperation == SPLIT_TXN_OP_ADD)
                {
                    //CODE TO ADD SPLIT TRANSACTION                    
                    String originalTxnAmtStr = Formatter.formatMoney(txnView[0].amount);
                    double? originalTxnAmt = txnView[0].amount.amount;
                    System.Console.WriteLine("Txn amount:" + originalTxnAmtStr);
                    System.Console.WriteLine("Enter split transaction amount:");
                    String splitTxnAmtStr = IOUtils.readStr();
                    double splitTxnAmt;
                    if (splitTxnAmtStr != null && !splitTxnAmtStr.Trim().Equals(""))
                    {
                        splitTxnAmt = Double.Parse(splitTxnAmtStr);
                    }
                    else
                    {
                        System.Console.WriteLine("Invalid input");
                        return;
                    }
                    if (splitTxnAmt >= originalTxnAmt)
                    {
                        System.Console.WriteLine("Split transaction amount cannot be greater than the original transaction amount");
                        return;
                    }
                    SplitTransaction splitTxn = new SplitTransaction();

                    //set split txn amount in YMoney
                    YMoney splitTxnMoney = new YMoney();
                    splitTxnMoney.amount = splitTxnAmt;
                    splitTxnMoney.amountSpecified = true;
                    splitTxnMoney.currencyCode = txnView[0].amount.currencyCode;
                    splitTxn.yMoney = splitTxnMoney;

                    //set txn desc
                    System.Console.WriteLine("Enter description for split txn");
                    String splitTxnDesc = IOUtils.readStr();
                    splitTxn.description = splitTxnDesc;

                    //set category
                    //Category category = new Category();

                    Category1 category = new Category1();

                    //category.categoryId = txnView[0].category.categoryId; //25;
                    category.categoryId = txnView[0].category.categoryId; //25;
                    //TransactionCategory tc = (TransactionCategory)txnView[0].category.categoryName;
                    TransactionCategory tc = (TransactionCategory)txnView[0].category;
                    //category.categoryId = tc.Value;
                    category.categoryId=(long)tc.categoryId;
                    category.categoryIdSpecified = true;
                    if (txnView[0].category.categoryLevelId == 0 || txnView[0].category.categoryLevelId == null)
                    {

                       category.categoryLevelId = CATEGORY;
                       category.categoryLevelIdSpecified = true;

                    }
                    else
                    {

                        category.categoryLevelId = txnView[0].category.categoryLevelId;
                        category.categoryLevelIdSpecified = true;
                    }
                    if (txnView[0].category.parentTransactionCategory != null)
                    {

                        category.parentCategoryId = txnView[0].category.parentTransactionCategory.categoryId;
                        category.parentCategoryIdSpecified = true;
                    }
                    else
                    {

                        category.parentCategoryId = 25;
                        category.parentCategoryIdSpecified = true;

                    }

                    splitTxn.category = category;

                    //set txn_id
                    splitTxn.transactionId = txnView[0].viewKey.transactionId;
                    splitTxn.transactionIdSpecified = true;

                    //set itemDataTableId
                    String containerType = txnView[0].viewKey.containerType;
                    splitTxn.itemDataTableId = fetchItemDataTableId(containerType);
                    splitTxn.itemDataTableIdSpecified = true;
                    //set TransactionClass
                    TransactionClass txnClass = new TransactionClass();
                    txnClass.isBusinessExpense = 1;
                    txnClass.isMedicalExpense = 0;
                    txnClass.isTaxDeductible = 1;
                    txnClass.isReimbursable = 1;
                    splitTxn.transactionClass = txnClass;

                    splitTxn.isSystemGeneratedSplit = txnView[0].viewKey.isSystemGeneratedSplit;

                    //set memo
                    System.Console.WriteLine("Eneter memo:");
                    String memo = IOUtils.readStr();
                    splitTxn.memo = memo;

                    SplitTransactionWithOperation splitTxnWithOperation = new SplitTransactionWithOperation();
                    splitTxnWithOperation.splitTransaction = splitTxn;

                    splitTxnWithOperation.splitTransactionOperation = SplitTransactionOperation.ADD_SPLIT_TRANSACTON;
                    splitTxnWithOperation.splitTransactionOperationSpecified = true;

                    SplitTransactionWithOperation[] splitTxnWithOperations = new SplitTransactionWithOperation[] { splitTxnWithOperation };
                    try
                    {
                        transactionManagementService.manageSplitTransactions(userContext, splitTxnWithOperations);
                        System.Console.WriteLine("\n**Transaction split successfully\n");
                    }
                    catch (SoapException soapEx)
                    {
                        System.Console.WriteLine("Exception:" + soapEx.Message);
                    }
                }
                else
                {
                    //CODE TO DELETE SPLIT TRANSACTION
                    if (txnView[0].viewKey.splitType != null) {
                        TransactionKeyData txnKeyData = new TransactionKeyData();
                        txnKeyData.transactionId = txnView[0].viewKey.transactionId;
                        txnKeyData.transactionIdSpecified = true;
                        txnKeyData.containerType = txnView[0].viewKey.containerType;
                        txnKeyData.itemDataTableId = fetchItemDataTableId(txnView[0].viewKey.containerType);
                        txnKeyData.itemDataTableIdSpecified = true;
                        try {
                            transactionManagementService.deleteAllSplitTransactions(userContext, txnKeyData);
                            System.Console.WriteLine("\n**All Split transactions for the TransactionId " + transactionId  + " have been deleted successfully\n");
                        } catch (SoapException soapEx) {
                            System.Console.WriteLine("Exception:" + soapEx.Message);
                        }
                    } else  {
                        System.Console.WriteLine("\n**The transactionId " + transactionId + " has no split transactions\n");
                    }
                }
            } else {
                System.Console.WriteLine("\n**No transactions available\n");
            }            
        }

        /**
         * returns ItemDataTableId based on containerType
         */ 
        private static long fetchItemDataTableId(String containerType) 
        {
            if (containerType.Equals(ContainerTypes.BANK, StringComparison.OrdinalIgnoreCase)) {
                return BANK_TRANSACTION;
            } else if (containerType.Equals(ContainerTypes.CREDIT_CARD, StringComparison.OrdinalIgnoreCase))  {
                return CARD_TRANSACTION;
            } else if (containerType.Equals(ContainerTypes.LOAN, StringComparison.OrdinalIgnoreCase)) {
                return LOAN_TRANSACTION;
            } else if (containerType.Equals(ContainerTypes.INVESTMENT, StringComparison.OrdinalIgnoreCase)) {
                return INVESTMENT_TRANSACTION;
            }
            return 0;
        }

        public void displaySubCategory(UserContext userContext)
        {
            System.Console.WriteLine("\nFetching all transaction categories....");
            // Get the list of supported transaction categories to display
            Category1[] category = tcService.getUserCategoriesAtLevel(userContext, 4, true);
            if (category != null)
            {
                for (int i = 0; i < category.Length; ++i)
                {
                    System.Console.WriteLine(category[i].categoryName + " [" + category[i].categoryId + "]");
                    if (category[i].childCategory != null && category[i].childCategory.Length > 0)
                    {
                        Category1[] childCat = category[i].childCategory;
                        for (int c = 0; c < childCat.Length; ++c)
                        {
                            System.Console.WriteLine("\t" + childCat[c].categoryName + " [" + childCat[c].categoryId + "] " + childCat[c].categoryLevelId);
                        }

                    }
                }
            }
        }
        /**
         * This method adds a sub-category to an existing system defined category
         * Note: The newly added sub-category takes a while to be reflected in the application
         */ 
        public void addSubCategory(UserContext userContext)
        {
            System.Console.WriteLine("\nFetching all transaction categories....");
            // Get the list of supported transaction categories to display
            Category1[] category = tcService.getSupportedTransactionCategrories(getCobrandContext());
            if (category != null) {
                for (int i = 0; i < category.Length; ++i) {
                    System.Console.WriteLine(category[i].categoryName + " [" + category[i].categoryId + "]");
                    if (category[i].childCategory != null && category[i].childCategory.Length>0)
                    {
                        Category1[] childCat = category[i].childCategory;
                        for (int c = 0; c < childCat.Length; ++c)
                        {
                            System.Console.WriteLine("\t" + childCat[c].categoryName + " [" + childCat[c].categoryId + "] "+childCat[c].categoryLevelId);
                        }

                    }
                }

                //set parentCategoryId
                System.Console.WriteLine("\nEnter categoryId to which sub-category is to be added");
                long parentCategoryId = IOUtils.readLong();

                Category1 subCategory = new Category1();
                subCategory.parentCategoryId = parentCategoryId;
                subCategory.parentCategoryIdSpecified = true;

                //set category name
                System.Console.WriteLine("\nEnter sub-category name");
                String subCategoryName = IOUtils.readStr();
                subCategory.categoryName = subCategoryName;

                //set category level
                subCategory.categoryLevelId = SUB_CATEGORY;
                subCategory.categoryLevelIdSpecified = true;

                subCategory.categoryDescription = "TEST";

                Category1[] subCategoryToBeAdded = { subCategory };

                try
                {
                    tcService.manageUserCategories(userContext, subCategoryToBeAdded);
                    System.Console.WriteLine("\n**Sub-Category successfully added (This change could take a while to be reflected in the application)\n");
                } catch (SoapException soapExc) {
                    System.Console.WriteLine("Exception::" + soapExc.Message);
                    System.Console.WriteLine("Error adding subcategory:" + soapExc.StackTrace);

                    /*
                     CoreException coreEx = ExceptionHandler.processException(soapExc);

                    if (coreEx != null) {
                        if (coreEx is CategoryLevelNotSupportedException) {
                            System.Console.WriteLine("Exception: Category Level Not Supported");
                        } else if (coreEx is CategoryAlreadyExistsException) {
                            System.Console.WriteLine("Exception: Category Already Exists");
                        } if (coreEx is InvalidCategoryException) {
                            System.Console.WriteLine("Exception: Invalid Category");
                        } else if (coreEx is CreateCategoryLimitException) {
                            System.Console.WriteLine("Exception: Create Category Limit");
                        } else
                            System.Console.WriteLine("Exception:" + coreEx.message);
                    }*/

                }
            } else {
                System.Console.WriteLine("Error fetching transaction categories");
            }
        }

        /**
         * This method deletes a user added sub-category
         * Note: The deleted sub-category takes a while to be reflected in the application
         */
        public void deleteSubCategories(UserContext userContext)
        {            
            System.Console.WriteLine("\nFetching all transaction categories....");
            // Get the list of supported transaction categories to display
            Category1[] category = tcService.getUserCategoriesAtLevel(userContext, SUB_CATEGORY, true);
            if (category != null)
            {
                Hashtable subCategoryMap = new Hashtable();
                for (int i = 0; i < category.Length; ++i)
                {                    
                    System.Console.WriteLine(category[i].categoryName + " [" + category[i].categoryId + "]");                    
                    subCategoryMap.Add(category[i].categoryId, category[i]);                    
                }
                System.Console.WriteLine("\nEnter Sub-CategoryId which is to be deleted");
                long categoryId = IOUtils.readLong();

                Category1 subCategory =
                    (Category1)subCategoryMap[categoryId];
                if (subCategory == null)
                {
                    System.Console.WriteLine("\n**Cannot delete sub-category\n");
                }
                else
                {                    
                    Category1 CategoryToBeEdited = new 
                        Category1();
                    CategoryToBeEdited.categoryId = subCategory.categoryId;
                    CategoryToBeEdited.categoryIdSpecified = true;
                    CategoryToBeEdited.categoryName = null;
                    CategoryToBeEdited.categoryLevelId = SUB_CATEGORY;
                    CategoryToBeEdited.categoryLevelIdSpecified = true;
                    CategoryToBeEdited.isDeleted = 1;
                    Category1[] categoriesToBeEdited = { CategoryToBeEdited };
                    try
                    {
                        tcService.manageUserCategories(userContext, categoriesToBeEdited);
                        System.Console.WriteLine("\n**The Sub-Category has been successfully deleted (This change could take a while to be reflected in the application)\n");
                    }
                    catch (SoapException soapExc)
                    {
                        System.Console.WriteLine("\nError deleting sub-category:" + soapExc.StackTrace);
                        /*CoreException coreEx = ExceptionHandler.processException(soapExc);
                        if (coreEx != null)
                        {
                            if (coreEx is CategoryLevelNotSupportedException) {
                                System.Console.WriteLine("\nException: Category Level Not Supported\n");
                            } else if (coreEx is CategoryAlreadyExistsException) {
                                System.Console.WriteLine("\nException: Category Already Exists\n");
                            } if (coreEx is InvalidCategoryException) {
                                System.Console.WriteLine("\nException: Invalid Category\n");
                            } else if (coreEx is CreateCategoryLimitException) {
                                System.Console.WriteLine("\nException: Create Category Limit\n");
                            } else
                                System.Console.WriteLine("\nException:" + coreEx.message+" \n");
                        }*/
                    }
                }
            }
            else
            {
                System.Console.WriteLine("\nError fetching transaction categories\n");
            }
        }

        /**
         * This method sets the description in 'userDescription; field for a 
         * given transaction identified by the TransactionId. It also updates
         * the field 'descriptionViewPref' to 'true'. Based on these two fileds
         * the user entered description could be displayed. 
         * Note: The field description will always have the scraped value        
         */
        public void editTransactionDescription(UserContext userContext)
        {
            System.Console.WriteLine("\nEnter TransactionId:");
            long transactionId = IOUtils.readLong();
            TransactionSearchExecInfo txSearchExecInfo = 
                fetchTransactionDataForAGivenTransactionId(userContext, transactionId);
            if (txSearchExecInfo != null)
            {
                TransactionSearchResult txnSearchResult = txSearchExecInfo.searchResult;
                TransactionView[] txnView = txnSearchResult.transactions;
                TransactionView view = txnView[0];

                //create transactionData object
                TransactionData transactionData = new TransactionData();

                TransactionAmountData transactionAmountData = new TransactionAmountData();
                transactionAmountData.amount = view.amount;

                TransactionDatesData transactionDatesData = new TransactionDatesData();
                transactionDatesData.postDate = view.postDate;
                transactionDatesData.postDateSpecified = true;
                transactionDatesData.transDate = view.transactionDate;
                transactionDatesData.transDateSpecified = true;

                TransactionTypeData transactionTypeData = new TransactionTypeData();
                transactionTypeData.transactionTypeId = view.transactionTypeId;
                transactionTypeData.transactionTypeIdSpecified = true;
                transactionTypeData.transactionType = view.transactionType;

                TransactionCategoryData transactionCategoryData = new TransactionCategoryData();
                /**DEBUG BEGIN*/
                /*transactionCategoryData.categoryId = view.category.categoryId;
                transactionCategoryData.categoryName = view.category.categoryName;*/
                /**DEBUG END*/

                System.Console.WriteLine("Enter Description for the transaction:");
                String description = IOUtils.readStr();
                UserInputData userInputData = new UserInputData();                                
                userInputData.userDescription = view.description.userDescription;
                userInputData.memo = view.memo.memo;
                userInputData.categorizationKeyword = view.categorizationKeyword;
                userInputData.isTaxable = view.isTaxable;
                userInputData.isReimbursable = view.isReimbursable;
                userInputData.isBusiness = view.isBusiness;
                userInputData.isMedical = view.isMedical;
                userInputData.userDescription = description;
                userInputData.descriptionViewPref = true;

                TransactionKeyData transactionKeyData = new TransactionKeyData();
                transactionKeyData.containerType = view.viewKey.containerType;
                transactionKeyData.itemDataTableId = fetchItemDataTableId(view.viewKey.containerType);
                transactionKeyData.itemDataTableIdSpecified = true;
                transactionKeyData.splitType = view.viewKey.splitType;
                transactionKeyData.transactionId = view.viewKey.transactionId;
                transactionKeyData.transactionIdSpecified = true;
                transactionKeyData.itemAccountId = view.account.itemAccountId;
                transactionKeyData.itemAccountIdSpecified = true;
                transactionData.transactionAmountData = transactionAmountData;
                transactionData.transactionDatesData = transactionDatesData;
                transactionData.transactionTypeData = transactionTypeData;
                transactionData.transactionCategoryData = transactionCategoryData;
                transactionData.userInputData = userInputData;
                transactionData.transactionKeyData = transactionKeyData;

                //create updateCriteria object
                UpdateCriteria updateCriteria = new UpdateCriteria();
                updateCriteria.updateCategorizationData = false;
                updateCriteria.updateUserData = true;

                try
                {
                    transactionManagementService.updateTransactionData(userContext, transactionData, updateCriteria);
                    System.Console.WriteLine("\n**The transaction data has been successfully updated\n");
                } catch (Exception e) {
                    System.Console.WriteLine("The transaction data could not be updated\n" + e.Message + "\n");
                }
            }
            else
            {
                System.Console.WriteLine("\nNo transaction available for the transactionId [" + transactionId + "]\n");
            }
        }

        /**
         * This methid returns txn data (TransactionSearchExecInfo) for a given TransactionId
         */
        private TransactionSearchExecInfo fetchTransactionDataForAGivenTransactionId(UserContext userContext, long transactionId)
        {
            TransactionSearchExecInfo txSearchExecInfo = null;
            TransactionSearchResultRange txSearchResultRange = new TransactionSearchResultRange();
            txSearchResultRange.startNumber = 1;
            txSearchResultRange.startNumberSpecified = true;
            txSearchResultRange.endNumber = 2;
            txSearchResultRange.endNumberSpecified = true;

            TransactionSearchFilter txSearchFilter = new TransactionSearchFilter();
            txSearchFilter.transactionId = new long?[] { transactionId };
            //txSearchFilter.transactionCategory.categoryId = 22;
            txSearchFilter.transactionSplitType = TransactionSplitType.ALL_TRANSACTION;
            txSearchFilter.transactionSplitTypeSpecified = true;
            // Create TransactionSearchRequest
            TransactionSearchRequest txSearchRequest = new TransactionSearchRequest();
            txSearchRequest.searchFilter = txSearchFilter;
            txSearchRequest.resultRange = txSearchResultRange;
            txSearchRequest.containerType = "all";
            txSearchRequest.ignorePaymentTransactions = false;
            txSearchRequest.ignoreUserInput = true;
            txSearchRequest.userInput = "";
            txSearchRequest.ignoreManualTransactions = true;
            txSearchRequest.includeAggregatedTransactions = true;
            txSearchRequest.isSharedAccountTransactionReq = true;
            txSearchRequest.higherFetchLimit = 0;
            txSearchRequest.higherFetchLimitSpecified = true;

            TransactionSearchClients txSearchClients = TransactionSearchClients.DEFAULT_SERVICE_CLIENT;            
            txSearchRequest.searchClients = txSearchClients;
            txSearchRequest.searchClientsSpecified = true;

            try
            {
                txSearchExecInfo =
                        transactionSearchService.executeUserSearchRequest(userContext, txSearchRequest);
            }
            catch (Exception e) 
            {
                Console.WriteLine(e.Message); 
            }
            return txSearchExecInfo;
        }

        //displays all manual transactions added for an account
        public void viewManualTransactions(UserContext userContext) 
        {
            System.Console.WriteLine("\nFetching user accounts...");
            //bool isAcctsPresent = displayItemAccountsForManualTxn(userContext);
            //if (!isAcctsPresent)
            //{
             //   System.Console.WriteLine("\nNo active accounts available for this user\n");
            //}
            //else
            //{
                System.Console.WriteLine("\nEnter an ItemAccountId:");
                long itemAccountId = IOUtils.readLong();

                DateTime startRange = new DateTime(2005, 01, 01);
                DateTime endRange = DateTime.Now;

                ManualTransactionServiceService manualTxnService = new ManualTransactionServiceService();
                manualTxnService.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + "ManualTransactionService";
                ManualTransactionData[] manualTxns = null;
                try
                {
                    manualTxns = manualTxnService.getManualTransactions1(userContext, itemAccountId, true, startRange, true, endRange, true);
                } catch (Exception e) {
                    System.Console.WriteLine("\nError fetching manual txn:" + e.Message);
                    return;
                }
                if (manualTxns != null && manualTxns.Length > 0)
                {
                    System.Console.WriteLine("\n****Transaction Data****\n");
                    for (int i = 0; i < manualTxns.Length; i++)
                    {
                        ManualTransactionData txnData = manualTxns[i];
                        System.Console.WriteLine(
                            "Desc:" + txnData.description + "\n"
                            + "ItemAccountId:" + txnData.itemAccountId + "\n"
                            + "TxnId:" + txnData.manualTransactionId + "\n"
                            + "PostDate:" + txnData.postDate + "\n"
                            + "Amount:" + txnData.transactionAmount + "\n"
                            + "TxnType:" + txnData.transactionBaseType + "\n"
                            + "TxnDate:" + txnData.transDate + "\n"
                            );
                    }
                }
                else
                {
                    System.Console.WriteLine("\nNo manual transactions available for the user\n");
                }
            //}
        }

        public void addManualTransaction(UserContext userContext) 
        {
            System.Console.WriteLine("\nFetching user accounts...");
            /*bool isAcctsPresent = displayItemAccountsForManualTxn(userContext);            
            if (!isAcctsPresent)
            {
                System.Console.WriteLine("\nNo active accounts available for this user\n");
            }
            else
            {*/
                System.Console.WriteLine("\nEnter an ItemAccountId:");
                long itemAccountId = IOUtils.readLong();

                System.Console.WriteLine("\nEnter Container for ItemAccountId " + itemAccountId + ":");
                String containerType = IOUtils.readStr();

                ManualTransactionData manualTxnData = new ManualTransactionData();
                manualTxnData.itemAccountId = itemAccountId;
                manualTxnData.itemAccountIdSpecified = true;
                manualTxnData.containerType = containerType;
                manualTxnData.categorizationKeyword = "Test";
                manualTxnData.currency = Currency.USD;

                System.Console.WriteLine("\nEnter transaction description:");
                String txnDesc = IOUtils.readStr();
                manualTxnData.description = txnDesc;

                manualTxnData.frequencyType ="One_time";
                manualTxnData.memo = "manual txn";

                System.Console.WriteLine("\nEnter transaction amount:");
                String txnAmt = IOUtils.readStr();
                manualTxnData.transactionAmount = Double.Parse(txnAmt);
                manualTxnData.transactionAmountSpecified = true;
                manualTxnData.transactionBaseType = "credit";
                manualTxnData.transactionCategoryId = 25;
                manualTxnData.transactionCategoryIdSpecified = true;
                manualTxnData.categoryLevelId = CATEGORY;
                manualTxnData.categoryLevelIdSpecified = true;
                manualTxnData.transDate = DateTime.Now;
                manualTxnData.transDateSpecified = true;

                ManualTransactionServiceService manualTxnService = new ManualTransactionServiceService();
                manualTxnService.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + "ManualTransactionService";
                try
                {
                    manualTxnService.createManualTransactionRequest(userContext, manualTxnData);
                    System.Console.WriteLine("\nManual Transaction successfully created\n");                   
                }
                catch (Exception e)
                {
                    System.Console.WriteLine("\nError createing manual txn:" + e.Message);
                }
          //  }            
        }

        //displays all ItemAccounts for this user
        private bool displayItemAccountsForManualTxn(UserContext userContext)
        {
            bool isAcctsPresent = false;            
            DataServiceService dataService = new DataServiceService();
            dataService.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + "DataService";
            object[] itemSummaries = dataService.getItemSummaries(userContext);
            if (itemSummaries != null)
            {
                for (int i = 0; i < itemSummaries.Length; i++)
                {
                    ItemSummary itemSummary = (ItemSummary)itemSummaries[i];
                    if (itemSummary.itemData != null)
                    {
                        Object[] accts = itemSummary.itemData.accounts;
                        if (accts != null || accts.Length == 0)
                        {
                            for (int j = 0; j < accts.Length; j++)
                            {
                                BaseTagData baseData = (BaseTagData)accts[j];
                                isAcctsPresent = true;
                                Entry[] entry = baseData.accountDisplayName.accountNames;
                                System.Console.WriteLine("ItemAccountId:" + baseData.itemAccountId);
                                for (int e = 0; e < entry.Length; e++)
                                {
                                    if (entry[e].key.ToString() == "ACCNAME")
                                    {
                                        System.Console.Write(" AcctName:" + entry[e].value);
                                    }
                                }
                                System.Console.Write(" Container:" + itemSummary.contentServiceInfo.containerInfo.containerName);
                            }
                        }
                    }
                }
            }
            return isAcctsPresent;
        }
    }
}

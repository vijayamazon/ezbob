using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.Yodlee
{
    public class YodleeCommands
    {
        public const string GET_FASTLINK_TOKEN = "authenticator/token";
        public const string USER_REGISTRATION = "jsonsdk/UserRegistration/register3";
        
        public const string COB_LOGIN = "authenticate/coblogin";
        public const string USER_LOGIN = "authenticate/login";

        public const string SEARCH_SITE = "jsonsdk/SiteTraversal/searchSite";
        public const string GET_CONTENT_SERVICE_INFO1 = "jsonsdk/ContentServiceTraversal/getContentServiceInfo1";

        public const string GET_SITE_INFO = "jsonsdk/SiteTraversal/getSiteInfo";
        public const string SITE_LOGIN_FORM = "jsonsdk/SiteAccountManagement/getSiteLoginForm";
        public const string ADD_SITE_ACCOUNT1 = "jsonsdk/SiteAccountManagement/addSiteAccount1";
        public const string GET_SITE_REFRESH_INFO = "jsonsdk/Refresh/getSiteRefreshInfo";

        public const string GET_ITEM_SUMMARIES_FOR_SITE = "jsonsdk/DataService/getItemSummariesForSite";
        public const string GET_MFA_RESPONSE_FOR_SITE = "jsonsdk/Refresh/getMFAResponseForSite";
        public const string PUT_MFA_REQUEST_FOR_SITE = "jsonsdk/Refresh/putMFARequestForSite";
        public const string GET_SITE_ACCOUNTS = "jsonsdk/SiteAccountManagement/getSiteAccounts";

        public const string GET_LOGIN_FORM_CONTENT_SERVICE = "jsonsdk/ItemManagement/getLoginFormForContentService";
        public const string SEARCH_CONTENT_SERVICES = "jsonsdk/Search/searchContentServices";
        public const string ADD_ITEM_FORM_CONTENT_SERVICE1 = "jsonsdk/ItemManagement/addItemForContentService1";
        public const string GET_REFRESH_INFO1 = "jsonsdk/Refresh/getRefreshInfo1";
        public const string GET_ITEM_SUMMARIES_FOR_ITEM1 = "jsonsdk/DataService/getItemSummaryForItem1";
        public const string GET_MFA_RESPONSE = "jsonsdk/Refresh/getMFAResponse";
        public const string START_REFRESH7 = "jsonsdk/Refresh/startRefresh7";
        public const string IS_ITEM_REFRESHING = "jsonsdk/Refresh/isItemRefreshing";
        public const string PUT_MFA_REQUEST = "jsonsdk/Refresh/putMFARequest";

        public const string GET_ITEM_SUMMARIES_WITHOUT_ITEM_DATA = "jsonsdk/DataService/getItemSummariesWithoutItemData";
        public const string REMOVE_ITEM = "jsonsdk/ItemManagement/removeItem";

        public const string START_SITE_REFRESH = "jsonsdk/Refresh/startSiteRefresh";
        public const string UPDATE_SITE_ACCOUNT_CREDENTIALS = "jsonsdk/SiteAccountManagement/updateSiteAccountCredentials";
        public const string UPDATE_CREDENTNIALS_FORM_ITEM1 = "jsonsdk/ItemManagement/updateCredentialsForItem1";

        public const string EXECUTE_USER_SEARCH_REQUEST = "jsonsdk/TransactionSearchService/executeUserSearchRequest";
        public const string GET_USER_TRANSACTIONS = "jsonsdk/TransactionSearchService/getUserTransactions";

        public const string REMOVE_SITE_ACCOUNT = "jsonsdk/SiteAccountManagement/removeSiteAccount";
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dPartiesTests {
    using System.Security.AccessControl;
    using System.Security.Cryptography.X509Certificates;
    using EzBob3dParties.Yodlee;
    using EzBob3dParties.Yodlee.Models;
    using EzBob3dParties.Yodlee.Models.Login;
    using EzBob3dParties.Yodlee.Models.SiteAccount;
    using EzBob3dParties.Yodlee.Models.Transactions;
    using EzBob3dParties.Yodlee.RequestResponse;
    using EzBobModels.Yodlee;
    using NUnit.Framework;
    using StructureMap;

    [TestFixture]
    public class YodleeTests : TestBase {
        private static readonly string CoBrandUserName = "sbCobromanp";
        private static readonly string coBrandPassworkd = "01267bdc-7685-477d-a012-b97d3cfb697e";
        private static readonly string UserName = "sbMemromanp5";
        private static readonly string UserPassword = "sbMemromanp5#123";

        //This test can be used only limited amount of time
        //because it uses account that expires after 30 days
        //after each re-registration the credentials should by updated
        //and this test may be temporarily enabled
        [Ignore]
        [Test]
        public async void TestTransactionRequest() {
            IContainer container = InitContainer(typeof(YodleeService));

            YodleeService yodleeService = container.GetInstance<YodleeService>();

            YCobrandLoginRequest cobrandLoginRequest = new YCobrandLoginRequest().SetCobrandUserName(CoBrandUserName)
                .SetCobrandPassword(coBrandPassworkd);

            YCobrandLoginResponse yCobrandLoginResponse = await yodleeService.LoginCobrand(cobrandLoginRequest);

            YUserLoginRequest userLoginRequest = new YUserLoginRequest()
                .SetCobrandSessionToken(yCobrandLoginResponse.cobrandConversationCredentials.sessionToken)
                .SetUserName(UserName)
                .SetUserPassword(UserPassword);

            YUserLoginResponse yUserLoginResponse = await yodleeService.LoginUser(userLoginRequest);

            YTransactionsSearchRequest transactionsSearchRequest = new YTransactionsSearchRequest()
                .SetCoBrandToken(yCobrandLoginResponse.cobrandConversationCredentials.sessionToken)
                .SetUserSessionToken(yUserLoginResponse.userContext.conversationCredentials.sessionToken)
                .SetContainerType(EContainerType.bank)
                .SetPageSize(1, 10);

            YTransactionsSearchResponse t = await yodleeService.GetTransactions2(transactionsSearchRequest);
            YTransactionsSearchResponse tt = yodleeService.GetTransactions3(transactionsSearchRequest);

            YTransactionsSearchResponse transactions = yodleeService.GetTransactions(transactionsSearchRequest).First();

            Assert.AreEqual(transactions.searchResult.Transactions.Count, 10);
        }


        //This test can be used only limited amount of time
        //because it uses account that expires after 30 days
        //after each re-registration the credentials should by updated
        //and this test may be temporarily enabled
        [Ignore]
        [Test]
        public async void TestSiteAccountsRequest() {

            IContainer container = InitContainer(typeof(YodleeService));

            YodleeService yodleeService = container.GetInstance<YodleeService>();

            YCobrandLoginRequest cobrandLoginRequest = new YCobrandLoginRequest().SetCobrandUserName(CoBrandUserName)
                .SetCobrandPassword(coBrandPassworkd);

            YCobrandLoginResponse yCobrandLoginResponse = await yodleeService.LoginCobrand(cobrandLoginRequest);

            YUserLoginRequest userLoginRequest = new YUserLoginRequest()
                .SetCobrandSessionToken(yCobrandLoginResponse.cobrandConversationCredentials.sessionToken)
                .SetUserName(UserName)
                .SetUserPassword(UserPassword);

            YUserLoginResponse yUserLoginResponse = await yodleeService.LoginUser(userLoginRequest);

            YSiteAccountsRequest siteAccountsRequest = new YSiteAccountsRequest()
                .SetCobrandSessionToken(yCobrandLoginResponse.cobrandConversationCredentials.sessionToken)
                .SetUserSessionToken(yUserLoginResponse.userContext.conversationCredentials.sessionToken);

            YSiteAccountsResponse ySiteAccountsResponse = await yodleeService.GetSiteAccounts(siteAccountsRequest);
            Assert.AreEqual(ySiteAccountsResponse.AccountInfos.Count, 4);
        }

        [Test]
        public async void TestFastLink() {
            IContainer container = InitContainer(typeof(YodleeService));

            YodleeService yodleeService = container.GetInstance<YodleeService>();

            YCobrandLoginRequest cobrandLoginRequest = new YCobrandLoginRequest().SetCobrandUserName(CoBrandUserName)
                .SetCobrandPassword(coBrandPassworkd);

            YCobrandLoginResponse yCobrandLoginResponse = await yodleeService.LoginCobrand(cobrandLoginRequest);

            YUserLoginRequest userLoginRequest = new YUserLoginRequest()
                .SetCobrandSessionToken(yCobrandLoginResponse.cobrandConversationCredentials.sessionToken)
                .SetUserName(UserName)
                .SetUserPassword(UserPassword);

            YUserLoginResponse yUserLoginResponse = await yodleeService.LoginUser(userLoginRequest);

            YGetFastLinkTokenRequest getFastLinkTokenRequest = new YGetFastLinkTokenRequest()
                .SetCobrandSessionToken(yCobrandLoginResponse.cobrandConversationCredentials.sessionToken)
                .SetUserSessionToken(yUserLoginResponse.userContext.conversationCredentials.sessionToken);

            var yGetFastLinkTokenResponse = await yodleeService.GetFastLinkToken(getFastLinkTokenRequest);

            YFastLinkRequest yFastLinkRequest = new YFastLinkRequest()
                .SetFastLinkToken(yGetFastLinkTokenResponse.FastLinkToken)
                .SetUserSessionToken(yUserLoginResponse.userContext.conversationCredentials.sessionToken)
                .SetOptionalFastLinkSite(16441);

            string formHtml = yFastLinkRequest.GetFormHtml;

            string json = await yodleeService.MakeFastLink(yFastLinkRequest);

            int kk = 0;
        }

        private YodleeOrderItem ConvertAccountsResponse(SiteAccountInfo acc) {
            YodleeOrderItem orderItem = new YodleeOrderItem();

            //orderItem.accountNumber = yTransaction.account.accountNumber;
            //orderItem.accountHolder == ?

            orderItem.bankAccountId = acc.siteAccountId;
            orderItem.OrderId = acc.siteInfo.contentServiceInfos.First()
                .contentServiceId;

            DateTime beginning = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan span = DateTimeOffset.Parse(acc.created)
                .UtcDateTime - beginning;

            orderItem.created = (int)span.TotalSeconds;
            orderItem.link = acc.siteInfo.loginUrl;

            return orderItem;
        }


        //        private void Convert(YTransactionsSearchResponse transactions) {
        //            ICollection<YTransaction> yTransactions = transactions.searchResult.Transactions;
        //        }

        private YodleeOrderItemTransaction ConvertTransaction(YTransaction yTransaction)
        {
//            yTransaction.account.accountNumber == may be updated from transaction

            YodleeOrderItemTransaction t = new YodleeOrderItemTransaction();
            t.bankAccountId = yTransaction.account.itemAccountId;
            t.bankTransactionId = yTransaction.viewKey.transactionId;
            t.categoryId = yTransaction.category.categoryId;
            t.OrderItemId = yTransaction.account.itemAccountId;
            t.category = yTransaction.category.categoryName;
            t.EzbobCategory = ConvertToEzBobCategory(yTransaction.category.categoryName);
            t.transactionBaseType = yTransaction.transactionBaseType;
            t.transactionBaseTypeId = yTransaction.transactionBaseTypeId;
            t.localizedTransactionBaseType = yTransaction.localizedTransactionBaseType;
            t.runningBalance = yTransaction.runningBalance;
            t.categorizationKeyword = yTransaction.categorizationKeyword;
            t.postDate = DateTimeOffset.Parse(yTransaction.postDate)
                .UtcDateTime;
            t.transactionDate = DateTimeOffset.Parse(yTransaction.transactionDate)
                .UtcDateTime;
            t.transactionAmount = ConvertToPounds(yTransaction.amount);
            t.transactionAmountCurrency = yTransaction.amount.currencyCode;
            t.transactionStatus = yTransaction.status.description;
            t.categorisationSourceId = yTransaction.categorisationSourceId.ToString();
            t.description = yTransaction.description.description;
            t.runningBalance = yTransaction.runningBalance;
            t.runningBalanceCurrency = yTransaction.amount.currencyCode;

            return t;
        }

        private decimal ConvertToPounds(YMoney money)
        {
            throw new NotImplementedException();
        }

        private string ConvertToEzBobCategory(string category)
        {
            throw new NotImplementedException();
        }
    }
}

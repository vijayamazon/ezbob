namespace EzBob3dParties.Yodlee.Handlers {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EzBob3dParties.Yodlee.Models.SiteAccount;
    using EzBob3dParties.Yodlee.RequestResponse;
    using EzBob3dPartiesApi.Yodlee;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using NServiceBus;

    internal class YodleeGetUserAccountsCommandHandler : HandlerBase<YodleeGetUserAccountsCommandResponse>, IHandleMessages<YodleeGetUserAccountsCommand> {

        [Injected]
        public YodleeService Yodlee { get; set; }

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="cmd">The command.</param>
        public async void Handle(YodleeGetUserAccountsCommand cmd) {

            YCobrandLoginRequest cobrandLoginRequest = new YCobrandLoginRequest().SetCobrandUserName(cmd.CobrandUserName)
                .SetCobrandPassword(cmd.CobrandPassword);

            YCobrandLoginResponse yCobrandLoginResponse = await Yodlee.LoginCobrand(cobrandLoginRequest);

            YUserLoginRequest userLoginRequest = new YUserLoginRequest()
                .SetCobrandSessionToken(yCobrandLoginResponse.cobrandConversationCredentials.sessionToken)
                .SetUserName(cmd.UserName)
                .SetUserPassword(cmd.UserPassword);

            YUserLoginResponse yUserLoginResponse = await Yodlee.LoginUser(userLoginRequest);

            YSiteAccountsRequest siteAccountsRequest = new YSiteAccountsRequest()
                .SetCobrandSessionToken(yCobrandLoginResponse.cobrandConversationCredentials.sessionToken)
                .SetUserSessionToken(yUserLoginResponse.userContext.conversationCredentials.sessionToken);

            InfoAccumulator info = new InfoAccumulator();
            YSiteAccountsResponse ySiteAccountsResponse = await Yodlee.GetSiteAccounts(siteAccountsRequest);

            var yodleeUserAccounts = ySiteAccountsResponse.AccountInfos.Select(TransformToApiAccount).ToList();

            string json = await Yodlee.GetSiteAccountsAsJson(siteAccountsRequest);
            if (IsError(json)) {
                info.AddError("got error response:\n" + json);
            }

            SendReply(info, cmd, reply => {
                reply.CustomerId = cmd.CustomerId;
                reply.UserName = cmd.UserName;
                reply.UserPassword = cmd.UserPassword;
                reply.Accounts = yodleeUserAccounts;
            });
        }

        /// <summary>
        /// Transforms SiteAccountInfo to API account.
        /// </summary>
        /// <param name="accountInfo">The account information.</param>
        /// <returns></returns>
        private YodleeContentServiceAccount TransformToApiAccount(SiteAccountInfo accountInfo) {
            return new YodleeContentServiceAccount {
                ContentServiceId = accountInfo.siteInfo.contentServiceInfos.First().contentServiceId,
                SiteAccountId = accountInfo.siteAccountId,
                CreatedInSeconds = CalculateCreatedTimeInSeconds(accountInfo),
                LoginUrl = accountInfo.siteInfo.loginUrl
            };
        }

        /// <summary>
        /// Calculates the created time in seconds.
        /// </summary>
        /// <param name="accountInfo">The account information.</param>
        /// <returns></returns>
        private int CalculateCreatedTimeInSeconds(SiteAccountInfo accountInfo) {
            DateTime beginning = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan span = DateTimeOffset.Parse(accountInfo.created)
                .UtcDateTime - beginning;

            return (int)span.TotalSeconds;
        }

        /// <summary>
        /// Determines whether the specified json is error.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        private bool IsError(string json) {
            int idx = json.IndexOf("error", StringComparison.InvariantCultureIgnoreCase);
            return (idx > -1) && (idx < 30);
        }
    }
}

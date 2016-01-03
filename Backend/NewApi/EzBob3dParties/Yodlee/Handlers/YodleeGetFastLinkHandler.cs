namespace EzBob3dParties.Yodlee.Handlers {
    using EzBob3dParties.Yodlee.RequestResponse;
    using EzBob3dPartiesApi.Yodlee;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using NServiceBus;

    public class YodleeGetFastLinkHandler : HandlerBase<YodleeGetFastLinkCommandResponse>, IHandleMessages<YodleeGetFastLinkCommand> {

        [Injected]
        public YodleeService Yodlee { get; set; }

        /// <summary>
        /// Handles a YodleeGetFastLinkCommand.
        /// </summary>
        /// <param name="cmd">The command.</param>
        public async void Handle(YodleeGetFastLinkCommand cmd) {
            YCobrandLoginRequest cobrandLoginRequest = new YCobrandLoginRequest().SetCobrandUserName(cmd.CobrandUserName)
               .SetCobrandPassword(cmd.CobrandPassword);

            YCobrandLoginResponse yCobrandLoginResponse = await Yodlee.LoginCobrand(cobrandLoginRequest);

            YUserLoginRequest userLoginRequest = new YUserLoginRequest()
                .SetCobrandSessionToken(yCobrandLoginResponse.cobrandConversationCredentials.sessionToken)
                .SetUserName(cmd.UserName)
                .SetUserPassword(cmd.UserPassword);

            YUserLoginResponse yUserLoginResponse = await Yodlee.LoginUser(userLoginRequest);

            YGetFastLinkTokenRequest getFastLinkTokenRequest = new YGetFastLinkTokenRequest()
                .SetCobrandSessionToken(yCobrandLoginResponse.cobrandConversationCredentials.sessionToken)
                .SetUserSessionToken(yUserLoginResponse.userContext.conversationCredentials.sessionToken);

            var yGetFastLinkTokenResponse = await Yodlee.GetFastLinkToken(getFastLinkTokenRequest);

            YFastLinkRequest yFastLinkRequest = new YFastLinkRequest()
                .SetFastLinkToken(yGetFastLinkTokenResponse.FastLinkToken)
                .SetUserSessionToken(yUserLoginResponse.userContext.conversationCredentials.sessionToken);
            if (cmd.ContentServiceId > 0) {
                yFastLinkRequest.SetOptionalFastLinkSite(cmd.SiteId);
            }

            string formHtml = yFastLinkRequest.GetFormHtml;

            string json = await Yodlee.MakeFastLink(yFastLinkRequest);

            SendReply(new InfoAccumulator(), cmd, reply => {
                reply.FastLinkUrl = json;
                reply.FormHtml = formHtml;
            });

        }
    }
}

namespace EzBobService.ThirdParties.Yodlee {
    using System.IO;
    using EzBob3dPartiesApi.Yodlee;
    using EzBobApi.Commands.Yodlee;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobPersistence.Yodlee;
    using NServiceBus;

    public class YodleeAddAccountHandler : HandlerBase<YodleeAddUserAccountResponse>, IHandleMessages<YodleeAddUserAccountCommand>, IHandleMessages<YodleeGetFastLinkCommandResponse> {

        [Injected]
        public IYodleeQueries YodleeQueries { get; set; }

        [Injected]
        public YodleeConfig Config { get; set; }

        [Injected]
        public ThirdPartyServiceConfig ThirdPartyConfig { get; set; }

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="cmd">The command.</param>
        /// <exception cref="System.IO.InvalidDataException"></exception>
        public void Handle(YodleeAddUserAccountCommand cmd) {

            InfoAccumulator info = new InfoAccumulator();

            if (YodleeQueries.IsUserAlreadyHaveContentService(cmd.CustomerId, cmd.ContentServiceId)) {
                string msg = "the customer already added this content service" + cmd.ContentServiceId;
                info.AddError(msg);
                Log.Info(msg);
                SendReply(info, cmd);
                return;
            }

            int? siteId = YodleeQueries.GetSiteIdFromContentServiceId(cmd.ContentServiceId);
            if (!siteId.HasValue) {
                string err = "could not retrieve site id from DB";
               
                Log.Error(err);
                info.AddError(err);
                RegisterError(info, cmd);
                //go to retry
                throw new InvalidDataException(err);
            }

            var yodleeUserAccount = YodleeQueries.GetUserAccount(cmd.CustomerId);
            if (yodleeUserAccount == null) {
                var err = "could not get user account from DB for id: " + cmd.CustomerId;
                Log.Error(err);
                info.AddError(err);
                RegisterError(info, cmd);
                //go to retry
                throw new InvalidDataException(err);
            }

            YodleeGetFastLinkCommand fastLinkCommand = new YodleeGetFastLinkCommand {
                ContentServiceId = cmd.ContentServiceId,
                CobrandUserName = Config.CoBrandUserName,
                CobrandPassword = Config.CoBrandPassword,
                UserName = yodleeUserAccount.Username,
                UserPassword = yodleeUserAccount.Password,
                SiteId = siteId.Value
            };

            SendCommand(ThirdPartyConfig.Address, fastLinkCommand, cmd);
        }

        /// <summary>
        /// Handles the specified response.
        /// </summary>
        /// <param name="response">The response.</param>
        public void Handle(YodleeGetFastLinkCommandResponse response)
        {
            if (response.IsFailed) {
                ReplyToOrigin(response);
                return;
            }

            ReplyToOrigin(response, reply => {
                reply.FastLinkUrl = response.FastLinkUrl;
            });
        }
    }
}

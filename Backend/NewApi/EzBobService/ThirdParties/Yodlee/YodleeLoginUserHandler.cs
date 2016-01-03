namespace EzBobService.ThirdParties.Yodlee {
    using EzBob3dPartiesApi.Yodlee;
    using EzBobApi.Commands.Yodlee;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobModels.Yodlee;
    using EzBobPersistence.Yodlee;
    using NServiceBus;

    public class YodleeHandler : HandlerBase<YodleeLoginUserCommandResponse>, IHandleMessages<YodleeLoginUserCommand>, IHandleMessages<YodleeLoginUser3dPartyCommandResponse> {

        [Injected]
        public ThirdPartyServiceConfig ThirdPartyService { get; set; }

        [Injected]
        public YodleeConfig Config { get; set; }

        [Injected]
        public IYodleeQueries YodleeQueries { get; set; }

        public void Handle(YodleeLoginUserCommand handledCommand) {

            InfoAccumulator info = new InfoAccumulator();

            if (!ValidateModel(handledCommand, info)) {
                SendReply(info, handledCommand);
                return;
            }

            YodleeLoginUser3dPartyCommand commandToSend = CreateLoginUserCommand(handledCommand);

            SendCommand(ThirdPartyService.Address, commandToSend, handledCommand);
        }

        public void Handle(YodleeLoginUser3dPartyCommandResponse handledResponse) {
            //at this point command not failed but may get error (failed user login)
            if (handledResponse.IsLoginFailed)
            {
                //recover state
                YodleeLoginUserCommand fakeCommand = new YodleeLoginUserCommand {
                    CustomerId = handledResponse.CustomerId, //recovers saved state
                    SiteId = handledResponse.SiteId, //recovers saved state
                };

                YodleeLoginUser3dPartyCommand commandToSend = CreateLoginUserCommand(fakeCommand);

                //copies infrastructure related info, to insure proper error handling
                CopyHeaders(@from: handledResponse, to: commandToSend);
                Bus.Send(ThirdPartyService.Address, commandToSend);

            } else {
                ReplyToOrigin(handledResponse, resp => {
                    resp.UserToken = handledResponse.UserToken;
                });
            }
        }

        /// <summary>
        /// Creates the login user command.
        /// First goes to DB to fetch available account and then populates command with fetched parameters
        /// </summary>
        /// <param name="handledCommand">The handled command.</param>
        /// <returns></returns>
        private YodleeLoginUser3dPartyCommand CreateLoginUserCommand(YodleeLoginUserCommand handledCommand) {
            YodleeUserAccount userAccount = YodleeQueries.BookUserAccount(handledCommand.CustomerId);

            YodleeLoginUser3dPartyCommand commandToSend = new YodleeLoginUser3dPartyCommand
            {
                PasswordEncrypted = userAccount.Password,
                UserName = userAccount.Username,
                MessageId = handledCommand.MessageId,
                CobrandToken = handledCommand.CobrandToken,

                CustomerId = handledCommand.CustomerId, //stores 'state' to be able to book another user account if yodlee return error on login
                SiteId = handledCommand.SiteId //stores 'state' to be able to book another user account if yodlee return error on login
            };

            return commandToSend;
        }
    }
}

namespace EzBob3dParties.Yodlee.Handlers {
    using System.Web.UI.WebControls;
    using EzBob3dParties.Yodlee.RequestResponse;
    using EzBob3dPartiesApi.Yodlee;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobCommon.Utils.Encryption;
    using NServiceBus;

    public class YodleeUserLoginHandler : HandlerBase<YodleeLoginUser3dPartyCommandResponse>, IHandleMessages<YodleeLoginUser3dPartyCommand> {

        [Injected]
        public YodleeService Yodlee { get; set; }

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        public void Handle(YodleeLoginUser3dPartyCommand command) {
            InfoAccumulator info = new InfoAccumulator();
            if (ValidateModel(command, info)) {
                SendReply(info, command);
                return;
            }

            //created request
            YUserLoginRequest loginRequest = new YUserLoginRequest()
                .SetCobrandSessionToken(command.CobrandToken)
                .SetUserName(command.UserName)
                .SetUserPassword(EncryptionUtils.Decrypt(command.PasswordEncrypted));

            //send REST request to yodlee
            var yUserLoginResponse = Yodlee.LoginUser(loginRequest)
                .Result;

            //handle yodlee response
            if (yUserLoginResponse.HasError) {
                string errorMessage = yUserLoginResponse.GetError()
                    .ErrorMessage;

                Log.Error(errorMessage);
                info.AddError(errorMessage);
            }

            SendReply(info, command, resp => {
                if (!yUserLoginResponse.HasError) {
                    resp.UserToken = yUserLoginResponse.userContext.conversationCredentials.sessionToken;
                    resp.IsLoginFailed = false;
                } else {
                    resp.IsLoginFailed = true;
                }
            });
        }
    }
}

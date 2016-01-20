using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.PayPalService.Soap.Handlers {
    using EzBob3dPartiesApi.PayPal.Soap;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using NServiceBus;
    using PayPal.Permissions.Model;

    public class PayPalGetAccessTokenCommandHandler : HandlerBase<PayPalGetAccessToken3dPartyCommandResponse>, IHandleMessages<PayPalGetAccessToken3dPartyCommand> {

        [Injected]
        public PayPalSoapService PayPalService { get; set; }

        public async void Handle(PayPalGetAccessToken3dPartyCommand command) {
            GetAccessTokenResponse tokenResponse = await PayPalService.GetAccessToken(command.RequestToken, command.VerificationCode);
            InfoAccumulator info = new InfoAccumulator();

            SendReply(info, command, resp => {
                resp.Token = tokenResponse.token;
                resp.TokenSecret = tokenResponse.tokenSecret;
            });
        }
    }
}

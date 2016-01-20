namespace EzBobService.ThirdParties.PayPal.SendRecieve {
    using EzBob3dPartiesApi.PayPal.Soap;
    using EzBobApi.Commands.PayPal;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using NServiceBus;

    public class PayPalGetPermissionsRedirectUrlHandler : HandlerBase<PayPalGetPermissionsRedirectUrlCommandResponse>, IHandleMessages<PayPalGetPermissionsRedirectUrlCommand>, IHandleMessages<PayPalGetPermissionsRedirectUrl3dPartyCommandResponse> {
        [Injected]
        public ThirdPartyServiceConfig ThirdPartyService { get; set; }

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        public void Handle(PayPalGetPermissionsRedirectUrlCommand command) {
            var commandToSend = new PayPalGetPermissionsRedirectUrl3dPartyCommand {
                Callback = command.Callback
            };

            SendCommand(ThirdPartyService.Address, commandToSend, command);
        }

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        public void Handle(PayPalGetPermissionsRedirectUrl3dPartyCommandResponse command) {
            var info = new InfoAccumulator();
            SendReply(info, command, resp => resp.PermissionsRedirectUrl = command.PermissionsRedirectUrl);
        }
    }
}

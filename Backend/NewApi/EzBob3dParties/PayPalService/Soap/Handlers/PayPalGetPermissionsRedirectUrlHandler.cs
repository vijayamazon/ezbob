namespace EzBob3dParties.PayPalService.Soap.Handlers
{
    using EzBob3dPartiesApi.PayPal.Soap;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using NServiceBus;

    public class PayPalGetPermissionsRedirectUrlHandler : HandlerBase<PayPalGetPermissionsRedirectUrl3dPartyCommandResponse>, IHandleMessages<PayPalGetPermissionsRedirectUrl3dPartyCommand>
    {
        [Injected]
        public PayPalSoapService PayPalService { get; set; }

        public async void Handle(PayPalGetPermissionsRedirectUrl3dPartyCommand command) {
            var url = await PayPalService.GetPermissionsRedirectUrl(command.Callback);

            InfoAccumulator info = new InfoAccumulator();
            SendReply(info, command, resp => resp.PermissionsRedirectUrl = url);
        }
    }
}

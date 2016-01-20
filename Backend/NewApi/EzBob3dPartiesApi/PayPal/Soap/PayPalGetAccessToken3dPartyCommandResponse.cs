namespace EzBob3dPartiesApi.PayPal.Soap
{
    using EzBobCommon.NSB;

    public class PayPalGetAccessToken3dPartyCommandResponse : CommandResponseBase
    {
        public string Token { get; set; }
        public string TokenSecret { get; set; }
    }
}

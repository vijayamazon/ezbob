namespace EzBob3dPartiesApi.PayPal.Soap
{
    using EzBobCommon.NSB;

    public class PayPalGetAccessToken3dPartyCommand : CommandBase
    {
        public string VerificationCode { get; set; }
        public string RequestToken { get; set; }
    }
}

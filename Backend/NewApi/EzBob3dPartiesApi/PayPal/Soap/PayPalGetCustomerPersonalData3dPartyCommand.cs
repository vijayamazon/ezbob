namespace EzBob3dPartiesApi.PayPal.Soap
{
    using EzBobCommon.NSB;

    public class PayPalGetCustomerPersonalData3dPartyCommand : CommandBase
    {
        public string Token { get; set; }
        public string TokenSecret { get; set; }
    }
}

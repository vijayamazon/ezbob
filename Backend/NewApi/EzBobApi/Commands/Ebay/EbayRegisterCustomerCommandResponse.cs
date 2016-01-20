namespace EzBobApi.Commands.Ebay
{
    using EzBobCommon.NSB;

    public class EbayRegisterCustomerCommandResponse : CommandResponseBase
    {
        public bool? IsAccountValid { get; set; }
    }
}

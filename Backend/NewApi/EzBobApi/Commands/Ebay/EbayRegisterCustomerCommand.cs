namespace EzBobApi.Commands.Ebay
{
    using EzBobCommon.NSB;

    public class EbayRegisterCustomerCommand : CommandBase
    {
        public string CustomerId { get; set; }
        public string SessionId { get; set; }
        public string Token { get; set; }
        public string MarketplaceName { get; set; }
    }
}

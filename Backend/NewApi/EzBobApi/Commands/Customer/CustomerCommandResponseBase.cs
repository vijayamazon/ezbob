namespace EzBobApi.Commands.Customer
{
    using EzBobCommon.NSB;

    public class CustomerCommandResponseBase : CommandResponseBase
    {
        public string CustomerId { get; set; }
    }
}

namespace EzBobApi.Commands.Customer
{
    using System;
    using EzBobCommon.NSB;

    public class CustomerSignupCommandResponse : CommandResponseBase
    {
        public string CustomerId { get; set; }
    }
}

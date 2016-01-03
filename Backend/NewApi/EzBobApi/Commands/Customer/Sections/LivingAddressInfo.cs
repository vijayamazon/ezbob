namespace EzBobApi.Commands.Customer.Sections
{
    using System;

    public class LivingAddressInfo : AddressInfo
    {
        public bool IsLivingNow { get; set; }
        public TimeSpan TimeAtAddress { get; set; }
    }
}

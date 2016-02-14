namespace EzBobApi.Commands.Customer.Sections {
    using System;

    public class LivingAddressInfo : AddressInfo {
        public HousingType HousingType { get; set; }
        public string MonthsAtAddress { get; set; }

        public bool IsOwns
        {
            get { return this.HousingType == HousingType.OwnProperty; }
        }
    }
}

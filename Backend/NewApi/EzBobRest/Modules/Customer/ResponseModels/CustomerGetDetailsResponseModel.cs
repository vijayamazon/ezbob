namespace EzBobRest.Modules.Customer.ResponseModels
{
    using EzBobApi.Commands.Customer.Sections;

    class CustomerGetDetailsResponseModel
    {
        public PersonalDetailsInfo PersonalDetails { get; set; }
        public CustomerContactDetailsResponseModel ContactDetails { get; set; }
        public LivingAddressInfo CurrentLivingAddress { get; set; }
        public LivingAddressInfo PreviousLivingAddress { get; set; }
        public LivingAddressInfo[] AdditionalOwnedProperties { get; set; }
        public decimal RequestedAmount { get; set; }
    }
}

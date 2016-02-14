namespace EzBobApi.Commands.Company
{
    using EzBobApi.Commands.Customer.Sections;

    /// <summary>
    /// Person that have a right to make decisions
    /// </summary>
    public class AuthorityInfo
    {
//        public int Position { get; set; }
        public bool IsShareHolder { get; set; }
        public bool IsDirector { get; set; }
        public PersonalDetailsInfo PersonalDetails { get; set; }
        public ContactDetailsInfo ContactDetails { get; set; }
        public AddressInfo AddressInfo { get; set; }
    }
}

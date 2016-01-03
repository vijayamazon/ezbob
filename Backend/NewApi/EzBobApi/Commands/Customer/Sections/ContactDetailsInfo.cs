namespace EzBobApi.Commands.Customer.Sections {
    /// <summary>
    /// Contains contact details related data
    /// </summary>
    public class ContactDetailsInfo {
        public string MobilePhone { get; set; }
        public bool IsMobilePhoneVerified { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
    }
}
      
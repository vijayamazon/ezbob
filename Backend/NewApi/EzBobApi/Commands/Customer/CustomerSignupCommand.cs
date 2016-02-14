namespace EzBobApi.Commands.Customer {
    using EzBobCommon.NSB;

    /// <summary>
    /// Customer sign-up command
    /// </summary>
    public class CustomerSignupCommand : CommandBase {
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneNumberValidationCode { get; set; }
        public int? SequrityQuestionId { get; set; }
        public string SecurityQuestionAnswer { get; set; } 
    }
}

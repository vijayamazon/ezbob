namespace EzBobApi.Commands.Customer.Sections {
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Contains account related data
    /// </summary>
    public class AccountInfo {
        [Required]
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public int? SecretQuestion { get; set; }
        public string SecretAnswer { get; set; }
        public string PromotionalCode { get; set; }
        public string RemoteIp { get; set; }
    }
}

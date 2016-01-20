namespace EzBobApi.Commands.PayPal
{
    using EzBobCommon.NSB;

    public class PayPalRegisterCustomerCommand : CommandBase
    {
        public string CustomerId { get; set; }
        public string CustomerEmailAddress { get; set; }
        public string RequestToken { get; set; }
        public string VerificationToken { get; set; }
    }
}

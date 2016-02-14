namespace EzBobApi.Commands.Customer
{
    using EzBobCommon.NSB;

    public class CustomerLoginCommand : CommandBase
    {
        public string EmailAddress { get; set; }
        public string Password { get; set; }
    }
}

namespace EzBobApi.Commands.Customer
{
    using EzBobCommon.NSB;

    /// <summary>
    /// Response to <see cref="CustomerLoginCommand"/>
    /// </summary>
    public class CustomerLoginCommandResponse : CommandResponseBase {
        public string CustomerId;
    }
}

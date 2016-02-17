namespace EzBob3dPartiesApi.Amazon
{
    using EzBobCommon.NSB;

    /// <summary>
    /// Response to <see cref="AmazonGetCustomerInfo3dPartyCommand"/>.
    /// </summary>
    public class AmazonGetCustomerInfo3dPartyCommandResponse : CommandResponseBase {
        public string BusinessName { get; set; }
    }
}

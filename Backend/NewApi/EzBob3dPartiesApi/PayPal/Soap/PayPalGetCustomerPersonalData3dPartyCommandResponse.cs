namespace EzBob3dPartiesApi.PayPal.Soap
{
    using EzBobCommon.NSB;
    using EzBobModels.PayPal;

    /// <summary>
    /// response to PayPalGetCustomerPersonalData3dPartyCommand
    /// </summary>
    public class PayPalGetCustomerPersonalData3dPartyCommandResponse : CommandResponseBase
    {
        /// <summary>
        /// Gets or sets the personal information.
        /// </summary>
        /// <value>
        /// The personal information.
        /// </value>
        public PayPalUserPersonalInfo UserPersonalInfo { get; set; }
    }
}

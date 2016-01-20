namespace EzBob3dPartiesApi.PayPal.Soap
{
    using EzBobCommon.NSB;

    /// <summary>
    /// Response to <see cref="PayPalGetPermissionsRedirectUrl3dPartyCommand"/>
    /// </summary>
    public class PayPalGetPermissionsRedirectUrl3dPartyCommandResponse : CommandResponseBase
    {
        /// <summary>
        /// Gets or sets the permissions redirect URL.
        /// The url where to send user in order to get his consent to share private information with us
        /// </summary>
        /// <value>
        /// The permissions redirect URL.
        /// </value>
        public string PermissionsRedirectUrl { get; set; }
    }
}

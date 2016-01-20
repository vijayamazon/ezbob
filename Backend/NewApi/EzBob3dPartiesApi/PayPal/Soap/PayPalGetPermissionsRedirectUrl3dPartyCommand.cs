namespace EzBob3dPartiesApi.PayPal.Soap
{
    using EzBobCommon.NSB;

    /// <summary>
    /// Asks for permissions redirect url
    /// </summary>
    public class PayPalGetPermissionsRedirectUrl3dPartyCommand : CommandBase {

        /// <summary>
        /// Gets or sets the callback.
        /// </summary>
        /// <value>
        /// The callback.
        /// </value>
        public string Callback { get; set; }
    }
}

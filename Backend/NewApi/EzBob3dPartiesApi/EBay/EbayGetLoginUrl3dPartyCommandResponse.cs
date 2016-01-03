namespace EzBob3dPartiesApi.EBay
{
    using EzBobCommon.NSB;

    /// <summary>
    /// response to  GetEbayLoginUrl3dPartyCommand
    /// </summary>
    public class EbayGetLoginUrl3dPartyCommandResponse : CommandResponseBase {
        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        public string Url { get; set; }
        /// <summary>
        /// Gets or sets the session identifier.
        /// </summary>
        /// <value>
        /// The session identifier.
        /// </value>
        public string SessionId { get; set; }
    }
}

namespace EzBobApi.Commands.Yodlee
{
    using EzBobCommon.NSB;

    /// <summary>
    /// Commands to login specific customer
    /// </summary>
    public class YodleeLoginUserCommand : CommandBase
    {
        /// <summary>
        /// Gets or sets the customer identifier.
        /// </summary>
        /// <value>
        /// The customer identifier.
        /// </value>
        public int CustomerId { get; set; }
        /// <summary>
        /// Gets or sets the site identifier.
        /// </summary>
        /// <value>
        /// The site identifier.
        /// </value>
        public int SiteId { get; set; }


        /// <summary>
        /// Exists for backward compatibility
        /// Gets or sets the content service identifier.
        /// </summary>
        /// <value>
        /// The content service identifier.
        /// </value>
        public int ContentServiceId { get; set; }
        /// <summary>
        /// Code that ezbob got from yodlee
        /// </summary>
        /// <value>
        /// The cobrand token.
        /// </value>
        public string CobrandToken { get; set; }
    }
}

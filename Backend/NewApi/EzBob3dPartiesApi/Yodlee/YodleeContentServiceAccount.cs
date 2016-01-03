namespace EzBob3dPartiesApi.Yodlee
{
    using System;

    /// <summary>
    /// Contains account information that should be stored in DB
    /// </summary>
    public class YodleeContentServiceAccount
    {
        /// <summary>
        /// Gets or sets the account number.
        /// </summary>
        /// <value>
        /// The account number.
        /// </value>
        public string AccountNumber { get; set; }
        /// <summary>
        /// Gets or sets the account holder.
        /// </summary>
        /// <value>
        /// The account holder.
        /// </value>
        public string AccountHolder { get; set; }

        /// <summary>
        /// Gets or sets the site account identifier.
        /// </summary>
        /// <value>
        /// The site account identifier.
        /// </value>
        public int SiteAccountId { get; set; }
        /// <summary>
        /// Gets or sets the content service identifier.
        /// </summary>
        /// <value>
        /// The content service identifier.
        /// </value>
        public int ContentServiceId { get; set; }
        /// <summary>
        /// Gets or sets the created in seconds.
        /// </summary>
        /// <value>
        /// The created in seconds.
        /// </value>
        public int CreatedInSeconds { get; set; }
        /// <summary>
        /// Gets or sets the login URL.
        /// </summary>
        /// <value>
        /// The login URL.
        /// </value>
        public string LoginUrl { get; set; }
    }
}

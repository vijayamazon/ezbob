namespace EzBob3dPartiesApi.Yodlee
{
    using EzBobCommon.NSB;

    /// <summary>
    /// Command to get user accounts
    /// </summary>
    public class YodleeGetUserAccountsCommand : CommandBase
    {
        /// <summary>
        /// Gets or sets the name of the co-brand user.
        /// </summary>
        /// <value>
        /// The name of the co-brand user.
        /// </value>
        public string CobrandUserName { get; set; }
        /// <summary>
        /// Gets or sets the co-brand password.
        /// </summary>
        /// <value>
        /// The co-brand password.
        /// </value>
        public string CobrandPassword { get; set; }
        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        public string UserName { get; set; }
        /// <summary>
        /// Gets or sets the user password.
        /// </summary>
        /// <value>
        /// The user password.
        /// </value>
        public string UserPassword { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier.
        /// </summary>
        /// <value>
        /// The customer identifier.
        /// </value>
        public int CustomerId { get; set; }
    }
}

namespace EzBob3dPartiesApi.Yodlee
{
    using System.Collections.Generic;
    using EzBobCommon.NSB;

    public class YodleeGetUserAccountsCommandResponse : CommandResponseBase
    {
        /// <summary>
        /// Gets or sets the customer identifier.
        /// </summary>
        /// <value>
        /// The customer identifier.
        /// </value>
        public int CustomerId { get; set; }
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
        /// Gets or sets the accounts.
        /// </summary>
        /// <value>
        /// The accounts.
        /// </value>
        public IList<YodleeContentServiceAccount> Accounts { get; set; }
    }
}

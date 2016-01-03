namespace EzBobApi.Commands.Customer {
    using EzBobApi.Commands.Customer.Sections;
    using EzBobCommon.NSB;
    using EzBobModels;

    /// <summary>
    /// Customer sign-up command
    /// </summary>
    public class CustomerSignupCommand : CommandBase {
        /// <summary>
        /// Gets or sets the account.
        /// </summary>
        /// <value>
        /// The account.
        /// </value>
        public AccountInfo Account { get; set; }

        /// <summary>
        /// Gets or sets the requested amount.
        /// </summary>
        /// <value>
        /// The requested amount.
        /// </value>
        public int RequestedAmount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is test.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is test; otherwise, <c>false</c>.
        /// </value>
        public bool IsTest { get; set; }

        /// <summary>
        /// Gets or sets the customer origin.
        /// </summary>
        /// <value>
        /// The customer origin.
        /// </value>
        public string CustomerOrigin { get; set; }
    }
}

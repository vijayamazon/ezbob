namespace EzBobApi.Commands.Customer {
    using EzBobApi.Commands.Customer.Sections;
    using EzBobCommon.NSB;
    using EzBobModels;

    public class CustomerUpdateCommand : CommandBase {
        /// <summary>
        /// Gets or sets the contact details.
        /// </summary>
        /// <value>
        /// The contact details.
        /// </value>
        public ContactDetailsInfo ContactDetails { get; set; }

        /// <summary>
        /// Gets or sets the current living address.
        /// </summary>
        /// <value>
        /// The current living address.
        /// </value>
        public LivingAddressInfo CurrentLivingAddress { get; set; }

        /// <summary>
        /// Gets or sets the previous living address.
        /// </summary>
        /// <value>
        /// The previous living address.
        /// </value>
        public LivingAddressInfo PreviousLivingAddress { get; set; }

        /// <summary>
        /// Gets or sets the additional owned properties.
        /// </summary>
        /// <value>
        /// The additional owned properties.
        /// </value>
        public LivingAddressInfo[] AdditionalOwnedProperties { get; set; }

        /// <summary>
        /// Gets or sets the personal details.
        /// </summary>
        /// <value>
        /// The personal details.
        /// </value>
        public PersonalDetailsInfo PersonalDetails { get; set; }

        /// <summary>
        /// Gets or sets the alibaba information.
        /// </summary>
        /// <value>
        /// The alibaba information.
        /// </value>
        public AlibabaInfo AlibabaInfo { get; set; }

        /// <summary>
        /// Gets or sets the cookies.
        /// </summary>
        /// <value>
        /// The cookies.
        /// </value>
        public CookiesInfo Cookies { get; set; }

        /// <summary>
        /// Gets or sets the campaign source reference.
        /// </summary>
        /// <value>
        /// The campaign source reference.
        /// </value>
        public CampaignSourceRef CampaignSourceRef { get; set; }

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
        /// Gets or sets the customer identifier.<br/>
        /// Created at sign-up
        /// </summary>
        /// <value>
        /// The customer identifier.
        /// </value>
        public string CustomerId { get; set; }
    }
}

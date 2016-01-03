namespace EzBob3dParties.Yodlee.RequestResponse
{
    using EzBob3dParties.Yodlee.Models.Login;

    /// <summary>
    /// The co-brand context
    /// </summary>
    class YCobrandLoginResponse : YResponseBase
    {
        /// <summary>
        /// Gets or sets the co-brand conversation credentials.
        /// </summary>
        /// <value>
        /// The co-brand conversation credentials.
        /// </value>
        public ConversationCredentials cobrandConversationCredentials { get; set; }
    }
}

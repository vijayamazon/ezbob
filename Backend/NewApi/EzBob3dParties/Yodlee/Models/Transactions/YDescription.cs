namespace EzBob3dParties.Yodlee.Models.Transactions
{
    /// <summary>
    /// The description
    /// </summary>
    class YDescription
    {
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string description { get; set; }

        public bool viewPref { get; set; }
        public bool isOlbUserDescription { get; set; }
        public string simpleDescription { get; set; }
        public string transactionTypeDesc { get; set; }
        public string merchantName { get; set; }
    }
}

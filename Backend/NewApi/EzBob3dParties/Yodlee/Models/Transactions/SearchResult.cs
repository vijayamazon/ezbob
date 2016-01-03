namespace EzBob3dParties.Yodlee.Models.Transactions
{
    using System.Collections.Generic;

    /// <summary>
    /// The search result
    /// </summary>
    class SearchResult
    {
        /// <summary>
        /// Gets or sets the transactions.
        /// </summary>
        /// <value>
        /// The transactions.
        /// </value>
        public ICollection<YTransaction> Transactions { get; set; } 
    }
}

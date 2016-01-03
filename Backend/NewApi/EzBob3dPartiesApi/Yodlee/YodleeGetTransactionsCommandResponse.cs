namespace EzBob3dPartiesApi.Yodlee
{
    using System.Collections.Generic;
    using EzBobCommon.NSB;

    public class YodleeGetTransactionsCommandResponse : CommandResponseBase
    {
        /// <summary>
        /// Gets or sets the hits count.
        /// </summary>
        /// <value>
        /// The hits count.
        /// </value>
        public int HitsCount { get; set; }
        /// <summary>
        /// Gets or sets the total transactions count.
        /// </summary>
        /// <value>
        /// The total transactions count.
        /// </value>
        public int TotalTransactionsCount { get; set; }
        /// <summary>
        /// Gets or sets the transactions.
        /// </summary>
        /// <value>
        /// The transactions.
        /// </value>
        public IEnumerable<YodleeTransaction> Transactions { get; set; }
        /// <summary>
        /// It's the state that brought back
        /// Gets or sets the account numbers.
        /// </summary>
        /// <value>
        /// The account numbers.
        /// </value>
        public IEnumerable<int> AccountNumbers { get; set; } 
    }
}

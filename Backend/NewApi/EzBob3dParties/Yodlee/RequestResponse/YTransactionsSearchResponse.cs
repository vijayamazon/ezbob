namespace EzBob3dParties.Yodlee.RequestResponse
{
    using EzBob3dParties.Yodlee.Models.Transactions;

    /// <summary>
    /// The resulst of yodlee transactions request
    /// </summary>
    class YTransactionsSearchResponse : YResponseBase
    {
        public SearchIdentifier searchIdentifier { get; set; }

        public int numberOfHits { get; set; }

        public SearchResult searchResult { get; set; }

        public int countOfAllTransaction { get; set; }
    }
}

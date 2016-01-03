namespace EzBob3dParties.Yodlee.RequestResponse
{
    using System;
    using System.Collections.Generic;
    using EzBob3dParties.Yodlee.Models;

    /// <summary>
    /// The yodlee transactions request
    /// </summary>
    class YTransactionsSearchRequest : YRequestBase
    {
        private string filter;
        private bool isDateRangeSet = false;

        public YTransactionsSearchRequest() {
            InitDefaultParameters();
        }

        /// <summary>
        /// Initializes the default parameters.
        /// </summary>
        private void InitDefaultParameters() {
            Insert("transactionSearchRequest.containerType", "bank");
            Insert("transactionSearchRequest.higherFetchLimit", int.MaxValue);
            Insert("transactionSearchRequest.lowerFetchLimit", 1);
            Insert("transactionSearchRequest.searchFilter.currencyCode", "USD");
        }

        /// <summary>
        /// Sets the co brand token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public YTransactionsSearchRequest SetCoBrandToken(string token) {
            Insert(CobSessionToken, token);
            return this;
        }

        /// <summary>
        /// Sets the user session token.
        /// </summary>
        /// <param name="userToken">The user token.</param>
        /// <returns></returns>
        public YTransactionsSearchRequest SetUserSessionToken(string userToken) {
            Insert(UserSessionToken, userToken);
            return this;
        }

        /// <summary>
        /// Sets the optional date range.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <returns></returns>
        public YTransactionsSearchRequest SetOptionalDateRange(DateTime from, DateTime to) {
            Insert("transactionSearchRequest.searchFilter.postDateRange.fromDate", from.ToString("MM-dd-yyyy"));
            Insert("transactionSearchRequest.searchFilter.postDateRange.toDate", to.ToString("MM-dd-yyyy"));
            this.isDateRangeSet = true;
            return this;
        }

        /// <summary>
        /// Sets the optional string filter.
        /// </summary>
        /// <param name="filterString">The filter string.</param>
        /// <returns></returns>
        public YTransactionsSearchRequest SetOptionalStringFilter(string filterString) {
            this.filter = filterString;
            SetIgnoreUserInput(false);
            Insert("transactionSearchRequest.userInput", filterString.ToString());
            return this;
        }

        /// <summary>
        /// Sets the size of the page.
        /// </summary>
        /// <param name="startNumber">The start number.</param>
        /// <param name="endNumber">The end number.</param>
        /// <returns></returns>
        public YTransactionsSearchRequest SetPageSize(int startNumber, int endNumber) {
            Insert("transactionSearchRequest.resultRange.startNumber", startNumber.ToString());
            Insert("transactionSearchRequest.resultRange.endNumber", endNumber.ToString());
            return this;
        }

        /// <summary>
        /// Call if you want to get all container types (default is 'bank').
        /// </summary>
        /// <returns></returns>
        public YTransactionsSearchRequest SetGetAllContainerTypes() {
            Insert("transactionSearchRequest.containerType", "All");
            return this;
        }

        /// <summary>
        /// Sets the type of the container which transaction we are interested of (default is 'bank').
        /// </summary>
        /// <param name="containerType">Type of the container.</param>
        /// <returns></returns>
        public YTransactionsSearchRequest SetContainerType(EContainerType containerType) {
            Insert("transactionSearchRequest.containerType", Enum.GetName(typeof(EContainerType), containerType));
            return this;
        }

        /// <summary>
        /// Builds the request content.
        /// </summary>
        /// <returns></returns>
        internal override IEnumerable<KeyValuePair<string, string>> Build()
        {
            if (string.IsNullOrEmpty(this.filter) || this.isDateRangeSet)
            {
                SetIgnoreUserInput(true);
            }
            else
            {
                SetIgnoreUserInput(false);
            }

            return base.Build();
        }

        /// <summary>
        /// Sets the ignore user input.
        /// </summary>
        /// <param name="isIgnore">if set to <c>true</c> [is ignore].</param>
        private void SetIgnoreUserInput(bool isIgnore) {
            Insert("transactionSearchRequest.ignoreUserInput", isIgnore.ToString().ToLowerInvariant());
        }
    }
}

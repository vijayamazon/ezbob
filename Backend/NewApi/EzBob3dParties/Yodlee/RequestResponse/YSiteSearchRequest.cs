namespace EzBob3dParties.Yodlee.RequestResponse
{
    /// <summary>
    /// Request to  find some site (bank) by text search
    /// </summary>
    class YSiteSearchRequest : YRequestBase
    {
        /// <summary>
        /// Sets the co-brand session token.
        /// </summary>
        /// <param name="coBrandToken">The co brand token.</param>
        /// <returns></returns>
        public YSiteSearchRequest SetCobrandSessionToken(string coBrandToken)
        {
            Insert(CobSessionToken, coBrandToken);
            return this;
        }

        /// <summary>
        /// Sets the user session token.
        /// </summary>
        /// <param name="userToken">The user token.</param>
        /// <returns></returns>
        public YSiteSearchRequest SetUserSessionToken(string userToken) {
            Insert(UserSessionToken, userToken);
            return this;
        }

        /// <summary>
        /// Sets the search string.
        /// </summary>
        /// <param name="searchString">The search string.</param>
        /// <returns></returns>
        public YSiteSearchRequest SetSearchString(string searchString)
        {
            Insert("siteSearchString", searchString);
            return this;
        }
    }
}

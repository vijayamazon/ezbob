namespace EzBob3dParties.Yodlee.RequestResponse
{
    class YSiteAccountsRequest : YRequestBase {
        /// <summary>
        /// Sets the cobrand session token.
        /// </summary>
        /// <param name="coBrandToken">The co brand token.</param>
        /// <returns></returns>
        public YSiteAccountsRequest SetCobrandSessionToken(string coBrandToken)
        {
            Insert(CobSessionToken, coBrandToken);
            return this;
        }

        /// <summary>
        /// Sets the user session token.
        /// </summary>
        /// <param name="userToken">The user token.</param>
        /// <returns></returns>
        public YSiteAccountsRequest SetUserSessionToken(string userToken) {
            Insert(UserSessionToken, userToken);
            return this;
        }
    }
}

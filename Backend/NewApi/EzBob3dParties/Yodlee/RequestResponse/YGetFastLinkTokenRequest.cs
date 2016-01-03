namespace EzBob3dParties.Yodlee.RequestResponse {
    internal class YGetFastLinkTokenRequest : YRequestBase {
        /// <summary>
        /// Sets the co-brand session token.
        /// </summary>
        /// <param name="coBrandToken">The co brand token.</param>
        /// <returns></returns>
        public YGetFastLinkTokenRequest SetCobrandSessionToken(string coBrandToken) {
            Insert(CobSessionToken, coBrandToken);
            return this;
        }

        /// <summary>
        /// Sets the user session token.
        /// </summary>
        /// <param name="userToken">The user token.</param>
        /// <returns></returns>
        public YGetFastLinkTokenRequest SetUserSessionToken(string userToken) {
            Insert(Rsession, userToken);
            Insert(FinAppId, Fastlink2AggregationAppID);
            return this;
        }
    }
}

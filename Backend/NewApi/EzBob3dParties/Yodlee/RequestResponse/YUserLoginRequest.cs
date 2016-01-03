namespace EzBob3dParties.Yodlee.RequestResponse {
    /// <summary>
    /// The yodlee user login request
    /// </summary>
    class YUserLoginRequest : YRequestBase {
        /// <summary>
        /// Sets the co-brand session token.
        /// </summary>
        /// <param name="cobrandToken">The co-brand token.</param>
        /// <returns></returns>
        public YUserLoginRequest SetCobrandSessionToken(string cobrandToken) {
            Insert(CobSessionToken, cobrandToken);
            return this;
        }

        /// <summary>
        /// Sets the name of the user.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        public YUserLoginRequest SetUserName(string userName) {
            Insert("login", userName);
            return this;
        }

        /// <summary>
        /// Sets the user password.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public YUserLoginRequest SetUserPassword(string password) {
            Insert("password", password);
            return this;
        }
    }
}

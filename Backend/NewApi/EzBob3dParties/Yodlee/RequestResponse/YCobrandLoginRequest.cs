namespace EzBob3dParties.Yodlee.RequestResponse {
    /// <summary>
    /// The yodlee login request
    /// </summary>
    class YCobrandLoginRequest : YRequestBase {
        /// <summary>
        /// Sets the name of the co-brand user.
        /// </summary>
        /// <param name="coBrandUserName">Name of the co brand user.</param>
        /// <returns></returns>
        public YCobrandLoginRequest SetCobrandUserName(string coBrandUserName) {
            Insert("cobrandLogin", coBrandUserName);
            return this;
        }

        /// <summary>
        /// Sets the co-brand password.
        /// </summary>
        /// <param name="coBrandPassword">The co brand password.</param>
        /// <returns></returns>
        public YCobrandLoginRequest SetCobrandPassword(string coBrandPassword) {
            Insert("cobrandPassword", coBrandPassword);
            return this;
        }
    }
}

namespace YodleeLib
{
	using EzBob.Configuration;
    using config;
	using System;
    using log4net;

	/// <summary>
    /// Contains methods related to logging in and logging out
    /// a user to the Yodlee platform.
    /// </summary>
    public class LoginUser : ApplicationSuper
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(LoginUser));
        readonly LoginService loginService;
        OAuthAccessTokenManagementServiceService oAuthAccessTokenManagementService;
		private static YodleeEnvConnectionConfig _config;

        public LoginUser()
        {
			_config = YodleeConfig._Config;
            loginService = new LoginService();
            loginService.Url = _config.soapServer + "/" + loginService.GetType().FullName;
        }

        /// <summary>
        /// Logs in a registered and certified user to the Yodlee platform.
        /// </summary>
        /// <param name="loginName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public UserContext loginUser(String loginName, String password)
        {
            var passwordCredentials = new PasswordCredentials {loginName = loginName, password = password};
            UserInfo1 userInfo1;
            userInfo1 = loginService.login1(GetCobrandContext(), passwordCredentials, null, false);
            return userInfo1 == null ? null : userInfo1.userContext;
        }

        /// <summary>
        /// Logs out a user from the Yodlee platform.
        /// </summary>
        /// <param name="userContext"></param>
        public void logoutUser(UserContext userContext)
        {
            loginService.logout(userContext);
        }

        /// <summary>
        /// Displays logged in user information.
        /// </summary>
        /// <param name="userContext"></param>
        public void getUserInfo(UserContext userContext)
        {
            UserInfo1 userInfo1 = loginService.getUserInfo(userContext);
			log.InfoFormat("User Name: {0}. Login Count: {1}. Email Address: {2}", userInfo1.loginName, userInfo1.loginCount, userInfo1.emailAddress);
        }

        /// <summary>
        /// Changes the password of a user on the Yodlee platform.
        /// </summary>
        /// <param name="userContext"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="changePassword"></param>
        public void changePassword(UserContext userContext,
                                   String userName,
                                   String password,
                                   String changePassword)
        {
            var oldCredentials = new PasswordCredentials();
            oldCredentials.loginName = userName;
            oldCredentials.password = password;

            var newCredentials = new PasswordCredentials();
            newCredentials.loginName = userName;
            newCredentials.password = changePassword;
            loginService.changeCredentials(userContext,
                                           oldCredentials,
                                           newCredentials);
        }

        public OAuthAccessToken getAccessTokens(UserContext userContext)
        {
            oAuthAccessTokenManagementService = new OAuthAccessTokenManagementServiceService();
            oAuthAccessTokenManagementService.Url = _config.soapServer + "/OAuthAccessTokenManagementService_11_1";
	        long? applicationId = _config.BridgetApplicationID;

            try
            {
	            OAuthAccessToken authAccessToken = oAuthAccessTokenManagementService.getOAuthAccessToken(userContext, applicationId, true);
	            if (authAccessToken != null && authAccessToken.token != null && authAccessToken.tokenSecret != null)
                {
	                log.InfoFormat("Successfully received access token. Access Token:{0}. Access Token Secret:{1}. Token Creation Time:{2}",
		                authAccessToken.token, authAccessToken.tokenSecret, authAccessToken.tokenCreationTime);
                    return authAccessToken;
                }

				log.ErrorFormat("Received null access token. authAccessToken:{0} authAccessToken.token:{1} authAccessToken.tokenSecret:{2}", 
					authAccessToken == null ? "null" : authAccessToken.ToString(),
					authAccessToken == null || authAccessToken.token == null ? "null" : authAccessToken.token,
					authAccessToken == null || authAccessToken.tokenSecret == null ? "null" : authAccessToken.tokenSecret);
            }
            catch (Exception e)
            {
				log.ErrorFormat("Exception while getting access token. Maybe the application id is invalid. Application id:{0} Exception:{1}", applicationId, e);
            }
            return null;
        }

        /// <summary>
        /// Touches the underlying conversation credentials (session) of the user
        /// on the Yodlee platform, to get a new lease on the inactivity timeout.
        /// </summary>
        /// <param name="userContext">userContext</param>
        public void extendInactivityTimeout(UserContext userContext)
        {
            loginService.touchConversationCredentials(userContext);
        }

        /// <summary>
        /// Renews the underlying conversation credentials (session) of the user
        /// on the Yodlee platform, to get a new lease on the absolute timeout.
        /// The returned UserContext must be used and the UserContext
        /// passed in as argument discarded.
        /// </summary>
        /// <param name="userContext"></param>
        /// <returns>The new UserContext</returns>
        public UserContext getNewContext(UserContext userContext)
        {
            return loginService.renewConversation(userContext);
        }
    }
}

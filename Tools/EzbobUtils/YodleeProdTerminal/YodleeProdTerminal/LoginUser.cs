using System;
using System.Web.Services.Protocols;

namespace com.yodlee.sampleapps
{
	/// <summary>
	/// Contains methods related to logging in and logging out
	/// a user to the Yodlee platform.
	/// </summary>
	public class LoginUser : ApplicationSuper
	{
		LoginService loginService = null;
        OAuthAccessTokenManagementServiceService oAuthAccessTokenManagementService = null;
       
		public LoginUser()
		{
			loginService = new LoginService();
            loginService.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + loginService.GetType().FullName;
		}

		/// <summary>
		/// Logs in a registered and certified user to the Yodlee platform.
		/// </summary>
		/// <param name="loginName"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public UserContext loginUser(String loginName, String password)
		{
			PasswordCredentials passwordCredentials = new PasswordCredentials();
			passwordCredentials.loginName = loginName;
			passwordCredentials.password  = password;            
			UserInfo1 userInfo1 = null;			
            userInfo1 = loginService.login1(getCobrandContext(), passwordCredentials, null, false);	
            if (userInfo1 == null)
            {
                return null;
            }
            else
            {
                return userInfo1.userContext;
            }
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
			System.Console.WriteLine("\tUser Name: {0}", userInfo1.loginName);
			System.Console.WriteLine("\tLogin Count: {0}", userInfo1.loginCount);
			System.Console.WriteLine("\tEmail Address: {0}", userInfo1.emailAddress);
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
			PasswordCredentials oldCredentials = new PasswordCredentials();
			oldCredentials.loginName = userName;
			oldCredentials.password = password;

			PasswordCredentials newCredentials = new PasswordCredentials();
			newCredentials.loginName = userName;
			newCredentials.password = changePassword;
			loginService.changeCredentials(userContext,
										   oldCredentials,
										   newCredentials);
		}

        //oauth
        public void getAccessTokens(UserContext userContext) {

            oAuthAccessTokenManagementService = new OAuthAccessTokenManagementServiceService();
            //oAuthAccessTokenManagementService.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + oAuthAccessTokenManagementService.GetType().FullName + "_11_1";
            oAuthAccessTokenManagementService.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + "OAuthAccessTokenManagementService_11_1";
         	OAuthAccessToken authAccessToken = null;
            long? applicationId = 10003200;
 		 
    	 try {

             authAccessToken = oAuthAccessTokenManagementService.getOAuthAccessToken(userContext, applicationId, true);
    		 if(authAccessToken!=null && authAccessToken.token !=null && authAccessToken.tokenSecret !=null){
    				
    				String message = "Access Tokens Retrieved Successfully..\n";
    				message = message + "\n";
    				message = message + "Access Token: " + authAccessToken.token+"\n";
    				message = message + "Access Token Secret: " + authAccessToken.tokenSecret+"\n";
    				message = message + "Token Creation Time: " + authAccessToken.tokenCreationTime+"\n";
                    System.Console.WriteLine(message);
    		 }
    	  }
         catch (SoapException se)
         {
             System.Console.WriteLine("The given application id is invalid\nException:\n" + se.ToString());
         }
        }

		/// <summary>
		/// Touches the underlying conversation credentials (session) of the user
		/// on the Yodlee platform, to get a new lease on the inactivity timeout.
		/// </summary>
		/// <param name="userContext"></param
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
			return loginService.renewConversation (userContext);
		}
	}
}

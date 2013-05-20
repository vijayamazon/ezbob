using System;
using System.Web.Services.Protocols;

namespace YodleeLib
{
    using StructureMap;
    using config;
    using util;

	public class YodleeMain : ApplicationSuper
    {
        public UserContext userContext = null;
        ContentServiceTraversalService contentServieTravelService = new ContentServiceTraversalService();
        ServerVersionManagementService serverVersionManagementService = new ServerVersionManagementService();
        private static IYodleeMarketPlaceConfig _config;

        public YodleeMain()
        {
            _config = ObjectFactory.GetInstance<IYodleeMarketPlaceConfig>();
            contentServieTravelService.Url = _config.soapServer + "/" + contentServieTravelService.GetType().FullName;
            serverVersionManagementService.Url = _config.soapServer + "/" + serverVersionManagementService.GetType().FullName;
        }

        public string loginUser(string userName, string password)
        {
            //userName = "Stas";
            //password = "Ab12cD";
            LoginUser loginUser = new LoginUser();

            try
            {
                userContext = loginUser.loginUser(userName, password);
                return "User Logged in Successfully..";
            }
            catch (SoapException se)
            {
                return "User login failed -> " + se.Message;
            }
        }

		public void RegisterUser(string userName, string password, string email)
		{
			var registerUser = new RegisterUser();
			try
			{
				userContext = registerUser.DoRegisterUser(userName, password, email);
			}
			catch (SoapException se)
			{
				//System.Console.WriteLine("Exception Message -> " + se.Message);
				//if (se.Message.Equals("IllegalArgumentValueExceptionFaultMessage"))
				//{
				//	System.Console.WriteLine("\n\nGot Illegal Arguments for Registration.");
				//	System.Console.WriteLine("Please note that Yodlee enforces the following restrictions:");
				//	System.Console.WriteLine("On username:");
				//	System.Console.WriteLine("  >= 3 characters");
				//	System.Console.WriteLine("  <= 150 characters");
				//	System.Console.WriteLine("  No Whitespace");
				//	System.Console.WriteLine("  No Control Characters");
				//	System.Console.WriteLine("  Contains at least one Letter");
				//	System.Console.WriteLine("\nOn password");
				//	System.Console.WriteLine("  >= 6 characters");
				//	System.Console.WriteLine("  <= 50 characters");
				//	System.Console.WriteLine("  No Whitespace");
				//	System.Console.WriteLine("  No Control Characters");
				//	System.Console.WriteLine("  Contains at least one Number");
				//	System.Console.WriteLine("  Contains at least one Letter");
				//	System.Console.WriteLine("  Does not contain the same letter/number three or more times in a row.  (e.g. aaa123 would fail for three \"a\"'s in a row, but a1a2a3 would pass)");
				//	System.Console.WriteLine("  Does not equal username");
				//	System.Console.WriteLine("\n");
				//}
			}
			catch (Exception exc)
			{
				//System.Console.WriteLine("Exception Stack Trace -> " + exc.StackTrace);
			}
		}

	    public string GetFinalUrl(int csId, string callback, string username, string password)
	    {
			string url = "https://lawint.yodlee.com/apps/private-ezbob/addAccounts.pfmlaw.action?access_type=oauthdeeplink&displayMode=desktop&_csid=";//from config
			
			var oAuthBase = new OAuthBase();
			string timestamp = oAuthBase.GenerateTimeStamp();
			string nonce = oAuthBase.GenerateNonce();
			string consumerKey = "a458bdf184d34c0cab7ef7ffbb5f016b"; //from config
			string consumerSecret = "1ece74e1ca9e4befbb1b64daba7c4a24";//from config

			loginUser(username, password);
			var lu = new LoginUser();
			OAuthAccessToken token = lu.getAccessTokens(userContext);

			string normalizedUrl;
			string normalizedRequestParameters;
			string signatureBase = oAuthBase.GenerateSignatureBase(new Uri(string.Format("{0}{1}", url, csId)),
					consumerKey, token.token, token.tokenSecret, "GET",
					 timestamp, nonce, OAuthBase.HMACSHA1SignatureType, callback, out normalizedUrl, out normalizedRequestParameters);

			string signature = oAuthBase.GenerateSignature(signatureBase, consumerSecret, token.tokenSecret);
			//Logger.Logger.DebugFormat("sig: {0}\nsigbase: {1}\nurl:{2}\nparams: {3}\ntoken secret: {4}", signature, signatureBase, normalizedUrl, normalizedRequestParameters, token.tokenSecret);
			string finalUrl = string.Format("{0}?{1}&oauth_signature={2}",
				normalizedUrl,
				normalizedRequestParameters,
				signature);
		    return finalUrl;
	    }
		
		public long GetItemId(string username, string password)
		{
			loginUser(username, password);
			var di = new DisplayItemInfo();
			object[] oa = di.displayItemSummariesWithoutItemData(userContext); // TODO: this should be run before the redirection only for customers that have existing yodlee accounts for this csid
			
			if (oa == null || oa.Length == 0)
			{
				// No items were found for the user.
				return -1;
			}
			
			foreach (object t in oa)
			{
				var itemSummary = (ItemSummary)t;
				return itemSummary.itemId; // TODO: the normal id should be added
			}

			return -1;
		}
    }
}
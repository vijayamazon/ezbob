using System;
using System.Web.Services.Protocols;

namespace YodleeLib
{
    using StructureMap;
    using config;

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
            userName = "Stas";
            password = "Ab12cD";
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

	    public string GetFinalUrl(int csId, string callback)
	    {
			string url = "https://lawint.yodlee.com/apps/private-ezbob/addAccounts.pfmlaw.action?access_type=oauthdeeplink&displayMode=desktop&_csid=";//from config
			
			// string callbackUrl = "www.google.com";
			string normalizedUrl;
			string normalizedRequestParameters;
			OAuthBase OAuthBase = new OAuthBase();
			string timestamp = OAuthBase.GenerateTimeStamp();
			string nonce = OAuthBase.GenerateNonce();
			string consumerKey = "a458bdf184d34c0cab7ef7ffbb5f016b"; //from config
			string consumerSecret = "1ece74e1ca9e4befbb1b64daba7c4a24";//from config
			YodleeMain yodlee = new YodleeMain();
			yodlee.loginUser("a","a");//todo:create and store
			LoginUser lu = new LoginUser();
			OAuthAccessToken token = lu.getAccessTokens(yodlee.userContext);
			string signatureBase = OAuthBase.GenerateSignatureBase(new Uri(string.Format("{0}{1}", url, csId)),
					consumerKey, token.token, token.tokenSecret, "GET",
					 timestamp, nonce, OAuthBase.HMACSHA1SignatureType, callback, out normalizedUrl, out normalizedRequestParameters);

			string signature = OAuthBase.GenerateSignature(signatureBase, consumerSecret, token.tokenSecret);
			//Logger.Logger.DebugFormat("sig: {0}\nsigbase: {1}\nurl:{2}\nparams: {3}\ntoken secret: {4}", signature, signatureBase, normalizedUrl, normalizedRequestParameters, token.tokenSecret);
			string finalUrl = string.Format("{0}?{1}&oauth_signature={2}",
				normalizedUrl,
				normalizedRequestParameters,
				signature);
		    return finalUrl;
	    }
    }
}
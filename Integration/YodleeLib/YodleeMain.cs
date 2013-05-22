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
			//userName = "Stas";
			//password = "Ab12cD";
			var loginUser = new LoginUser();

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
				
			}
			catch (Exception exc)
			{
				
			}
		}

		public string GetFinalUrl(int csId, string callback, string username, string password)
		{
			string url = _config.AddAccountURL;
			const string sParams = "?access_type=oauthdeeplink&displayMode=desktop&_csid=";
			var oAuthBase = new OAuthBase();
			string timestamp = oAuthBase.GenerateTimeStamp();
			string nonce = oAuthBase.GenerateNonce();
			string applicationKey = _config.ApplicationKey;
			string applicationToken = _config.ApplicationToken;

			loginUser(username, password);
			var lu = new LoginUser();
			OAuthAccessToken token = lu.getAccessTokens(userContext);

			string normalizedUrl;
			string normalizedRequestParameters;
			string signatureBase = oAuthBase.GenerateSignatureBase(
				new Uri(string.Format("{0}{1}{2}", url, sParams, csId)),
				applicationKey,
				token.token,
				token.tokenSecret,
				"GET",
				timestamp,
				nonce,
				OAuthBase.HMACSHA1SignatureType,
				callback,
				out normalizedUrl,
				out normalizedRequestParameters
			);

			string signature = oAuthBase.GenerateSignature(signatureBase, applicationToken, token.tokenSecret);
			string finalUrl = string.Format("{0}?{1}&oauth_signature={2}",
				normalizedUrl,
				normalizedRequestParameters,
				signature
			);

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

			var itemSummary = (ItemSummary)oa[oa.Length - 1];

			if (itemSummary.refreshInfo.statusCode != 0)
			{
				return -1;
			}


			return itemSummary.itemId;
		}
	}
}
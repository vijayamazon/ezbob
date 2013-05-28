using System;
using System.Web.Services.Protocols;

namespace YodleeLib
{
	using StructureMap;
	using config;

	public class YodleeMain : ApplicationSuper
	{
		public UserContext UserContext = null;
		readonly ContentServiceTraversalService contentServieTravelService = new ContentServiceTraversalService();
		readonly ServerVersionManagementService serverVersionManagementService = new ServerVersionManagementService();
		private static IYodleeMarketPlaceConfig config;

		public YodleeMain()
		{
			config = ObjectFactory.GetInstance<IYodleeMarketPlaceConfig>();
			contentServieTravelService.Url = config.soapServer + "/" + contentServieTravelService.GetType().FullName;
			serverVersionManagementService.Url = config.soapServer + "/" + serverVersionManagementService.GetType().FullName;
		}

		public string LoginUser(string userName, string password)
		{
			var loginUser = new LoginUser();

			try
			{
				UserContext = loginUser.loginUser(userName, password);
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
				UserContext = registerUser.DoRegisterUser(userName, password, email);
			}
			catch (SoapException)
			{
				
			}
			catch (Exception)
			{
				
			}
		}

		public string GetFinalUrl(int csId, string callback, string username, string password)
		{
			string url = config.AddAccountURL;
			const string sParams = "?access_type=oauthdeeplink&displayMode=desktop&_csid=";
			var oAuthBase = new OAuthBase();
			string timestamp = oAuthBase.GenerateTimeStamp();
			string nonce = oAuthBase.GenerateNonce();
			string applicationKey = config.ApplicationKey;
			string applicationToken = config.ApplicationToken;

			LoginUser(username, password);
			var lu = new LoginUser();
			OAuthAccessToken token = lu.getAccessTokens(UserContext);

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
			LoginUser(username, password);
			var di = new DisplayItemInfo();
			object[] oa = di.displayItemSummariesWithoutItemData(UserContext);

			if (oa == null || oa.Length == 0)
			{
				// No items were found for the user.
				return -1;
			}

			var itemSummary = (ItemSummary)oa[oa.Length - 1];

			if (itemSummary.refreshInfo.statusCode != 0)
			{
				//RemoveItem(itemSummary.itemId); // TODO: we should delete for credentials associated failures but not all of them
				return -1;
			}

			return itemSummary.itemId;
		}

		private void RemoveItem(long itemId)
		{
			try
			{
				var itemManagement = new ItemManagementService();
				itemManagement.Url = config.soapServer + "/" + "ItemManagementService";
				itemManagement.removeItem(UserContext, itemId, true);
			}
			catch (Exception)
			{
			}
		}
	}
}
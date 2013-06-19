namespace YodleeLib
{
	using System;
	using StructureMap;
	using config;
	using log4net;

	public class YodleeMain : ApplicationSuper
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(YodleeMain));
		public UserContext UserContext = null;
		private readonly ContentServiceTraversalService contentServieTravelService = new ContentServiceTraversalService();
		private readonly ServerVersionManagementService serverVersionManagementService = new ServerVersionManagementService();
		private static IYodleeMarketPlaceConfig config;
		private bool isLoggedIn;
		
		public YodleeMain()
		{
			config = ObjectFactory.GetInstance<IYodleeMarketPlaceConfig>();
			contentServieTravelService.Url = config.soapServer + "/" + contentServieTravelService.GetType().FullName;
			serverVersionManagementService.Url = config.soapServer + "/" + serverVersionManagementService.GetType().FullName;
		}

		public void LoginUser(string userName, string password)
		{
			try
			{
				var loginUser = new LoginUser();
				UserContext = loginUser.loginUser(userName, password);
				isLoggedIn = true;
				Log.InfoFormat("Yodlee user '{0}' logged in successfully", userName);
			}
			catch (Exception e)
			{
				Log.WarnFormat("Yodlee user '{0}' login failed: {1} ", userName, e.Message);
			}
		}

		public void RegisterUser(string userName, string password, string email)
		{
			var registerUser = new RegisterUser();
			try
			{
				UserContext = registerUser.DoRegisterUser(userName, password, email);
				isLoggedIn = true;
				Log.InfoFormat("Yodlee user '{0}' registered successfully", userName);
			}
			catch (Exception e)
			{
				Log.WarnFormat("Yodlee user '{0}' registration failed: {1}", userName, e.Message);
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

			if (!isLoggedIn)
			{
				LoginUser(username, password);
			}
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

		public long GetItemId(string username, string password, out string displayname, out long csId)
		{
			displayname = string.Empty;
			csId = -1;
			LoginUser(username, password);
			var di = new DisplayItemInfo();

			object[] oa;
			try
			{
				oa = di.displayItemSummariesWithoutItemData(UserContext);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Failure getting yodlee item. Error:{0}", e);
				return -1;
			}

			if (oa == null || oa.Length == 0)
			{
				// No items were found for the user.
				Log.InfoFormat("No items were found for the user '{0}'", username);
				return -1;
			}

			var itemSummary = (ItemSummary)oa[oa.Length - 1];

			if (itemSummary.refreshInfo.statusCode != 0)
			{
				Log.WarnFormat("Item status code is not '0' but '{0}' for user {1}", itemSummary.refreshInfo.statusCode, username);
				//RemoveItem(itemSummary.itemId); // TODO: we should delete for credentials associated failures but not all of them
				return -1;
			}

			csId = itemSummary.contentServiceId;
			displayname = itemSummary.itemDisplayName;
			
			Log.InfoFormat("Received yodlee item id: '{0}'", itemSummary.itemId);

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

		public string GenerateRandomPassword()
		{
			return YodleePasswrodGenerator.GenerateRandomPassword();
		}
	}
}
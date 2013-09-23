namespace YodleeLib
{
	using System;
	using System.Threading;
	using EzBob.Configuration;
	using config;
	using log4net;

	public class YodleeMain : ApplicationSuper
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(YodleeMain));
		public UserContext UserContext = null;
		private readonly ContentServiceTraversalService contentServiceTravelService = new ContentServiceTraversalService();
		private readonly ServerVersionManagementService serverVersionManagementService = new ServerVersionManagementService();
		private static YodleeEnvConnectionConfig config;
		private bool isLoggedIn;
		private int numOfRetriesForGetItemSummary;

		public YodleeMain()
		{
			config = YodleeConfig._Config;
			contentServiceTravelService.Url = config.soapServer + "/" + contentServiceTravelService.GetType().FullName;
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

		public string GetAddAccountUrl(int csId, string callback, string username, string password)
		{
			try
			{
				string url = config.AddAccountURL;
				const string sParams = "?access_type=oauthdeeplink&displayMode=desktop&_csid=";
				var oAuthBase = new OAuthBase();
				if (!isLoggedIn)
				{
					LoginUser(username, password);
				}
				var lu = new LoginUser();
				OAuthAccessToken token = lu.getAccessTokens(UserContext);

				string signature = null;
				string normalizedUrl = null;
				string normalizedRequestParameters = null;
				while (string.IsNullOrEmpty(signature) || signature.Contains("+"))
				{
					string timestamp = oAuthBase.GenerateTimeStamp();
					string nonce = oAuthBase.GenerateNonce();
					string applicationKey = config.ApplicationKey;
					string applicationToken = config.ApplicationToken;

					string signatureBase = oAuthBase.GenerateSignatureBase(
						new Uri(string.Format("{0}{1}{2}", url, sParams, csId)),
						applicationKey,
						token.token,
						token.tokenSecret,
						"GET",
						timestamp,
						nonce,
						OAuthBase.HMACSHA1SignatureType,
						OAuthBase.UrlEncode(callback),
						out normalizedUrl,
						out normalizedRequestParameters
						);

					signature = oAuthBase.GenerateSignature(signatureBase, applicationToken, token.tokenSecret);
				}

				string finalUrl = string.Format("{0}?{1}&oauth_signature={2}",
					normalizedUrl,
					normalizedRequestParameters,
					signature
				);

				return finalUrl;
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Exception while generating yodlee redirection url:{0}", e);
				throw;
			}
		}

		public string GetEditAccountUrl(long itemId, string callback, string username, string password)
		{
			try
			{
				string url = config.EditAccountURL;
				const string sParams = "?access_type=oauthdeeplink&displayMode=desktop&_flowId=editSiteCredentials&_itemid=";
				var oAuthBase = new OAuthBase();

				LoginUser(username, password);
				var lu = new LoginUser();
				OAuthAccessToken token = lu.getAccessTokens(UserContext);

				string signature = null;
				string normalizedUrl = null;
				string normalizedRequestParameters = null;
				string applicationKey = config.ApplicationKey;
				string applicationToken = config.ApplicationToken;

				while (string.IsNullOrEmpty(signature) || signature.Contains("+"))
				{
					string timestamp = oAuthBase.GenerateTimeStamp();
					string nonce = oAuthBase.GenerateNonce();
					string signatureBase = oAuthBase.GenerateSignatureBase(
						new Uri(string.Format("{0}{1}{2}", url, sParams, itemId)),
						applicationKey,
						token.token,
						token.tokenSecret,
						"GET",
						timestamp,
						nonce,
						OAuthBase.HMACSHA1SignatureType,
						OAuthBase.UrlEncode(callback),
						out normalizedUrl,
						out normalizedRequestParameters
						);

					signature = oAuthBase.GenerateSignature(signatureBase, applicationToken, token.tokenSecret);
				}

				string finalUrl = string.Format("{0}?{1}&oauth_signature={2}",
					normalizedUrl,
					normalizedRequestParameters,
					signature
				);

				return finalUrl;
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Exception while generating yodlee redirection url:{0}", e);
				throw;
			}
		}

		public long GetItemId(string username, string password, out string displayname, out long csId)
		{
			displayname = string.Empty;
			csId = -1;
			LoginUser(username, password);

			numOfRetriesForGetItemSummary = 5;
			ItemSummary itemSummary = GetItemSummary(username);

			if (itemSummary == null)
			{
				return -1;
			}

			csId = itemSummary.contentServiceId;
			displayname = itemSummary.itemDisplayName;

			Log.InfoFormat("Received yodlee item id: '{0}'", itemSummary.itemId);

			return itemSummary.itemId;
		}

		private ItemSummary GetItemSummary(string username)
		{
			var di = new DisplayItemInfo();
			object[] oa;
			try
			{
				oa = di.displayItemSummariesWithoutItemData(UserContext);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Failure getting yodlee item. Error:{0}", e);
				return null;
			}

			if (oa == null || oa.Length == 0)
			{
				// No items were found for the user.
				Log.InfoFormat("No items were found for the user '{0}'", username);
				return null;
			}

			var itemSummary = (ItemSummary)oa[0];

			if (itemSummary.refreshInfo.statusCode != 0)
			{
				Log.WarnFormat("Item status code is not '0' but '{0}' for user {1}", itemSummary.refreshInfo.statusCode, username);
				numOfRetriesForGetItemSummary--;
				if (numOfRetriesForGetItemSummary > 0)
				{
					Log.InfoFormat("Will retry {0} more time\\s", numOfRetriesForGetItemSummary);
					Thread.Sleep(5000);
					return GetItemSummary(username);
				}
				return null;
			}

			return itemSummary;
		}

		public bool RefreshNotMFAItem(long itemId, bool forceRefresh = false)
		{
			if (IsMFA(itemId))
			{
				return false;
			}

			var refreshItem = new RefreshNotMFAItem();
			refreshItem.RefreshItem(UserContext, itemId, forceRefresh);

			// Poll for the refresh status and display the item summary if refresh succeeds
			return refreshItem.PollRefreshStatus(UserContext, itemId);
		}


		public void RemoveItem(long itemId)
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
			return YodleePasswordGenerator.GenerateRandomPassword();
		}

		public bool IsMFA(long itemId)
		{
			var dataService = new DataServiceService { Url = config.soapServer + "/" + "DataService" };
			ItemSummary itemSummary = dataService.getItemSummaryForItem(UserContext, itemId, true);
			if (itemSummary == null)
			{
				throw new Exception("The item does not exist");
			}

			var mfaType = itemSummary.contentServiceInfo.mfaType;
			if (mfaType != null && mfaType.HasValue) //mfaType.typeId > 0
			{
				return true;
			}

			return false;
		}
	}
}
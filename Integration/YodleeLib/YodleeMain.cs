namespace YodleeLib
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using ConfigManager;
	using log4net;

	public class YodleeMain : ApplicationSuper
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(YodleeMain));
		public UserContext UserContext = null;
		private int numOfRetriesForGetItemSummary;

		public LoginUser LoginUser(string userName, string password)
		{
			try
			{
				var loginUser = new LoginUser();
				UserContext = loginUser.loginUser(userName, password);
				Log.InfoFormat("Yodlee user '{0}' logged in successfully", userName);
				return loginUser;
			}
			catch (Exception e)
			{
				Log.WarnFormat("Yodlee user '{0}' pass {1} login failed: {2} ", userName, password, e);
			}
			return null;
		}

		public bool RegisterUser(string userName, string password, string email)
		{
			var isSuccess = false;
			var registerUser = new RegisterUser();
			try
			{
				UserContext = registerUser.DoRegisterUser(userName, password, email);
				if (UserContext != null && UserContext.valid)
				{
					Log.InfoFormat("Yodlee user '{0}' registered successfully", userName);
					isSuccess = true;
				}
				else
				{
					registerUser.UnregisterUser(UserContext);
					Log.WarnFormat("Yodlee user '{0}' registered unsuccessfully, UserContext is: {1}", userName, UserContext == null ? "null" : "invalid");
				}
			}
			catch (Exception e)
			{
				Log.WarnFormat("Yodlee user '{0}' registration failed: {1}", userName, e.Message);
			}

			return isSuccess;
		}

		public string GetAddAccountUrl(int csId, string callback, string username, string password)
		{
			try
			{
				string url = CurrentValues.Instance.YodleeAddAccountURL;
				const string sParams = "?access_type=oauthdeeplink&displayMode=desktop&_csid=";
				var oAuthBase = new OAuthBase();
				OAuthAccessToken token = null;
				var lu = new LoginUser();
				UserContext = lu.loginUser(username, password);
				token = lu.getAccessTokens(UserContext);

				string signature = null;
				string normalizedUrl = null;
				string normalizedRequestParameters = null;
				while (string.IsNullOrEmpty(signature) || signature.Contains("+"))
				{
					string timestamp = oAuthBase.GenerateTimeStamp();
					string nonce = oAuthBase.GenerateNonce();
					string applicationKey = CurrentValues.Instance.YodleeApplicationKey;
					string applicationToken = CurrentValues.Instance.YodleeApplicationToken;

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

				Log.InfoFormat("generated add url: {0}", finalUrl);

				lu.logoutUser(UserContext);
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
				string url = CurrentValues.Instance.YodleeEditAccountURL;
				const string sParams = "?access_type=oauthdeeplink&displayMode=desktop&_flowId=editSiteCredentials&_itemid=";
				var oAuthBase = new OAuthBase();
				OAuthAccessToken token = null;
				var lu = new LoginUser();
				UserContext = lu.loginUser(username, password);
				token = lu.getAccessTokens(UserContext);

				string signature = null;
				string normalizedUrl = null;
				string normalizedRequestParameters = null;
				string applicationKey = CurrentValues.Instance.YodleeApplicationKey;
				string applicationToken = CurrentValues.Instance.YodleeApplicationToken;

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
				Log.InfoFormat("generated update url: {0}", finalUrl);
				lu.logoutUser(UserContext);
				return finalUrl;
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Exception while generating yodlee redirection url:{0}", e);
				throw;
			}
		}

		public long GetItemId(string username, string password, List<long> customerItems, out string displayname, out long csId)
		{
			displayname = string.Empty;
			csId = -1;
			var lu = new LoginUser();
			UserContext = lu.loginUser(username, password);

			numOfRetriesForGetItemSummary = 5;
			ItemSummary itemSummary = GetItemSummary(username, customerItems);

			if (itemSummary == null)
			{
				return -1;
			}

			csId = itemSummary.contentServiceId;
			displayname = itemSummary.itemDisplayName;

			Log.InfoFormat("Received yodlee item id: '{0}' display name: {1}", itemSummary.itemId, displayname);

			lu.logoutUser(UserContext);

			return itemSummary.itemId;
		}

		private ItemSummary GetItemSummary(string username, List<long> customerItems)
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

			foreach (ItemSummary item in oa)
			{
				if(customerItems != null && customerItems.Contains(item.itemId)){ continue; }
				Log.DebugFormat("item {0} {1} added {2} status {3}", item.itemId, item.itemDisplayName, item.refreshInfo.itemCreateDate, item.refreshInfo.statusCode);
				if (item.refreshInfo.statusCode == 801)
				{
					Log.WarnFormat("Item status code is not '0' but '{0}' for user {1}", item.refreshInfo.statusCode, username);
					numOfRetriesForGetItemSummary--;
					if (numOfRetriesForGetItemSummary > 0)
					{
						Log.InfoFormat("Will retry {0} more time\\s", numOfRetriesForGetItemSummary);
						Thread.Sleep(5000);
						return GetItemSummary(username, customerItems);
					}

					if (item.refreshInfo.statusCode == 801) //REFRESH_NEVER_DONE
					{
						return item;
					}
				}
				else if (item.refreshInfo.statusCode == 0)//STATUS_OK
				{
					return item;
				}
				else
				{
					Log.WarnFormat("Item status code is not '0' but '{0}' for user {1}", item.refreshInfo.statusCode, username);
				}
			}
			return null;
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
				string soapServer = CurrentValues.Instance.YodleeSoapServer;
				var itemManagement = new ItemManagementService { Url = soapServer + "/" + "ItemManagementService" };
				itemManagement.removeItem(UserContext, itemId, true);
			}
			catch (Exception ex)
			{
				Log.Error(ex);
			}
		}

		public string GenerateRandomPassword()
		{
			return YodleePasswordGenerator.GenerateRandomPassword();
		}

		public bool IsMFA(long itemId)
		{
			string soapServer = CurrentValues.Instance.YodleeSoapServer;
			var dataService = new DataServiceService { Url = soapServer + "/" + "DataService" };
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

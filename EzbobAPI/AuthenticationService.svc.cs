namespace EzbobAPI {
	using System;
	using System.Collections;
	using System.Collections.Specialized;
	using System.IO;
	using System.Net;
	using System.Security.Cryptography;
	using System.ServiceModel.Activation;
	using System.ServiceModel.Web;
	using System.Text;
	using System.Web;
	using System.Web.Security;

	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	public class AuthenticationService : IAuthenticationService {

		public string GetSampleMethod_With_OAuth(string userName) {
			Console.WriteLine(WebOperationContext.Current);
			if (Authenticate(WebOperationContext.Current.IncomingRequest)) {
				StringBuilder strReturnValue = new StringBuilder();
				// return username prefixed as shown below
				strReturnValue.Append(string.Format("You have entered userName as {0}", userName));
				return strReturnValue.ToString();
			} 

			WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Unauthorized;
			return "Unauthorized Request";
		}

		public Stream DoWork() {
			Console.WriteLine(WebOperationContext.Current.IncomingRequest.Headers);
			string authToken = WebOperationContext.Current.IncomingRequest.Headers["Authorization"];
			WebOperationContext.Current.OutgoingResponse.ContentType = "text/plain";
			if (string.IsNullOrEmpty(authToken)) {
				return this.ReturnForbidden();
			}
			// Check cache to see if the auth token exists.
			foreach (DictionaryEntry cacheEntry in HttpContext.Current.Cache) {
				AuthTokenData authTokenData = cacheEntry.Value as AuthTokenData;
				if (authTokenData == null || authTokenData.Expires < DateTime.Now) {
					return this.ReturnForbidden();
				}
				if (authTokenData.AuthToken == authToken) {
					// Return the associated username.
					string username = (string)cacheEntry.Key;
					return new MemoryStream(Encoding.UTF8.GetBytes("You have access to the serivce. Your username is: " + username));
				}
			}
			return this.ReturnForbidden();
		}

		private Stream ReturnForbidden() {
			WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Forbidden;
			return new MemoryStream(Encoding.UTF8.GetBytes("You're not authorized to invoke the service"));
		}

		private static bool Authenticate(IncomingWebRequestContext context) {
			bool Authenticated = false;
			string normalizedUrl;
			string normalizedRequestParameters;
			//context.Headers
			NameValueCollection pa = context.UriTemplateMatch.QueryParameters;
			if (pa != null && pa["oauth_consumer_key"] != null) {
				// to get uri without oauth parameters
				string uri = context.UriTemplateMatch.RequestUri.OriginalString.Replace(context.UriTemplateMatch.RequestUri.Query, "");
				string consumersecret = "suryabhai";
				OAuthBase oauth = new OAuthBase();
				string hash = oauth.GenerateSignature(
					new Uri(uri),
					pa["oauth_consumer_key"],
					consumersecret,
					null, // token
					null, //token secret
					"GET",
					pa["oauth_timestamp"],
					pa["oauth_nonce"],
					out normalizedUrl,
					out normalizedRequestParameters
					);
				Authenticated = pa["oauth_signature"] == hash;
			}
			return Authenticated;
		}

		public Stream Signin(string username, string password) {
			Console.WriteLine("============> {0}, {1}", username, password);
			WebOperationContext.Current.OutgoingResponse.ContentType = "text/plain";
			if (Membership.ValidateUser(username, password)) {
				// Create a hashed auth token based on username and the current time.
				string key = username + DateTime.Now.Ticks.ToString();
				SHA256 sha = SHA256.Create();
				string authToken = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(key)));
				// Store the auth token in cache, and associate it with the username. This simple sample uses ASP.NET cache. You may want to use AppFabric cache for better scaling.
				HttpContext.Current.Cache[username] = new AuthTokenData() { AuthToken = authToken, Expires = DateTime.Now.AddMinutes(20) };
				WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
				// return the auth token.
				return new MemoryStream(Encoding.UTF8.GetBytes(authToken));
			}
			WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Forbidden;
			return new MemoryStream(Encoding.UTF8.GetBytes("You're not authenticated."));
		}

		public Stream Login(string username, string password) {
		//	Console.WriteLine("============> {0}, {1}", username, password);
			if (WebOperationContext.Current != null) {
				WebOperationContext.Current.OutgoingResponse.ContentType = "text/plain";
				if (Membership.ValidateUser(username, password)) {
					// Create a hashed auth token based on username and the current time.
					string key = username + DateTime.Now.Ticks.ToString();
					SHA256 sha = SHA256.Create();
					string authToken = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(key)));
					// Store the auth token in cache, and associate it with the username. This simple sample uses ASP.NET cache. You may want to use AppFabric cache for better scaling.
					HttpContext.Current.Cache[username] = new AuthTokenData() { AuthToken = authToken, Expires = DateTime.Now.AddMinutes(20) };
					WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
					// return the auth token.
					return new MemoryStream(Encoding.UTF8.GetBytes(authToken));
				}
				WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Forbidden;
			}
			return new MemoryStream(Encoding.UTF8.GetBytes("You're not authenticated."));
		}
	}
}
namespace Ezbob.API.AuthenticationAPI.Providers {
	using System.Data.Entity;
	using System.Diagnostics;
	using System.Security.Claims;
	using System.Threading.Tasks;
	using Ezbob.API.AuthenticationAPI.Entities;
	using Ezbob.API.AuthenticationAPI.Models;
	using Microsoft.AspNet.Identity;
	using Microsoft.AspNet.Identity.EntityFramework;
	using Microsoft.AspNet.Identity.Owin;
	using Microsoft.Owin.Security.OAuth;
	using Newtonsoft.Json;

	public class MyOAuthAuthorizationServerProvider : OAuthAuthorizationServerProvider {
		public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context) {
			string cId = string.Empty;
			string cSecret = string.Empty;
			Client client = null;

			if (!context.TryGetBasicCredentials(out cId, out cSecret)) {
				context.TryGetFormCredentials(out cId, out cSecret);
			}

			//	Trace.WriteLine(string.Format("1------------ValidateClientAuthentication----client {0} secret {1} , context: client {2} ---------", cId, cSecret, context.ClientId));

			if (context.ClientId == null) {
				context.Validated();
				context.SetError("invalid_clientId", "ClientId should be sent.");
				//String.Format(==context.ClientId:{0}, clientId: {1}, clientSecret: {2}", context.ClientId, clientId, clientSecret));
				return Task.FromResult<object>(null);
			}

			using (AuthRepository _repo = new AuthRepository()) {
				client = _repo.FindClient(context.ClientId);
				//			Trace.WriteLine(string.Format("2------------ValidateClientAuthentication----client found: Name:{0}, Id: {1}, Secret: {2}, ApplicationType: {3}, cSecret: {4}", client.Name, client.Id, client.Secret, client.ApplicationType, cSecret));
			}

			if (client == null) {
				context.SetError("invalid_clientId", string.Format("Client '{0}' is not registered in the system.", context.ClientId));
				return Task.FromResult<object>(null);
			}

			//	Trace.WriteLine(string.Format("3------------ValidateClientAuthentication----client found: Name:{0}, Id: {1}, Secret: {2}, ApplicationType: {3}, hashed sercet: {4}", client.Name, client.Id, client.Secret, client.ApplicationType, Helper.GetHash(client.Secret)));

			if (client.ApplicationType == ApplicationTypes.NativeConfidential) {
				if (string.IsNullOrWhiteSpace(cSecret)) {
					context.SetError("invalid_clientId", "Client secret should be sent.");
					return Task.FromResult<object>(null);
				}
				if (client.Secret != Helper.GetHash(cSecret)) {
					context.SetError("invalid_clientId", "Client secret is invalid.");
					return Task.FromResult<object>(null);
				}
			}

			if (!client.Active) {
				context.SetError("invalid_clientId", "Client is inactive.");
				return Task.FromResult<object>(null);
			}

			context.OwinContext.Set<string>("as:clientAllowedOrigin", client.AllowedOrigin);
			context.OwinContext.Set<string>("as:clientRefreshTokenLifeTime", client.RefreshTokenLifeTime.ToString());

			context.Validated();
			return Task.FromResult<object>(null);
		}

		public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context) {
			Client client = context.OwinContext.Get<Client>("oauth:client");

			//if (client.AllowedGrant == OAuthGrant.ResourceOwner){

			// Client flow matches the requested flow. Continue...
			UserManager<IdentityUser> userManager = context.OwinContext.GetUserManager<UserManager<IdentityUser>>();

			IdentityUser user;
			try {
				user = await userManager.FindAsync(context.UserName, context.Password);
			} catch {
				// Could not retrieve the user.
				context.SetError("server_error");
				context.Rejected();

				// Return here so that we don't process further. Not ideal but needed to be done here.
				return;
			}

			if (user != null) {
				try {
					// User is found. Signal this by calling context.Validated
					ClaimsIdentity identity = await userManager.CreateIdentityAsync(
						user,
						DefaultAuthenticationTypes.ExternalBearer);

					context.Validated(identity);

	Trace.WriteLine(
		string.Format("2------------GrantResourceOwnerCredentials---identity {0}",JsonConvert.SerializeObject(identity, Formatting.None, new JsonSerializerSettings {Formatting = Newtonsoft.Json.Formatting.None,ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
				})));


				} catch {
					// The ClaimsIdentity could not be created by the UserManager.
					context.SetError("server_error");
					context.Rejected();
				}
			}
			//	else
			//	{
			//		// The resource owner credentials are invalid or resource owner does not exist.
			//		context.SetError(
			//			"access_denied", 
			//			"The resource owner credentials are invalid or resource owner does not exist.");

			//		context.Rejected();
			//	}
			//}
			//else
			//{
			//	// Client is not allowed for the 'Resource Owner Password Credentials Grant'.
			//	context.SetError(
			//		"invalid_grant", 
			//		"Client is not allowed for the 'Resource Owner Password Credentials Grant'");

			//	context.Rejected();
		}
	}
}
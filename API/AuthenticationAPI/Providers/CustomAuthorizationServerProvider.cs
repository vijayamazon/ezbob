namespace Ezbob.API.AuthenticationAPI.Providers {
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Security.Claims;
	using System.Threading.Tasks;
	using Ezbob.API.AuthenticationAPI.Entities;
	using Ezbob.API.AuthenticationAPI.Models;
	using Microsoft.AspNet.Identity.EntityFramework;
	using Microsoft.Owin.Security;
	using Microsoft.Owin.Security.OAuth;
	using Newtonsoft.Json;

	public class CustomAuthorizationServerProvider : OAuthAuthorizationServerProvider {
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
				context.SetError("invalid_clientId","ClientId should be sent."); 
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



		/*public override async Task bkp_GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context) {

			var allowedOrigin = context.OwinContext.Get<string>("as:clientAllowedOrigin");

			if (allowedOrigin == null)
				allowedOrigin = "*";

			context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { allowedOrigin });

			IdentityUser user;

			using (AuthRepository _repo = new AuthRepository()) {

				user = await _repo.FindUser(context.UserName, context.Password);

				if (user == null) {
					context.SetError("invalid_grant", "The user name or password is incorrect.");
					return;
				}

				Trace.WriteLine(string.Format("GrantResourceOwnerCredentials--- finduser {0}", JsonConvert.SerializeObject(user, Helper.JsonReferenceLoopHandling())));
			}

			// Create or retrieve a ClaimsIdentity to represent the Authenticated user:
			ClaimsIdentity identity = new ClaimsIdentity(context.Options.AuthenticationType);
			identity.AddClaim(new Claim("user_name", context.UserName));

			// Add a Role Claim:
			identity.AddClaim(new Claim(ClaimTypes.Role, "PartnerAlibaba"));

			// Identity info will ultimatly be encoded into an Access Token as a result of this call:
			context.Validated(identity);
		}*/


		public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context) {

				var allowedOrigin = context.OwinContext.Get<string>("as:clientAllowedOrigin");

		//		Trace.WriteLine(string.Format("1------------GrantResourceOwnerCredentials----context: Password {0}, UserName: {1} ", context.Password, context.UserName));

				if (allowedOrigin == null)
					allowedOrigin = "*";

				context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { allowedOrigin });

				IdentityUser user;
				IList<string> roles;
		
				using (AuthRepository _repo = new AuthRepository()) {

					user = await _repo.FindUser(context.UserName, context.Password);

					Trace.WriteLine(string.Format("------------GrantResourceOwnerCredentials--- finduser {0}", JsonConvert.SerializeObject(user, Helper.JsonReferenceLoopHandling())));

					if (user == null) {
						context.SetError("invalid_grant", "The user name or password is incorrect.");
						return;
					}

					roles = _repo.GetRoles(user.Id);
				}

				var identity = new ClaimsIdentity(context.Options.AuthenticationType);
				identity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));
				identity.AddClaim(new Claim(ClaimTypes.Role, "user"));
				identity.AddClaim(new Claim("sub", context.UserName));
				identity.AddClaim(new Claim("user_name", context.UserName));
			
				var props = new AuthenticationProperties(new Dictionary<string, string>{
						{ "as:client_id", (context.ClientId == null) ? string.Empty : context.ClientId	},
						{ "userName", context.UserName	}
					});

				var ticket = new AuthenticationTicket(identity, props);

				context.Validated(ticket);

				Trace.WriteLine(string.Format("2------------GrantResourceOwnerCredentials----ticket {0}", JsonConvert.SerializeObject(ticket.Identity, Helper.JsonReferenceLoopHandling())));
			}


		public override Task GrantRefreshToken(OAuthGrantRefreshTokenContext context) {
			var originalClient = context.Ticket.Properties.Dictionary["as:client_id"];
			var currentClient = context.ClientId;

			if (originalClient != currentClient) {
				context.SetError("invalid_clientId", "Refresh token is issued to a different clientId.");
				return Task.FromResult<object>(null);
			}

			// Change auth ticket for refresh token requests
			var newIdentity = new ClaimsIdentity(context.Ticket.Identity);

			var newClaim = newIdentity.Claims.Where(c => c.Type == "newClaim").FirstOrDefault();

			if (newClaim != null) {
				newIdentity.RemoveClaim(newClaim);
			}
			newIdentity.AddClaim(new Claim("newClaim", "newValue"));

			var newTicket = new AuthenticationTicket(newIdentity, context.Ticket.Properties);
			context.Validated(newTicket);

		//	Trace.WriteLine(string.Format("1------------GrantRefreshToken----newTicket {0}",JsonConvert.SerializeObject(newTicket.Identity, Helper.JsonReferenceLoopHandling());
	
			return Task.FromResult<object>(null);
		}

		public override Task TokenEndpoint(OAuthTokenEndpointContext context) {
			foreach (KeyValuePair<string, string> property in context.Properties.Dictionary) {
				context.AdditionalResponseParameters.Add(property.Key, property.Value);
			}

			return Task.FromResult<object>(null);
		}

	}
}
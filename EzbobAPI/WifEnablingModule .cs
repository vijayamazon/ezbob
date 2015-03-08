namespace EzbobAPI {
	using System;
	using System.IO;
	using System.Web;
	using System.Web.Http;
	using System.Xml;
	using EzbobAPI.Extentions;
	using Microsoft.Web.Services3.Security.Tokens;

	public class WifEnablingModule : IHttpModule {
		public void Init(HttpApplication context) {
			context.AuthenticateRequest += OnAuthenticateRequest;
			context.EndRequest += OnEndRequest;
		}

		private void OnEndRequest(object sender, EventArgs e) {
			//Console.WriteLine("onendrequest");
		}

		private void OnAuthenticateRequest(object sender, EventArgs e) {
			HttpContext context = HttpContext.Current;

			if (context.Request.ContainsAuthorizationHeader()) {
				HttpError error;
				bool valid = AuthenticateRequest(context.Request);

				if (!valid)
					context.SendUnauthorizedResponse();
			} else
				context.SendUnauthorizedResponse();
		}

		private bool AuthenticateRequest(HttpRequest request) {
			string accessToken = request.ExtractSamlTokenFromAuthorizationHeader();

			if (string.IsNullOrWhiteSpace(accessToken))
				return false;
			SecurityToken token;

			using (var stringReader = new StringReader(accessToken)) {
				var reader = XmlReader.Create(stringReader);

				//if (!ServiceConfiguration.SecurityTokenHandlers.CanReadToken(reader)) {
				//		return false;
				//}

				//	token = ServiceConfiguration.SecurityTokenHandlers.ReadToken(reader);
			}
			//ClaimsIdentityCollection identities = ServiceConfiguration.SecurityTokenHandlers.ValidateToken(token);

			//HttpContext.Current.User = new ClaimsPrincipal(identities);

			return true;
		}

		/// <summary>
		///     Disposes of the resources (other than memory) used by the module that implements
		///     <see cref="T:System.Web.IHttpModule" />.
		/// </summary>
		public void Dispose() {}
	}
}

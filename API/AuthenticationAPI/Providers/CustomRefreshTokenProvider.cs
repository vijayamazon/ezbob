namespace Ezbob.API.AuthenticationAPI.Providers {
	using System;
	using System.Threading.Tasks;
	using Ezbob.API.AuthenticationAPI.Entities;
	using Microsoft.Owin.Security.Infrastructure;

	public class CustomRefreshTokenProvider : IAuthenticationTokenProvider {
		public async Task CreateAsync(AuthenticationTokenCreateContext context) {
			var clientid = context.Ticket.Properties.Dictionary["as:client_id"];

			if (string.IsNullOrEmpty(clientid)) {
				return;
			}

			var refreshTokenId = Guid.NewGuid().ToString("n");

			//Trace.WriteLine(string.Format("\n\n\n \t 1------------SimpleRefreshTokenProvider----refreshTokenId {0}", refreshTokenId));

			using (AuthRepository _repo = new AuthRepository()) {
				var refreshTokenLifeTime = context.OwinContext.Get<string>("as:clientRefreshTokenLifeTime");

				//	Trace.WriteLine(string.Format("2------------SimpleRefreshTokenProvider----refreshTokenLifeTime {0}, +time {1}", refreshTokenLifeTime, Convert.ToDouble(refreshTokenLifeTime).ToString()));

				var token = new RefreshToken() {
					Id = Helper.GetHash(refreshTokenId),
					ClientId = clientid,
					Subject = context.Ticket.Identity.Name,
					IssuedUtc = DateTime.UtcNow,
					ExpiresUtc = DateTime.UtcNow.AddMinutes(Convert.ToDouble(refreshTokenLifeTime))
				};

				//Trace.WriteLine(string.Format("2------------SimpleRefreshTokenProvider----token new {0}", JsonConvert.SerializeObject(token, Formatting.None, new JsonSerializerSettings {
				//	Formatting = Newtonsoft.Json.Formatting.None,
				//	ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
				//})));

				context.Ticket.Properties.IssuedUtc = token.IssuedUtc;
				context.Ticket.Properties.ExpiresUtc = token.ExpiresUtc;

				//Trace.WriteLine(string.Format("3------------SimpleRefreshTokenProvider----context.Ticket.Properties {0}", JsonConvert.SerializeObject(context.Ticket.Properties, Formatting.None, new JsonSerializerSettings {
				//	Formatting = Newtonsoft.Json.Formatting.None,
				//	ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
				//})));

				token.ProtectedTicket = context.SerializeTicket();

				//Trace.WriteLine(string.Format("4------------SimpleRefreshTokenProvider---- token.ProtectedTicket {0}", JsonConvert.SerializeObject(token.ProtectedTicket, Formatting.None, new JsonSerializerSettings {
				//	Formatting = Newtonsoft.Json.Formatting.None,
				//	ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
				//})));

				var result = await _repo.AddRefreshToken(token);

				//Trace.WriteLine(string.Format("5------------SimpleRefreshTokenProvider---- token result (added) {0}", JsonConvert.SerializeObject(result, Formatting.None, new JsonSerializerSettings {
				//	Formatting = Newtonsoft.Json.Formatting.None,
				//	ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
				//})));

				if (result) {
					context.SetToken(refreshTokenId);


					//Trace.WriteLine(string.Format("6------------SimpleRefreshTokenProvider----context.Token {0}", JsonConvert.SerializeObject(context.Token, Formatting.None, new JsonSerializerSettings {
					//	Formatting = Newtonsoft.Json.Formatting.None,
					//	ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
					//})));

				}

			}
		}

		public async Task ReceiveAsync(AuthenticationTokenReceiveContext context) {

			var allowedOrigin = context.OwinContext.Get<string>("as:clientAllowedOrigin");
			context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { allowedOrigin });

			string hashedTokenId = Helper.GetHash(context.Token);

			using (AuthRepository _repo = new AuthRepository()) {
				var refreshToken = await _repo.FindRefreshToken(hashedTokenId);

				if (refreshToken != null) {
					//Get protectedTicket from refreshToken class
					context.DeserializeTicket(refreshToken.ProtectedTicket);
					var result = await _repo.RemoveRefreshToken(hashedTokenId);
				}
			}
		}

		public void Create(AuthenticationTokenCreateContext context) {
			throw new NotImplementedException();
		}

		public void Receive(AuthenticationTokenReceiveContext context) {
			throw new NotImplementedException();
		}
	}
}
namespace Ezbob.API.ServerAPI {
	using System;
	using Microsoft.Owin.Security.Jwt;

	public class MyJwtOptions : JwtBearerAuthenticationOptions {
		public MyJwtOptions() {
			var issuer = "localhost";
			var audience = "all";
			var key = Convert.FromBase64String("UHxNtYMRYwvfpO1dS5pWLKL0M2DgOj40EbN4SoBWgfc");
			

			AllowedAudiences = new[] {
				audience
			};
			IssuerSecurityTokenProviders = new[] {
				new SymmetricKeyIssuerSecurityTokenProvider(issuer, key)
			};
		}
	}
}
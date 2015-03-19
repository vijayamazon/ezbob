namespace Ezbob.API.ServerAPI {
	using System;
	using Microsoft.Owin;
	using Microsoft.Owin.Security.OAuth;

	public class MyOAuthOptions : OAuthAuthorizationServerOptions {

		// ReSharper disable once FunctionRecursiveOnAllPaths
		public MyOAuthOptions() {

			TokenEndpointPath = new PathString("/token");
			AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(60);
			AccessTokenFormat = new MyJwtFormat(new MyOAuthOptions());
			Provider = new MyOAuthProvider();
			//#if DEBUG
			AllowInsecureHttp = true;
			//#endif
		}
	}
}
﻿namespace Ezbob.API.ServerAPI {
	using System;
	using System.Diagnostics;
	using System.Linq;
	using System.Security.Claims;

	public class AuthorizationManager : ClaimsAuthorizationManager {
		public override bool CheckAccess(AuthorizationContext context) {

			Trace.WriteLine("ClaimsAuthorizationManager_______________________");

			Trace.WriteLine("\nAction:");
			Trace.WriteLine("  " + context.Action.First().Value);

			Trace.WriteLine("\nResources:");
			foreach (var resource in context.Resource) {
				Trace.WriteLine("  " + resource.Value);
			}

			Trace.WriteLine("\nClaims:");
			foreach (var claim in context.Principal.Claims) {
				Trace.WriteLine("  " + claim.Value);
			}
		
			/*var resourc = context.Resource.First();
			if (resourc == null)
				throw new ArgumentNullException("resourc");

			string resource = context.Resource.First().Value;
			string action = context.Action.First().Value;

			if (action == "Show" && resource == "Code") {
				bool livesInSweden = context.Principal.HasClaim(ClaimTypes.Country, "Sweden");
				bool isAndras = context.Principal.HasClaim(ClaimTypes.GivenName, "Andras");
				return isAndras && livesInSweden;
			}*/

			return true;
		}
	}
}
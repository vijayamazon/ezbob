namespace EzbobAPI {
	using System.Security.Claims;

	public class CustomClaimsAuthenticationManager : ClaimsAuthenticationManager {
		public override ClaimsPrincipal Authenticate(string resourceName, ClaimsPrincipal incomingPrincipal) {
			return base.Authenticate(resourceName, incomingPrincipal);
		}
	}
}

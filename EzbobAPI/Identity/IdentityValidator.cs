using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EzbobAPI.Identity {
	using System.Diagnostics;
	using System.IdentityModel.Claims;
	using System.IdentityModel.Selectors;
	using System.Net;
	using System.Security.Principal;
	using System.ServiceModel;
	using System.ServiceModel.Web;

	public class IdentityValidator : UserNamePasswordValidator {
		public override void Validate(string userName, string password) {
			System.Diagnostics.Debug.WriteLine("==userName {0}, password {1}", userName, password);

			Debug.WriteLine("Check user name");

			if ((userName != "test") || (password != "tttttttt")) {
				var msg = String.Format("Unknown Username {0} or incorrect password {1}", userName, password);
				Trace.TraceWarning(msg);

				throw new WebFaultException(HttpStatusCode.BadRequest);
				//throw new FaultException(msg);//the client actually will receive MessageSecurityException. But if I throw MessageSecurityException, the runtime will give FaultException to client without clear message.
			}

			/*using (var context = new IdentityDbContext()) {
				using (var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context))) {
					var user = userManager.Find(userName, password);
					if (user == null) {
						var msg = String.Format("Unknown Username {0} or incorrect password {1}", userName, password);
						Trace.TraceWarning(msg);
						throw new FaultException(msg);//the client actually will receive MessageSecurityException. But if I throw MessageSecurityException, the runtime will give FaultException to client without clear message.
					}
				}

			}*/
		}
	}

	public class RoleAuthorizationManager : ServiceAuthorizationManager {
		protected override bool CheckAccessCore(OperationContext operationContext) {

		/*	// Find out the roleNames from the user database, for example, var roleNames = userManager.GetRoles(user.Id).ToArray();
			var roleNames = new string[] { "Alibaba" };
			operationContext.ServiceSecurityContext.AuthorizationContext.Properties["Principal"] = new GenericPrincipal(operationContext.ServiceSecurityContext.PrimaryIdentity, roleNames);
			return true;*/

			Debug.WriteLine("HasSupportingTokens: {0}", operationContext.HasSupportingTokens);

			// Extract the action URI from the OperationContext. Match this against the claims 
			// in the AuthorizationContext. 
			string action = operationContext.RequestContext.RequestMessage.Headers.Action;
		
			Debug.WriteLine("action: {0}", action);

			/*// Iterate through the various claim sets in the AuthorizationContext. 
			foreach (ClaimSet cs in operationContext.ServiceSecurityContext.AuthorizationContext.ClaimSets) {
				// Examine only those claim sets issued by System. 
				if (cs.Issuer == ClaimSet.System) {
					// Iterate through claims of type "http://example.org/claims/allowedoperation".
					foreach (Claim c in cs.FindClaims("http://example.org/claims/allowedoperation", Rights.PossessProperty)) {
						// Write the Claim resource to the console.
						Console.WriteLine("resource: {0}", c.Resource.ToString());

						// If the Claim resource matches the action URI then return true to allow access. 
						if (action == c.Resource.ToString())
							return true;
					}
				}
			}
			// If this point is reached, return false to deny access. 
			return false;  */

			return true;

		}

	}
}
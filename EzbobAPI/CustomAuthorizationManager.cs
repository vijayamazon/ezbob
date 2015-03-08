namespace EzbobAPI {
	using System;
	using System.Diagnostics;
	using System.Net;
	using System.ServiceModel;
	using System.ServiceModel.Web;
	using System.Text;

	public class CustomAuthorizationManager : ServiceAuthorizationManager {
		protected override bool CheckAccessCore(OperationContext operationContext) {

			System.Diagnostics.Debug.WriteLine("CheckAccessCore ");

			// Extract the action URI from the OperationContext. Match this against the claims
			// in the AuthorizationContext.
			//string action = operationContext.RequestContext.RequestMessage.Headers.Action;
			
			//Extract the Authorization header, and parse out the credentials converting the Base64 string:
			/*string authHeader;
			if (WebOperationContext.Current != null) {

				authHeader = WebOperationContext.Current.IncomingRequest.Headers["Authorization"];

				if ((authHeader != null) && (authHeader != string.Empty)) {

					var svcCredentials = ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(authHeader.Substring(6))).Split(':');

					var user = new { Name = svcCredentials[0], Password = svcCredentials[1] };

					System.Diagnostics.Debug.WriteLine("==userName {0}, password {1}", user.Name, user.Password);

					if ((user.Name == "user1" && user.Password == "test")) {
						//User is authrized and originating call will proceed
						System.Diagnostics.Debug.WriteLine("user found");
						return true;
					} else {
						//not authorized
						System.Diagnostics.Debug.WriteLine("user not found, authorized");
						return false;
					}
				} else {
					//No authorization header was provided, so challenge the client to provide before proceeding:
					WebOperationContext.Current.OutgoingResponse.Headers.Add("WWW-Authenticate: Basic realm=\"MyWCFService\"");
					//Throw an exception with the associated HTTP status code equivalent to HTTP status 401
					throw new WebFaultException(HttpStatusCode.Unauthorized);
				}
			}*/

			return false;
		}

		/*protected override bool CheckAccessCore(OperationContext operationContext) {
			//Extract the Authorization header, and parse out the credentials converting the Base64 string:
			var authHeader = WebOperationContext.Current.IncomingRequest.Headers["Authorization"];
			if ((authHeader != null) && (authHeader != string.Empty)) {
				var svcCredentials = ASCIIEncoding.ASCII
						.GetString(Convert.FromBase64String(authHeader.Substring(6)))
						.Split(':');
				var user = new { Name = svcCredentials[0], Password = svcCredentials[1] };
				if ((user.Name == "user1" && user.Password == "test")) {
					//User is authrized and originating call will proceed
					return true;
				} else {
					//not authorized
					return false;
				}
			} else {
				//No authorization header was provided, so challenge the client to provide before proceeding:
				WebOperationContext.Current.OutgoingResponse.Headers.Add("WWW-Authenticate: Basic realm=\"MyWCFService\"");
				//Throw an exception with the associated HTTP status code equivalent to HTTP status 401
				throw new WebFaultException(HttpStatusCode.Unauthorized);
			}
		}*/

		public override bool CheckAccess(OperationContext operationContext) {
			System.Diagnostics.Debug.WriteLine("CheckAccess");
			return true;
		}
	}
}

namespace EzbobAPI {
	using System;
	using System.Diagnostics;
	using System.IdentityModel.Selectors;
	using System.Net;
	using System.Security.Authentication;
	using System.ServiceModel;
	using System.ServiceModel.Web;
	

	public class ServiceValidator : UserNamePasswordValidator
    {
       /* [Inject]
        public IUserRepository UserRepository { get; set; }

		public ServiceValidator()
        {
            var resolver = new ServiceNinjectResolverFactory().CreateResolver();
            resolver.Inject(this);
        }*/

		public override void Validate(string userName, string password) {

			System.Diagnostics.Debug.WriteLine("==userName {0}, password {1}", userName, password);

			if (userName == null || password == null) {
				// auth headers not found  - how to add message?
				throw new WebFaultException(HttpStatusCode.BadRequest);
			}

			//No authorization header was provided, so challenge the client to provide before proceeding:
					//WebOperationContext.Current.OutgoingResponse.Headers.Add("WWW-Authenticate: Basic realm=\"MyWCFService\"");

			var user = new {
				Name = userName,
				Password = password
			};

			if (user.Name != "tom" || user.Password != "123") {

				
				//Throw an exception with the associated HTTP status code equivalent to HTTP status 401
				//try {

					System.Diagnostics.Debug.WriteLine("wrong user!!!");

					var msg = String.Format("Unknown Username {0} or incorrect password {1}", userName, password);
					Trace.TraceWarning(msg);
					throw new FaultException(msg);//the client actually will receive MessageSecurityException. But if I throw MessageSecurityException, the runtime will give FaultException to client without clear message.


				//	throw new WebFaultException(HttpStatusCode.Unauthorized);

				//} 
				/*catch (WebFaultException webFaultException) {

					System.Diagnostics.Debug.WriteLine("webFaultException: {0}", webFaultException.Message);

				}*/
			}
		}
    }

}
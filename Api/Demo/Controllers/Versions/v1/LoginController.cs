namespace Demo.Controllers.Versions.v1 {
	using System;
	using System.Net;
	using System.Web.Http;
	using Filters;
	using Infrastructure;
	using Models;

	/// <summary>
	/// Validates user credentials.
	/// </summary>
	[RoutePrefix("api/v1/login")]
	public class LoginController : ApiController {
		// POST api/login
		/// <summary>
		/// Validates user login credentials.
		/// <para>appkey header must be provided. HTTP status code 403 is returned when this header is missing.
		/// HTTP status code 401 is returned when this header contains an unexpected value.
		/// </para>
		/// </summary>
		/// <param name="oModel">User credentials (user name, password, app key).</param>
		/// <returns>Session token when credentials have been validated; HTTP status code 401 on unsuccessful login.</returns>
		[ValidateAppKey]
		[Route("")]
		public string Post([FromBody]LoginModel oModel) {
			try {
				var oSec = new SecurityStub();

				string sToken = oSec.Login(oModel, Request.GetRemoteIp());

				if (string.IsNullOrWhiteSpace(sToken))
					throw Return.Status(HttpStatusCode.Unauthorized, "Invalid user name or password.");

				return sToken;
			}
			catch (HttpResponseException) {
				throw;
			}
			catch (Exception e) {
				throw Return.Error("Failed to validate user credentials: {0}.", e.Message);
			} // try
		} // Post
	} // class LoginController
} // namespace

namespace Demo.Controllers.Versions.v1 {
	using System.Web.Http;
	using Filters;
	using Models;

	/// <summary>
	/// Validates user credentials.
	/// </summary>
	[HandleActionExecuted(2)]
	[RoutePrefix("api/v2/login")]
	public class V2LoginController : V1LoginController {
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
			return base.Post(oModel);
		} // Post
	} // class LoginController
} // namespace

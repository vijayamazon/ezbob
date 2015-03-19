namespace Ezbob.API.ServerAPI.Controllers
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Claims;
	using System.Threading.Tasks;
	using System.Web.Http;
	using Newtonsoft.Json;

	//using Thinktecture.IdentityModel.Authorization.WebApi;

	//[Authorize]
    [RoutePrefix("api/protected")]
    public class ProtectedController : ApiController
    {

		//[ClaimsAuthorize]
		//[PrincipalPermission(SecurityAction.Demand)]
        [Route("")]
        public IEnumerable<object> Get()
        {
            string token = "";

            //Microsoft.Owin.Security.AuthenticationTicket ticket = Startup.OAuthBearerOptions.AccessTokenFormat.Unprotect(token);
			//Console.WriteLine(ticket.Identity);

            var identity = User.Identity as ClaimsIdentity;
           
            return identity.Claims.Select(c => new
            {
                Type = c.Type,
                Value = c.Value,
				//ticket =  ticket.Identity.AuthenticationType
				somth = "hello"
            });
        }

		//[ClaimsAuthorize]
		[Authorize(Users = "notelka")]
		[Route("protected")]
		public async Task<IHttpActionResult> GetProtected() {

			var currentPrincipal = ClaimsPrincipal.Current;

			return Ok("UUUUU=====" + 
				JsonConvert.SerializeObject(currentPrincipal, Formatting.None, new JsonSerializerSettings {
					Formatting = Formatting.None,
					ReferenceLoopHandling = ReferenceLoopHandling.Ignore
				}));
		}


    }
}